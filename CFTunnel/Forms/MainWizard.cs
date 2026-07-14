using CFTunnel.Models;
using CFTunnel.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace CFTunnel.Forms
{
    public partial class MainWizard : Form
    {
        private readonly WizardState _state;
        private readonly AppSettings _settings;
        private readonly BindingList<Tunnel> _tunnels = new();
        private readonly BindingList<IngressRule> _rules = new();
        private Process? _cloudflaredProcess;
        private NotifyIcon? _tray;
        private bool _exitRequested;

        public MainWizard()
        {
            InitializeComponent();
            _state = new WizardState();
            _settings = AppSettings.Load();
            InitTray();
            Load += MainWizard_Load;
            FormClosing += MainWizard_FormClosing;
        }

        private void InitTray()
        {
            var menu = new ContextMenuStrip
            {
                Renderer = new ToolStripProfessionalRenderer(new DarkMenuColors()),
                BackColor = Theme.Panel,
                ForeColor = Theme.Fore,
            };
            menu.Items.Add("Show", null, (s, e) => ShowFromTray());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => { _exitRequested = true; Close(); });
            foreach (ToolStripItem item in menu.Items)
                item.ForeColor = Theme.Fore;

            _tray = new NotifyIcon
            {
                Icon = CreateAppIcon(),
                Text = "CFTunnel",
                Visible = true,
                ContextMenuStrip = menu,
            };
            _tray.DoubleClick += (s, e) => ShowFromTray();
            Icon = _tray.Icon;
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void MainWizard_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_exitRequested && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _tray?.ShowBalloonTip(2000, "CFTunnel", "Running in tray. Tunnel stays up. Right-click icon > Exit to quit.", ToolTipIcon.Info);
                return;
            }
            StopTunnel();
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
        }

        private static Icon CreateAppIcon()
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Theme.Panel);
                using var brush = new SolidBrush(Theme.Accent);
                g.FillEllipse(brush, 3, 3, 26, 26);
                TextRenderer.DrawText(g, "CF", new Font("Segoe UI", 11f, FontStyle.Bold),
                    new Rectangle(0, 0, 32, 32), Theme.Panel,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private void ShowPage(int index)
        {
            var pages = new Control[] { flowSetup, flowTunnels, flowRules, flowRun };
            var navs = new[] { btnNavSetup, btnNavTunnels, btnNavRules, btnNavRun };
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Visible = i == index;
                navs[i].BackColor = i == index ? Theme.Selection : Theme.Panel;
                navs[i].ForeColor = i == index ? Theme.Accent : Theme.ForeDim;
                navs[i].Font = new Font("Segoe UI", 10.5f, i == index ? FontStyle.Bold : FontStyle.Regular);
                navs[i].Tag = i == index;
                navs[i].Invalidate();
            }
            ActiveControl = null; // kill the white focus rectangle on flat nav buttons
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0); // WM_NCLBUTTONDOWN, HTCAPTION
            }
        }

        private void TitleBar_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        // Borderless form: hand back resize hit-testing on the 6px edge band.
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            if (m.Msg == WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);
                var pos = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16));
                const int grip = 6;
                bool left = pos.X < grip, right = pos.X >= ClientSize.Width - grip;
                bool top = pos.Y < grip, bottom = pos.Y >= ClientSize.Height - grip;
                int hit = 0;
                if (top && left) hit = 13;
                else if (top && right) hit = 14;
                else if (bottom && left) hit = 16;
                else if (bottom && right) hit = 17;
                else if (left) hit = 10;
                else if (right) hit = 11;
                else if (top) hit = 12;
                else if (bottom) hit = 15;
                if (hit != 0) m.Result = hit;
                return;
            }
            base.WndProc(ref m);
        }

        private async void MainWizard_Load(object? sender, EventArgs e)
        {
            dgvTunnels.DataSource = _tunnels;
            dgvRules.DataSource = _rules;
            MaximizedBounds = Screen.FromControl(this).WorkingArea;
            WireEvents();
            ShowPage(0);
            LoadSavedCredentials();
            LoadExistingConfig();
            await RefreshStatusAsync();
            UpdateRunningState();

            if (_state.SelectedTunnel != null && _state.BinaryInstalled && _state.LoggedIn)
                await ResolveTunnelNameAsync();
            if (!string.IsNullOrEmpty(_state.ApiToken))
                await VerifySavedTokenAsync();
        }

        private void LoadSavedCredentials()
        {
            var token = _settings.GetApiToken();
            if (string.IsNullOrEmpty(token))
                return;

            txtApiKey.Text = token;
            txtApiEmail.Text = _settings.ApiEmail;
            _state.ApiToken = token;
            _state.ApiEmail = _settings.ApiEmail;
            lblKeyStatus.Text = "Status: loaded from saved settings";
            Log("Loaded saved API credentials.");
        }

        private void LoadExistingConfig()
        {
            var (tunnelId, rules) = ConfigWriter.ReadConfig();
            if (tunnelId == null && rules.Count == 0)
                return;

            foreach (var rule in rules)
                _rules.Add(rule);

            if (!string.IsNullOrEmpty(tunnelId))
            {
                _state.SelectedTunnel = new Tunnel { Id = tunnelId, Name = "(from config)" };
                lblSelectedTunnel.Text = $"Selected tunnel: (from config) ({tunnelId})";
            }

            Log($"Loaded existing config: {rules.Count} rule(s), tunnel {tunnelId ?? "none"}.");
        }

        private async Task ResolveTunnelNameAsync()
        {
            try
            {
                var configId = _state.SelectedTunnel?.Id;
                await RefreshTunnelsAsync();
                var match = _tunnels.FirstOrDefault(t => t.Id.Equals(configId, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    SelectTunnel(match);
            }
            catch (Exception ex)
            {
                Log($"Could not resolve tunnel name: {ex.Message}");
            }
        }

        private async Task VerifySavedTokenAsync()
        {
            try
            {
                var api = new CloudflareApi(_state.ApiToken, string.IsNullOrEmpty(_state.ApiEmail) ? null : _state.ApiEmail);
                _state.TokenVerified = await api.VerifyTokenAsync();
                lblKeyStatus.Text = _state.TokenVerified ? "Status: verified (saved)" : "Status: saved credentials invalid";
                Log(_state.TokenVerified ? "Saved API credentials verified." : "Saved API credentials invalid. Re-verify on Setup tab.");
            }
            catch (Exception ex)
            {
                Log($"Saved credential check failed: {ex.Message}");
            }
        }

        private void WireEvents()
        {
            btnNavSetup.Click += (s, e) => ShowPage(0);
            btnNavTunnels.Click += (s, e) => ShowPage(1);
            btnNavRules.Click += (s, e) => ShowPage(2);
            btnNavRun.Click += (s, e) => ShowPage(3);
            btnWinMin.Click += (s, e) => WindowState = FormWindowState.Minimized;
            btnWinClose.Click += (s, e) => Close();
            btnDownload.Click += async (s, e) => await DownloadAsync();
            btnLogin.Click += async (s, e) => await LoginAsync();
            btnVerifyKey.Click += async (s, e) => await VerifyKeyAsync();
            btnRefreshTunnels.Click += async (s, e) => await RefreshTunnelsAsync();
            btnSelectTunnel.Click += (s, e) => SelectTunnel();
            btnDeleteTunnel.Click += async (s, e) => await DeleteTunnelAsync();
            btnCreateTunnel.Click += async (s, e) => await CreateTunnelAsync();
            btnAddRule.Click += (s, e) => AddRule();
            btnEditRule.Click += (s, e) => EditRule();
            btnDeleteRule.Click += (s, e) => DeleteRule();
            btnQuickAdd.Click += (s, e) => QuickAddRule();
            txtQuickPort.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; QuickAddRule(); } };
            dgvRules.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditRule(); };
            btnCreateDns.Click += async (s, e) => await CreateDnsAsync();
            btnWriteConfig.Click += async (s, e) => await WriteConfigAsync();
            btnSaveCredentials.Click += async (s, e) => await SaveCredentialsAsync();
            btnStartTunnel.Click += async (s, e) => await StartTunnelAsync();
            btnStopTunnel.Click += (s, e) => StopTunnel();
            btnInstallService.Click += async (s, e) => await InstallServiceAsync();
            btnStartService.Click += async (s, e) => await StartServiceAsync();
            btnStopService.Click += async (s, e) => await StopServiceAsync();
            btnRestartService.Click += async (s, e) => await RestartServiceAsync();
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetBusy), busy);
                return;
            }
            pnlMain.Enabled = !busy;
            sidebar.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private async Task RefreshStatusAsync()
        {
            _state.BinaryInstalled = CloudflaredCli.IsInstalled;
            _state.LoggedIn = CloudflaredCli.IsLoggedIn;

            lblCloudflaredStatus.Text = _state.BinaryInstalled
                ? $"cloudflared: installed ({CloudflaredCli.ExePath})"
                : "cloudflared: not installed";
            btnDownload.Enabled = !_state.BinaryInstalled;

            lblCertStatus.Text = _state.LoggedIn
                ? $"Cert: found ({CloudflaredCli.CertPath})"
                : "Cert: not found";

            await RefreshServiceStatusAsync();
        }

        private async Task RefreshServiceStatusAsync()
        {
            await Task.Yield();
            var status = WindowsService.GetStatus();
            lblServiceStatus.Text = status.HasValue
                ? $"Service status: {status.Value}"
                : "Service status: not installed";
        }

        private void UpdateRunningState()
        {
            bool running = _cloudflaredProcess != null && !_cloudflaredProcess.HasExited;
            btnStartTunnel.Enabled = !running;
            btnStopTunnel.Enabled = running;
        }

        private async Task DownloadAsync()
        {
            SetBusy(true);
            try
            {
                await CloudflaredCli.DownloadAsync(new Progress<string>(Log), CancellationToken.None);
                Log("cloudflared installed.");
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Download failed: {ex.Message}");
                MessageBox.Show(ex.Message, "Download failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false); }
        }

        private async Task LoginAsync()
        {
            if (CloudflaredCli.IsLoggedIn)
            {
                var result = MessageBox.Show(
                    $"A cert already exists at {CloudflaredCli.CertPath}. Login will overwrite it. Continue?",
                    "Overwrite cert?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    Log("Login cancelled. Existing cert kept.");
                    return;
                }
                File.Delete(CloudflaredCli.CertPath);
            }

            SetBusy(true);
            try
            {
                var loginResult = await CloudflaredCli.LoginAsync(new Progress<string>(Log), CancellationToken.None);
                if (loginResult.ExitCode != 0)
                {
                    Log($"Login failed: {loginResult.Error}");
                    return;
                }
                Log("Login completed. Waiting for cert.pem...");
                var deadline = DateTime.Now.AddMinutes(2);
                while (!CloudflaredCli.IsLoggedIn && DateTime.Now < deadline)
                    await Task.Delay(1000);
                Log(CloudflaredCli.IsLoggedIn ? "Cert.pem found." : "Cert.pem not found after timeout.");
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Login failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task VerifyKeyAsync()
        {
            var key = txtApiKey.Text.Trim();
            var email = txtApiEmail.Text.Trim();
            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Enter an API token or Global API Key.", "Auth", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var api = new CloudflareApi(key, string.IsNullOrEmpty(email) ? null : email);
                _state.TokenVerified = await api.VerifyTokenAsync();
                _state.ApiToken = _state.TokenVerified ? key : string.Empty;
                _state.ApiEmail = _state.TokenVerified ? email : string.Empty;
                lblKeyStatus.Text = _state.TokenVerified ? "Status: verified" : "Status: invalid";
                Log(_state.TokenVerified ? "API credentials verified." : "API credentials invalid.");

                if (_state.TokenVerified)
                {
                    _settings.SetApiToken(key);
                    _settings.ApiEmail = email;
                    _settings.Save();
                    Log("Credentials saved for next launch (DPAPI encrypted).");
                }
            }
            catch (Exception ex)
            {
                Log($"Verification failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task RefreshTunnelsAsync()
        {
            SetBusy(true);
            try
            {
                var list = await CloudflaredCli.ListTunnelsAsync(CancellationToken.None);
                _tunnels.Clear();
                foreach (var t in list) _tunnels.Add(t);
                Log($"Loaded {list.Count} tunnels.");
            }
            catch (Exception ex)
            {
                Log($"List tunnels failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private void SelectTunnel()
        {
            if (dgvTunnels.CurrentRow?.DataBoundItem is Tunnel tunnel)
            {
                SelectTunnel(tunnel);
            }
        }

        private void SelectTunnel(Tunnel tunnel)
        {
            _state.SelectedTunnel = tunnel;
            lblSelectedTunnel.Text = $"Selected tunnel: {tunnel.Name} ({tunnel.Id})";
            Log($"Selected tunnel: {tunnel.Name}");
        }

        private async Task CreateTunnelAsync()
        {
            var name = txtNewTunnelName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Enter a tunnel name.", "Create tunnel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var tunnel = await CloudflaredCli.CreateTunnelAsync(name, new Progress<string>(Log), CancellationToken.None);
                if (tunnel != null)
                {
                    Log($"Using tunnel {tunnel.Name} ({tunnel.Id}).");
                    txtNewTunnelName.Clear();
                    await RefreshTunnelsAsync();
                    SelectTunnel(tunnel);
                }
                else
                {
                    Log("Failed to create tunnel.");
                }
            }
            catch (Exception ex)
            {
                Log($"Create tunnel failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task DeleteTunnelAsync()
        {
            if (dgvTunnels.CurrentRow?.DataBoundItem is not Tunnel tunnel)
                return;
            var confirm = MessageBox.Show($"Delete tunnel {tunnel.Name}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            SetBusy(true);
            try
            {
                var result = await CloudflaredCli.RunAsync($"tunnel delete {tunnel.Id}", null, CancellationToken.None);
                if (result.ExitCode == 0)
                {
                    Log($"Deleted tunnel {tunnel.Name}.");
                    await RefreshTunnelsAsync();
                }
                else
                {
                    Log($"Failed to delete tunnel: {result.Error}");
                }
            }
            catch (Exception ex)
            {
                Log($"Delete tunnel failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private void QuickAddRule()
        {
            var host = txtQuickHost.Text.Trim().TrimEnd('.');
            var portText = txtQuickPort.Text.Trim();

            if (string.IsNullOrEmpty(host) || !host.Contains('.'))
            {
                MessageBox.Show("Enter a full hostname, e.g. app.imtaqin.id", "Add mapping", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(portText, out var port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Enter a valid local port (1-65535), e.g. 3000", "Add mapping", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_rules.Any(r => r.Hostname.Equals(host, StringComparison.OrdinalIgnoreCase)))
            {
                Log($"Mapping for {host} already exists.");
                return;
            }

            _rules.Add(new IngressRule { Hostname = host, Service = $"http://localhost:{port}" });
            Log($"Added mapping: {host} -> localhost:{port}");
            txtQuickHost.Clear();
            txtQuickPort.Clear();
            txtQuickHost.Focus();
        }

        private void AddRule()
        {
            using var dlg = new EditIngressDialog();
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Rule != null)
                _rules.Add(dlg.Rule);
        }

        private void EditRule()
        {
            if (dgvRules.CurrentRow?.DataBoundItem is not IngressRule rule) return;
            using var dlg = new EditIngressDialog(rule);
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Rule != null)
                _rules.ResetBindings();
        }

        private void DeleteRule()
        {
            if (dgvRules.CurrentRow?.DataBoundItem is IngressRule rule)
                _rules.Remove(rule);
        }

        private async Task WriteConfigAsync()
        {
            if (_state.SelectedTunnel == null)
            {
                MessageBox.Show("Select a tunnel first.", "Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var credPath = Path.Combine(CloudflaredCli.UserCloudflaredDir, $"{_state.SelectedTunnel.Id}.json");
                if (!File.Exists(credPath))
                    await SaveCredentialsAsync();
                ConfigWriter.WriteConfig(_state.SelectedTunnel.Id, credPath, _rules.ToList());
                Log($"Wrote config to {ConfigWriter.ConfigPath}");
            }
            catch (Exception ex)
            {
                Log($"Write config failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task SaveCredentialsAsync()
        {
            if (_state.SelectedTunnel == null)
            {
                MessageBox.Show("Select a tunnel first.", "Credentials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true);
            try
            {
                var credPath = Path.Combine(CloudflaredCli.UserCloudflaredDir, $"{_state.SelectedTunnel.Id}.json");
                var result = await CloudflaredCli.TokenCredFileAsync(_state.SelectedTunnel.Id, credPath, CancellationToken.None);
                if (result.ExitCode == 0)
                    Log($"Saved credentials: {credPath}");
                else
                    Log($"Failed to save credentials: {result.Error}");
            }
            catch (Exception ex)
            {
                Log($"Save credentials failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task CreateDnsAsync()
        {
            if (_state.SelectedTunnel == null)
            {
                MessageBox.Show("Select a tunnel first.", "DNS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool hasApi = !string.IsNullOrEmpty(_state.ApiToken);
            bool hasCert = CloudflaredCli.IsLoggedIn;
            if (!hasApi && !hasCert)
            {
                MessageBox.Show("Verify API credentials or login with browser first (Setup tab).", "DNS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var zoneName = txtZoneName.Text.Trim();
            SetBusy(true);
            try
            {
                var api = hasApi
                    ? new CloudflareApi(_state.ApiToken, string.IsNullOrEmpty(_state.ApiEmail) ? null : _state.ApiEmail)
                    : null;

                string? manualZoneId = null;
                if (api != null && !string.IsNullOrEmpty(zoneName))
                {
                    manualZoneId = await api.GetZoneIdAsync(zoneName);
                    if (string.IsNullOrEmpty(manualZoneId))
                        Log($"Zone {zoneName} not found via API. Will auto-detect per hostname instead.");
                }

                var zoneCache = new Dictionary<string, string?>();
                int success = 0, total = 0;
                foreach (var rule in _rules)
                {
                    if (string.IsNullOrWhiteSpace(rule.Hostname)) continue;
                    total++;

                    bool created = false;
                    if (hasCert)
                    {
                        var result = await CloudflaredCli.RouteDnsAsync(_state.SelectedTunnel.Id, rule.Hostname, CancellationToken.None);
                        created = result.ExitCode == 0;
                        if (!created)
                            Log($"route dns failed for {rule.Hostname}: {(result.Output + result.Error).Trim()}");
                    }

                    if (!created && api != null)
                    {
                        Log($"Trying Cloudflare API for {rule.Hostname}...");
                        string? zoneId = manualZoneId;
                        if (string.IsNullOrEmpty(zoneId) && !zoneCache.TryGetValue(rule.Hostname, out zoneId))
                        {
                            zoneId = await api.GetZoneIdFromHostnameAsync(rule.Hostname);
                            zoneCache[rule.Hostname] = zoneId;
                        }

                        if (!string.IsNullOrEmpty(zoneId))
                            created = await api.CreateCnameAsync(zoneId, rule.Hostname, _state.SelectedTunnel.Id);
                    }

                    if (created)
                    {
                        success++;
                        Log($"DNS OK: {rule.Hostname} -> {_state.SelectedTunnel.Id}.cfargotunnel.com");
                    }
                    else
                    {
                        Log($"DNS FAILED for {rule.Hostname}. Check token permissions (Zone:Read + DNS:Edit) or add CNAME manually in the dashboard.");
                    }
                }
                Log($"Created {success}/{total} DNS records.");
            }
            catch (Exception ex)
            {
                Log($"Create DNS failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task StartTunnelAsync()
        {
            if (_cloudflaredProcess != null && !_cloudflaredProcess.HasExited)
            {
                Log("Tunnel already running.");
                return;
            }
            if (_state.SelectedTunnel == null)
            {
                MessageBox.Show("Select a tunnel first.", "Run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(ConfigWriter.ConfigPath))
            {
                MessageBox.Show("Write config first.", "Run", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var psi = new ProcessStartInfo(CloudflaredCli.ExePath, $"--config \"{ConfigWriter.ConfigPath}\" tunnel run")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            _cloudflaredProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _cloudflaredProcess.OutputDataReceived += (s, e) => { if (e.Data != null) Log(e.Data); };
            _cloudflaredProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) Log(e.Data); };
            _cloudflaredProcess.Exited += (s, e) => { Log("cloudflared stopped."); UpdateRunningState(); };
            _cloudflaredProcess.Start();
            _cloudflaredProcess.BeginOutputReadLine();
            _cloudflaredProcess.BeginErrorReadLine();
            Log("cloudflared started.");
            UpdateRunningState();
        }

        private void StopTunnel()
        {
            if (_cloudflaredProcess != null && !_cloudflaredProcess.HasExited)
            {
                try { _cloudflaredProcess.Kill(); } catch (Exception ex) { Log($"Stop failed: {ex.Message}"); }
            }
            UpdateRunningState();
        }

        private async Task InstallServiceAsync()
        {
            SetBusy(true);
            try
            {
                var status = WindowsService.GetStatus();
                if (status.HasValue)
                {
                    Log("Service is already installed.");
                    return;
                }

                var result = await CloudflaredCli.InstallServiceAsync(ConfigWriter.ConfigPath, CancellationToken.None);
                if (result.ExitCode == 0)
                    Log("Service installed.");
                else
                    Log($"Service install failed: {result.Error}");
                await RefreshServiceStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Install service failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task StartServiceAsync()
        {
            SetBusy(true);
            try
            {
                var status = WindowsService.GetStatus();
                if (!status.HasValue)
                {
                    Log("Service is not installed.");
                    return;
                }
                if (status.Value == ServiceControllerStatus.Running)
                {
                    Log("Service is already running.");
                    return;
                }
                if (status.Value == ServiceControllerStatus.StartPending)
                {
                    Log("Service is starting.");
                    return;
                }

                try
                {
                    await WindowsService.StartAsync(CancellationToken.None);
                    Log("Service started.");
                }
                catch (Exception ex)
                {
                    Log($"ServiceController: {ex.Message}");
                    Log("Trying sc start for details...");
                    var sc = await RunScAsync("start cloudflared");
                    Log(sc.Output + sc.Error);
                }
                await RefreshServiceStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Start service failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task StopServiceAsync()
        {
            SetBusy(true);
            try
            {
                var status = WindowsService.GetStatus();
                if (!status.HasValue)
                {
                    Log("Service is not installed.");
                    return;
                }
                if (status.Value == ServiceControllerStatus.Stopped)
                {
                    Log("Service is already stopped.");
                    return;
                }

                await WindowsService.StopAsync(CancellationToken.None);
                Log("Service stopped.");
                await RefreshServiceStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Stop service failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task RestartServiceAsync()
        {
            SetBusy(true);
            try
            {
                var status = WindowsService.GetStatus();
                if (!status.HasValue)
                {
                    Log("Service is not installed. Install it first.");
                    return;
                }

                await WindowsService.RestartAsync(CancellationToken.None);
                Log("Service restarted.");
                await RefreshServiceStatusAsync();
            }
            catch (Exception ex)
            {
                Log($"Restart service failed: {ex.Message}");
            }
            finally { SetBusy(false); }
        }

        private async Task<ProcessResult> RunScAsync(string arguments)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();
            var psi = new ProcessStartInfo("sc", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var output = new StringBuilder();
            var error = new StringBuilder();
            process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };
            process.Exited += (s, e) => tcs.TrySetResult(new ProcessResult { ExitCode = process.ExitCode, Output = output.ToString(), Error = error.ToString() });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return await tcs.Task;
        }
    }
}
