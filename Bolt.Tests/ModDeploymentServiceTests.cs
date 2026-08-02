using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class ModDeploymentServiceTests
{
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
        Assert.IsTrue(executor.Operations.All(operation => operation.Action == LinkAction.Restore));
        Assert.IsTrue(executor.Operations.All(operation => operation.CleanupRootPath == gameRoot));
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
