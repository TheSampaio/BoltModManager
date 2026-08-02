using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Deployment;
using Bolt.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class ModDeploymentServiceTests
{
    [TestMethod]
    public void FindConflictPairsGroupsSharedFilesByStableModificationPair()
    {
        using var directory = new TestDirectory();
        var first = new Modification
        {
            Name = "First",
            FolderName = "First",
            InstalledAt = new DateTime(2026, 1, 1),
            IsEnabled = true,
            Content = ["unique-first.txt", "shared.txt", "folder/shared.bin"]
        };
        var second = new Modification
        {
            Name = "Second",
            FolderName = "Second",
            InstalledAt = new DateTime(2026, 1, 2),
            IsEnabled = true,
            Content = ["unique-second.txt", "SHARED.txt", "folder/shared.bin"]
        };
        var disabled = CreateModification("Disabled", "shared.txt");
        disabled.IsEnabled = false;
        var profile = new Profile { Name = "Main", Modifications = [second, disabled, first] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Game"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var service = new ModDeploymentService(new CapturingExecutor(OperationResult.Success()));

        var conflicts = service.FindConflictPairs(session);

        Assert.HasCount(1, conflicts);
        Assert.AreEqual(first.Id, conflicts[0].LeftModificationId);
        Assert.AreEqual(second.Id, conflicts[0].RightModificationId);
        CollectionAssert.AreEquivalent(
            new[] { "shared.txt", Path.Combine("folder", "shared.bin") },
            conflicts[0].Files.ToArray());
    }

    [TestMethod]
    public void ProfileOrderDeterminesWhichConflictingModificationWinsDeployment()
    {
        using var directory = new TestDirectory();
        var first = CreateModification("First", "shared.txt");
        var second = CreateModification("Second", "shared.txt");
        var profile = new Profile { Name = "Main", Modifications = [first, second] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Game"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var capture = new CapturingExecutor(OperationResult.Success());
        var service = new ModDeploymentService(capture);

        service.Synchronize(session);

        Assert.HasCount(1, capture.Operations);
        Assert.AreEqual(
            Path.Combine(session.GetModificationPath(second), "shared.txt"),
            capture.Operations[0].SourcePath);

        profile.Modifications.Clear();
        profile.Modifications.AddRange([second, first]);
        service.Synchronize(session);

        Assert.HasCount(1, capture.Operations);
        Assert.AreEqual(
            Path.Combine(session.GetModificationPath(first), "shared.txt"),
            capture.Operations[0].SourcePath);
    }

    [TestMethod]
    public void SynchronizeMaterializesFilesThatCannotUseDirectoryLink()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var executablePath = Path.Combine(gameRoot, "game.exe");
        var modification = CreateModification("Executable Fix", "game.exe");
        modification.Content.Add("support.dll");
        modification.Content.Add(Path.Combine("modloader", "Fixes", "Loader.txt"));
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            ExecutablePath = executablePath,
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var executor = new CapturingExecutor(OperationResult.Success());
        var service = new ModDeploymentService(executor);

        var result = service.Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.HasCount(3, executor.Operations);
        Assert.AreEqual(
            LinkAction.Materialize,
            executor.Operations.Single(operation => operation.DestinationPath == executablePath).Action);
        Assert.AreEqual(
            LinkAction.Materialize,
            executor.Operations.Single(operation =>
                operation.DestinationPath.EndsWith("support.dll", StringComparison.Ordinal)).Action);
        Assert.AreEqual(
            LinkAction.Materialize,
            executor.Operations.Single(operation =>
                operation.DestinationPath.EndsWith("Loader.txt", StringComparison.Ordinal)).Action);
    }

    [TestMethod]
    public void SynchronizeUsesMaterializedRestoreForDisabledConfiguredExecutable()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var executablePath = Path.Combine(gameRoot, "game.exe");
        var modification = CreateModification("Executable Fix", "game.exe");
        modification.IsEnabled = false;
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            ExecutablePath = executablePath,
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var statePath = Path.Combine(session.BackupsPath, ".bolt-state", "game.exe.materialized");
        Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        File.WriteAllText(executablePath, "deployed executable");
        File.WriteAllText(statePath, string.Empty);
        var executor = new CapturingExecutor(OperationResult.Success());
        var service = new ModDeploymentService(executor);

        var result = service.Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.HasCount(1, executor.Operations);
        Assert.AreEqual(LinkAction.RestoreMaterialized, executor.Operations[0].Action);
        Assert.IsTrue(executor.Operations[0].StatePath.EndsWith(
            "game.exe.materialized",
            StringComparison.Ordinal));
    }

    [TestMethod]
    public void SynchronizeSkipsConfiguredExecutableAlreadyMaterializedFromCurrentSource()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var executablePath = Path.Combine(gameRoot, "game.exe");
        var modification = CreateModification("Executable Fix", "game.exe");
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            ExecutablePath = executablePath,
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var sourcePath = Path.Combine(session.GetModificationPath(modification), "game.exe");
        var statePath = Path.Combine(session.BackupsPath, ".bolt-state", "game.exe.materialized");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        File.WriteAllText(sourcePath, "same executable");
        File.WriteAllText(executablePath, "same executable");
        File.WriteAllText(statePath, string.Empty);
        var executor = new CapturingExecutor(OperationResult.Success());
        var service = new ModDeploymentService(executor);

        var result = service.Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.IsEmpty(executor.Operations);
    }

    [TestMethod]
    public void RestoreDefaultsDisablesAllProfilesAndCreatesBoundedRestoreOperations()
    {
        using var directory = new TestDirectory();
        var first = CreateModification("First", "Data", "first.txt");
        var second = CreateModification("Second", "Config", "second.ini");
        var firstProfile = new Profile { Name = "Main", Modifications = [first] };
        var secondProfile = new Profile { Name = "Alternate", Modifications = [second] };
        var gameRoot = directory.GetPath("Game");
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = firstProfile.Id,
            Profiles = [firstProfile, secondProfile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));

        foreach (var modification in new[] { first, second })
        {
            var backupPath = Path.Combine(session.BackupsPath, modification.Content[0]);
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            File.WriteAllText(backupPath, "backup");
        }

        var executor = new CapturingExecutor(OperationResult.Success());
        var service = new ModDeploymentService(executor);
        var result = service.RestoreDefaults(session);

        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(first.IsEnabled);
        Assert.IsFalse(second.IsEnabled);
        Assert.HasCount(2, executor.Operations);
        Assert.AreEqual(
            LinkAction.RestoreMaterialized,
            executor.Operations.Single(operation =>
                operation.DestinationPath.EndsWith("first.txt", StringComparison.Ordinal)).Action);
        Assert.AreEqual(
            LinkAction.RestoreMaterialized,
            executor.Operations.Single(operation =>
                operation.DestinationPath.EndsWith("second.ini", StringComparison.Ordinal)).Action);
        Assert.IsTrue(executor.Operations.All(operation => operation.CleanupRootPath == gameRoot));
    }

    [TestMethod]
    public void SynchronizeLinksIsolatedModificationDirectoryAndMaterializesRootFile()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var modification = new Modification
        {
            Name = "Map Pack",
            FolderName = "Map Pack",
            IsEnabled = true,
            Content =
            [
                Path.Combine("modloader", "Map Pack", "models", "building.dff"),
                Path.Combine("modloader", "Map Pack", "textures", "building.txd"),
                "plugin.asi"
            ]
        };
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        Directory.CreateDirectory(Path.Combine(gameRoot, "modloader"));
        File.WriteAllText(Path.Combine(gameRoot, "modloader", "modloader.ini"), "existing game file");

        foreach (var relativePath in modification.Content)
        {
            var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, relativePath);
        }

        var executor = new CapturingExecutor(OperationResult.Success());
        var result = new ModDeploymentService(executor).Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.HasCount(2, executor.Operations);

        var directoryLink = executor.Operations.Single(operation => operation.Action == LinkAction.LinkDirectory);
        Assert.AreEqual(
            Path.Combine(gameRoot, "modloader", "Map Pack"),
            directoryLink.DestinationPath);
        Assert.AreEqual(
            Path.Combine(session.GetModificationPath(modification), "modloader", "Map Pack"),
            directoryLink.SourcePath);

        var materialized = executor.Operations.Single(operation => operation.Action == LinkAction.Materialize);
        Assert.AreEqual(Path.Combine(gameRoot, "plugin.asi"), materialized.DestinationPath);
    }

    [TestMethod]
    public void SynchronizeRestoresDirectoryLinkWhenModificationIsDisabled()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var modification = CreateModification(
            "Map Pack",
            "modloader",
            "Map Pack",
            "models",
            "building.dff");
        modification.Content.Add(Path.Combine("modloader", "Map Pack", "textures", "building.txd"));
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        Directory.CreateDirectory(Path.Combine(gameRoot, "modloader"));
        File.WriteAllText(Path.Combine(gameRoot, "modloader", "modloader.ini"), "existing game file");

        foreach (var relativePath in modification.Content)
        {
            var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, relativePath);
        }

        var deploymentCapture = new CapturingExecutor(OperationResult.Success());
        new ModDeploymentService(deploymentCapture).Synchronize(session);
        var deployment = LinkOperationRunner.Run(deploymentCapture.Operations);

        Assert.IsTrue(deployment.Succeeded, string.Join(Environment.NewLine, deployment.Errors));

        modification.IsEnabled = false;
        var restoreCapture = new CapturingExecutor(OperationResult.Success());
        var result = new ModDeploymentService(restoreCapture).Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.HasCount(1, restoreCapture.Operations);
        Assert.AreEqual(LinkAction.RestoreDirectory, restoreCapture.Operations[0].Action);
        Assert.AreEqual(
            Path.Combine(gameRoot, "modloader", "Map Pack"),
            restoreCapture.Operations[0].DestinationPath);

        var restore = LinkOperationRunner.Run(restoreCapture.Operations);
        Assert.IsTrue(restore.Succeeded, string.Join(Environment.NewLine, restore.Errors));
    }

    [TestMethod]
    public void SynchronizeMigratesManagedCopiesIntoDirectoryLink()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var modification = CreateModification(
            "Map Pack",
            "modloader",
            "Map Pack",
            "Loader.txt");
        modification.Content.Add(Path.Combine("modloader", "Map Pack", "building.dff"));
        var profile = new Profile { Name = "Main", Modifications = [modification] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        Directory.CreateDirectory(Path.Combine(gameRoot, "modloader"));
        File.WriteAllText(Path.Combine(gameRoot, "modloader", "modloader.ini"), "existing game file");

        foreach (var relativePath in modification.Content)
        {
            var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
            var destinationPath = Path.Combine(gameRoot, relativePath);
            var statePath = Path.Combine(session.BackupsPath, ".bolt-state", $"{relativePath}.materialized");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
            File.WriteAllText(sourcePath, relativePath);
            File.WriteAllText(destinationPath, relativePath);
            File.WriteAllText(statePath, string.Empty);
        }

        var capture = new CapturingExecutor(OperationResult.Success());
        var result = new ModDeploymentService(capture).Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.HasCount(3, capture.Operations);
        Assert.HasCount(2, capture.Operations.Where(operation => operation.Action == LinkAction.RestoreMaterialized));
        Assert.AreEqual(LinkAction.LinkDirectory, capture.Operations[^1].Action);
    }

    [TestMethod]
    public void SynchronizeDoesNotMountDirectoryContainingAnotherModDeployment()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var essentials = CreateModification("Essentials", "CLEO", "engine.cleo");
        var urbanize = CreateModification("Urbanize", "CLEO", "Urbanize", "script.cs");
        var profile = new Profile { Name = "Main", Modifications = [essentials, urbanize] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));

        foreach (var modification in profile.Modifications)
        {
            foreach (var relativePath in modification.Content)
            {
                var sourcePath = Path.Combine(session.GetModificationPath(modification), relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
                File.WriteAllText(sourcePath, modification.Name);
            }
        }

        var capture = new CapturingExecutor(OperationResult.Success());
        var result = new ModDeploymentService(capture).Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(capture.Operations.Any(operation =>
            operation.Action == LinkAction.LinkDirectory
            && operation.DestinationPath.Equals(
                Path.Combine(gameRoot, "CLEO"),
                StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(capture.Operations.Any(operation =>
            operation.Action == LinkAction.Materialize
            && operation.DestinationPath.EndsWith("engine.cleo", StringComparison.Ordinal)));
        Assert.IsTrue(capture.Operations.Any(operation =>
            operation.Action == LinkAction.LinkDirectory
            && operation.DestinationPath.Equals(
                Path.Combine(gameRoot, "CLEO", "Urbanize"),
                StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void SynchronizeRestoresConflictingParentJunctionBeforeWritingFiles()
    {
        using var directory = new TestDirectory();
        var gameRoot = directory.GetPath("Game");
        var urbanize = CreateModification("Urbanize", "CLEO", "Urbanize", "script.cs");
        var essentials = CreateModification("Essentials", "CLEO", "engine.cleo");
        var profile = new Profile { Name = "Main", Modifications = [urbanize, essentials] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = gameRoot,
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var essentialsDirectory = Path.Combine(session.GetModificationPath(essentials), "CLEO");
        var essentialsFile = Path.Combine(essentialsDirectory, "engine.cleo");
        var leakedFile = Path.Combine(essentialsDirectory, "Urbanize", "leaked.cs");
        var urbanizeFile = Path.Combine(
            session.GetModificationPath(urbanize),
            "CLEO",
            "Urbanize",
            "script.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(leakedFile)!);
        Directory.CreateDirectory(Path.GetDirectoryName(urbanizeFile)!);
        File.WriteAllText(essentialsFile, "engine");
        File.WriteAllText(leakedFile, "left by interrupted deployment");
        File.WriteAllText(urbanizeFile, "urbanize");
        var essentialsState = Path.Combine(
            session.BackupsPath,
            ".bolt-state",
            "CLEO",
            "engine.cleo.materialized");
        Directory.CreateDirectory(Path.GetDirectoryName(essentialsState)!);
        File.WriteAllText(essentialsState, string.Empty);

        var existingLink = LinkOperationRunner.Run(
        [
            new LinkOperation
            {
                Action = LinkAction.LinkDirectory,
                SourcePath = essentialsDirectory,
                DestinationPath = Path.Combine(gameRoot, "CLEO"),
                CleanupRootPath = gameRoot
            }
        ]);
        Assert.IsTrue(existingLink.Succeeded, string.Join(Environment.NewLine, existingLink.Errors));

        var capture = new CapturingExecutor(OperationResult.Success());
        var result = new ModDeploymentService(capture).Synchronize(session);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(LinkAction.RestoreDirectory, capture.Operations[0].Action);
        Assert.AreEqual(Path.Combine(gameRoot, "CLEO"), capture.Operations[0].DestinationPath);
        Assert.IsFalse(capture.Operations.Any(operation =>
            operation.Action == LinkAction.LinkDirectory
            && operation.DestinationPath.Equals(
                Path.Combine(gameRoot, "CLEO"),
                StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(capture.Operations.Any(operation =>
            operation.Action == LinkAction.Materialize
            && operation.DestinationPath.EndsWith("engine.cleo", StringComparison.Ordinal)));

        var deployment = LinkOperationRunner.Run(capture.Operations);

        Assert.IsTrue(deployment.Succeeded, string.Join(Environment.NewLine, deployment.Errors));
        Assert.AreEqual("engine", File.ReadAllText(essentialsFile));
        Assert.AreEqual("left by interrupted deployment", File.ReadAllText(leakedFile));
        Assert.AreEqual("engine", File.ReadAllText(Path.Combine(gameRoot, "CLEO", "engine.cleo")));
        Assert.AreEqual(
            "urbanize",
            File.ReadAllText(Path.Combine(gameRoot, "CLEO", "Urbanize", "script.cs")));
    }

    [TestMethod]
    public void RestoreDefaultsRestoresEnabledStatesWhenDeploymentFails()
    {
        using var directory = new TestDirectory();
        var enabled = CreateModification("Enabled", "enabled.txt");
        var disabled = CreateModification("Disabled", "disabled.txt");
        disabled.IsEnabled = false;
        var profile = new Profile { Name = "Main", Modifications = [enabled, disabled] };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Game"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var service = new ModDeploymentService(new CapturingExecutor(OperationResult.Failure("failed")));

        var result = service.RestoreDefaults(session);

        Assert.IsTrue(result.Failed);
        Assert.IsTrue(enabled.IsEnabled);
        Assert.IsFalse(disabled.IsEnabled);
    }

    private static Modification CreateModification(string name, params string[] pathParts) => new()
    {
        Name = name,
        FolderName = name,
        IsEnabled = true,
        Content = [Path.Combine(pathParts)]
    };

    private sealed class CapturingExecutor(OperationResult result) : ILinkOperationExecutor
    {
        public IReadOnlyList<LinkOperation> Operations { get; private set; } = [];

        public OperationResult Apply(IReadOnlyList<LinkOperation> operations)
        {
            Operations = operations;
            return result;
        }
    }
}
