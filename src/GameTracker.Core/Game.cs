using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTracker.Core
{
    public class Game
    {
        public int ID { get; set; }
        
        public string Title { get; set; } = string.Empty;

        public GameCategory Category { get; set; }

        public string? Platform { get; set; } 

        public string? Publisher { get; set; }

        public int? MinPlayers { get; set; }

        public int? MaxPlayers { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public string? Notes { get; set; }

        public bool Played { get; set; } = false;

        public bool Completed { get; set; } = false;

        // where is the game from (Steam, Epic, Gog, physical copy, etc.)
        public string PurchasedFrom { get; set; } = string.Empty;

        public bool PhysicalCopy { get; set; } = false;

        // relative paths only - resolved against the app's data folder at runtime
        public string? IconPath { get; set; }

        public string? CoverImagePath { get; set; }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
