namespace CFTunnel.Models
{
    public class IngressRule
    {
        public string Hostname { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public Dictionary<string, string> OriginRequest { get; set; } = new();
    }
}
