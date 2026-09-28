using GameTracker.Core.Import;

namespace GameTracker.Data.Import
{
    public interface IGameImporter
    {
        ImportFormat Format { get; }

        Task ImportGamesAsync(IEnumerable<ImportRecord> games, string parentPath, IProgress<(int current, int total)>? progress = null);
        
        Task<IEnumerable<ImportRecord>> ReadImportFileAsync(string InputPath);
    }
}
