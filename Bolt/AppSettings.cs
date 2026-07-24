namespace Bolt;

/// <summary>Values read from <c>appsettings.json</c> and shipped with the application.</summary>
internal sealed class AppSettings
{
    /// <summary>Section holding these values in the configuration file.</summary>
    public const string SectionName = "App";

    public string Version { get; set; } = "0.0.0";

    /// <summary>Folder proposed for new games the first time Bolt runs.</summary>
    public string DefaultGamesPath { get; set; } = string.Empty;
}
