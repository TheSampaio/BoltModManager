using Bolt.Core.Abstractions;
using Bolt.Core.Models;

namespace Bolt.Infrastructure.Storage;

/// <summary>
/// Stores the user preferences as a single typed JSON document in the application data folder.
/// </summary>
internal sealed class UserPreferencesService : IUserPreferencesService
{
    private const int MaxRecentGames = 10;

    private readonly string _filePath;

    public UserPreferencesService(string? defaultGamesRoot = null)
    {
        _filePath = AppPaths.PreferencesFile;
        Current = JsonFileStore.ReadOrDefault(_filePath, () => new UserPreferences());

        if (string.IsNullOrWhiteSpace(Current.GamesRoot))
            Current.GamesRoot = defaultGamesRoot is { Length: > 0 } root ? root : AppPaths.DefaultGamesRoot;
    }

    public UserPreferences Current { get; }

    public void Save()
    {
        AppPaths.EnsureDataDirectory();
        JsonFileStore.Write(Current, _filePath);
    }

    public void AddRecentGame(string gameFilePath)
    {
        if (string.IsNullOrWhiteSpace(gameFilePath))
            return;

        var fullPath = Path.GetFullPath(gameFilePath);

        Current.RecentGames.RemoveAll(p => p.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
        Current.RecentGames.Insert(0, fullPath);

        if (Current.RecentGames.Count > MaxRecentGames)
            Current.RecentGames.RemoveRange(MaxRecentGames, Current.RecentGames.Count - MaxRecentGames);

        Save();
    }

    public void RemoveRecentGame(string gameFilePath)
    {
        if (Current.RecentGames.RemoveAll(p => p.Equals(gameFilePath, StringComparison.OrdinalIgnoreCase)) > 0)
            Save();
    }

    public void ClearRecentGames()
    {
        Current.RecentGames.Clear();
        Save();
    }
}
