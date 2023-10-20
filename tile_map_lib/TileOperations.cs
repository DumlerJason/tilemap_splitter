using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using tile_map_lib.Models;

namespace tile_map_lib
{
    public class TileOperations
    {
        // This won't work if there are more than 9999 rows or columns.
        // source file name _ column _ row
        static string Default_Tile_Name_Template { get; } = "{0}_{1:0000}_{2:0000}";

        /// <summary>
        /// You might be thinking this is ridiculous.
        /// However, if you don't explicitly test a variable for == null, Visual Studio gets cranky.
        /// Using string.IsNullOrEmpty does not seem to be a valid null check.
        /// Sometimes VS still complains, but it complains less.
        /// </summary>
        /// <param name="test_string"></param>
        /// <returns></returns>
        static bool StringIsNullOrEmpty(string? test_string)
        {
            return (test_string == null || string.IsNullOrEmpty(test_string));
        }

        static Size GetTileMapSize(int image_count, int rows, int cols, out string message)
        {
            message = string.Empty;
            try
            {
                if (rows < 0) rows = 0;
                if (cols < 0) cols = 0;

                int tile_rows = 0;
                int tile_cols = 0;
                if (rows == 0 && cols == 0)
                {
                    // Figure out a squarish tileset.
                    double square_root = Math.Sqrt(image_count);
                    tile_rows = (int)Math.Floor(square_root);
                    if (square_root % 1.0 > 0.0) tile_cols = tile_rows + 1;
                    else tile_cols = tile_rows;
                }
                else if (rows == 0)
                {
                    // A single column of tiles.
                    tile_rows = image_count;
                    tile_cols = 1;
                }
                else if (cols == 0)
                {
                    // A single row of tiles.
                    tile_cols = image_count;
                    tile_rows = 1;
                }
                else
                {
                    if (rows * cols < image_count)
                    {
                        // Not enough space allocated, add columns.
                        double dividend = image_count / rows;
                        if (dividend % 1.0 > 0) dividend += 1;
                        tile_rows = (int)rows;
                        tile_cols = (int)Math.Floor(dividend);
                    }
                    else
                    {
                        // Go with whatever they provided.
                        tile_rows = (int)rows;
                        tile_cols = (int)cols;
                    }
                }

                return new Size(tile_cols, tile_rows);
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                return Size.Empty;
            }
        }

        /// <summary>
        /// Sample the images in a directory and get the maximum size in width and height of the images
        /// as well as the names of the images that could be opened.
        /// Does not recurse sub-directories.
        /// </summary>
        /// <param name="directory_name"></param>
        /// <param name="size">Maximum image size in width and height.</param>
        /// <param name="image_files">List of image files that could be opened.</param>
        /// <param name="message"></param>
        /// <returns></returns>
        static bool SampleImages(string directory_name, out Size size, out List<string> image_files, out string message)
        {
            message = string.Empty;
            size = Size.Empty;
            image_files = new List<string>();

            try
            {
                if (StringIsNullOrEmpty(directory_name))
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                DirectoryInfo di = new DirectoryInfo(directory_name);
                if (!di.Exists)
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                List<string> filters = new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.gif" };
                foreach(string filter in filters)
                {
                    List<FileInfo> file_info_list = di.GetFiles(filter).ToList();
                    image_files.AddRange(file_info_list.Select(fi => fi.FullName));
                }

                int width = 0;
                int height = 0;

                List<string> bad_images = new();

                foreach(string image_file in image_files)
                {
                    try
                    {
                        using Image image = Image.FromFile(image_file);
                        width = Math.Max(width, image.Width);
                        height = Math.Max(height, image.Height);
                    }
                    catch
                    {
                        // A file wouldn't open, and that's OK.
                        bad_images.Add(image_file);
                    }
                }

                int removed = image_files.RemoveAll(image_file => bad_images.Contains(image_file));

                if (removed != bad_images.Count)
                {
                    // Honestly not sure what to do here.
                }

                size = new Size(width, height);
                return true;
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                size = Size.Empty;
                image_files = new List<string>();
                SimpleLog.ToConsole(ex.ToString(), true);
                return false;
            }
        }

