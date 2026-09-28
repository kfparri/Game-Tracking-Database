using GameTracker.Core.Import;
using GameTracker.Data.Import;

namespace GameTracker.Data.Services
{
    public class ImportService : IImportService
    {
        private readonly GameTrackerContext _db;
        private readonly IEnumerable<IGameImporter> _importers;
        
        public ImportService(GameTrackerContext db,
                             IEnumerable<IGameImporter> importers)
        {
            _db = db;
            _importers = importers;
        }

        public async Task ImportAsync(ImportFormat format, string ImportPath, IProgress<(int current, int total)> progress)
        {
            var importer = _importers.FirstOrDefault(i => i.Format == format)
                ?? throw new NotSupportedException($"No importer registered for {format}");

            var records = await importer.ReadImportFileAsync(ImportPath);

            await importer.ImportGamesAsync(records, Path.GetDirectoryName(ImportPath), progress);
                
        }        

        public async Task ImportGameAsync(ImportFormat format, IEnumerable<ImportRecord> games, string ImportPath, IProgress<(int current, int total)>? progress = null)
        {
            var importer = _importers.FirstOrDefault(i => i.Format == format)
                ?? throw new NotSupportedException($"No importer registered for {format}");

            await importer.ImportGamesAsync(games, ImportPath, progress);
        }

        public async Task<IEnumerable<ImportRecord>> ReadImportFileAsync(ImportFormat format, string filePath)
        {
            var importer = _importers.FirstOrDefault(i => i.Format == format)
                ?? throw new NotSupportedException($"No importer registered for {format}");

            return await importer.ReadImportFileAsync(filePath);
        }
    }
}
