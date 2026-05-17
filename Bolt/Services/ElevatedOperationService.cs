using Bolt.Interfaces;
using Bolt.Models;
using Bolt.Utilities;

namespace Bolt.Services
{
    internal class ElevatedOperationService : IElevatedOperationService
    {
        public void ExecuteManifest(string manifestPath)
        {
            try
            {
                var operations = Json.Deserialize<List<LinkOperationModel>>(manifestPath);
                if (operations == null || operations.Count == 0) return;

                foreach (var op in operations)
                {
                    ProcessOperation(op);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in elevated process:\n{ex.Message}", "Elevation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(manifestPath))
                    File.Delete(manifestPath);
            }
        }

        private static void ProcessOperation(LinkOperationModel op)
        {
            if (op.Action == "Link")
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(op.DestinationPath)!);

                if (File.Exists(op.DestinationPath) && !IsSymbolicLink(op.DestinationPath))
                {
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(op.BackupPath)!);
                    File.Move(op.DestinationPath, op.BackupPath, true);
                }

                if (File.Exists(op.SourcePath))
                    SymbolicLink.Create(op.DestinationPath, op.SourcePath, Enums.SymbolicLinkType.File);
            }
            else if (op.Action == "Restore")
            {
                if (IsSymbolicLink(op.DestinationPath))
                    File.Delete(op.DestinationPath);

                if (File.Exists(op.BackupPath))
                    File.Move(op.BackupPath, op.DestinationPath, true);
            }
        }

        private static bool IsSymbolicLink(string path)
        {
            if (!File.Exists(path) && !System.IO.Directory.Exists(path)) return false;
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }
    }
}