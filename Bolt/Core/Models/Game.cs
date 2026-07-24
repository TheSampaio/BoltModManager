namespace Bolt.Core.Models;

/// <summary>
/// Persisted description of a game managed by Bolt.
/// </summary>
/// <remarks>
/// Only location-independent data is stored. The modifications and backup folders are derived
/// from the location of the game file itself (see <see cref="GameSession"/>), so moving or
/// renaming the games root does not invalidate previously created games.
/// </remarks>
internal sealed class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Full path of the executable used to launch the game.</summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>Root folder of the game installation, where mod files are linked into.</summary>
    public string TargetPath { get; set; } = string.Empty;

    public Guid ActiveProfileId { get; set; }

    public List<Profile> Profiles { get; set; } = [];
}
