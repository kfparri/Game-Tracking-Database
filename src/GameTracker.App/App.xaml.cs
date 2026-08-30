using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GameTracker.Data;
using System.IO;
using GameTracker.App.ViewModels;

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

        Directory.CreateDirectory(appDataRoot);

        var dbPath = Path.Combine(appDataRoot, "gameTracker.db");

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<GameTrackerContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbPath}");
                });

                services.AddSingleton<IImageStorageService>(new ImageStorageService(appDataRoot));

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
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

