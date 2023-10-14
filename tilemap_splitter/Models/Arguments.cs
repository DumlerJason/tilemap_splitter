using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_splitter.Models
{
    public class Arguments
    {

        public OperatingMode OperatingMode { get; set; } = OperatingMode.None;

        public string OperatingModeString
        {
            get
            {
                return OperatingMode.ToString();
            }
        }

        public string NamesFile { get; set; } = string.Empty;
        
        public string SourceFile { get; set; } = string.Empty;

        public string SourceDirectory { get; set; } = string.Empty;

        public string TargetFile { get; set; } = string.Empty;

        public string TargetDirectory { get; set; } = string.Empty;

        public System.Drawing.Size TileSize { get; set; } = System.Drawing.Size.Empty;

        public System.Drawing.Size TileMapSize { get; set; } = System.Drawing.Size.Empty;

        public System.Drawing.Point Offset { get; set; } = System.Drawing.Point.Empty;

        public uint Scale { get; set; } = 0;

        public uint AltRowStart { get; set; } = 0;

        public bool SaveEmptyTiles { get; set; } = false;

        public bool Test { get; set; } = false;

        public bool Help { get; set; } = false;

        int ParseInt(string int_in, int default_int)
        {
            if (!int.TryParse(int_in, out int result)) return default_int;
            return result;
        }

        /// <summary>
        /// Parse the arguments from the command line.
        /// Arguments will start with -- and may include an equal sign.
        /// </summary>
        /// <param name="args"></param>
        public void ParseArguments(string[] args)
        {
            foreach(string arg in args)
            {
                if (arg.Contains('='))
                {
                    string[] arg_key_value = arg.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (arg_key_value.Length != 2) continue;
                    string key = arg_key_value[0].Replace("_", "").Replace("-", "").ToLower();
                    string val = arg_key_value[1].ToLower();
                    if (key == "mode" || key == "operatingmode")
                    {
                        if (val == "split" || val == "disassemble") OperatingMode = OperatingMode.Split;
                        else if (val == "enbiggen" || val == "grow" || val == "enlarge") OperatingMode = OperatingMode.Enbiggen;
                        else if (val == "ensmallen" || val == "shrink" || val == "reduce") OperatingMode = OperatingMode.Ensmallen;
                        else if (val == "join" || val == "combine") OperatingMode = OperatingMode.Join;
                        else OperatingMode = OperatingMode.None;
                    }
                    else if (key == "namesfile" || key == "namefile")
                    {
                        NamesFile = val;
                    }
                    else if (key == "sourcefile" || key == "source")
                    {
                        SourceFile = val;
                    }
                    else if (key == "sourcedirectory")
                    {
                        SourceDirectory = val;
                    }
                    else if (key == "targetfile" || key == "target")
                    {
                        TargetFile = val;
                    }
                    else if (key == "targetdirectory")
                    {
                        TargetDirectory = val;
                    }
                    else if (key == "tilesize")
                    {
                        // Tile size is formatted as WxH
                        string[] tile_size_parts = val.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (tile_size_parts.Length != 2) continue;
                        string w = tile_size_parts[0];
                        string h = tile_size_parts[1];

                        TileSize = new System.Drawing.Size(ParseInt(w, 0), ParseInt(h, 0));
                        if (TileSize.Width == 0 || TileSize.Height == 0) TileSize = System.Drawing.Size.Empty;
                    }
                    else if (key == "tilemapsize")
                    {
                        // Tile map size is formatted as WxH
                        string[] tile_map_size_parts = val.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (tile_map_size_parts.Length != 2) continue;
                        string w = tile_map_size_parts[0];
                        string h = tile_map_size_parts[1];

                        // Tile map size can be assigned 0x0 as a size.
                        TileMapSize = new System.Drawing.Size(ParseInt(w, 0), ParseInt(h, 0));
                    }
                    else if (key == "offset")
                    {
                        // Offset is formatted as XxY.
                        string[] offset_parts = val.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (offset_parts.Length != 2) continue;
                        string x = offset_parts[0];
                        string y = offset_parts[1];

                        Offset = new System.Drawing.Point(ParseInt(x, 0), ParseInt(y, 0));
                    }
                    else if (key == "scale")
                    {
                        Scale = (uint)ParseInt(val, 0);
                    }
                    else if (key == "altrowstart")
                    {
                        AltRowStart = (uint)ParseInt(val, 0);
                    }
                }
                else
                {
                    if (arg.Replace("_", "").Replace("-", "") == "saveemptytiles")
                    {
                        SaveEmptyTiles = true;
                    }
                    else if (arg.Replace("_", "").Replace("-", "") == "test")
                    {
                        Test = true;
                    }
                    else if (arg.Replace("_", "").Replace("-", "") == "help")
                    {
                        Help = true;
                    }
                }
            }
        }

    }
}
