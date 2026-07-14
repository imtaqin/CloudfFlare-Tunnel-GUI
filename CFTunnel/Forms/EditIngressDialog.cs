namespace CFTunnel.Forms
{
    public partial class EditIngressDialog : Form
    {
        public Models.IngressRule? Rule { get; private set; }

        public EditIngressDialog(Models.IngressRule? rule = null)
        {
            InitializeComponent();
            if (rule != null)
            {
                Rule = rule;
                txtHostname.Text = rule.Hostname;
                txtService.Text = rule.Service;
                txtOrigin.Text = string.Join(Environment.NewLine, rule.OriginRequest.Select(kv => $"{kv.Key}={kv.Value}"));
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var hostname = txtHostname.Text.Trim();
            var service = txtService.Text.Trim();
            if (string.IsNullOrEmpty(hostname) || string.IsNullOrEmpty(service))
            {
                MessageBox.Show("Hostname and service are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var origin = new Dictionary<string, string>();
            foreach (var line in txtOrigin.Lines)
            {
                var parts = line.Split(['='], 2);
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                    origin[parts[0].Trim()] = parts[1].Trim();
            }

            Rule = new Models.IngressRule
            {
                Hostname = hostname,
                Service = service,
                OriginRequest = origin
            };
            DialogResult = DialogResult.OK;
        }
    }
}
