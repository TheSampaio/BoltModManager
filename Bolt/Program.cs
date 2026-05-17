using Bolt.Forms;
using Bolt.Interfaces;
using Bolt.Models;
using Bolt.Services;
using Bolt.Utilities;
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
                RunElevatedOperations(args[1]);
                return;
            }

            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var mainForm = ServiceProvider.GetRequiredService<FrmHome>();
            Application.Run(mainForm);
        }

        private static void RunElevatedOperations(string manifestPath)
        {
            try
            {
                var operations = Json.Deserialize<List<LinkOperationModel>>(manifestPath);
                if (operations == null || operations.Count == 0) return;

                foreach (var op in operations)
                {
                    if (op.Action == "Link")
                    {
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(op.DestinationPath)!);

                        if (File.Exists(op.DestinationPath) && !IsSymbolicLink(op.DestinationPath))
                        {
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(op.BackupPath)!);
                            File.Move(op.DestinationPath, op.BackupPath, true);
                        }

                        if (File.Exists(op.SourcePath))
                            SymbolicLink.Create(op.DestinationPath, op.SourcePath, Enums.SymbolicLinkType.File);
                    }
                    else if (op.Action == "Restore")
                    {
                        if (IsSymbolicLink(op.DestinationPath))
                            File.Delete(op.DestinationPath);

                        if (File.Exists(op.BackupPath))
                            File.Move(op.BackupPath, op.DestinationPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro no processo elevado:\n{ex.Message}", "Erro de Elevação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(manifestPath))
                    File.Delete(manifestPath);
            }
        }

        private static bool IsSymbolicLink(string path)
        {
            if (!File.Exists(path) && !System.IO.Directory.Exists(path)) return false;
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGameSessionService, GameSessionService>();
            services.AddSingleton<IGameProcessService, GameProcessService>();
            services.AddTransient<IModImportService, ModImportService>();

            services.AddTransient<FrmHome>(provider => new FrmHome(
                provider.GetRequiredService<IGameSessionService>(),
                provider.GetRequiredService<IGameProcessService>(),
                provider.GetRequiredService<IModImportService>()
            ));

            services.AddTransient<FrmNewGame>(provider => new FrmNewGame(
                provider.GetRequiredService<IGameSessionService>()
            ));

            services.AddTransient<FrmPreferences>();
        }
    }
}