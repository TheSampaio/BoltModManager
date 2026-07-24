using System.Text;
using Bolt.Core.Abstractions;
using Bolt.Infrastructure.Archives;
using Bolt.Infrastructure.Deployment;
using Bolt.Infrastructure.Storage;
using Bolt.Services;
using Bolt.UI.Forms;
using Bolt.UI.Services;
using Bolt.UI.Theme;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt;

/// <summary>Entry point and composition root of the application.</summary>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // The elevated helper runs headless and must not initialise any UI.
        if (ElevatedHelper.TryParseArguments(args, out var manifestPath))
            return ElevatedHelper.Run(manifestPath);

        ApplicationConfiguration.Initialize();

        Application.ThreadException += (_, e) => ReportCrash(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportCrash(e.ExceptionObject as Exception);

        using var services = BuildServiceProvider(LoadConfiguration());

        AppTheme.Apply(services.GetRequiredService<IUserPreferencesService>().Current.Theme);

        Application.Run(services.GetRequiredService<MainForm>());

        return 0;
    }

    private static IConfiguration LoadConfiguration() =>
        new ConfigurationBuilder()
            // Resolved from the install folder: the working directory changes with the shortcut
            // used to start the application, and the file is optional so a missing or damaged
            // deployment degrades to the defaults instead of crashing on startup.
            .SetBasePath(AppPaths.InstallDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

    private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var settings = configuration.GetSection(AppSettings.SectionName).Get<AppSettings>() ?? new AppSettings();

        var services = new ServiceCollection();

        services.AddSingleton(settings);

        services.AddSingleton<IUserPreferencesService>(_ => new UserPreferencesService(settings.DefaultGamesPath));
        services.AddSingleton<IGameRepository, GameRepository>();
        services.AddSingleton<IArchiveReader, ZipArchiveReader>();
        services.AddSingleton<ILinkOperationExecutor, LinkOperationExecutor>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<IGameProcessService, GameProcessService>();
        services.AddSingleton<IModDeploymentService, ModDeploymentService>();
        services.AddSingleton<IModImportService, ModImportService>();

        services.AddTransient<MainForm>();
        services.AddTransient<NewGameForm>();
        services.AddTransient<PreferencesForm>();

        // Factories instead of handing the container to the forms: a window can create the dialogs
        // it owns without turning the service provider into a global service locator.
        services.AddSingleton<Func<NewGameForm>>(provider => provider.GetRequiredService<NewGameForm>);
        services.AddSingleton<Func<PreferencesForm>>(provider => provider.GetRequiredService<PreferencesForm>);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Records an unexpected failure and tells the user where the details are, instead of letting
    /// the process disappear without a trace.
    /// </summary>
    private static void ReportCrash(Exception? exception)
    {
        if (exception is null)
            return;

        var logPath = Path.Combine(AppPaths.DataDirectory, "crash.log");

        try
        {
            AppPaths.EnsureDataDirectory();

            File.AppendAllText(
                logPath,
                $"[{DateTime.Now:u}] {exception}{Environment.NewLine}{Environment.NewLine}",
                Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Reporting must never throw on top of the original failure.
        }

        MessageBox.Show(
            $"Bolt ran into an unexpected problem:{Environment.NewLine}{Environment.NewLine}{exception.Message}"
            + $"{Environment.NewLine}{Environment.NewLine}Details were written to:{Environment.NewLine}{logPath}",
            "Bolt",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
