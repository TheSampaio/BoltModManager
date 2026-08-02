using System.Diagnostics;
using Bolt.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bolt.Tests;

[TestClass]
public sealed class GameProcessServiceTests
{
    [TestMethod]
    public void TerminateStopsTrackedProcessTreeAndPublishesExit()
    {
        using var directory = new TestDirectory();
        var executablePath = directory.GetPath("game.exe");
        File.WriteAllBytes(executablePath, []);

        using var exited = new ManualResetEventSlim();
        using var service = new GameProcessService(_ => StartLongRunningProcess());
        service.GameExited += exited.Set;

        var launch = service.Run(executablePath);
        var termination = service.Terminate();

        Assert.IsTrue(launch.Succeeded, launch.Error);
        Assert.IsTrue(termination.Succeeded, termination.Error);
        Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(5)), "The process exit event was not published.");
        Assert.IsFalse(service.IsRunning);
    }

    private static Process StartLongRunningProcess()
    {
        var commandInterpreter = Environment.GetEnvironmentVariable("ComSpec")
            ?? throw new InvalidOperationException("The command interpreter was not found.");

        return Process.Start(new ProcessStartInfo
        {
            FileName = commandInterpreter,
            Arguments = "/d /c ping 127.0.0.1 -n 30 > nul",
            CreateNoWindow = true,
            UseShellExecute = false
        }) ?? throw new InvalidOperationException("The test process could not be started.");
    }
}
