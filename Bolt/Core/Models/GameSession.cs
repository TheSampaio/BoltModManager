namespace Bolt.Core.Models;

/// <summary>
/// A loaded <see cref="Game"/> together with the on-disk location it was loaded from.
/// </summary>
/// <remarks>
/// Keeping the origin path alongside the model removes the need to reconstruct it from the
/// games root plus the game name, which silently broke whenever a game file lived elsewhere.
/// </remarks>
internal sealed class GameSession(Game game, string filePath)
{
    public const string ModificationsFolderName = "Modifications";
    public const string BackupsFolderName = "Backups";

    public Game Game { get; } = game;

    /// <summary>Full path of the <c>.bltg</c> file this session was loaded from.</summary>
    public string FilePath { get; } = Path.GetFullPath(filePath);

    /// <summary>Folder containing the game file and all of its managed data.</summary>
    public string RootPath => Path.GetDirectoryName(FilePath)!;

    /// <summary>Folder holding the extracted modification files.</summary>
    public string ModificationsPath => Path.Combine(RootPath, ModificationsFolderName);

    /// <summary>Folder holding the original game files replaced by a modification.</summary>
    public string BackupsPath => Path.Combine(RootPath, BackupsFolderName);

    /// <summary>The profile currently selected, falling back to the first available one.</summary>
    public Profile ActiveProfile
    {
        get
        {
            EnsureProfileExists();

            return Game.Profiles.FirstOrDefault(p => p.Id == Game.ActiveProfileId)
                ?? Game.Profiles[0];
        }
    }

    public void SelectProfile(Profile profile) => Game.ActiveProfileId = profile.Id;

    /// <summary>Absolute folder holding the files of <paramref name="modification"/>.</summary>
    public string GetModificationPath(Modification modification) =>
        Path.Combine(ModificationsPath, modification.FolderName);

    /// <summary>
    /// Guarantees the game always exposes at least one profile, so profile-dependent code never
    /// has to deal with an empty collection.
    /// </summary>
    public void EnsureProfileExists()
    {
        if (Game.Profiles.Count == 0)
            Game.Profiles.Add(new Profile { Name = "Main" });
    }
}
