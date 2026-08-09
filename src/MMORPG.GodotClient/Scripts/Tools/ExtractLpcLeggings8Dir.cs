using Godot;
using System.IO;
using System.Collections.Generic;

[Tool]
public partial class ExtractLpcLeggings8Dir : Node
{
    private const int FrameSize = 64;

    private static readonly Dictionary<string, int> WalkRowY = new()
    {
        ["north"] = 512,
        ["west"] = 576,
        ["south"] = 640,
        ["east"] = 704
    };

    public static void ProcessIronLeggings8Dir()
    {
        string projectRoot = ProjectSettings.GlobalizePath("res://");
        string parentLpcRepo = Path.Combine(projectRoot, "../../tools/lpc_generator/spritesheets");
        string targetSheet = Path.Combine(parentLpcRepo, "legs/armour/plate/male/walk.png");

        if (!File.Exists(targetSheet))
        {
            GD.PrintErr($"[LPC 8-Dir Extractor] Target sheet not found at '{targetSheet}'");
            return;
        }

        Image fullSheet = Image.LoadFromFile(targetSheet);
        if (fullSheet == null) return;

        string outDir = Path.Combine(projectRoot, "Assets/Textures/Paperdoll/Legs/IronLeggings");
        Directory.CreateDirectory(outDir);

        // 1. Extract 4 Cardinal Directions
        foreach (var (dirName, startY) in WalkRowY)
        {
            int y = startY;
            if (y + FrameSize > fullSheet.GetHeight()) y = 0;

            Image frameImg = fullSheet.GetRegion(new Rect2I(0, y, FrameSize, FrameSize));
            string framePath = Path.Combine(outDir, $"{dirName}.png");
            frameImg.SavePng(framePath);
            GD.Print($"[LPC 8-Dir Extractor] Extracted cardinal '{dirName}' -> {framePath}");
        }

        // 2. Synthesize 4 Diagonal Directions for 8-Directional Engine
        // South-East: Blend East and South angles
        CreateDiagonalFrame(outDir, "east.png", "south.png", "south-east.png");

        // North-East: Blend East and North angles
        CreateDiagonalFrame(outDir, "east.png", "north.png", "north-east.png");

        // North-West: Blend West and North angles
        CreateDiagonalFrame(outDir, "west.png", "north.png", "north-west.png");

        // South-West: Blend West and South angles
        CreateDiagonalFrame(outDir, "west.png", "south.png", "south-west.png");

        GD.Print("[LPC 8-Dir Extractor] Successfully generated full 8-directional IronLeggings PNG set!");
    }

    private static void CreateDiagonalFrame(string folder, string src1Name, string src2Name, string targetName)
    {
        string path1 = Path.Combine(folder, src1Name);
        string path2 = Path.Combine(folder, src2Name);
        string targetPath = Path.Combine(folder, targetName);

        if (File.Exists(targetPath)) return;

        if (File.Exists(path1))
        {
            Image img1 = Image.LoadFromFile(path1);
            if (File.Exists(path2))
            {
                Image img2 = Image.LoadFromFile(path2);
                Image blendImg = Image.CreateEmpty(FrameSize, FrameSize, false, Image.Format.Rgba8);
                
                for (int y = 0; y < FrameSize; y++)
                {
                    for (int x = 0; x < FrameSize; x++)
                    {
                        Color c1 = img1.GetPixel(x, y);
                        Color c2 = img2.GetPixel(x, y);
                        Color blended = (c1.A > 0f) ? c1 : c2;
                        blendImg.SetPixel(x, y, blended);
                    }
                }
                blendImg.SavePng(targetPath);
                return;
            }

            img1.SavePng(targetPath);
        }
    }
}
