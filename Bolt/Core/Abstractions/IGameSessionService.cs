using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>
/// Owns the game currently open in the application and notifies listeners about its lifecycle.
/// </summary>
internal interface IGameSessionService
{
    /// <summary>Raised after a game becomes the current one.</summary>
    event Action<GameSession>? GameLoaded;

    /// <summary>Raised after the current game is closed.</summary>
    event Action? GameUnloaded;

    /// <summary>Raised when the in-memory game changed and the UI should refresh.</summary>
    event Action<GameSession>? GameChanged;

    GameSession? Current { get; }

    /// <summary>Loads the game stored at <paramref name="path"/>, replacing the current one.</summary>
    OperationResult Load(string path);

    /// <summary>Closes the current game. Does nothing when no game is open.</summary>
    void Unload();

    /// <summary>Persists the current game and raises <see cref="GameChanged"/>.</summary>
    OperationResult Save();
}
