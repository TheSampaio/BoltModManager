using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        // Contexts
        public static string PreferencesFile { get; } = "preferences.json";

        // Keys and values
        public static string PackagesDirectoryKey { get; } = "PackagesDirectory";
        public static string PackagesDirectoryValue
        {
            get => Json.Read(PackagesDirectoryKey, PreferencesFile)!;
            set => Json.Write(PackagesDirectoryKey, value, PreferencesFile);
        }
    }
}
