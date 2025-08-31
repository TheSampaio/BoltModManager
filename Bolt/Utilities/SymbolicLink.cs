using Bolt.Enums;
using System.Runtime.InteropServices;

namespace Bolt.Utilities
{
    internal class SymbolicLink
    {
        public static bool Create(string symlinkFileName, string targetFileName, SymbolicLinkType type) =>
            CreateSymbolicLink(symlinkFileName, targetFileName, (int)type);

#pragma warning disable IDE0079     // Remove unnecessary suppression
#pragma warning disable SYSLIB1054  // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079
    }
}
