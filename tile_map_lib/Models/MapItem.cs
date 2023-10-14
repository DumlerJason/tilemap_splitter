using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tile_map_lib.Models
{
    public class MapItem
    {
        public string name { get; set; } = string.Empty;
        public uint x { get; set; } = 0;
        public uint y { get; set; } = 0;
    }
}
