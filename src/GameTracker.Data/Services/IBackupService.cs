namespace GameTracker.Data.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync(string backupPath);

        Task RestoreBackupAsync(string backupZipPath);
    }
}
