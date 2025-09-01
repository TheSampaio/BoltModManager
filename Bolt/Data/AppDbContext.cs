using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        // Contexts
        public static string SettingsFile { get; } = "settings.json";

        // Keys and values
        public static string ModsDirectoryKey { get; } = "ModsDirectory";
        public static string ModsDirectoryValue
        {
            get => Json.Read(ModsDirectoryKey, SettingsFile)!;
            set => Json.Write(ModsDirectoryKey, value, SettingsFile);
        }
    }
}
