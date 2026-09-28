namespace GameTracker.Core.Import
{
    public class SteamImportRecord : ImportRecord
    {
        public int appid { get; set; }
        public string name { get; set; } = string.Empty;
        public string release_date { get; set; } = string.Empty;
        public string developer { get; set; } = string.Empty;
        public string publisher { get; set; } = string.Empty;
        public string platforms { get; set; } = string.Empty;
        public string genres { get; set; } = string.Empty;
        public string categories { get; set; } = string.Empty;
        public string tags { get; set; } = string.Empty;
        public string min_players { get; set; } = string.Empty;
        public string max_players { get; set; } = string.Empty;
        public int playtime_minutes { get; set; }
        public int playtime_2weeks { get; set; }
        public int last_played { get; set; }
        public string icon { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public string header { get; set; } = string.Empty;
        public string capsule { get; set; } = string.Empty;
        public string library_art { get; set; } = string.Empty;
    }
}
