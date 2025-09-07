namespace Bolt.Utilities
{
    internal static class Directory
    {
        public static void MoveAcrossVolumes(string sourceDir, string destinationDir)
        {
            // If it’s the same drive, move directly
            if (Path.GetPathRoot(sourceDir) == Path.GetPathRoot(destinationDir))
            {
                System.IO.Directory.Move(sourceDir, destinationDir);
                return;
            }

            // Otherwise, perform recursive copy + delete
            CopyRecursive(sourceDir, destinationDir);
            System.IO.Directory.Delete(sourceDir, true);
        }

        public static void CopyRecursive(string sourceDir, string destinationDir)
        {
            System.IO.Directory.CreateDirectory(destinationDir);

            foreach (var file in System.IO.Directory.GetFiles(sourceDir))
            {
                var targetFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, targetFile, overwrite: true);
            }

            foreach (var directory in System.IO.Directory.GetDirectories(sourceDir))
            {
                var targetDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyRecursive(directory, targetDir);
            }
        }
    }
}
