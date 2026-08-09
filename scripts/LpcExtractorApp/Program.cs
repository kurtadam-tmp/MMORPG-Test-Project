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

        Console.WriteLine("[LPC Exporter] Extracting clean single-body 8-directional sprite sets...");

        // 1. Extract Complete Composite LPC Base Body (Body Torso + Human Head)
        string bodyWalkSheet = Path.Combine(lpcRoot, @"body\bodies\male\walk.png");
        string headWalkSheet = Path.Combine(lpcRoot, @"head\heads\human\male\walk.png");
        string bodyIdleSheet = Path.Combine(lpcRoot, @"body\bodies\male\idle.png");
        string headIdleSheet = Path.Combine(lpcRoot, @"head\heads\human\male\idle.png");

        if (File.Exists(bodyWalkSheet) && File.Exists(headWalkSheet))
        {
            ExtractCompositeBaseBodyWalkFrames(bodyWalkSheet, headWalkSheet, Path.Combine(clientRoot, "BaseBody/Walking"));
        }

        if (File.Exists(bodyIdleSheet) && File.Exists(headIdleSheet))
        {
            ExtractCompositeBaseBodyIdleFrames(bodyIdleSheet, headIdleSheet, Path.Combine(clientRoot, "BaseBody/Idle"));
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

        Console.WriteLine("[LPC Exporter] CLEAN SINGLE-BODY EXTRACTIONS COMPLETED!");
    }

    private static void ExtractCompositeBaseBodyWalkFrames(string bodySheetPath, string headSheetPath, string outBaseFolder)
    {
        using (Bitmap bodySheet = new Bitmap(bodySheetPath))
        using (Bitmap headSheet = new Bitmap(headSheetPath))
        {
            foreach (var (dirName, startY) in WalkRowY)
            {
                int y = startY;
                if (y + FrameSize > bodySheet.Height) y = 0;

                string dirFolder = Path.Combine(outBaseFolder, dirName);
                Directory.CreateDirectory(dirFolder);

                for (int f = 0; f < 6; f++)
                {
                    Rectangle cropRect = new Rectangle(f * FrameSize, y, FrameSize, FrameSize);
                    using (Bitmap frameImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(frameImg))
                        {
                            g.DrawImage(bodySheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                            g.DrawImage(headSheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                        }

                        string framePath = Path.Combine(dirFolder, $"frame_00{f}.png");
                        frameImg.Save(framePath, ImageFormat.Png);
                    }
                }
            }
        }

        // Map Diagonal Base Body directions directly to nearest clean 1-body angle (NO double-blending!)
        CopyDirWalkFrames(outBaseFolder, "east", "south-east");
        CopyDirWalkFrames(outBaseFolder, "east", "north-east");
        CopyDirWalkFrames(outBaseFolder, "west", "north-west");
        CopyDirWalkFrames(outBaseFolder, "west", "south-west");
    }

    private static void ExtractCompositeBaseBodyIdleFrames(string bodySheetPath, string headSheetPath, string outBaseFolder)
    {
        Directory.CreateDirectory(outBaseFolder);

        using (Bitmap bodySheet = new Bitmap(bodySheetPath))
        using (Bitmap headSheet = new Bitmap(headSheetPath))
        {
            foreach (var (dirName, startY) in WalkRowY)
            {
                int y = startY;
                if (y + FrameSize > bodySheet.Height) y = 0;

                Rectangle cropRect = new Rectangle(0, y, FrameSize, FrameSize);
                using (Bitmap frameImg = new Bitmap(FrameSize, FrameSize, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(frameImg))
                    {
                        g.DrawImage(bodySheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                        g.DrawImage(headSheet, new Rectangle(0, 0, FrameSize, FrameSize), cropRect, GraphicsUnit.Pixel);
                    }

                    string framePath = Path.Combine(outBaseFolder, $"{dirName}.png");
                    frameImg.Save(framePath, ImageFormat.Png);
                }
            }
        }

        CopySingleFile(outBaseFolder, "east.png", "south-east.png");
        CopySingleFile(outBaseFolder, "east.png", "north-east.png");
        CopySingleFile(outBaseFolder, "west.png", "north-west.png");
        CopySingleFile(outBaseFolder, "west.png", "south-west.png");
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

        CopySingleFile(outFolder, "east.png", "south-east.png");
        CopySingleFile(outFolder, "east.png", "north-east.png");
        CopySingleFile(outFolder, "west.png", "north-west.png");
        CopySingleFile(outFolder, "west.png", "south-west.png");

        Console.WriteLine($"[SUCCESS] Exported 8-directional set for '{itemKey}'");
    }

    private static void CopyDirWalkFrames(string outBaseFolder, string srcDir, string targetDir)
    {
        string srcFolder = Path.Combine(outBaseFolder, srcDir);
        string targetFolder = Path.Combine(outBaseFolder, targetDir);

        Directory.CreateDirectory(targetFolder);

        for (int f = 0; f < 6; f++)
        {
            string srcFile = Path.Combine(srcFolder, $"frame_00{f}.png");
            string targetFile = Path.Combine(targetFolder, $"frame_00{f}.png");
            if (File.Exists(srcFile))
            {
                File.Copy(srcFile, targetFile, true);
            }
        }
    }

    private static void CopySingleFile(string folder, string srcFile, string targetFile)
    {
        string p1 = Path.Combine(folder, srcFile);
        string pTarget = Path.Combine(folder, targetFile);

        if (File.Exists(p1))
        {
            File.Copy(p1, pTarget, true);
        }
    }
}
