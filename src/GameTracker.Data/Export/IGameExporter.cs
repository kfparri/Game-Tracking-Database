using GameTracker.Core.Export;

namespace GameTracker.Data.Export
{
    public interface IGameExporter
    {
        ExportFormat Format { get; }
        Task ExportAsync(IEnumerable<ExportRecord> games, string outputPath);
    }
}