        public static bool Enbiggen(string source_file, string target_file, uint scale, out string message)
        {
            message = string.Empty;

            try
            {
                if (StringIsNullOrEmpty(source_file))
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                // If the target file isn't provided a name, provide one.
                if (StringIsNullOrEmpty(target_file))
                {
                    target_file = $"{Path.GetFileNameWithoutExtension(source_file)}_Enbiggen{Path.GetExtension(source_file)}";
                }

                // Scale is kept rational for pixel art.
                if (scale < 2 || scale > 8)
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                if (!File.Exists(source_file))
                {
                    message = Constants.FILE_NOT_FOUND;
                    return false;
                }

                // If the target file wasn't given a full path, put it in the same directory as the source file.
                string source_directory = Path.GetDirectoryName(source_file) ?? "";
                string target_directory = Path.GetDirectoryName(target_file) ?? "";
                string target_filename = string.Empty;
                if (StringIsNullOrEmpty(target_directory) && !StringIsNullOrEmpty(source_directory))
                {
                    target_directory = source_directory;
                    target_filename = Path.Combine(target_directory, target_file);
                }
                else
                {
                    target_filename = target_file;
                }

                using Bitmap source_bitmap = (Bitmap)Bitmap.FromFile(source_file);
                if (source_bitmap == null)
                {
                    message = $"Could not open bitmap file.";
                    return false;
                }

                uint new_width = (uint)source_bitmap.Width * scale;
                uint new_height = (uint)source_bitmap.Height * scale;

                using Bitmap target_bitmap = new ((int)new_width, (int)new_height);
                using Graphics target_graphics = Graphics.FromImage(target_bitmap);
                target_graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                target_graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                target_graphics.DrawImage(source_bitmap, 0, 0, new_width, new_height);

                if (File.Exists(target_filename)) File.Delete(target_filename);
                target_bitmap.Save(target_filename);

                return File.Exists(target_filename);
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                SimpleLog.ToConsole(ex.ToString(), true);
                return false;
            }
        }

        public static bool Ensmallen(string source_file, string target_file, uint scale, out string message)
        {
            message = string.Empty;
            try
            {
                if (StringIsNullOrEmpty(source_file))
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                // If the target file isn't provided a name, provide one.
                if (StringIsNullOrEmpty(target_file))
                {
                    target_file = $"{Path.GetFileNameWithoutExtension(source_file)}_Ensmallen{Path.GetExtension(source_file)}";
                }

                // Scale is kept rational for pixel art.
                if (scale < 2 || scale > 8)
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                if (!File.Exists(source_file))
                {
                    message = Constants.FILE_NOT_FOUND;
                    return false;
                }

                // If the target file wasn't given a full path, put it in the same directory as the source file.
                string source_directory = Path.GetDirectoryName(source_file) ?? "";
                string target_directory = Path.GetDirectoryName(target_file) ?? "";
                string target_filename = string.Empty;
                if (StringIsNullOrEmpty(target_directory) && !StringIsNullOrEmpty(source_directory))
                {
                    target_directory = source_directory;
                    target_filename = Path.Combine(target_directory, target_file);
                }

                using Bitmap source_bitmap = (Bitmap)Bitmap.FromFile(source_file);
                if (source_bitmap == null)
                {
                    message = $"Could not open bitmap file.";
                    return false;
                }

                // We can't have a remainder of pixels left over.
                if (source_bitmap.Width % scale > 0 || source_bitmap.Height % scale > 0)
                {
                    message = $"Unable to ensmallen image cleanly.";
                    return false;
                }

                uint new_width = (uint)source_bitmap.Width / scale;
                uint new_height = (uint)source_bitmap.Height / scale;

                if (new_width < 16 || new_height < 16)
                {
                    message = "Resulting bitmap would be too small.";
                    return false;
                }

                using Bitmap target_bitmap = new Bitmap((int)new_width, (int)new_height);
                using Graphics target_graphics = Graphics.FromImage(target_bitmap);
                target_graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                target_graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                target_graphics.DrawImage(source_bitmap, 0, 0, new_width, new_height);

                if (File.Exists(target_filename)) File.Delete(target_filename);
                target_bitmap.Save(target_filename);
                return File.Exists(target_filename);
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                SimpleLog.ToConsole(ex.ToString(), true);
                return false;
            }

        }

