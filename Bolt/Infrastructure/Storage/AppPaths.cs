namespace Bolt.Infrastructure.Storage;

/// <summary>
/// Well-known locations used by the application.
/// </summary>
/// <remarks>
/// Settings used to be written to <c>Preferences.blts</c> relative to the working directory, so
/// they were silently lost whenever the application was started from a different folder (a
/// shortcut, or the elevated helper). Everything now lives under the per-user data folder.
/// </remarks>
internal static class AppPaths
{
    private const string CompanyFolder = "Bolt";

    /// <summary>Folder holding user specific application data.</summary>
    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create),
        CompanyFolder);

    /// <summary>File holding the user preferences.</summary>
    public static string PreferencesFile { get; } = Path.Combine(DataDirectory, "preferences.json");

    /// <summary>Folder next to the executable, used to resolve <c>appsettings.json</c>.</summary>
    public static string InstallDirectory { get; } = AppContext.BaseDirectory;

    /// <summary>Default folder proposed for new games.</summary>
    public static string DefaultGamesRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Bolt Mod Manager");

    public static void EnsureDataDirectory() => Directory.CreateDirectory(DataDirectory);
}
