using Bolt.Enums;
using System.IO.Compression;

namespace Bolt.Utilities
{
    // This class will implement the SevenZipSharp library in the future
    internal class Archive
    {
        public static void ExtractToDirectory(string filePath, string targetPath, bool overwrite = true)
        {
            try
            {
                ZipFile.ExtractToDirectory(filePath, targetPath, overwriteFiles: overwrite);
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to extract archive:\n{ex.Message}",
                    "Extraction Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public static ZipArchive OpenRead(string filePath) => ZipFile.OpenRead(filePath);
    }
}
