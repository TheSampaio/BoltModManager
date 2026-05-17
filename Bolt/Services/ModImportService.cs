using Bolt.Interfaces;
using Bolt.Models;
using Bolt.Utilities;
using System.IO.Compression;

namespace Bolt.Services
{
    internal class ModImportService : IModImportService
    {
        public async Task ImportModsAsync(string[] zipFiles, GameModel currentGame, ProfileModel currentProfile, IProgress<int> progress)
        {
            int progressCount = 0;

            await Task.Run(() =>
            {
                foreach (var zipFile in zipFiles)
                {
                    if (Path.GetExtension(zipFile)?.ToLower() != ".zip")
                        continue;

                    string modName = Path.GetFileNameWithoutExtension(zipFile);
                    string modExtractedPath = Path.Combine(currentGame.ModificationsPath, modName);

                    var modification = new ModificationModel
                    {
                        Id = Guid.NewGuid(),
                        Name = modName,
                        Version = "N/A",
                        Category = "Imported",
                        InstalledAt = DateTime.Now,
                        IsEnabled = true
                    };

                    using var archive = Archive.OpenRead(zipFile);
                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        string destinationPath = Path.GetFullPath(Path.Combine(modExtractedPath, entry.FullName));

                        if (!destinationPath.StartsWith(modExtractedPath, StringComparison.OrdinalIgnoreCase))
                            continue;

                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                        entry.ExtractToFile(destinationPath, overwrite: true);

                        modification.Content.Add(destinationPath);

                        progressCount++;
                        progress.Report(progressCount);
                    }

                    currentProfile.Modifications.RemoveAll(m => m.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));
                    currentProfile.Modifications.Add(modification);
                }
            });
        }
    }
}