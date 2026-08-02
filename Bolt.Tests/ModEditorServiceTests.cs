using Bolt.Core;
using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Infrastructure.Storage;
using Bolt.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class ModEditorServiceTests
{
    private static readonly string[] OriginalPaths = ["first.txt", @"old\second.txt"];
    private static readonly string[] EditedPaths = [@"Data\first.txt", "second.txt"];
    private static readonly string[] RemainingPaths = ["first.txt"];
    private static readonly string[] FailedEditPaths = [@"new\first.txt"];

    [TestMethod]
    public void ApplyValidEditUpdatesMetadataFilesAndDeployment()
    {
        using var directory = new TestDirectory();
        var (session, modification) = CreateSession(directory);
        var deployment = new StubDeploymentService();
        var service = new ModEditorService(deployment, new GameRepository());

        var result = service.Apply(session, modification, new ModificationEdit(
            "Better Mod",
            "A useful description.",
            "2.0",
            "Visuals",
            [
                new ModFileEdit("first.txt", @"Data\first.txt"),
                new ModFileEdit(@"old\second.txt", "second.txt")
            ]));

        Assert.IsTrue(result.Succeeded, result.Error);
        Assert.AreEqual("Better Mod", modification.Name);
        Assert.AreEqual("A useful description.", modification.Description);
        Assert.AreEqual("2.0", modification.Version);
        Assert.AreEqual("Visuals", modification.Category);
        CollectionAssert.AreEqual(EditedPaths, modification.Content);
        Assert.AreEqual("first", File.ReadAllText(directory.GetPath("Modifications", "Sample", "Data", "first.txt")));
        Assert.AreEqual("second", File.ReadAllText(directory.GetPath("Modifications", "Sample", "second.txt")));
        Assert.HasCount(2, deployment.Calls);
        Assert.IsFalse(deployment.Calls[0].ModificationWasEnabled);
        Assert.IsTrue(deployment.Calls[1].ModificationWasEnabled);
        Assert.IsTrue(File.Exists(session.FilePath));
    }

    [TestMethod]
    public void ApplyPathOutsideModificationIsRejectedWithoutChanges()
    {
        using var directory = new TestDirectory();
        var (session, modification) = CreateSession(directory);
        var deployment = new StubDeploymentService();
        var service = new ModEditorService(deployment, new GameRepository());

        var result = service.Apply(session, modification, new ModificationEdit(
            modification.Name,
            string.Empty,
            string.Empty,
            string.Empty,
            [
                new ModFileEdit("first.txt", @"..\outside.txt"),
                new ModFileEdit(@"old\second.txt", @"old\second.txt")
            ]));

        Assert.IsTrue(result.Failed);
        CollectionAssert.AreEqual(OriginalPaths, modification.Content);
        Assert.IsTrue(File.Exists(directory.GetPath("Modifications", "Sample", "first.txt")));
        Assert.IsEmpty(deployment.Calls);
    }

    [TestMethod]
    public void ApplyRemovedFileDeletesManagedSourceAndEmptyDirectories()
    {
        using var directory = new TestDirectory();
        var (session, modification) = CreateSession(directory);
        var deployment = new StubDeploymentService();
        var service = new ModEditorService(deployment, new GameRepository());

        var result = service.Apply(session, modification, new ModificationEdit(
            modification.Name,
            modification.Description,
            modification.Version,
            modification.Category,
            [new ModFileEdit("first.txt", "first.txt")]));

        Assert.IsTrue(result.Succeeded, result.Error);
        CollectionAssert.AreEqual(RemainingPaths, modification.Content);
        Assert.IsTrue(File.Exists(directory.GetPath("Modifications", "Sample", "first.txt")));
        Assert.IsFalse(File.Exists(directory.GetPath("Modifications", "Sample", "old", "second.txt")));
        Assert.IsFalse(Directory.Exists(directory.GetPath("Modifications", "Sample", "old")));
        Assert.IsFalse(Directory.EnumerateDirectories(session.ModificationsPath, ".bolt-edit-*").Any());
        Assert.HasCount(2, deployment.Calls);
    }

    [TestMethod]
    public void ApplyRedeploymentFailureRestoresOriginalState()
    {
        using var directory = new TestDirectory();
        var (session, modification) = CreateSession(directory);
        var deployment = new StubDeploymentService(
            OperationResult.Success(),
            OperationResult.Failure("Deployment failed."),
            OperationResult.Success());
        var service = new ModEditorService(deployment, new GameRepository());

        var result = service.Apply(session, modification, new ModificationEdit(
            "Changed",
            "Changed",
            "2.0",
            "Changed",
            [
                new ModFileEdit("first.txt", @"new\first.txt")
            ]));

        Assert.IsTrue(result.Failed);
        Assert.AreEqual("Sample Mod", modification.Name);
        Assert.AreEqual(string.Empty, modification.Description);
        CollectionAssert.AreEqual(OriginalPaths, modification.Content);
        Assert.IsTrue(File.Exists(directory.GetPath("Modifications", "Sample", "first.txt")));
        Assert.IsTrue(File.Exists(directory.GetPath("Modifications", "Sample", "old", "second.txt")));
        Assert.IsFalse(Directory.Exists(directory.GetPath("Modifications", "Sample", "new")));
        Assert.HasCount(3, deployment.Calls);
        CollectionAssert.AreEquivalent(
            FailedEditPaths,
            deployment.Calls[2].Removed!.Single().Content);
    }

    private static (GameSession Session, Modification Modification) CreateSession(TestDirectory directory)
    {
        var modification = new Modification
        {
            Name = "Sample Mod",
            FolderName = "Sample",
            IsEnabled = true,
            Content = ["first.txt", @"old\second.txt"]
        };

        var profile = new Profile
        {
            Name = "Main",
            Modifications = [modification]
        };

        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Game"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };

        var session = new GameSession(game, directory.GetPath("Game.bltg"));
        var modificationRoot = session.GetModificationPath(modification);

        Directory.CreateDirectory(Path.Combine(modificationRoot, "old"));
        File.WriteAllText(Path.Combine(modificationRoot, "first.txt"), "first");
        File.WriteAllText(Path.Combine(modificationRoot, "old", "second.txt"), "second");

        return (session, modification);
    }

    private sealed class StubDeploymentService(params OperationResult[] results) : IModDeploymentService
    {
        private readonly Queue<OperationResult> _results = new(results);

        public List<DeploymentCall> Calls { get; } = [];

        public OperationResult Synchronize(GameSession session, IReadOnlyCollection<Modification>? removed = null)
        {
            Calls.Add(new DeploymentCall(session.ActiveProfile.Modifications[0].IsEnabled, removed));

            return _results.Count > 0 ? _results.Dequeue() : OperationResult.Success();
        }

        public OperationResult RestoreDefaults(GameSession session) =>
            _results.Count > 0 ? _results.Dequeue() : OperationResult.Success();

        public IReadOnlyDictionary<string, IReadOnlyList<string>> FindConflicts(GameSession session) =>
            new Dictionary<string, IReadOnlyList<string>>();
    }

    private sealed record DeploymentCall(
        bool ModificationWasEnabled,
        IReadOnlyCollection<Modification>? Removed);
}
