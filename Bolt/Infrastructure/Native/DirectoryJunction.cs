using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Bolt.Infrastructure.Native;

/// <summary>Creates an NTFS directory junction without requiring an elevated process.</summary>
internal static class DirectoryJunction
{
    private const int ReparseHeaderSize = 8;
    private const int MountPointHeaderSize = 8;

    public static void Create(string junctionPath, string targetPath)
    {
        var fullJunctionPath = Path.GetFullPath(junctionPath);
        var fullTargetPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetPath));

        if (!Directory.Exists(fullTargetPath))
            throw new DirectoryNotFoundException($"The modification directory \"{fullTargetPath}\" is missing.");

        Directory.CreateDirectory(fullJunctionPath);

        try
        {
            using var handle = NativeMethods.CreateFile(
                fullJunctionPath,
                NativeMethods.GenericWrite,
                0,
                IntPtr.Zero,
                NativeMethods.OpenExisting,
                NativeMethods.FileFlagOpenReparsePoint | NativeMethods.FileFlagBackupSemantics,
                IntPtr.Zero);

            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var buffer = BuildReparseBuffer(fullTargetPath);
            var nativeBuffer = Marshal.AllocHGlobal(buffer.Length);

            try
            {
                Marshal.Copy(buffer, 0, nativeBuffer, buffer.Length);

                if (!NativeMethods.DeviceIoControl(
                    handle,
                    NativeMethods.FsctlSetReparsePoint,
                    nativeBuffer,
                    (uint)buffer.Length,
                    IntPtr.Zero,
                    0,
                    out _,
                    IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                Marshal.FreeHGlobal(nativeBuffer);
            }
        }
        catch
        {
            if (Directory.Exists(fullJunctionPath) && !SymbolicLink.IsLink(fullJunctionPath))
                Directory.Delete(fullJunctionPath);

            throw;
        }
    }

    private static byte[] BuildReparseBuffer(string targetPath)
    {
        var substituteName = Encoding.Unicode.GetBytes($"\\??\\{targetPath}");
        var printName = Encoding.Unicode.GetBytes(targetPath);
        var pathBufferLength = substituteName.Length + sizeof(char) + printName.Length + sizeof(char);
        var reparseDataLength = MountPointHeaderSize + pathBufferLength;
        var buffer = new byte[ReparseHeaderSize + reparseDataLength];
        var span = buffer.AsSpan();

        BinaryPrimitives.WriteUInt32LittleEndian(span, NativeMethods.IoReparseTagMountPoint);
        BinaryPrimitives.WriteUInt16LittleEndian(span[4..], checked((ushort)reparseDataLength));
        BinaryPrimitives.WriteUInt16LittleEndian(span[8..], 0);
        BinaryPrimitives.WriteUInt16LittleEndian(span[10..], checked((ushort)substituteName.Length));
        BinaryPrimitives.WriteUInt16LittleEndian(span[12..], checked((ushort)(substituteName.Length + sizeof(char))));
        BinaryPrimitives.WriteUInt16LittleEndian(span[14..], checked((ushort)printName.Length));

        substituteName.CopyTo(span[16..]);
        printName.CopyTo(span[(16 + substituteName.Length + sizeof(char))..]);

        return buffer;
    }
}
