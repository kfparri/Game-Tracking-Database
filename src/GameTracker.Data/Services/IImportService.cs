using GameTracker.Core.Import;

namespace GameTracker.Data.Services
{
    public interface IImportService
    {
        Task<IEnumerable<ImportRecord>> ReadImportFileAsync(ImportFormat format, string filePath);

        Task ImportGameAsync(ImportFormat format, IEnumerable<ImportRecord> games, string ImportPath, IProgress<(int current, int total)>? progress = null);
         
        //Task ImportAsync(ImportFormat format, string ImportPath);
    }
}
