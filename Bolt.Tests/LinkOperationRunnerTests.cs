using Bolt.Core.Models;
using Bolt.Infrastructure.Deployment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class LinkOperationRunnerTests
{
    [TestMethod]
    public void DirectoryLinkPreservesFileLengthAndRestoreKeepsSource()
    {
        using var directory = new TestDirectory();
        var source = directory.GetPath("Modification", "modloader", "Map Pack");
        var sourceFile = Path.Combine(source, "models", "building.dff");
        var destination = directory.GetPath("Game", "modloader", "Map Pack");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        File.WriteAllBytes(sourceFile, new byte[4097]);

        var operation = new LinkOperation
        {
            Action = LinkAction.LinkDirectory,
            SourcePath = source,
            DestinationPath = destination,
            CleanupRootPath = directory.GetPath("Game")
        };

        var deployment = LinkOperationRunner.Run([operation]);

        Assert.IsTrue(deployment.Succeeded, string.Join(Environment.NewLine, deployment.Errors));
        Assert.IsTrue(Directory.Exists(destination));
        Assert.AreEqual(4097, new FileInfo(Path.Combine(destination, "models", "building.dff")).Length);

        operation.Action = LinkAction.RestoreDirectory;
        var restore = LinkOperationRunner.Run([operation]);

        Assert.IsTrue(restore.Succeeded, string.Join(Environment.NewLine, restore.Errors));
        Assert.IsFalse(Directory.Exists(destination));
        Assert.IsTrue(File.Exists(sourceFile));
    }

    [TestMethod]
    public void MaterializePreservesOriginalAcrossRepeatedDeploymentsAndRestore()
    {
        using var directory = new TestDirectory();
        var source = directory.GetPath("Modification", "game.exe");
        var destination = directory.GetPath("Game", "game.exe");
        var backup = directory.GetPath("Managed", "Backups", "game.exe");
        var state = directory.GetPath("Managed", "Backups", ".bolt-state", "game.exe.materialized");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(source, "modified-v1");
        File.WriteAllText(destination, "original");

        var operation = new LinkOperation
        {
            Action = LinkAction.Materialize,
            SourcePath = source,
            DestinationPath = destination,
            BackupPath = backup,
            StatePath = state
        };

        var firstDeployment = LinkOperationRunner.Run([operation]);
        File.WriteAllText(source, "modified-v2");
        var secondDeployment = LinkOperationRunner.Run([operation]);

        Assert.IsTrue(firstDeployment.Succeeded);
        Assert.IsTrue(secondDeployment.Succeeded);
        Assert.AreEqual("modified-v2", File.ReadAllText(destination));
        Assert.AreEqual("original", File.ReadAllText(backup));
        Assert.IsTrue(File.Exists(state));

        operation.Action = LinkAction.RestoreMaterialized;
        var restore = LinkOperationRunner.Run([operation]);

        Assert.IsTrue(restore.Succeeded);
        Assert.AreEqual("original", File.ReadAllText(destination));
        Assert.IsFalse(File.Exists(backup));
        Assert.IsFalse(File.Exists(state));
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(state)));
    }

    [TestMethod]
    public void RestoreMaterializedRemovesFileWhenThereWasNoOriginal()
    {
        using var directory = new TestDirectory();
        var source = directory.GetPath("Modification", "game.exe");
        var destination = directory.GetPath("Game", "game.exe");
        var backup = directory.GetPath("Managed", "Backups", "game.exe");
        var state = directory.GetPath("Managed", "Backups", ".bolt-state", "game.exe.materialized");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllText(source, "added by modification");

        var operation = new LinkOperation
        {
            Action = LinkAction.Materialize,
            SourcePath = source,
            DestinationPath = destination,
            BackupPath = backup,
            StatePath = state
        };

        var deployment = LinkOperationRunner.Run([operation]);
        operation.Action = LinkAction.RestoreMaterialized;
        var restore = LinkOperationRunner.Run([operation]);

        Assert.IsTrue(deployment.Succeeded);
        Assert.IsTrue(restore.Succeeded);
        Assert.IsFalse(File.Exists(destination));
        Assert.IsFalse(File.Exists(backup));
        Assert.IsFalse(File.Exists(state));
    }

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
