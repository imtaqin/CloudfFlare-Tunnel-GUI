namespace CFTunnel.Forms
{
    partial class EditIngressDialog
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
            lblHostname = new Label();
            txtHostname = new TextBox();
            lblService = new Label();
            txtService = new TextBox();
            lblOrigin = new Label();
            txtOrigin = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();

            lblHostname.AutoSize = true;
            lblHostname.Location = new Point(10, 10);
            lblHostname.Text = "Public hostname (the URL people will visit):";

            txtHostname.Location = new Point(10, 30);
            txtHostname.Size = new Size(400, 23);
            txtHostname.PlaceholderText = "spse.imtaqin.id";

            lblService.AutoSize = true;
            lblService.Location = new Point(10, 65);
            lblService.Text = "Local service (where your app runs on this PC):";

            txtService.Location = new Point(10, 85);
            txtService.Size = new Size(400, 23);
            txtService.PlaceholderText = "http://localhost:3000";

            lblOrigin.AutoSize = true;
            lblOrigin.Location = new Point(10, 120);
            lblOrigin.Text = "Origin request options (key=value per line):";

            txtOrigin.Location = new Point(10, 140);
            txtOrigin.Size = new Size(400, 100);
            txtOrigin.Multiline = true;
            txtOrigin.ScrollBars = ScrollBars.Vertical;

            btnOK.Location = new Point(240, 260);
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;
            Theme.StyleButton(btnOK);
            Theme.StylePrimaryButton(btnOK);
            btnOK.AutoSize = false;
            btnOK.Size = new Size(80, 27);

            btnCancel.Location = new Point(330, 260);
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            Theme.StyleButton(btnCancel);
            btnCancel.AutoSize = false;
            btnCancel.Size = new Size(80, 27);

            BackColor = Theme.Panel;
            ForeColor = Theme.Fore;
            Font = Theme.UI;
            lblHostname.ForeColor = Theme.Fore;
            lblService.ForeColor = Theme.Fore;
            lblOrigin.ForeColor = Theme.Fore;
            Theme.StyleTextBox(txtHostname);
            Theme.StyleTextBox(txtService);
            Theme.StyleTextBox(txtOrigin);

            AcceptButton = btnOK;
            CancelButton = btnCancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 310);
            Controls.Add(lblHostname);
            Controls.Add(txtHostname);
            Controls.Add(lblService);
            Controls.Add(txtService);
            Controls.Add(lblOrigin);
            Controls.Add(txtOrigin);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditIngressDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ingress Rule";
        }

        private Label lblHostname;
        private TextBox txtHostname;
        private Label lblService;
        private TextBox txtService;
        private Label lblOrigin;
        private TextBox txtOrigin;
        private Button btnOK;
        private Button btnCancel;
    }
}
