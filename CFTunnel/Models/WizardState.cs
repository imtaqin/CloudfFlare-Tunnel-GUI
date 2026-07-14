namespace CFTunnel.Models
{
    public class WizardState
    {
        public bool BinaryInstalled { get; set; }
        public bool LoggedIn { get; set; }
        public string ApiToken { get; set; } = string.Empty;
        public string ApiEmail { get; set; } = string.Empty;
        public bool TokenVerified { get; set; }
        public Tunnel? SelectedTunnel { get; set; }
    }
}
