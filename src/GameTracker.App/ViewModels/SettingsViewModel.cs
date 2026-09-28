using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameTracker.Core.Export;
using GameTracker.Data;
using GameTracker.Data.Services;
using GameTracker.Data.Import;
using System.IO;
using System.Windows;
using GameTracker.Core.Import;

namespace GameTracker.App.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly GameTrackerContext _db;

        private readonly IBackupService _backupService;

        private readonly AppPaths _appPaths;

        private readonly IExportService _exportService;

        private readonly IDatabaseResetService _databaseResetService;

        private readonly IImportService _importService;

        [ObservableProperty]
        private string databaseLocation;

        [ObservableProperty]
        private string lastBackupDisplay;

        public ExportFormat[] AvailableFormats { get; } = Enum.GetValues<ExportFormat>();

        [ObservableProperty]
        private ExportFormat selectedExportFormat = ExportFormat.Csv;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResetDatabaseCommand))]
        private string resetConfirmationText = string.Empty;

        private bool CanResetDatabase() => ResetConfirmationText == "RESET";

        [ObservableProperty]
        private bool isImporting;

        [ObservableProperty]
        private int importCurrent;

        [ObservableProperty]
        private int importTotal;

        public string ImportStatusText => $"Importing {ImportCurrent} of {ImportTotal}...";

        public SettingsViewModel(GameTrackerContext db, 
                                 IBackupService backupService,
                                 AppPaths appPaths,
                                 IExportService exportService,
                                 IDatabaseResetService databaseResetService,
                                 IImportService importService)
        {
            _db = db;
            _backupService = backupService;
            _appPaths = appPaths;
            _exportService = exportService;
            _databaseResetService = databaseResetService;
            _importService = importService;

            databaseLocation = _appPaths.DatabasePath;
            LastBackupDisplay = "Never";
        }

        [RelayCommand]
        private async Task Restore()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "GameTracker Backup (*.zip)|*.zip",
                Title = "Select backup to restore"
            };

            if (dialog.ShowDialog() != true) return;

            var confirm = MessageBox.Show(
                "Restoring will replace your current library with the contents of this backup.  This cannot be undone.  Continue?",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            await _backupService.RestoreBackupAsync(dialog.FileName);

            MessageBox.Show(
                "Restore complete.  The application will now restart to apply changes.",
                "Restore Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // restart the process so every service/dbcontext initializes fresh
            // against the restored database and image files
            System.Diagnostics.Process.Start(Environment.ProcessPath);
            Application.Current.Shutdown();
        }

        [RelayCommand(CanExecute = nameof(CanResetDatabase))]
        private async Task ResetDatabase()
        {
            var confirm = MessageBox.Show(
                "This will permanently delete all games, tags, and images, and recreate an empty database.  A backup will be saved automatically first, but this action cannot be undone within the app.  Continue?",
                "Confirm Database Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            await _databaseResetService.ResetDatabaseAsync();

            MessageBox.Show(
                "Database reset complete.  The app will now restart",
                "Reset Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            System.Diagnostics.Process.Start(Environment.ProcessPath);
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private async Task Backup()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "GameTracker Backup (*.zip)|*.zip",
                Title = "Select location to save backup",
                FileName = $"GameTracker_Backup_{DateTime.Now:yyyy-MM-dd_HHmmss}.zip"
            };

            if (dialog.ShowDialog() != true) return;

            await _backupService.CreateBackupAsync(dialog.FileName);

            MessageBox.Show(
                "Backup complete.",
                "Backup Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // open the folder containing the file backup
            var folderPath = Path.GetDirectoryName(dialog.FileName);

            if (folderPath == null) return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        [RelayCommand]
        private async Task Export()
        {
            var extension = SelectedExportFormat == ExportFormat.Csv ? "csv" : "txt";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = SelectedExportFormat == ExportFormat.Csv
                    ? "CSV file (*.csv)|*.csv"
                    : "Text file (*.txt)|*.txt",
                FileName = $"GameTracker_Export.{extension}"
            };

            if (dialog.ShowDialog() != true) return;

            await _exportService.ExportAsync(SelectedExportFormat, dialog.FileName);

            MessageBox.Show("Export complete.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ImportGogJson()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "GameTracker import(*.json)|*.json",
                Title = "Select import file"
            };

            if (dialog.ShowDialog() != true) return;

            var confirm = MessageBox.Show(
                "This will import the data from the JSON file, it will update existing games and insert new games.  Continue?",
                "Confirm Import",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var records = await _importService.ReadImportFileAsync(ImportFormat.GOGJson, dialog.FileName);

            IsImporting = true;
            ImportTotal = records.Count();
            ImportCurrent = 0;

            var progress = new Progress<(int current, int total)>(p =>
            {
                ImportCurrent = p.current;
                ImportTotal = p.total;
                OnPropertyChanged(nameof(ImportCurrent));
            });

            try
            {
                await _importService.ImportGameAsync(ImportFormat.GOGJson,
                                                     records,
                                                     Path.GetDirectoryName(dialog.FileName),
                                                     progress);
            }
            finally
            {
                IsImporting = false;
            }

            MessageBox.Show(
                "Import complete",
                "Import Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ImportSteamJson()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "GameTracker import(*.json)|*.json",
                Title = "Select import file"
            };

            if (dialog.ShowDialog() != true) return;

            var confirm = MessageBox.Show(
                "This will import the data from the JSON file, it will update existing games and insert new games.  Continue?",
                "Confirm Import",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var records = await _importService.ReadImportFileAsync(ImportFormat.SteamJson, dialog.FileName);

            IsImporting = true;
            ImportTotal = records.Count();
            ImportCurrent = 0;

            var progress = new Progress<(int current, int total)>(p =>
            {
                ImportCurrent = p.current;
                ImportTotal = p.total;
                OnPropertyChanged(nameof(ImportStatusText));
            });

            try
            {
                await _importService.ImportGameAsync(ImportFormat.SteamJson,
                                                     records, 
                                                     Path.GetDirectoryName(dialog.FileName), 
                                                     progress);
            }
            finally
            {
                IsImporting = false;
            }

            //await _importService.ImportAsync(Core.Import.ImportFormat.SteamJson, dialog.FileName);

            MessageBox.Show(
                "Import complete",
                "Import Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
