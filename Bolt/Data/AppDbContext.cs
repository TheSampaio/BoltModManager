using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        // Contexts
        public static string GameFile { get; } = "game.bltg";
        public static string SettingsFile { get; } = "settings.blts";

        // Keys and values
        public static string PackagesDirectoryKey { get; } = "PackagesDirectory";
        public static string PackagesDirectoryValue
        {
            get => Json.Read(PackagesDirectoryKey, SettingsFile)!;
            set => Json.Write(PackagesDirectoryKey, value, SettingsFile);
        }
    }
}
