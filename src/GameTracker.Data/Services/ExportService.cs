using GameTracker.Core.Export;
using GameTracker.Data.Export;
using Microsoft.EntityFrameworkCore;

namespace GameTracker.Data.Services
{
    public class ExportService : IExportService
    {
        private readonly GameTrackerContext _db;
        private readonly IEnumerable<IGameExporter> _exporters;

        public ExportService(GameTrackerContext db, 
                             IEnumerable<IGameExporter> exporters)
        {
            _db = db;
            _exporters = exporters;
        }

        public async Task ExportAsync(ExportFormat format, string outputPath)
        {
            var exporter = _exporters.FirstOrDefault(e => e.Format == format)
                ?? throw new NotSupportedException($"No exporter registered for {format}");

            var games = await _db.Games
                .Include(g => g.GameTags)
                .ThenInclude(gt => gt.Tag)
                .AsNoTracking()
                .ToListAsync();

            var records = games.Select(g => new ExportRecord
            {
                Title = g.Title,
                Category = g.GameType.ToString(),
                Platform = g.Platform,
                Publisher = g.Publisher,
                Players = (g.MinPlayers, g.MaxPlayers) switch
                {
                    (null, null) => null,
                    var (min, max) when min == max => $"{min}",
                    var (min, max) => $"{min}-{max}"
                },
                ReleaseDate = g.ReleaseDate?.ToString("yyyy-MM-dd"),
                Played = g.Played,
                Notes = g.Notes,
                Tags = string.Join(", ", g.GameTags.Select(gt => gt.Tag.Name))
            }).ToList();

            await exporter.ExportAsync(records, outputPath);
        }
    }
}
