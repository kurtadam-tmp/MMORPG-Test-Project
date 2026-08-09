using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

class Program
{
    private const int FrameSize = 64;

    private static readonly Dictionary<string, int> WalkRowY = new()
    {
        ["north"] = 512,
        ["west"] = 576,
        ["south"] = 640,
        ["east"] = 704
    };

    static void Main(string[] args)
    {
        string targetSheet = @"C:\Projects\Antigravity\tools\lpc_generator\spritesheets\legs\armour\plate\male\walk.png";
        string outDir = @"C:\Projects\Antigravity\MMORPG-Test-Project\src\MMORPG.GodotClient\Assets\Textures\Paperdoll\Legs\IronLeggings";

        Console.WriteLine($"[LPC Exporter] Target sheet: '{targetSheet}'");
        Console.WriteLine($"[LPC Exporter] Out folder: '{outDir}'");

        if (!File.Exists(targetSheet))
        {
            Console.WriteLine($"[ERROR] Target sheet not found: {targetSheet}");
            return;
        }

        Directory.CreateDirectory(outDir);

        using (Bitmap fullSheet = new Bitmap(targetSheet))
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

                    string framePath = Path.Combine(outDir, $"{dirName}.png");
                    frameImg.Save(framePath, ImageFormat.Png);
                    Console.WriteLine($"[SUCCESS] Exported '{dirName}' -> '{framePath}'");
                }
            }
        }

        // Synthesize Diagonal Angles (south-east, north-east, north-west, south-west)
        SynthesizeDiagonal(outDir, "east.png", "south.png", "south-east.png");
        SynthesizeDiagonal(outDir, "east.png", "north.png", "north-east.png");
        SynthesizeDiagonal(outDir, "west.png", "north.png", "north-west.png");
        SynthesizeDiagonal(outDir, "west.png", "south.png", "south-west.png");

        Console.WriteLine("[ALL COMPLETED] 8-Directional IronLeggings transparent PNGs generated!");
    }

    private static void SynthesizeDiagonal(string folder, string src1Name, string src2Name, string targetName)
    {
        string p1 = Path.Combine(folder, src1Name);
        string p2 = Path.Combine(folder, src2Name);
        string targetP = Path.Combine(folder, targetName);

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
                outImg.Save(targetP, ImageFormat.Png);
            }
        }
    }
}
