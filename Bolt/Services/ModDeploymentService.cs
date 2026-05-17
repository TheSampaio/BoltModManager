using Bolt.Interfaces;
using Bolt.Models;
using Bolt.Utilities;

namespace Bolt.Services
{
    internal class ModDeploymentService : IModDeploymentService
    {
        public bool DeployModification(GameModel game, ModificationModel modification)
        {
            return DeployModifications(game, [modification]);
        }

        public bool DeployModifications(GameModel game, IEnumerable<ModificationModel> modifications)
        {
            var operations = new List<LinkOperationModel>();

            foreach (var modification in modifications)
            {
                string modBasePath = Path.Combine(game.ModificationsPath, modification.Name);

                foreach (var sourcePath in modification.Content)
                {
                    if (System.IO.Directory.Exists(sourcePath))
                        continue;

                    string relativePath = Path.GetRelativePath(modBasePath, sourcePath);

                    operations.Add(new LinkOperationModel
                    {
                        Action = "Link",
                        SourcePath = sourcePath,
                        DestinationPath = Path.Combine(game.TargetPath, relativePath),
                        BackupPath = Path.Combine(game.BackupsPath, relativePath)
                    });
                }
            }

            return ExecuteElevatedHelper(operations);
        }

        public bool RevertModification(GameModel game, ModificationModel modification)
        {
            var operations = new List<LinkOperationModel>();
            string modBasePath = Path.Combine(game.ModificationsPath, modification.Name);

            foreach (var sourcePath in modification.Content)
            {
                if (System.IO.Directory.Exists(sourcePath))
                    continue;

                string relativePath = Path.GetRelativePath(modBasePath, sourcePath);

                operations.Add(new LinkOperationModel
                {
                    Action = "Restore",
                    DestinationPath = Path.Combine(game.TargetPath, relativePath),
                    BackupPath = Path.Combine(game.BackupsPath, relativePath)
                });
            }

            return ExecuteElevatedHelper(operations);
        }

        private static bool ExecuteElevatedHelper(List<LinkOperationModel> operations)
        {
            if (operations.Count == 0)
                return true;

            string manifestPath = Path.Combine(Path.GetTempPath(), $"BoltManifest_{Guid.NewGuid():N}.json");
            Json.Serialize(operations, manifestPath);

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Environment.ProcessPath,
                    Arguments = $"--elevated-helper \"{manifestPath}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                using var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit();

                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("Privilege elevation was canceled by the user. The modifications were not applied.", "Operation Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (File.Exists(manifestPath)) File.Delete(manifestPath);

                return false;
            }
        }
    }
}