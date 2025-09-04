using Bolt.Utilities;

namespace Bolt.Data
{
    internal class PackageData
    {
        public static string DirectoryKey { get; } = "PackagesDirectory";
        public static string DirectoryValue
        {
            get => Json.Read(DirectoryKey, AppData.PreferencesFile)!;
            set => Json.Write(DirectoryKey, value, AppData.PreferencesFile);
        }
    }
}
