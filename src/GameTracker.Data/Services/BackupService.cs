using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace GameTracker.Data.Services
{
    public class BackupService : IBackupService
    {
        private readonly AppPaths _appPaths;
        private readonly GameTrackerContext _db;


        public BackupService(AppPaths appPaths, GameTrackerContext db)
        {
            _appPaths = appPaths;
            _db = db;
        }

        public async Task<string> CreateBackupAsync(string backupPath)
        {
            //var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            //var backupPath = Path.Combine(destinationFolder, $"GameTracker_Backup_{timestamp}.zip");

            using var zip = ZipFile.Open(backupPath, ZipArchiveMode.Create);
            
            // close database connection so we can back it up
            await _db.Database.CloseConnectionAsync();
            SqliteConnection.ClearAllPools();

            zip.CreateEntryFromFile(_appPaths.DatabasePath, "GameTracker.db");

            await _db.Database.OpenConnectionAsync();

            foreach (var file in Directory.GetFiles(_appPaths.ImagesDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(_appPaths.ImagesDirectory, file);
                zip.CreateEntryFromFile(file, Path.Combine("Images", relativePath));
            }

            return backupPath;
        }

        public async Task RestoreBackupAsync(string backupZipPath)
        {
            // 1 validate the zip has what we expect before touching anything
            using (var validationZip = ZipFile.OpenRead(backupZipPath))
            {
                if (validationZip.GetEntry("GameTracker.db") == null)
                    throw new InvalidOperationException("Backup file is missing the database file aborting restore...");
            }

            // 2. Close EF Core's connection and clear the underlying SQLite connection pool.
            //    Both steps matter: closing the DbContext's connection isn't enough on its own —
            //    SQLite's ADO.NET provider pools connections, so a pooled-but-"closed" connection
            //    can still hold the file lock until the pool itself is cleared.
            await _db.Database.CloseConnectionAsync();
            SqliteConnection.ClearAllPools();

            // 3. Extract to a temp folder first, that way if something failes the original data is untouched
            var stagingDir = Path.Combine(Path.GetTempPath(), $"GameTrackerRestore_{Guid.NewGuid}");
            Directory.CreateDirectory(stagingDir);

            try
            {
                ZipFile.ExtractToDirectory(backupZipPath, stagingDir);

                var stagedDbPath = Path.Combine(stagingDir, "GameTracker.db");
                var stagedImagesPath = Path.Combine(stagingDir, "Images");

                // 4. replace the live database file
                File.Copy(stagedDbPath, _appPaths.DatabasePath, overwrite: true);

                // 5. Replace the live images folder, clear it first to remove any
                //    images that are not linked
                if (Directory.Exists(_appPaths.ImagesDirectory))
                {
                    Directory.Delete(_appPaths.ImagesDirectory, recursive: true);
                }

                if (Directory.Exists(stagedImagesPath))
                {
                    CopyDirectory(stagedImagesPath, _appPaths.ImagesDirectory);
                }
                else
                {
                    Directory.CreateDirectory(_appPaths.ImagesDirectory);
                }
            }
            finally
            {
                // 6. Clean up the staging directory
                if (Directory.Exists(stagingDir))
                {
                    Directory.Delete(stagingDir, recursive: true);
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                var destPath = Path.Combine(destDir, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                File.Copy(file, destPath, overwrite: true);
            }
        }
    }
}
