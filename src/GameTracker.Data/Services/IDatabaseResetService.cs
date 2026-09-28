namespace GameTracker.Data.Services
{
    public interface IDatabaseResetService
    {
        Task ResetDatabaseAsync(bool createSafetyBackup = true);
    }
}
