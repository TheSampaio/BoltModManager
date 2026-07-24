namespace Bolt.Core.Models;

/// <summary>
/// A named set of modifications that can be activated independently from the other sets.
/// </summary>
internal sealed class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<Modification> Modifications { get; set; } = [];
}