        public static bool Split(string source_file, string target_directory, string names_file, Size tile_size, Point offset, uint alt_row_start, bool save_empty_tiles, out string message)
        {
            message = string.Empty;

            try
            {
                if (StringIsNullOrEmpty(source_file) || !File.Exists(source_file))
                {
                    message = Constants.FILE_NOT_FOUND;
                    return false;
                }

                // Used in target file naming if we get a tile that isn't referenced in the tile name map.
                string source_file_base = Path.GetFileNameWithoutExtension(source_file);

                string source_directory = Path.GetDirectoryName(source_file) ?? "";
                if (!StringIsNullOrEmpty(source_directory))
                {
                    if (!Directory.Exists(source_directory))
                    {
                        message = Constants.INVALID_DIRECTORY;
                        return false;
                    }
                }

                if (StringIsNullOrEmpty(target_directory))
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                if (!Directory.Exists(target_directory)) Directory.CreateDirectory(target_directory);
                if (!Directory.Exists(target_directory))
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                if (StringIsNullOrEmpty(names_file) || !File.Exists(names_file))
                {
                    message = Constants.INVALID_NAME_MAP;
                    return false;
                }

                bool map_loaded = NamesMap.ReadMapFile(names_file, out NamesMap map, out message);
                if (!map_loaded)
                {
                    if (StringIsNullOrEmpty(message))
                    {
                        message = Constants.INVALID_NAME_MAP;
                    }
                    return false;
                }

                if (!File.Exists(source_file))
                {
                    message = Constants.FILE_NOT_FOUND;
                    return false;
                }

                using Bitmap source_bitmap = (Bitmap)Bitmap.FromFile(source_file);

                if (source_bitmap == null)
                {
                    message = "Could not read source bitmap.";
                    return false;
                }

                uint bitmap_width = (uint)source_bitmap.Width;
                uint bitmap_height = (uint)source_bitmap.Height;

                SimpleLog.ToConsole($"[Splitter.Split] Source Bitmap Width:  {bitmap_width}");
                SimpleLog.ToConsole($"[Splitter.Split] Source Bitmap Height: {bitmap_height}");

                if (bitmap_width == 0 || bitmap_height == 0)
                {
                    message = "Invalid bitmap width or height.";
                    return false;
                }

                // Total columns and rows of tilesize in the bitmap.
                uint tilemap_cols = (uint)Math.Floor((double)(bitmap_width - offset.X) / (double)tile_size.Width);
                uint tilemap_rows = (uint)Math.Floor((double)(bitmap_height - offset.Y) / (double)tile_size.Height);

                SimpleLog.ToConsole($"[Splitter.Split] Tilemap Columns: {tilemap_cols}");
                SimpleLog.ToConsole($"[Splitter.Split] Tilemap Rows:    {tilemap_rows}");

                if (tilemap_cols == 0 || tilemap_rows == 0)
                {
                    message = "Invalid bitmap tilemap rows or columns.";
                    return false;
                }

                using Graphics graphics = Graphics.FromImage(source_bitmap);
                uint element_name_index = 0;

                for (uint tilemap_col_index = 0; tilemap_col_index < tilemap_cols; tilemap_col_index++)
                {
                    for (uint tilemap_row_index = 0; tilemap_row_index < tilemap_rows; tilemap_row_index++)
                    {
                        string tilemap_item_name = string.Format(Default_Tile_Name_Template, source_file_base, tilemap_col_index, tilemap_row_index);
                        if (tilemap_row_index >= alt_row_start && alt_row_start > 0)
                        {
                            string suffix = map.GetMapItemName((uint)tilemap_row_index - (uint)alt_row_start, (uint)tilemap_col_index, (uint)element_name_index);
                            if (!StringIsNullOrEmpty(suffix)) tilemap_item_name = $"{tilemap_item_name}_{suffix}";
                        }
                        else
                        {
                            string suffix = map.GetMapItemName((uint)tilemap_row_index, (uint)tilemap_col_index, (uint)element_name_index);
                            if (!StringIsNullOrEmpty(suffix)) tilemap_item_name = $"{tilemap_item_name}_{suffix}";
                        }

                        // This is the target filename.
                        string tilemap_item_filename = Path.Combine(target_directory, $"{tilemap_item_name}.png");
                        if (File.Exists(tilemap_item_filename)) File.Delete(tilemap_item_filename);

                        int tile_width = tile_size.Width;
                        int tile_height = tile_size.Height;

                        // Ugh. This feels like it won't actually get disposed or something. :-(
                        using Bitmap map_item_bitmap = new Bitmap(tile_width, tile_height);
                        using Graphics map_item_graphics = Graphics.FromImage(map_item_bitmap);
                        map_item_graphics.Clear(Color.Transparent);
                        map_item_graphics.DrawImage(source_bitmap, new Rectangle(0, 0, tile_width, tile_height),
                            tilemap_col_index * tile_width, tilemap_row_index * tile_height, tile_width, tile_height,
                            GraphicsUnit.Pixel);

                        bool has_color = false;
                        if (!save_empty_tiles)
                        {
                            for (int x = 0; x < tile_width; x++)
                            {
                                for (int y = 0; y < tile_height; y++)
                                {
                                    if (map_item_bitmap.GetPixel(x, y).A > 0)
                                    {
                                        has_color = true;
                                        break;
                                    }
                                }
                                if (has_color) break;
                            }
                        }
                        if (!has_color && !save_empty_tiles)
                        {
                            SimpleLog.ToConsole($"{tilemap_item_name} is an empty file. Ignoring empty tiles.");
                            continue;
                        }
                        SimpleLog.ToConsole($"Saving {tilemap_item_name}.");
                        map_item_bitmap.Save(tilemap_item_filename, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                SimpleLog.ToConsole(ex.ToString(), true);
                return false;
            }
        }

        public static bool Join(string source_directory, string target_file, int cols, int rows, out string message)
        {
            message = string.Empty;
            try
            {
                if (StringIsNullOrEmpty(source_directory))
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                if (StringIsNullOrEmpty(target_file))
                {
                    message = Constants.INVALID_ARGUMENT;
                    return false;
                }

                if (!Directory.Exists(source_directory))
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                string target_directory = Path.GetDirectoryName(target_file)??"";
                if (StringIsNullOrEmpty(target_directory))
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                if (!Directory.Exists(target_directory)) Directory.CreateDirectory(target_directory);
                if (!Directory.Exists(target_directory))
                {
                    message = Constants.INVALID_DIRECTORY;
                    return false;
                }

                if (rows < 0) rows = 0;
                if (cols < 0) cols = 0;

                Size tile_size = Size.Empty;
                List<string> image_files = new();

                bool sampled = SampleImages(source_directory, out tile_size, out image_files, out message);
                if (!sampled)
                {
                    if (string.IsNullOrEmpty(message)) message = $"Could not sample source directory images.";
                    else message = $"Could not sample source directory images.\r\nMessage: '{message}'";
                    return false;
                }

                int image_count = image_files.Count;
                Size tile_map_size = GetTileMapSize(image_count, rows, cols, out message);
                if (tile_map_size.IsEmpty)
                {
                    if (string.IsNullOrEmpty(message)) message = $"Could not determine tile map size.";
                    else message = $"Could not determine tile map size.\r\nMessage: '{message}'";
                    return false;
                }

                Size image_size = new (tile_map_size.Width * tile_size.Width, tile_map_size.Height * tile_size.Height);
                using Bitmap tile_map_image = new Bitmap(image_size.Width, image_size.Height);
                using Graphics tile_map_graphics = Graphics.FromImage(tile_map_image);
                tile_map_graphics.Clear(Color.Transparent);
                int current_x = 0;
                int current_y = 0;
                foreach(string image_file in image_files)
                {
                    using Bitmap tile_image = (Bitmap)Image.FromFile(image_file);
                    tile_map_graphics.DrawImage(tile_image, new Point(current_x, current_y));

                    // Tiles are drawn starting at the top left, filling each row.
                    current_x += tile_size.Width;
                    if (current_x > (image_size.Width - tile_size.Width))
                    {
                        current_x = 0;
                        current_y += tile_size.Height;
                    }
                }

                tile_map_image.Save(target_file);
                return File.Exists(target_file);
            }
            catch(Exception ex)
            {
                message = $"Exception: '{ex.Message}'";
                SimpleLog.ToConsole(ex.ToString(), true);
                return false;
            }
        }

    }
}
