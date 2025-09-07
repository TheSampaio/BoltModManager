namespace Bolt.Data
{
    /// <summary>
    /// Contains application-wide file names for saving game data and preferences.
    /// </summary>
    internal class AppData
    {
        /// <summary>
        /// The default file name used to save and load game data.
        /// </summary>
        public static string GameFile { get; } = "Game.bltg";

        /// <summary>
        /// The default file name used to save and load user preferences.
        /// </summary>
        public static string PreferencesFile { get; } = "Preferences.blts";
    }
}
