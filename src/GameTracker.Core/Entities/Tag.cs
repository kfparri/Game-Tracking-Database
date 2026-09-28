using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTracker.Core.Entities
{
    public class Tag
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    }
}
