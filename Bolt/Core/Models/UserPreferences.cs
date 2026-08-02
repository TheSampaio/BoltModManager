namespace Bolt.Core.Models;

/// <summary>Colour scheme requested by the user.</summary>
internal enum ThemeMode
{
    Dark,
    Light,
    System
}

/// <summary>
/// User settings persisted outside of any game, in the per-user application data folder.
/// </summary>
internal sealed class UserPreferences
{
    /// <summary>Folder where new games are created.</summary>
    public string GamesRoot { get; set; } = string.Empty;

    /// <summary>Dark is the scheme the interface is designed around; the others are opt-in.</summary>
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;

    /// <summary>Executable used to open text files. Empty uses Windows Notepad.</summary>
    public string TextEditorPath { get; set; } = string.Empty;

    /// <summary>Most recently opened game files, newest first.</summary>
    public List<string> RecentGames { get; set; } = [];
}
