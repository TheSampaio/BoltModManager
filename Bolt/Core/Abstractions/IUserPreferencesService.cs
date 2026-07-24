using Bolt.Core.Models;

namespace Bolt.Core.Abstractions;

/// <summary>Persists and exposes the user settings.</summary>
internal interface IUserPreferencesService
{
    UserPreferences Current { get; }

    /// <summary>Writes the current settings to disk.</summary>
    void Save();

    /// <summary>Moves <paramref name="gameFilePath"/> to the top of the recent games list.</summary>
    void AddRecentGame(string gameFilePath);

    void RemoveRecentGame(string gameFilePath);

    void ClearRecentGames();
}
