namespace GameTracker.Core.Export
{
    public class ExportRecord
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set;  } = string.Empty;
        public string? Platform { get; set; }
        public string? Publisher { get; set; }
        public string? Players { get; set; }
        public string? ReleaseDate { get; set; }
        public bool Played { get; set; }
        public string? Notes { get; set; }
        public string Tags { get; set; } = string.Empty;
    }
}
