// See https://aka.ms/new-console-template for more information
using System.Drawing;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using tile_map_lib;
using tile_map_lib.Models;
using tilemap_splitter.Models;

void PrintHelp()
{
    Console.WriteLine($"tilemap_splitter.exe [arguments]");
    Console.WriteLine($"Arguments:");
    Console.WriteLine("  --mode={mode; Enbiggen, Ensmallen, Split, Join}");
    Console.WriteLine("");
    Console.WriteLine("  --mode=Enbiggen|Grow|Enlarge|Ensmallen|Shrink|Reduce");
    Console.WriteLine("  --source_file={source file name}");
    Console.WriteLine("  --target_file={target file name}");
    Console.WriteLine("  --scale={enbiggen or ensmallen scale}");
    Console.WriteLine("");
    Console.WriteLine(" --mode=Split|Disassemble");
    Console.WriteLine("  --source_file={source file name}");
    Console.WriteLine("  --target_directory={target directory}");
    Console.WriteLine("  --names_file={names file}");
    Console.WriteLine("  --tile-size=WxH");
    Console.WriteLine("  --ignore-empty-tiles");
    Console.WriteLine("  --offset=XxY");
    Console.WriteLine("  --alt-row-begins={beginning row}");
    Console.WriteLine("");
    Console.WriteLine(" --mode=Join|Combine");
    Console.WriteLine("  --source_directory={source directory name}");
    Console.WriteLine("  --target_file={target file name}");
    Console.WriteLine("  --tile-map-size=WxH");
}

Arguments arguments = new Arguments();
arguments.ParseArguments(args);

string message = string.Empty;
bool operation_success = false;

if (arguments.Help)
{
    PrintHelp();
    return;
}
else if (arguments.Test)
{
    
    SimpleLog.ToConsole("Test mode, printing arguments.");
    SimpleLog.ToConsole(JsonConvert.SerializeObject(arguments, Formatting.Indented));
    SimpleLog.ToConsole($"Names File:  {(File.Exists(arguments.NamesFile) ? "Exists" : "Does Not Exist")}");
    SimpleLog.ToConsole($"Source File: {(File.Exists(arguments.SourceFile) ? "Exists" : "Does Not Exist")}");
    SimpleLog.ToConsole($"Target File: {(File.Exists(arguments.TargetFile) ? "Exists" : "Does Not Exist")}");
    SimpleLog.ToConsole($"Source Directory: {(Directory.Exists(arguments.SourceDirectory) ? "Exists" : "Does Not Exist")}");
    SimpleLog.ToConsole($"Target Directory: {(Directory.Exists(arguments.TargetDirectory) ? "Exists" : "Does Not Exist")}");
    SimpleLog.ToConsole($"Tile Size: {(arguments.TileSize.IsEmpty ? "Invalid" : "Valid")}");
    SimpleLog.ToConsole($"Offset: Offset can only be compared against a source or target file.");
    SimpleLog.ToConsole($"Scale: {(arguments.Scale >= 2 && arguments.Scale <= 8 ? "Valid" : "Invalid")}");
    return;
}
else if (arguments.OperatingMode == tilemap_splitter.OperatingMode.Split)
{
    SimpleLog.ToConsole($"Operating Mode: {arguments.OperatingMode}");
    SimpleLog.ToConsole($"Map File: {arguments.NamesFile}");
    SimpleLog.ToConsole($"Source File: {arguments.SourceFile}");
    SimpleLog.ToConsole($"Target Directory: {arguments.TargetDirectory}");
    operation_success = TileOperations.Split(arguments.SourceFile, arguments.TargetDirectory, arguments.NamesFile, arguments.TileSize, arguments.Offset, arguments.AltRowStart, arguments.SaveEmptyTiles, out message);
}
else if (arguments.OperatingMode == tilemap_splitter.OperatingMode.Enbiggen)
{
    SimpleLog.ToConsole($"Operating Mode: {arguments.OperatingMode}");
    operation_success = TileOperations.Enbiggen(arguments.SourceFile, arguments.TargetFile, arguments.Scale, out message);
}
else if (arguments.OperatingMode == tilemap_splitter.OperatingMode.Ensmallen)
{
    SimpleLog.ToConsole($"Operating Mode: {arguments.OperatingMode}");
    operation_success = TileOperations.Ensmallen(arguments.SourceFile, arguments.TargetFile, arguments.Scale, out message);
}
else if (arguments.OperatingMode == tilemap_splitter.OperatingMode.Join)
{
    SimpleLog.ToConsole($"Operating Mode: {arguments.OperatingMode}");
    operation_success = TileOperations.Join(arguments.SourceDirectory, arguments.TargetFile, arguments.TileMapSize.Width, arguments.TileMapSize.Height, out message);
}
else
{
    SimpleLog.ToConsole("Invalid operating mode.");
    PrintHelp();
    return;
}

if (message != null && !string.IsNullOrEmpty(message))
{
    SimpleLog.ToConsole($"{arguments.OperatingMode} completed {(operation_success ? "successfully" : "unsuccessfully")}, message was: '{message}'");
}
else
{
    SimpleLog.ToConsole($"{arguments.OperatingMode} completed {(operation_success ? "successfully" : "unsuccessfully")}.");
}
