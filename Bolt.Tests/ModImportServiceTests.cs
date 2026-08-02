using System.Collections.Concurrent;
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
    public async Task ImportAsyncLimitsConcurrentExtractionAndCommitsInInputOrder()
    {
        using var directory = new TestDirectory();
        using var reader = new ConcurrentArchiveReader(expectedConcurrency: 2);
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
        var progress = new RecordingProgress();
        var imported = await service.ImportAsync(archives, session, profile, progress);

        Assert.AreEqual(2, reader.MaximumConcurrency);
        Assert.HasCount(3, progress.Reports);
        Assert.IsTrue(progress.Reports.All(report => report.Total == 3));
        Assert.AreEqual(3, progress.Reports.Max(report => report.Completed));
        CollectionAssert.AreEqual(
            ExpectedModificationNames,
            imported.Select(item => item.Modification.Name).ToArray());

        foreach (var modification in profile.Modifications)
            Assert.IsTrue(File.Exists(Path.Combine(session.GetModificationPath(modification), "content.txt")));
    }

    [TestMethod]
    public async Task ImportAsyncCancellationRemovesTemporaryFilesAndDoesNotChangeProfile()
    {
        using var directory = new TestDirectory();
        using var reader = new CancelingArchiveReader();
        using var cancellation = new CancellationTokenSource();
        var profile = new Profile { Name = "Main" };
        var game = new Game
        {
            Name = "Test Game",
            TargetPath = directory.GetPath("Target"),
            ActiveProfileId = profile.Id,
            Profiles = [profile]
        };
        var session = new GameSession(game, directory.GetPath("Managed", "Game.bltg"));
        var service = new ModImportService(reader);
        var import = service.ImportAsync(
            [directory.GetPath("Canceled.zip")],
            session,
            profile,
            cancellationToken: cancellation.Token);

        var extractionStarted = reader.Started.Wait(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        Assert.IsTrue(extractionStarted, "Extraction did not start.");
        await Assert.ThrowsAsync<OperationCanceledException>(() => import);

        Assert.IsEmpty(profile.Modifications);
        Assert.IsFalse(Directory.EnumerateFileSystemEntries(session.ModificationsPath).Any());
    }

    private sealed class ConcurrentArchiveReader(int expectedConcurrency) : IArchiveReader, IDisposable
    {
        private readonly CountdownEvent _started = new(expectedConcurrency);
        private readonly int _expectedConcurrency = expectedConcurrency;
        private int _active;
        private int _maximumConcurrency;
        private int _startedCount;

        public IReadOnlyCollection<string> SupportedExtensions => [".zip"];

        public int MaximumConcurrency => _maximumConcurrency;

        public bool CanRead(string archivePath) =>
            Path.GetExtension(archivePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);

        public IReadOnlyList<ArchiveEntry> ListEntries(string archivePath) =>
            [new ArchiveEntry("content.txt", 7)];

        public int CountEntries(string archivePath, CancellationToken cancellationToken = default) => 1;

        public IReadOnlyList<string> Extract(
            string archivePath,
            string destinationRoot,
            Action<string>? onEntryExtracted = null,
            CancellationToken cancellationToken = default)
        {
            var concurrency = Interlocked.Increment(ref _active);
            var startedCount = Interlocked.Increment(ref _startedCount);
            UpdateMaximum(concurrency);

            if (startedCount <= _expectedConcurrency)
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

    private sealed class RecordingProgress : IProgress<ImportProgress>
    {
        private readonly ConcurrentQueue<ImportProgress> _reports = new();

        public IReadOnlyCollection<ImportProgress> Reports => _reports;

        public void Report(ImportProgress value) => _reports.Enqueue(value);
    }

    private sealed class CancelingArchiveReader : IArchiveReader, IDisposable
    {
        public ManualResetEventSlim Started { get; } = new();

        public IReadOnlyCollection<string> SupportedExtensions => [".zip"];

        public bool CanRead(string archivePath) => true;

        public IReadOnlyList<ArchiveEntry> ListEntries(string archivePath) =>
            [new ArchiveEntry("content.txt", 7)];

        public int CountEntries(string archivePath, CancellationToken cancellationToken = default) => 1;

        public IReadOnlyList<string> Extract(
            string archivePath,
            string destinationRoot,
            Action<string>? onEntryExtracted = null,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(destinationRoot);
            File.WriteAllText(Path.Combine(destinationRoot, "partial.tmp"), "partial");
            Started.Set();
            cancellationToken.WaitHandle.WaitOne();
            cancellationToken.ThrowIfCancellationRequested();
            return [];
        }

        public void Dispose() => Started.Dispose();
    }
}
