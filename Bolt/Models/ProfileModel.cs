namespace Bolt.Models
{
    internal class ProfileModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public string PackagesPath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
    }
}
