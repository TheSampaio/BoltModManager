using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        // Contexts
        public static string PreferencesFile { get; } = "preferences.json";

        // Keys and values
        public static string ModsDirectoryKey { get; } = "ModsDirectory";
        public static string ModsDirectoryValue
        {
            get => Json.Read(ModsDirectoryKey, PreferencesFile)!;
            set => Json.Write(ModsDirectoryKey, value, PreferencesFile);
        }
    }
}
