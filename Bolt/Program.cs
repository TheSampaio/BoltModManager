using Bolt.Forms;
using Bolt.Interfaces;
using Bolt.Services;
using Bolt.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bolt
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static IConfiguration Configuration { get; private set; } = null!;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--elevated-helper")
            {
                new ElevatedOperationService().ExecuteManifest(args[1]);
                return;
            }

            ApplicationConfiguration.Initialize();

            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainForm = ServiceProvider.GetRequiredService<FrmHome>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            services.Configure<AppConfig>(Configuration.GetSection("AppConfig"));

            services.AddSingleton<IGameSessionService, GameSessionService>();
            services.AddSingleton<IGameProcessService, GameProcessService>();
            services.AddTransient<IModImportService, ModImportService>();
            services.AddTransient<IModDeploymentService, ModDeploymentService>();
            services.AddTransient<IElevatedOperationService, ElevatedOperationService>();

            services.AddTransient<FrmHome>();
            services.AddTransient<FrmNewGame>();
            services.AddTransient<FrmPreferences>();
        }
    }
}