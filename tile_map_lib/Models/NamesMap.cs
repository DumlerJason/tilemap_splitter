using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tile_map_lib.Models
{
    public class NamesMap
    {
        #region Instance Properties

        /// <summary>
        /// If the map is empty, just read from the list of names.
        /// Tilemap will be processed from the top left, working left to right, 
        /// then down and the names will be pulled from this list as they are
        /// reached.
        /// </summary>
        public List<string> names_list { get; set; } = new();

        /// <summary>
        /// A map to the tile names by x,y coordinates.
        /// </summary>
        public List<MapItem> names_map { get; set; } = new();

        #endregion Instance Properties




        #region Static Methods

        /// <summary>
        /// Read a buttons map file and return the ButtonsMap data.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ReadMapFile(string filename, out NamesMap buttonsMap, out string message)
        {
            message = string.Empty;
            buttonsMap = new();

            if (!File.Exists(filename))
            {
                message = Constants.FILE_NOT_FOUND;
                return false;
            }

            try
            {
                string json = File.ReadAllText(filename);
                NamesMap? buttonsMapBuffer = JsonConvert.DeserializeObject<NamesMap>(json);
                if (buttonsMapBuffer == null)
                {
                    message = Constants.FILE_INVALID;
                    return false;
                }

                buttonsMap = (NamesMap)buttonsMapBuffer;
                return true;
            }
            catch(Exception e)
            {
                message = e.Message;
                return false;
            }
        }

        #endregion Static Methods




        #region Instance Methods

        public string GetMapItemName(uint row, uint col, uint item_index)
        {
            if (names_map != null && names_map.Count > 0)
            {
                foreach(MapItem map_item in names_map)
                {
                    if (map_item.x == col && map_item.y == row) return map_item.name;
                }
            }
            else if (names_list != null && names_list.Count > 0 && item_index < names_list.Count)
            {
                return names_list[(int)item_index];
            }

            return string.Empty;
        }

        #endregion Instance Methods
    
    }
}
