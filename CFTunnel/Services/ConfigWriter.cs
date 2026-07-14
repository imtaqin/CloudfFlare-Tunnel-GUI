using CFTunnel.Models;
using System.Text;

namespace CFTunnel.Services
{
    public static class ConfigWriter
    {
        public static string ConfigDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Cloudflare",
            "cloudflared");

        public static string ConfigPath => Path.Combine(ConfigDirectory, "config.yml");

        public static string LogPath => Path.Combine(ConfigDirectory, "cloudflared.log");

        public static void WriteConfig(string tunnelId, string credentialsPath, IEnumerable<IngressRule> rules)
        {
            Directory.CreateDirectory(ConfigDirectory);

            var sb = new StringBuilder();
            sb.AppendLine($"tunnel: {tunnelId}");
            sb.AppendLine($"credentials-file: {credentialsPath}");
            sb.AppendLine($"logfile: {LogPath}");
            sb.AppendLine("ingress:");

            foreach (var rule in rules)
            {
                sb.AppendLine($"  - hostname: {rule.Hostname}");
                sb.AppendLine($"    service: {rule.Service}");

                if (rule.OriginRequest.Count > 0)
                {
                    sb.AppendLine("    originRequest:");
                    foreach (var kv in rule.OriginRequest)
                        sb.AppendLine($"      {kv.Key}: {kv.Value}");
                }
            }

            sb.AppendLine("  - service: http_status:404");
            File.WriteAllText(ConfigPath, sb.ToString());
        }

        // Parses only the config.yml layout produced by WriteConfig above.
        public static (string? TunnelId, List<IngressRule> Rules) ReadConfig()
        {
            var rules = new List<IngressRule>();
            string? tunnelId = null;
            if (!File.Exists(ConfigPath))
                return (null, rules);

            IngressRule? current = null;
            bool inOriginRequest = false;
            foreach (var raw in File.ReadAllLines(ConfigPath))
            {
                var trimmed = raw.Trim();
                if (trimmed.StartsWith("tunnel:"))
                {
                    tunnelId = trimmed["tunnel:".Length..].Trim();
                }
                else if (trimmed.StartsWith("- hostname:"))
                {
                    current = new IngressRule { Hostname = trimmed["- hostname:".Length..].Trim() };
                    rules.Add(current);
                    inOriginRequest = false;
                }
                else if (trimmed.StartsWith("- service:"))
                {
                    current = null; // catch-all rule, not editable
                    inOriginRequest = false;
                }
                else if (current != null && trimmed.StartsWith("originRequest:"))
                {
                    inOriginRequest = true;
                }
                else if (current != null && trimmed.StartsWith("service:"))
                {
                    current.Service = trimmed["service:".Length..].Trim();
                    inOriginRequest = false;
                }
                else if (current != null && inOriginRequest && trimmed.Contains(':'))
                {
                    var idx = trimmed.IndexOf(':');
                    current.OriginRequest[trimmed[..idx].Trim()] = trimmed[(idx + 1)..].Trim();
                }
            }
            return (tunnelId, rules);
        }
    }
}
