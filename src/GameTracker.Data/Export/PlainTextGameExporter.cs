using GameTracker.Core.Export;

namespace GameTracker.Data.Export
{
    public class PlainTextGameExporter : IGameExporter
    {
        public ExportFormat Format => ExportFormat.PlainText;

        public async Task ExportAsync(IEnumerable<ExportRecord> games, string outputPath)
        {
            using var writer = new StreamWriter(outputPath);
            foreach (var g in games)
            {
                await writer.WriteLineAsync($"Title: {g.Title}");
                await writer.WriteLineAsync($"Category: {g.Category}");
                if (g.Platform is not null) await writer.WriteLineAsync($"Platform: {g.Platform}");
                if (g.Publisher is not null) await writer.WriteLineAsync($"Publisher: {g.Publisher}");
                if (g.Players is not null) await writer.WriteLineAsync($"Players: {g.Players}");
                if (g.ReleaseDate is not null) await writer.WriteLineAsync($"Released: {g.ReleaseDate}");
                await writer.WriteLineAsync($"Played: {g.Played}");
                if (!string.IsNullOrEmpty(g.Tags)) await writer.WriteLineAsync($"Tags: {g.Tags}");
                if (!string.IsNullOrEmpty(g.Notes)) await writer.WriteLineAsync($"Notes: {g.Notes}");
                await writer.WriteLineAsync(new string('-', 40));
            }
        }
    }
}
