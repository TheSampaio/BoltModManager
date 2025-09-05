using Bolt.Enums;
using System.Runtime.InteropServices;

namespace Bolt.Utilities
{
    /// <summary>
    /// Provides a simple wrapper to create symbolic links on Windows.
    /// </summary>
    internal class SymbolicLink
    {
        /// <summary>
        /// Creates a symbolic link at <paramref name="symlinkPath"/> pointing to <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="symlinkPath">The path where the symbolic link will be created.</param>
        /// <param name="targetPath">The target path that the symbolic link points to.</param>
        /// <param name="type">The type of symbolic link (file or directory).</param>
        /// <returns>True if the symbolic link was created successfully; otherwise, false.</returns>
        public static bool Create(string symlinkPath, string targetPath, SymbolicLinkType type) =>
            CreateSymbolicLink(symlinkPath, targetPath, (int)type);

        // Use DllImport instead of LibraryImport to avoid all the partial method issues
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
#pragma warning disable IDE0079     // Remove unnecessary suppression
#pragma warning disable SYSLIB1054  // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079
    }
}
