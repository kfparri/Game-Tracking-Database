using CsvHelper;
using GameTracker.Core.Export;
using System.Globalization;

namespace GameTracker.Data.Export
{
    public class CsvGameExporter : IGameExporter
    {
        public ExportFormat Format => ExportFormat.Csv;

        public async Task ExportAsync(IEnumerable<ExportRecord> games, string outputPath)
        {
            using var writer = new StreamWriter(outputPath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(games);
        }
    }
}
