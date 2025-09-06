using System.IO.Compression;

namespace Bolt.Utilities
{
    internal class Archive
    {
        public static void Extract(string filePath, string targetPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(filePath, targetPath);
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
    }
}
