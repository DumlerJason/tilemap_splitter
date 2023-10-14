# tilemap_splitter
I purchased or donated to some people on Itch.Io to get some pixel art.

https://hyohnoo.itch.io/controllers-buttons

https://aamatniekss.itch.io/free-pixelart-tileset-cute-forest

https://jamiebrownhill.itch.io/solaria-sprites

https://jamiebrownhill.itch.io/solaria-enemy-pack

but I'm not a pixel artist, and I'm lazy. I needed some of the sprites to be bigger and I needed the controller buttons to be in separate files instead of one big tile map.  This repo is the result.


## Platform:
* Because it uses the Bitmap class it will only run on Windows.
* Written in .Net Core 6 LTS using Visual Studio 2022 Community Edition.
* I did this in Windows 11. Windows 10 should be fine, but YMMV.

## Requirements:
* Windows 10 or Windows 11
* Visual Studio Community Edition; 2022 preferred
* Newtonsoft Json Nuget package.
* System.Drawing.Common Nuget package.

## Projects:
* tile_map_lib: library that does the work.
* tilemap_splitter: command line application front end for tile_map_lib.

## Usage:
Compile the application in Visual Studio.  Except for the tile maps everything should be free and accessible through Visual Studio Community Edition.

If you run the application in debug mode you'll need to edit the command line parameters that are passed to it.

Run the resulting application from the command line with the "--help" parameter to see what parameters are required on the command line.
Reference the tile_map_lib in your application and use it directly.

## Some Notes:
Shrinking a tilemap will result the underlying art changing.  Going from 64x64 tiles to 16x16 tiles will necessarily result in the loss of some information somewhere.

The grow operations should be "pixel perfect".  i.e. if growing a bitmap from 16x16 to 64x64, each pixel should now be 4x4 without any sort of blurring.  This is also true with shrink operations, but in that example pixel art starting at 64x64 isn't likely using a brush size of 4x4 so there will be some loss of information and the shrunken pixel art will not be "pixel perfect".

When splitting a tile map, you can provide a names file using the --names-file parameter. The file is a JSON formatted file like so:
```json
{
    "names_list": [
        "name1",
        "name2"
    ],
    "names_map": {
        {"name": "name1", "x": 0, "y": 0},
        {"name": "name2", "x": 1, "y": 0}
    }
}
```

If you use a names file, provide either a names list or a names map, the library isn't smart enough to work with both and it will opt to use the names list if both are provided.  

The splitter will start at the top left of the tile map and work its way left, then down. As it encounters a tile, the saved tile will use the name from the names_list if provided. If you provide a names map, if the file is at the x, y location specified, it will use the name provided.  All tiles are prefixed with source file name_row_col so that if a name isn't found the resulting file can be identified.

In the names file, x, y coordinates are tile map coordinates, not pixel coordinates.

To save empty tiles, provide the --save-empty-tiles parameter.

When scaling a tile map file, it can be grown between 2 to 8 times its current size.  A tile map file cannot be shrunk past 16x16 pixels.
