namespace Bolt.Models
{
    public class ModificationModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime InstalledAt { get; set; }
        public bool IsEnabled { get; set; } = true;
        public List<string> Content { get; set; } = [];
    }
}