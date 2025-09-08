namespace Bolt.Models
{
    internal class ModificationModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime InstalledAt { get; set; }
        public List<string> Content { get; set; } = [];
    }
}
