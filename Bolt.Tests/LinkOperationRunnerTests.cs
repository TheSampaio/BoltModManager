using Bolt.Core.Models;
using Bolt.Infrastructure.Deployment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class LinkOperationRunnerTests
{
    [TestMethod]
    public void RestoreRemovesEmptyDestinationFoldersUpToGameRoot()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var destination = Path.Combine(gameRoot, "Mods", "Textures", "removed.dds");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        Directory.CreateDirectory(Path.Combine(gameRoot, "Mods", "Unused", "Empty"));

        var result = LinkOperationRunner.Run([
            new LinkOperation
            {
                Action = LinkAction.Restore,
                BackupPath = directory.GetPath("Backups", "missing.dds"),
                CleanupRootPath = gameRoot,
                DestinationPath = destination
            }
        ]);

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(Directory.Exists(gameRoot));
        Assert.IsFalse(Directory.Exists(Path.Combine(gameRoot, "Mods")));
    }

    [TestMethod]
    public void RestoreKeepsDestinationFoldersWhenAnyFileRemains()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var destinationFolder = Path.Combine(gameRoot, "Mods", "Textures");
        var destination = Path.Combine(destinationFolder, "removed.dds");
        var retainedFile = Path.Combine(destinationFolder, "Shared", "retained.dds");
        Directory.CreateDirectory(Path.GetDirectoryName(retainedFile)!);
        File.WriteAllText(retainedFile, "used by the game or another modification");

        var result = LinkOperationRunner.Run([
            new LinkOperation
            {
                Action = LinkAction.Restore,
                BackupPath = directory.GetPath("Backups", "missing.dds"),
                CleanupRootPath = gameRoot,
                DestinationPath = destination
            }
        ]);

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(File.Exists(retainedFile));
        Assert.IsTrue(Directory.Exists(destinationFolder));
    }
}
