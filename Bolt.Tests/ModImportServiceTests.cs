using Bolt.Core.Abstractions;
using Bolt.Core.Models;
using Bolt.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class ModImportServiceTests
{
    private static readonly string[] ArchiveNames = ["First.zip", "Second.zip", "Third.zip"];
    private static readonly string[] ExpectedModificationNames = ["First", "Second", "Third"];

    [TestMethod]
    public async Task ImportAsyncExtractsUpToThreeArchivesConcurrentlyAndCommitsInInputOrder()
    {
        using var directory = new TestDirectory();
        using var reader = new ConcurrentArchiveReader(expectedConcurrency: 3);
        var profile = new Profile { Name = "Main" };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Target"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var archives = ArchiveNames
            .Select(name => directory.GetPath(name))
            .ToArray();

        var service = new ModImportService(reader);
        var imported = await service.ImportAsync(archives, session, profile);

        Assert.AreEqual(3, reader.MaximumConcurrency);
        CollectionAssert.AreEqual(
            ExpectedModificationNames,
            imported.Select(item => item.Modification.Name).ToArray());

        foreach (var modification in profile.Modifications)
            Assert.IsTrue(File.Exists(Path.Combine(session.GetModificationPath(modification), "content.txt")));
    }

    private sealed class ConcurrentArchiveReader(int expectedConcurrency) : IArchiveReader, IDisposable
    {
        private readonly CountdownEvent _started = new(expectedConcurrency);
        private int _active;
        private int _maximumConcurrency;

        public IReadOnlyCollection<string> SupportedExtensions => [".zip"];

        public int MaximumConcurrency => _maximumConcurrency;

        public bool CanRead(string archivePath) =>
            Path.GetExtension(archivePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);

        public IReadOnlyList<ArchiveEntry> ListEntries(string archivePath) =>
            [new ArchiveEntry("content.txt", 7)];

        public IReadOnlyList<string> Extract(
            string archivePath,
            string destinationRoot,
            Action<string>? onEntryExtracted = null,
            CancellationToken cancellationToken = default)
        {
            var concurrency = Interlocked.Increment(ref _active);
            UpdateMaximum(concurrency);
            _started.Signal();

            try
            {
                if (!_started.Wait(TimeSpan.FromSeconds(5), cancellationToken))
                    Assert.Fail("The expected concurrent archive workers did not start.");

                Directory.CreateDirectory(destinationRoot);
                File.WriteAllText(Path.Combine(destinationRoot, "content.txt"), "content");
                onEntryExtracted?.Invoke("content.txt");
                return ["content.txt"];
            }
            finally
            {
                Interlocked.Decrement(ref _active);
            }
        }

        public void Dispose() => _started.Dispose();

        private void UpdateMaximum(int value)
        {
            int snapshot;

            do
            {
                snapshot = _maximumConcurrency;

                if (snapshot >= value)
                    return;
            }
            while (Interlocked.CompareExchange(ref _maximumConcurrency, value, snapshot) != snapshot);
        }
    }
}
