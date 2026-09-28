namespace GameTracker.Core.Import
{
    public class GogImportRecord : ImportRecord
    {
        public int gog_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string release_date { get; set; } = string.Empty;
        public string developer { get; set; } = string.Empty;
        public string publisher { get; set; } = string.Empty;
        public string platforms { get; set; } = string.Empty;
        public string genres { get; set; } = string.Empty;
        public string tags { get; set; } = string.Empty;
        public string languages { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string store_url { get; set; } = string.Empty;
        public string game_type { get; set; } = string.Empty;
        public bool is_pre_order { get; set; }
        public string icon { get; set; } = string.Empty;
        public string logo { get; set; } = string.Empty;
        public string background { get; set; } = string.Empty;
        public string sidebar_icon { get; set; } = string.Empty;
        public string screenshots { get; set; } = string.Empty;
        public string dlcs { get; set; } = string.Empty;
        public string installers { get; set; } = string.Empty;
        public string min_players { get; set; } = string.Empty;
        public string max_players { get; set; } = string.Empty;
    }
}
