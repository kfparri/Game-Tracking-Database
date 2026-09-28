using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameTracker.Data.Services
{
    public class DatabaseResetService : IDatabaseResetService
    {
        private readonly AppPaths _paths;
        private readonly GameTrackerContext _db;
        private readonly IBackupService _backupService;

        public DatabaseResetService(AppPaths appPaths, 
                                    GameTrackerContext db,
                                    IBackupService backupService)
        {
            _paths = appPaths;
            _db = db;
            _backupService = backupService;
        }

        public async Task ResetDatabaseAsync(bool createSafetyBackup = true)
        {
            // create safety backup if marked true
            if (createSafetyBackup)
            {
                var preResetFolder = Path.Combine(_paths.RootDirectory, "PreResetBackups");
                Directory.CreateDirectory(preResetFolder);
                var backupPath = Path.Combine(preResetFolder, $"GameTracker_Backup_{DateTime.Now:yyyy-MM-dd_HHmmss}.zip");
                await _backupService.CreateBackupAsync(backupPath);
            }

            // close and flush db connections
            await _db.Database.CloseConnectionAsync();
            SqliteConnection.ClearAllPools();

            if (File.Exists(_paths.DatabasePath))
                File.Delete(_paths.DatabasePath);

            if(Directory.Exists(_paths.ImagesDirectory))
            {
                Directory.Delete(_paths.ImagesDirectory, recursive: true);
                Directory.CreateDirectory(_paths.ImagesDirectory);
            }
        }
    }
}
