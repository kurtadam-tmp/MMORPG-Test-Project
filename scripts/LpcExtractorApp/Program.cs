using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class Program
{
    private const int FrameSize = 64;

    // Standard 576x256 LPC Standalone Walk Sheet Row Mapping (4 Rows x 64px)
    private static readonly Dictionary<string, int> WalkRowY = new()
    {
        ["north"] = 0,    // Row 0 (Y: 0px)
        ["west"]  = 64,   // Row 1 (Y: 64px)
        ["south"] = 128,  // Row 2 (Y: 128px)
        ["east"]  = 192   // Row 3 (Y: 192px)
    };

    static void Main(string[] args)
    {
        string clientRoot = @"C:\Projects\Antigravity\MMORPG-Test-Project\src\MMORPG.GodotClient\Assets\Textures";
        string lpcRoot = @"C:\Projects\Antigravity\tools\lpc_generator\spritesheets";

        Console.WriteLine("[LPC Exporter] Beginning full LPC Base Body & 5-Piece Equipment Extraction...");

        // 1. Extract LPC Base Body (Walking 6-frames and Idle)
        string bodyWalkSheet = Path.Combine(lpcRoot, @"body\bodies\male\walk.png");
        string bodyIdleSheet = Path.Combine(lpcRoot, @"body\bodies\male\idle.png");

        if (File.Exists(bodyWalkSheet))
        {
            ExtractBaseBodyWalkFrames(bodyWalkSheet, Path.Combine(clientRoot, "BaseBody/Walking"));
        }

        if (File.Exists(bodyIdleSheet))
        {
            ExtractBaseBodyIdleFrames(bodyIdleSheet, Path.Combine(clientRoot, "BaseBody/Idle"));
        }

        // 2. Extract 5 Equipment Sets into Paperdoll Folder
        var itemsToExtract = new Dictionary<string, string>
        {
            ["Head/IronHelm"] = Path.Combine(lpcRoot, @"hat\helmet\barbuta\male\walk.png"),
            ["Armor/IronPlateChest"] = Path.Combine(lpcRoot, @"torso\armour\plate\male\walk.png"),
            ["Legs/IronLeggings"] = Path.Combine(lpcRoot, @"legs\armour\plate\male\walk.png"),
            ["Boots/IronBoots"] = Path.Combine(lpcRoot, @"feet\armour\plate\male\walk.png"),
            ["Weapons/IronSword"] = Path.Combine(lpcRoot, @"weapon\sword\longsword\walk\longsword.png")
        };

        foreach (var (itemRelativePath, sheetPath) in itemsToExtract)
        {
            if (!File.Exists(sheetPath))
            {
                Console.WriteLine($"[WARNING] Sheet not found for '{itemRelativePath}' at '{sheetPath}'");
                continue;
            }

            string outFolder = Path.Combine(clientRoot, "Paperdoll", itemRelativePath);
            Directory.CreateDirectory(outFolder);
            Extract8DirectionalEquipment(sheetPath, outFolder, itemRelativePath);
        }

        Console.WriteLine("[LPC Exporter] ALL EXTRACTIONS COMPLETED SUCCESSFULLY!");
    }

    private static void ExtractBaseBodyWalkFrames(string sheetPath, string outBaseFolder)
    {
        using (Bitmap fullSheet = new Bitmap(sheetPath))
        {
            foreach (var (dirName, startY) in WalkRowY)
            {
                int y = startY;
                if (y + FrameSize > fullSheet.Height) y = 0;

                string dirFolder = Path.Combine(outBaseFolder, dirName);
                Directory.CreateDirectory(dirFolder);

                for (int f = 0; f < 6; f++)
                {
                    Rectangle cropRect = new Rectangle(f * FrameSize, y, FrameSize, FrameSize);
                    using (Bitmap frameImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(frameImg))
                        {
                            g.DrawImage(fullSheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                        }

                        string framePath = Path.Combine(dirFolder, $"frame_00{f}.png");
                        frameImg.Save(framePath, ImageFormat.Png);
                    }
                }
            }
        }

        // Synthesize 4 Diagonal Base Body Directions
        SynthesizeDiagonalWalkFrames(outBaseFolder, "east", "south", "south-east");
        SynthesizeDiagonalWalkFrames(outBaseFolder, "east", "north", "north-east");
        SynthesizeDiagonalWalkFrames(outBaseFolder, "west", "north", "north-west");
        SynthesizeDiagonalWalkFrames(outBaseFolder, "west", "south", "south-west");
    }

    private static void ExtractBaseBodyIdleFrames(string sheetPath, string outBaseFolder)
    {
        Directory.CreateDirectory(outBaseFolder);

        using (Bitmap fullSheet = new Bitmap(sheetPath))
        {
            foreach (var (dirName, startY) in WalkRowY)
            {
                int y = startY;
                if (y + FrameSize > fullSheet.Height) y = 0;

                Rectangle cropRect = new Rectangle(0, y, FrameSize, FrameSize);
                using (Bitmap frameImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(frameImg))
                    {
                        g.DrawImage(fullSheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                    }

                    string framePath = Path.Combine(outBaseFolder, $"{dirName}.png");
                    frameImg.Save(framePath, ImageFormat.Png);
                }
            }
        }

        SynthesizeDiagonalSingle(outBaseFolder, "east.png", "south.png", "south-east.png");
        SynthesizeDiagonalSingle(outBaseFolder, "east.png", "north.png", "north-east.png");
        SynthesizeDiagonalSingle(outBaseFolder, "west.png", "north.png", "north-west.png");
        SynthesizeDiagonalSingle(outBaseFolder, "west.png", "south.png", "south-west.png");
    }

    private static void Extract8DirectionalEquipment(string sheetPath, string outFolder, string itemKey)
    {
        using (Bitmap fullSheet = new Bitmap(sheetPath))
        {
            foreach (var (dirName, startY) in WalkRowY)
            {
                int y = startY;
                if (y + FrameSize > fullSheet.Height) y = 0;

                Rectangle cropRect = new Rectangle(0, y, FrameSize, FrameSize);
                using (Bitmap frameImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(frameImg))
                    {
                        g.DrawImage(fullSheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                    }

                    string framePath = Path.Combine(outFolder, $"{dirName}.png");
                    frameImg.Save(framePath, ImageFormat.Png);
                }
            }
        }

        SynthesizeDiagonalSingle(outFolder, "east.png", "south.png", "south-east.png");
        SynthesizeDiagonalSingle(outFolder, "east.png", "north.png", "north-east.png");
        SynthesizeDiagonalSingle(outFolder, "west.png", "north.png", "north-west.png");
        SynthesizeDiagonalSingle(outFolder, "west.png", "south.png", "south-west.png");

        Console.WriteLine($"[SUCCESS] Exported 8-directional set for '{itemKey}'");
    }

    private static void SynthesizeDiagonalWalkFrames(string outBaseFolder, string d1, string d2, string targetDir)
    {
        string path1Folder = Path.Combine(outBaseFolder, d1);
        string path2Folder = Path.Combine(outBaseFolder, d2);
        string targetFolder = Path.Combine(outBaseFolder, targetDir);

        Directory.CreateDirectory(targetFolder);

        for (int f = 0; f < 6; f++)
        {
            string f1 = Path.Combine(path1Folder, $"frame_00{f}.png");
            string f2 = Path.Combine(path2Folder, $"frame_00{f}.png");
            string fTarget = Path.Combine(targetFolder, $"frame_00{f}.png");

            SynthesizeDiagonalSingleFile(f1, f2, fTarget);
        }
    }

    private static void SynthesizeDiagonalSingle(string folder, string file1, string file2, string targetFile)
    {
        string p1 = Path.Combine(folder, file1);
        string p2 = Path.Combine(folder, file2);
        string pTarget = Path.Combine(folder, targetFile);

        SynthesizeDiagonalSingleFile(p1, p2, pTarget);
    }

    private static void SynthesizeDiagonalSingleFile(string p1, string p2, string pTarget)
    {
        if (File.Exists(p1) && File.Exists(p2))
        {
            using (Bitmap img1 = new Bitmap(p1))
            using (Bitmap img2 = new Bitmap(p2))
            using (Bitmap outImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < FrameSize; y++)
                {
                    for (int x = 0; x < FrameSize; x++)
                    {
                        Color c1 = img1.GetPixel(x, y);
                        Color c2 = img2.GetPixel(x, y);
                        outImg.SetPixel(x, y, (c1.A > 0) ? c1 : c2);
                    }
                }
                outImg.Save(pTarget, ImageFormat.Png);
            }
        }
    }
}
