namespace Bolt.Models
{
    internal class ProfileModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ModificationModel> Modifications { get; set; } = [];
    }
}
