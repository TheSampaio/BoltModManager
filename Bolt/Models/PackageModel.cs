namespace Bolt.Models
{
    internal class PackageModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime InstalledAt { get; set; }
    }
}
