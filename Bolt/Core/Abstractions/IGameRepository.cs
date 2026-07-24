using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>Reads and writes game files from disk.</summary>
internal interface IGameRepository
{
    /// <summary>File extension (including the dot) used by Bolt game files.</summary>
    string FileExtension { get; }

    /// <summary>Canonical file name of a game file inside its own folder.</summary>
    string FileName { get; }

    /// <summary>
    /// Loads the game stored at <paramref name="path"/>, migrating legacy data when needed.
    /// </summary>
    /// <exception cref="IOException">The file cannot be read or is not a valid game file.</exception>
    GameSession Load(string path);

    /// <summary>Writes the session back to the file it was loaded from.</summary>
    void Save(GameSession session);

    /// <summary>Creates the folder structure of a new game and writes its game file.</summary>
    GameSession Create(Game game, string rootPath);
}
