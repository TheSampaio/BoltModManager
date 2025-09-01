namespace Bolt.Models
{
    internal class GameModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public List<ProfileModel> Profiles { get; set; } = [];
        public List<PackageModel> Packages { get; set; } = [];
    }
}
