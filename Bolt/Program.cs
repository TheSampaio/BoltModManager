using Bolt.Forms;
using Bolt.Interfaces;
using Bolt.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--elevated-helper")
            {
                new ElevatedOperationService().ExecuteManifest(args[1]);
                return;
            }

            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainForm = ServiceProvider.GetRequiredService<FrmHome>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGameSessionService, GameSessionService>();
            services.AddSingleton<IGameProcessService, GameProcessService>();
            services.AddTransient<IModImportService, ModImportService>();
            services.AddTransient<IModDeploymentService, ModDeploymentService>();
            services.AddTransient<IElevatedOperationService, ElevatedOperationService>();

            services.AddTransient<FrmHome>(provider => new FrmHome(
                provider.GetRequiredService<IGameSessionService>(),
                provider.GetRequiredService<IGameProcessService>(),
                provider.GetRequiredService<IModImportService>(),
                provider.GetRequiredService<IModDeploymentService>()
            ));

            services.AddTransient<FrmNewGame>(provider => new FrmNewGame(
                provider.GetRequiredService<IGameSessionService>()
            ));

            services.AddTransient<FrmPreferences>();
        }
    }
}