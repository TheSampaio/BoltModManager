using System.Text.Json;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;

namespace Bolt.Infrastructure.Storage;

/// <summary>
/// Loads and saves <c>.bltg</c> game files, migrating documents written by older versions.
/// </summary>
internal sealed class GameRepository : IGameRepository
{
    public string FileExtension => ".bltg";

    public string FileName => "Game.bltg";

    public GameSession Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new IOException("No game file was provided.");

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"The game file \"{fullPath}\" no longer exists.", fullPath);

        if (!string.Equals(Path.GetExtension(fullPath), FileExtension, StringComparison.OrdinalIgnoreCase))
            throw new IOException($"\"{Path.GetFileName(fullPath)}\" is not a Bolt game file.");

        GameDocument? document;

        try
        {
            document = JsonFileStore.Read<GameDocument>(fullPath);
        }
        catch (JsonException ex)
        {
            throw new IOException($"The game file \"{Path.GetFileName(fullPath)}\" is corrupted.", ex);
        }

        if (document is null || string.IsNullOrWhiteSpace(document.Name))
            throw new IOException($"The game file \"{Path.GetFileName(fullPath)}\" is empty or invalid.");

        var session = new GameSession(document.ToGame(), fullPath);
        session.EnsureProfileExists();

        Migrate(document, session);

        return session;
    }

    public void Save(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        JsonFileStore.Write(session.Game, session.FilePath);
    }

    public GameSession Create(Game game, string rootPath)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (Directory.Exists(rootPath) && Directory.EnumerateFileSystemEntries(rootPath).Any())
            throw new IOException($"The folder \"{rootPath}\" already exists and is not empty.");

        var session = new GameSession(game, Path.Combine(rootPath, FileName));
        session.EnsureProfileExists();
        session.Game.ActiveProfileId = session.Game.Profiles[0].Id;

        Directory.CreateDirectory(session.RootPath);
        Directory.CreateDirectory(session.ModificationsPath);
        Directory.CreateDirectory(session.BackupsPath);

        Save(session);

        return session;
    }

    /// <summary>
    /// Upgrades documents produced by earlier versions: modification folders were not recorded and
    /// file lists were stored as absolute paths, which broke as soon as the games root moved.
    /// </summary>
    private static void Migrate(GameDocument document, GameSession session)
    {
        var changed = false;

        if (session.Game.ActiveProfileId == Guid.Empty
            || session.Game.Profiles.TrueForAll(p => p.Id != session.Game.ActiveProfileId))
        {
            session.Game.ActiveProfileId = session.Game.Profiles[0].Id;
            changed = true;
        }

        foreach (var modification in session.Game.Profiles.SelectMany(p => p.Modifications))
        {
            if (string.IsNullOrWhiteSpace(modification.FolderName))
            {
                modification.FolderName = PathUtility.ToSafeFolderName(modification.Name);
                changed = true;
            }

            var legacyBase = string.IsNullOrWhiteSpace(document.ModificationsPath)
                ? session.GetModificationPath(modification)
                : Path.Combine(document.ModificationsPath, modification.Name);

            for (var i = 0; i < modification.Content.Count; i++)
            {
                var entry = modification.Content[i];

                if (!Path.IsPathRooted(entry))
                    continue;

                modification.Content[i] = Path.GetRelativePath(legacyBase, entry);
                changed = true;
            }
        }

        if (changed)
            JsonFileStore.Write(session.Game, session.FilePath);
    }

    /// <summary>
    /// On-disk representation of a game file, including the fields kept only for migration.
    /// </summary>
    private sealed class GameDocument
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ExecutablePath { get; set; } = string.Empty;

        public string TargetPath { get; set; } = string.Empty;

        public Guid ActiveProfileId { get; set; }

        public List<Profile> Profiles { get; set; } = [];

        /// <summary>Absolute modifications folder written by versions before 2026.7.</summary>
        public string? ModificationsPath { get; set; }

        public Game ToGame() => new()
        {
            Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
            Name = Name,
            ExecutablePath = ExecutablePath,
            TargetPath = TargetPath,
            ActiveProfileId = ActiveProfileId,
            Profiles = Profiles
        };
    }
}
