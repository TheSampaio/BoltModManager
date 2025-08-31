using System.Runtime.InteropServices;

namespace Bolt.Utilities
{
    internal class SymbolicLink
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool Create(string symlinkFileName, string targetFileName, int flags);
    }
}
