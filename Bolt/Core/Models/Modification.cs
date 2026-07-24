namespace Bolt.Core.Models;

/// <summary>
/// A single modification (package) imported into a profile.
/// </summary>
internal sealed class Modification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime InstalledAt { get; set; } = DateTime.Now;

    public bool IsEnabled { get; set; }

    /// <summary>
    /// Name of the folder holding the extracted files, relative to the modifications root.
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Files owned by this modification, stored relative to its own folder. Relative paths keep
    /// the game file portable: the same entries are valid no matter where the games root lives.
    /// </summary>
    public List<string> Content { get; set; } = [];
}
