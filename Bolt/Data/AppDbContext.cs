using Bolt.Utilities;

namespace Bolt.Data
{
    internal class AppDbContext
    {
        public static string ModsDirectoryKey { get; } = "ModsDirectory";
        public static string ModsDirectoryValue => Json.Read(ModsDirectoryKey)!;
    }
}
