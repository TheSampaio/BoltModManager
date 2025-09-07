namespace Bolt.Models
{
    internal class GameModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string BackupsPath { get; set; } = string.Empty;
        public string ModificationsPath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public List<ProfileModel> Profiles { get; set; } = [];
    }
}
