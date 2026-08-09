using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using MMORPG.Shared.Enums;
using MMORPG.Shared.Registry;

public static class LpcPaperdollEngine
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

    public static void ExtractLpcEquipmentSheets()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string repoBase = Path.Combine(projectRoot, "../../tools/lpc_generator/spritesheets");
        string outputBase = Path.Combine(projectRoot, "Assets/Textures/Paperdoll/LPC");

        if (!Directory.Exists(repoBase))
        {
            GD.PrintErr($"[LPC Engine] Local LPC repository not found at '{repoBase}'. Skipping extraction.");
            return;
        }

        var itemsToExtract = new Dictionary<string, string>
        {
            ["Head/IronHelm"] = FindFirstFile(repoBase, "head", "helmets", "*.png"),
            ["Armor/IronPlateChest"] = FindFirstFile(repoBase, "torso", "armors", "*.png"),
            ["Legs/IronLeggings"] = FindFirstFile(repoBase, "legs", "pants", "*.png"),
            ["Boots/IronBoots"] = FindFirstFile(repoBase, "feet", "boots", "*.png"),
            ["Weapons/IronSword"] = FindFirstFile(repoBase, "weapon", "sword", "*.png")
        };

        foreach (var (itemKey, sheetPath) in itemsToExtract)
        {
            if (string.IsNullOrEmpty(sheetPath) || !File.Exists(sheetPath))
            {
                GD.PrintErr($"[LPC Engine] Could not find sheet for '{itemKey}'.");
                continue;
            }

            string outDir = Path.Combine(outputBase, itemKey);
            Directory.CreateDirectory(outDir);
            ExtractSheet(sheetPath, outDir, itemKey);
        }
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
            GD.Print($"[LPC Engine] Successfully extracted '{itemKey}' ({dirName}) -> {framePath}");
        }

        // Copy diagonal aliases
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
