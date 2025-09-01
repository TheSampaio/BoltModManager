using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        public static string ModsDirectoryKey { get; } = "ModsDirectory";
        public static string ModsDirectoryValue
        {
            get => Json.Read(ModsDirectoryKey)!;
            set => Json.Write(ModsDirectoryKey, value);
        }
    }
}
