using Godot;
using System.IO;
using System.Collections.Generic;

[Tool]
public partial class LpcOfflineExporter : Node
{
    private const int FrameSize = 64;

    private static readonly Dictionary<string, int> WalkRowY = new()
    {
        ["north"] = 512,
        ["west"] = 576,
        ["south"] = 640,
        ["east"] = 704
    };

    private static readonly Dictionary<string, string> DiagMapping = new()
    {
        ["south"] = "south",
        ["south-east"] = "east",
        ["east"] = "east",
        ["north-east"] = "north",
        ["north"] = "north",
        ["north-west"] = "west",
        ["west"] = "west",
        ["south-west"] = "south"
    };

    public static void ExportAllEquipmentOffline()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string parentToolsRepo = Path.Combine(projectRoot, "../../tools/lpc_generator/spritesheets");
        string paperdollBase = Path.Combine(projectRoot, "Assets/Textures/Paperdoll");

        GD.Print($"[LPC Offline Exporter] Reading LPC library from '{parentToolsRepo}'...");

        if (!Directory.Exists(parentToolsRepo))
        {
            GD.PrintErr($"[LPC Offline Exporter] Parent LPC directory '{parentToolsRepo}' not found!");
            return;
        }

        // Clean old legacy item files
        CleanDirectory(paperdollBase);

        var itemsToExtract = new Dictionary<string, string>
        {
            ["Head/IronHelm"] = FindFirstFile(parentToolsRepo, "head", "helmets", "*.png"),
            ["Armor/IronPlateChest"] = FindFirstFile(parentToolsRepo, "torso", "armors", "*.png"),
            ["Legs/IronLeggings"] = FindFirstFile(parentToolsRepo, "legs", "pants", "*.png"),
            ["Boots/IronBoots"] = FindFirstFile(parentToolsRepo, "feet", "boots", "*.png"),
            ["Weapons/IronSword"] = FindFirstFile(parentToolsRepo, "weapon", "sword", "*.png")
        };

        foreach (var (itemKey, sheetPath) in itemsToExtract)
        {
            if (string.IsNullOrEmpty(sheetPath) || !File.Exists(sheetPath))
            {
                GD.PrintErr($"[LPC Offline Exporter] Missing sheet for '{itemKey}'.");
                continue;
            }

            string outDir = Path.Combine(paperdollBase, itemKey);
            Directory.CreateDirectory(outDir);
            ExtractSheet(sheetPath, outDir, itemKey);
        }

        GD.Print("[LPC Offline Exporter] ALL EQUIPMENT SPRITES OFFLINE EXPORT COMPLETED!");
    }

    private static void CleanDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
    }

    private static string FindFirstFile(string baseDir, string sub1, string sub2, string pattern)
    {
        string targetDir = Path.Combine(baseDir, sub1, sub2);
        if (!Directory.Exists(targetDir)) targetDir = Path.Combine(baseDir, sub1);
        if (!Directory.Exists(targetDir)) return string.Empty;

        string[] files = Directory.GetFiles(targetDir, pattern, SearchOption.AllDirectories);
        return files.Length > 0 ? files[0] : string.Empty;
    }

    private static void ExtractSheet(string sheetPath, string outDir, string itemKey)
    {
        Image fullSheet = Image.LoadFromFile(sheetPath);
        if (fullSheet == null) return;

        foreach (var (dirName, startY) in WalkRowY)
        {
            int y = startY;
            if (y + FrameSize > fullSheet.GetHeight()) y = 0;

            Image frameImg = fullSheet.GetRegion(new Rect2I(0, y, FrameSize, FrameSize));
            string framePath = Path.Combine(outDir, $"{dirName}.png");
            frameImg.SavePng(framePath);
            GD.Print($"[LPC Offline Exporter] Exported built-in '{itemKey}' ({dirName}) -> {framePath}");
        }

        foreach (var (diagDir, targetDir) in DiagMapping)
        {
            if (WalkRowY.ContainsKey(diagDir)) continue;
            string srcPath = Path.Combine(outDir, $"{targetDir}.png");
            string destPath = Path.Combine(outDir, $"{diagDir}.png");
            if (File.Exists(srcPath) && !File.Exists(destPath))
            {
                File.Copy(srcPath, destPath, true);
            }
        }
    }
}
