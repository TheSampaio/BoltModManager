using Bolt.Models;

namespace Bolt.Interfaces
{
    internal interface IModImportService
    {
        /// <summary>
        /// Imports modifications from zip files, extracts them, and updates the profile.
        /// </summary>
        Task ImportModsAsync(string[] zipFiles, GameModel currentGame, ProfileModel currentProfile, IProgress<int> progress);
    }
}