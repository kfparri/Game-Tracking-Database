using GameTracker.Core.Export;

namespace GameTracker.Data.Services
{
    public interface IExportService
    {
        Task ExportAsync(ExportFormat format, string outputPath);
    }
}
