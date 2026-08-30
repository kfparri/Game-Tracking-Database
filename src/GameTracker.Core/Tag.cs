using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTracker.Core
{
    public class Tag
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
