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
