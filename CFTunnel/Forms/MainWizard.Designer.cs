namespace CFTunnel.Forms
{
    partial class MainWizard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            titleBar = new Panel();
            lblTitle = new Label();
            btnWinMin = new Button();
            btnWinClose = new Button();

            sidebar = new Panel();
            navFlow = new FlowLayoutPanel();
            btnNavSetup = new Button();
            btnNavTunnels = new Button();
            btnNavRules = new Button();
            btnNavRun = new Button();
            lblVersion = new Label();

            pnlMain = new Panel();
            splitContainer = new SplitContainer();
            txtLog = new TextBox();
            lblLogHeader = new Label();

            flowSetup = new FlowLayoutPanel();
            lblSetupHeader = new Label();
            lblCloudflaredStatus = new Label();
            btnDownload = new Button();
            lblCertStatus = new Label();
            btnLogin = new Button();
            lblApiKey = new Label();
            lblApiHint = new Label();
            txtApiKey = new TextBox();
            lblApiEmail = new Label();
            txtApiEmail = new TextBox();
            btnVerifyKey = new Button();
            lblKeyStatus = new Label();

            flowTunnels = new FlowLayoutPanel();
            lblTunnelsHeader = new Label();
            lblSelectedTunnel = new Label();
            flowTunnelButtons = new FlowLayoutPanel();
            btnRefreshTunnels = new Button();
            btnSelectTunnel = new Button();
            btnDeleteTunnel = new Button();
            dgvTunnels = new DataGridView();
            txtNewTunnelName = new TextBox();
            btnCreateTunnel = new Button();

            flowRules = new FlowLayoutPanel();
            lblRulesHeader = new Label();
            lblRulesSteps = new Label();
            flowQuickAdd = new FlowLayoutPanel();
            txtQuickHost = new TextBox();
            txtQuickPort = new TextBox();
            btnQuickAdd = new Button();
            flowRuleButtons = new FlowLayoutPanel();
            btnAddRule = new Button();
            btnEditRule = new Button();
            btnDeleteRule = new Button();
            dgvRules = new DataGridView();
            lblZoneName = new Label();
            txtZoneName = new TextBox();
            lblZoneHint = new Label();
            btnCreateDns = new Button();
            btnWriteConfig = new Button();
            btnSaveCredentials = new Button();

            flowRun = new FlowLayoutPanel();
            lblRunHeader = new Label();
            lblServiceStatus = new Label();
            lblRunHint = new Label();
            flowRunButtons = new FlowLayoutPanel();
            btnStartTunnel = new Button();
            btnStopTunnel = new Button();
            btnInstallService = new Button();
            btnStartService = new Button();
            btnStopService = new Button();
            btnRestartService = new Button();

            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTunnels).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvRules).BeginInit();
            SuspendLayout();

            // MainWizard
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Theme.Back;
            ForeColor = Theme.Fore;
            Font = Theme.UI;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(920, 620);
            MinimumSize = new Size(720, 480);
            Name = "MainWizard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CFTunnel";
            Controls.Add(pnlMain);
            Controls.Add(sidebar);
            Controls.Add(titleBar);
            Padding = new Padding(1);

            // titleBar
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 36;
            titleBar.BackColor = Theme.Back;
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseDoubleClick += TitleBar_MouseDoubleClick;
            titleBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Theme.Selection);
                e.Graphics.DrawLine(pen, 0, titleBar.Height - 1, titleBar.Width, titleBar.Height - 1);
            };

            lblTitle.AutoSize = true;
            lblTitle.Text = "CFTunnel";
            lblTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblTitle.ForeColor = Theme.Fore;
            lblTitle.Location = new Point(14, 8);
            lblTitle.MouseDown += TitleBar_MouseDown;
            lblTitle.MouseDoubleClick += TitleBar_MouseDoubleClick;
            titleBar.Controls.Add(lblTitle);

            ConfigureWindowButton(btnWinClose, "✕");
            btnWinClose.Dock = DockStyle.Right;
            btnWinClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(150, 60, 66);
            titleBar.Controls.Add(btnWinClose);

            ConfigureWindowButton(btnWinMin, "─");
            btnWinMin.Dock = DockStyle.Right;
            titleBar.Controls.Add(btnWinMin);

            // sidebar
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 148;
            sidebar.BackColor = Theme.Panel;
            sidebar.Padding = new Padding(0, 8, 0, 0);

            navFlow.Dock = DockStyle.Fill;
            navFlow.FlowDirection = FlowDirection.TopDown;
            navFlow.WrapContents = false;
            navFlow.Padding = new Padding(0);
            navFlow.Margin = new Padding(0);
            ConfigureNavButton(btnNavSetup, "Setup");
            ConfigureNavButton(btnNavTunnels, "Tunnels");
            ConfigureNavButton(btnNavRules, "Rules");
            ConfigureNavButton(btnNavRun, "Run");
            navFlow.Controls.AddRange(new Control[] { btnNavSetup, btnNavTunnels, btnNavRules, btnNavRun });

            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.Height = 26;
            lblVersion.Text = "Cloudflare Tunnel Manager";
            lblVersion.ForeColor = Theme.Hint;
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            lblVersion.Font = new Font("Segoe UI", 8.5f);
            lblVersion.ForeColor = Theme.ForeDim;

            sidebar.Controls.Add(navFlow);
            sidebar.Controls.Add(lblVersion);

            // pnlMain + splitContainer
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BackColor = Theme.Back;
            pnlMain.Padding = new Padding(1, 0, 0, 0);
            pnlMain.Controls.Add(splitContainer);

            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = 400;
            splitContainer.SplitterWidth = 3;
            splitContainer.BackColor = Theme.Border;
            splitContainer.Panel1.BackColor = Theme.Panel;
            splitContainer.Panel2.BackColor = Theme.Back;
            splitContainer.Panel1.Controls.Add(flowSetup);
            splitContainer.Panel1.Controls.Add(flowTunnels);
            splitContainer.Panel1.Controls.Add(flowRules);
            splitContainer.Panel1.Controls.Add(flowRun);
            splitContainer.Panel2.Controls.Add(txtLog);
            splitContainer.Panel2.Controls.Add(lblLogHeader);

            // log
            lblLogHeader.Dock = DockStyle.Top;
            lblLogHeader.Height = 22;
            lblLogHeader.Text = "  Log";
            lblLogHeader.ForeColor = Theme.Hint;
            lblLogHeader.BackColor = Theme.Back;
            lblLogHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblLogHeader.Font = Theme.UIBold;

            txtLog.Dock = DockStyle.Fill;
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.BackColor = Theme.Back;
            txtLog.ForeColor = Theme.Green;
            txtLog.Font = Theme.Mono;
            txtLog.BorderStyle = BorderStyle.None;

            // Setup page
            ConfigureFlow(flowSetup);
            ConfigureHeader(lblSetupHeader, "Setup");
            ConfigureLabel(lblCloudflaredStatus, "cloudflared: checking...");
            ConfigureButton(btnDownload, "Download cloudflared");
            ConfigureLabel(lblCertStatus, "Cert: checking...");
            ConfigureButton(btnLogin, "Login with browser");
            ConfigureLabel(lblApiKey, "API token or Global API Key:");
            ConfigureHint(lblApiHint, "Token needs Zone:Read + DNS:Edit permissions (dash.cloudflare.com > My Profile > API Tokens). Optional if you logged in with the browser.");
            txtApiKey.Width = 400;
            txtApiKey.PasswordChar = '*';
            ConfigureLabel(lblApiEmail, "Account email (leave blank for API token):");
            txtApiEmail.Width = 400;
            txtApiEmail.PlaceholderText = "you@example.com";
            ConfigureButton(btnVerifyKey, "Verify credentials");
            ConfigureLabel(lblKeyStatus, "Status: not verified");
            flowSetup.Controls.AddRange(new Control[] { lblSetupHeader, lblCloudflaredStatus, btnDownload, lblCertStatus, btnLogin, lblApiKey, lblApiHint, txtApiKey, lblApiEmail, txtApiEmail, btnVerifyKey, lblKeyStatus });

            // Tunnels page
            ConfigureFlow(flowTunnels);
            ConfigureHeader(lblTunnelsHeader, "Tunnels");
            ConfigureLabel(lblSelectedTunnel, "Selected tunnel: none");
            ConfigureButtonPanel(flowTunnelButtons, new[] { btnRefreshTunnels, btnSelectTunnel, btnDeleteTunnel });
            btnRefreshTunnels.Text = "Refresh list";
            btnSelectTunnel.Text = "Select tunnel";
            btnDeleteTunnel.Text = "Delete tunnel";
            ConfigureGrid(dgvTunnels);
            dgvTunnels.Size = new Size(640, 170);
            txtNewTunnelName.Width = 250;
            txtNewTunnelName.PlaceholderText = "New tunnel name";
            ConfigureButton(btnCreateTunnel, "Create tunnel");
            flowTunnels.Controls.AddRange(new Control[] { lblTunnelsHeader, lblSelectedTunnel, flowTunnelButtons, dgvTunnels, txtNewTunnelName, btnCreateTunnel });

            // Rules page
            ConfigureFlow(flowRules);
            ConfigureHeader(lblRulesHeader, "Rules");
            ConfigureHint(lblRulesSteps, "Map as many domains to local ports as you need, then: Create DNS records -> Write config -> Run page: start tunnel.");

            flowQuickAdd.FlowDirection = FlowDirection.LeftToRight;
            flowQuickAdd.AutoSize = true;
            flowQuickAdd.WrapContents = false;
            flowQuickAdd.Margin = new Padding(0, 3, 0, 3);
            flowQuickAdd.BackColor = Theme.Panel;
            txtQuickHost.Width = 240;
            txtQuickHost.PlaceholderText = "app.imtaqin.id";
            txtQuickHost.Margin = new Padding(0, 3, 6, 0);
            txtQuickPort.Width = 70;
            txtQuickPort.PlaceholderText = "3000";
            txtQuickPort.Margin = new Padding(0, 3, 6, 0);
            btnQuickAdd.Text = "Add mapping";
            Theme.StyleButton(btnQuickAdd);
            btnQuickAdd.Margin = new Padding(0, 1, 0, 0);
            flowQuickAdd.Controls.AddRange(new Control[] { txtQuickHost, txtQuickPort, btnQuickAdd });

            ConfigureButtonPanel(flowRuleButtons, new[] { btnAddRule, btnEditRule, btnDeleteRule });
            btnAddRule.Text = "Add rule";
            btnEditRule.Text = "Edit rule";
            btnDeleteRule.Text = "Delete rule";
            ConfigureGrid(dgvRules);
            dgvRules.Size = new Size(640, 150);
            ConfigureLabel(lblZoneName, "Zone / root domain (optional):");
            txtZoneName.Width = 250;
            txtZoneName.PlaceholderText = "imtaqin.id";
            ConfigureHint(lblZoneHint, "Zone = root domain in your Cloudflare account, e.g. imtaqin.id — NOT the subdomain (spse.imtaqin.id). Leave empty to auto-detect.");
            ConfigureButton(btnCreateDns, "Create DNS records");
            ConfigureButton(btnWriteConfig, "Write config");
            ConfigureButton(btnSaveCredentials, "Save credentials");
            flowRules.Controls.AddRange(new Control[] { lblRulesHeader, lblRulesSteps, flowQuickAdd, flowRuleButtons, dgvRules, lblZoneName, txtZoneName, lblZoneHint, btnCreateDns, btnWriteConfig, btnSaveCredentials });

            // Run page
            ConfigureFlow(flowRun);
            ConfigureHeader(lblRunHeader, "Run");
            ConfigureLabel(lblServiceStatus, "Service status: unknown");
            ConfigureButtonPanel(flowRunButtons, new[] { btnStartTunnel, btnStopTunnel, btnInstallService, btnStartService, btnStopService, btnRestartService });
            btnStartTunnel.Text = "Start tunnel";
            btnStopTunnel.Text = "Stop tunnel";
            btnInstallService.Text = "Install service";
            btnStartService.Text = "Start service";
            btnStopService.Text = "Stop service";
            btnRestartService.Text = "Restart service";
            ConfigureHint(lblRunHint, "Start tunnel = runs with this app (close to tray keeps it alive). Service = background + auto-start at boot. Restart service after changing config. Don't run both at once.");
            flowRun.Controls.AddRange(new Control[] { lblRunHeader, lblServiceStatus, lblRunHint, flowRunButtons });

            foreach (var txt in new[] { txtApiKey, txtApiEmail, txtNewTunnelName, txtZoneName, txtQuickHost, txtQuickPort })
                Theme.StyleTextBox(txt);

            // Button hierarchy: accent = main action per page, red = destructive.
            Theme.StylePrimaryButton(btnLogin);
            Theme.StylePrimaryButton(btnVerifyKey);
            Theme.StylePrimaryButton(btnCreateTunnel);
            Theme.StylePrimaryButton(btnQuickAdd);
            Theme.StylePrimaryButton(btnCreateDns);
            Theme.StylePrimaryButton(btnStartTunnel);
            Theme.StyleDangerButton(btnDeleteTunnel);
            Theme.StyleDangerButton(btnDeleteRule);
            Theme.StyleDangerButton(btnStopTunnel);
            Theme.StyleDangerButton(btnStopService);

            foreach (var flow in new[] { flowTunnels, flowRules, flowRun })
                flow.Visible = false;

            ((System.ComponentModel.ISupportInitialize)dgvTunnels).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvRules).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            ResumeLayout(false);
        }

        private void ConfigureFlow(FlowLayoutPanel flow)
        {
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.AutoScroll = true;
            flow.Padding = new Padding(14, 10, 8, 8);
            flow.BackColor = Theme.Panel;
        }

        private void ConfigureHeader(Label label, string text)
        {
            label.AutoSize = true;
            label.Text = text;
            label.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            label.ForeColor = Theme.Fore;
            label.Margin = new Padding(0, 0, 0, 8);
        }

        private void ConfigureNavButton(Button btn, string text)
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Theme.Selection;
            btn.BackColor = Theme.Panel;
            btn.ForeColor = Theme.ForeDim;
            btn.Font = new Font("Segoe UI", 10.5f);
            btn.Size = new Size(148, 40);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(16, 0, 0, 0);
            btn.Margin = new Padding(0);
            btn.UseVisualStyleBackColor = false;
            btn.Paint += (s, e) =>
            {
                if (btn.Tag is true)
                {
                    using var bar = new SolidBrush(Theme.Accent);
                    e.Graphics.FillRectangle(bar, 0, 8, 3, btn.Height - 16);
                }
            };
        }

        private void ConfigureWindowButton(Button btn, string text)
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Theme.Selection;
            btn.BackColor = Theme.Back;
            btn.ForeColor = Theme.ForeDim;
            btn.Width = 44;
            btn.Font = new Font("Segoe UI", 9f);
            btn.UseVisualStyleBackColor = false;
            btn.TabStop = false;
        }

        private void ConfigureButtonPanel(FlowLayoutPanel panel, Button[] buttons)
        {
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.AutoSize = true;
            panel.WrapContents = false;
            panel.Margin = new Padding(0, 3, 0, 3);
            panel.BackColor = Theme.Panel;
            foreach (var btn in buttons)
            {
                Theme.StyleButton(btn);
                btn.Margin = new Padding(0, 0, 6, 0);
                panel.Controls.Add(btn);
            }
        }

        private void ConfigureLabel(Label label, string text)
        {
            label.AutoSize = true;
            label.Text = text;
            label.ForeColor = Theme.Fore;
            label.Margin = new Padding(0, 6, 0, 2);
        }

        private void ConfigureHint(Label label, string text)
        {
            label.AutoSize = true;
            label.Text = text;
            label.ForeColor = Theme.Hint;
            label.MaximumSize = new Size(640, 0);
            label.Margin = new Padding(0, 0, 0, 5);
        }

        private void ConfigureButton(Button button, string text)
        {
            button.Text = text;
            Theme.StyleButton(button);
            button.Margin = new Padding(0, 3, 0, 6);
        }

        private void ConfigureGrid(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoGenerateColumns = true;
            grid.Margin = new Padding(0, 3, 0, 6);
            Theme.StyleGrid(grid);
        }

        #region Controls

        private Panel titleBar;
        private Label lblTitle;
        private Button btnWinMin;
        private Button btnWinClose;

        private Panel sidebar;
        private FlowLayoutPanel navFlow;
        private Button btnNavSetup;
        private Button btnNavTunnels;
        private Button btnNavRules;
        private Button btnNavRun;
        private Label lblVersion;

        private Panel pnlMain;
        private SplitContainer splitContainer;
        private TextBox txtLog;
        private Label lblLogHeader;

        private FlowLayoutPanel flowSetup;
        private Label lblSetupHeader;
        private Label lblCloudflaredStatus;
        private Button btnDownload;
        private Label lblCertStatus;
        private Button btnLogin;
        private Label lblApiKey;
        private Label lblApiHint;
        private TextBox txtApiKey;
        private Label lblApiEmail;
        private TextBox txtApiEmail;
        private Button btnVerifyKey;
        private Label lblKeyStatus;

        private FlowLayoutPanel flowTunnels;
        private Label lblTunnelsHeader;
        private Label lblSelectedTunnel;
        private FlowLayoutPanel flowTunnelButtons;
        private Button btnRefreshTunnels;
        private Button btnSelectTunnel;
        private Button btnDeleteTunnel;
        private DataGridView dgvTunnels;
        private TextBox txtNewTunnelName;
        private Button btnCreateTunnel;

        private FlowLayoutPanel flowRules;
        private Label lblRulesHeader;
        private Label lblRulesSteps;
        private FlowLayoutPanel flowQuickAdd;
        private TextBox txtQuickHost;
        private TextBox txtQuickPort;
        private Button btnQuickAdd;
        private FlowLayoutPanel flowRuleButtons;
        private Button btnAddRule;
        private Button btnEditRule;
        private Button btnDeleteRule;
        private DataGridView dgvRules;
        private Label lblZoneName;
        private TextBox txtZoneName;
        private Label lblZoneHint;
        private Button btnCreateDns;
        private Button btnWriteConfig;
        private Button btnSaveCredentials;

        private FlowLayoutPanel flowRun;
        private Label lblRunHeader;
        private Label lblServiceStatus;
        private Label lblRunHint;
        private FlowLayoutPanel flowRunButtons;
        private Button btnStartTunnel;
        private Button btnStopTunnel;
        private Button btnInstallService;
        private Button btnStartService;
        private Button btnStopService;
        private Button btnRestartService;

        #endregion
    }
}
