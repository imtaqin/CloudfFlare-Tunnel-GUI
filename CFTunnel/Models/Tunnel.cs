namespace CFTunnel.Models
{
    public class Tunnel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ConnectorId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Selected { get; set; } = string.Empty;

        public bool IsRunning => Status.Equals("healthy", StringComparison.OrdinalIgnoreCase);
    }
}
