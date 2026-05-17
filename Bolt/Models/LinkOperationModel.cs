namespace Bolt.Models
{
    public class LinkOperationModel
    {
        public string Action { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
    }
}