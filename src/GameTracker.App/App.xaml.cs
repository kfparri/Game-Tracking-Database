using GameTracker.App.ViewModels;
using GameTracker.Data;
using GameTracker.Data.Export;
using GameTracker.Data.Import;
using GameTracker.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace GameTracker.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameTracker");

        var appPaths = new AppPaths(appDataRoot);

        Directory.CreateDirectory(appPaths.RootDirectory);

        //var dbPath = Path.Combine(appDataRoot, "gameTracker.db");

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {                
                services.AddSingleton(appPaths);

                services.AddDbContext<GameTrackerContext>(options =>
                {
                    options.UseSqlite($"Data Source={appPaths.DatabasePath}");
                });

                services.AddScoped<ITagService, TagService>();
                services.AddScoped<IGameExporter, CsvGameExporter>();
                services.AddScoped<IGameExporter, PlainTextGameExporter>();
                services.AddScoped<IExportService, ExportService>();
                services.AddScoped<IDatabaseResetService, DatabaseResetService>();
                services.AddScoped<IGameImporter, SteamJsonGameImporter>();
                services.AddScoped<IGameImporter, GogJsonGameImporter>();
                services.AddScoped<IImportService, ImportService>();
                services.AddScoped<IGameService, GameService>();

                services.AddSingleton<IImageStorageService>(sp =>
                    new ImageStorageService(sp.GetRequiredService<AppPaths>()));

                services.AddSingleton<IBackupService, BackupService>();

                //services.AddSingleton<IBackupService>(sp =>
                //   new BackupService(sp.GetRequiredService<AppPaths>()));

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();

                services.AddTransient<SettingsWindow>();
                services.AddTransient<SettingsViewModel>();
            })
            .Build();

        // Apply pending migrations at startup
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameTrackerContext>();
            db.Database.Migrate();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}

