using CFTunnel.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CFTunnel.Services
{
    public static class CloudflaredCli
    {
        public static string ExePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "cloudflared",
            "cloudflared.exe");

        public static string UserCloudflaredDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cloudflared");

        public static string CertPath => Path.Combine(UserCloudflaredDir, "cert.pem");

        public static bool IsInstalled => File.Exists(ExePath);

        public static bool IsLoggedIn => File.Exists(CertPath);

        public static async Task DownloadAsync(IProgress<string> progress, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            var url = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";
            var directory = Path.GetDirectoryName(ExePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            progress?.Report("Downloading cloudflared from GitHub...");
            using var stream = await client.GetStreamAsync(url, cancellationToken);
            using var file = File.Create(ExePath);
            await stream.CopyToAsync(file, cancellationToken);
            progress?.Report("Download complete.");
        }

        public static async Task<ProcessResult> LoginAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();
            var psi = new ProcessStartInfo(ExePath, "tunnel login")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    progress?.Report(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    error.AppendLine(e.Data);
                    progress?.Report(e.Data);

                    if (e.Data.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(e.Data) { UseShellExecute = true });
                        }
                        catch { }
                    }
                }
            };

            process.Exited += (s, e) => tcs.TrySetResult(new ProcessResult
            {
                ExitCode = process.ExitCode,
                Output = output.ToString(),
                Error = error.ToString()
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (cancellationToken.Register(() =>
            {
                try { process.Kill(); } catch { }
                tcs.TrySetCanceled();
            }))
            {
                return await tcs.Task;
            }
        }

        public static async Task<Tunnel?> CreateTunnelAsync(string name, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            var result = await RunAsync($"tunnel create \"{name}\"", progress, cancellationToken);
            var tunnels = await ListTunnelsAsync(cancellationToken);
            var existing = tunnels.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return existing;
        }

        public static async Task<List<Tunnel>> ListTunnelsAsync(CancellationToken cancellationToken = default)
        {
            var result = await RunAsync("tunnel list --output json", null, cancellationToken);
            if (result.ExitCode != 0)
                return new List<Tunnel>();

            try
            {
                return JsonSerializer.Deserialize<List<Tunnel>>(result.Output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<Tunnel>();
            }
            catch
            {
                return new List<Tunnel>();
            }
        }

        public static async Task<ProcessResult> RouteDnsAsync(string tunnelId, string hostname, CancellationToken cancellationToken = default)
        {
            return await RunAsync($"tunnel route dns --overwrite-dns {tunnelId} \"{hostname}\"", null, cancellationToken);
        }

        public static async Task<ProcessResult> TokenCredFileAsync(string tunnelId, string credPath, CancellationToken cancellationToken = default)
        {
            return await RunAsync($"tunnel token --cred-file \"{credPath}\" {tunnelId}", null, cancellationToken);
        }

        public static async Task<ProcessResult> InstallServiceAsync(string configPath, CancellationToken cancellationToken = default)
        {
            return await RunAsync($"--config \"{configPath}\" service install", null, cancellationToken);
        }

        public static async Task<ProcessResult> UninstallServiceAsync(CancellationToken cancellationToken = default)
        {
            return await RunAsync("service uninstall", null, cancellationToken);
        }

        public static async Task<ProcessResult> RunAsync(string arguments, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();
            var psi = new ProcessStartInfo(ExePath, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(ExePath),
            };

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    progress?.Report(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    error.AppendLine(e.Data);
                    progress?.Report(e.Data);
                }
            };

            process.Exited += (s, e) => tcs.TrySetResult(new ProcessResult
            {
                ExitCode = process.ExitCode,
                Output = output.ToString(),
                Error = error.ToString()
            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (cancellationToken.Register(() =>
            {
                try { process.Kill(); } catch { }
                tcs.TrySetCanceled();
            }))
            {
                return await tcs.Task;
            }
        }
    }
}
