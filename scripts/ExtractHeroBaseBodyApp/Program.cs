using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    static void Main(string[] args)
    {
        string imgPath = @"C:\Users\LK-Dell-Laptop\.gemini\antigravity\brain\ad509590-0363-4945-bd0c-19a6e6b056b5\hero_warrior_sprite_1786259378251.jpg";
        string clientRoot = @"C:\Projects\Antigravity\MMORPG-Test-Project\src\MMORPG.GodotClient\Assets\Textures\BaseBody";

        string idleDir = Path.Combine(clientRoot, "Idle");
        string walkDir = Path.Combine(clientRoot, "Walking");

        Directory.CreateDirectory(idleDir);
        Directory.CreateDirectory(walkDir);

        if (!File.Exists(imgPath))
        {
            Console.WriteLine("[ERROR] Input image not found: " + imgPath);
            return;
        }

        using (Bitmap srcImg = new Bitmap(imgPath))
        {
            int w = srcImg.Width;
            int h = srcImg.Height;

            // Crop 8 sectors (4 columns x 2 rows or 8 sub-regions)
            int cellW = w / 4;
            int cellH = h / 2;

            string[] dirs = new string[] { "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west" };

            for (int i = 0; i < 8; i++)
            {
                int col = i % 4;
                int row = i / 4;

                Rectangle cropRect = new Rectangle(col * cellW, row * cellH, cellW, cellH);

                using (Bitmap frameImg = new Bitmap(cellW, cellH, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(frameImg))
                    {
                        g.DrawImage(srcImg, new Rectangle(0, 0, cellW, cellH), cropRect, GraphicsUnit.Pixel);
                    }

                    MakeTransparent(frameImg);

                    string dirName = dirs[i];
                    string idlePath = Path.Combine(idleDir, $"{dirName}.png");
                    frameImg.Save(idlePath, ImageFormat.Png);

                    string dirWalkFolder = Path.Combine(walkDir, dirName);
                    Directory.CreateDirectory(dirWalkFolder);

                    for (int f = 0; f < 6; f++)
                    {
                        string walkFramePath = Path.Combine(dirWalkFolder, $"frame_00{f}.png");
                        frameImg.Save(walkFramePath, ImageFormat.Png);
                    }

                    Console.WriteLine($"[SUCCESS] Extracted Base Hero Body for direction '{dirName}'");
                }
            }
        }

        Console.WriteLine("[COMPLETE] Base Hero Body extracted into 8 directions for BaseBody!");
    }

    private static void MakeTransparent(Bitmap bmp)
    {
        Color bgSample = bmp.GetPixel(2, 2);
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                // If light/white background, make transparent
                if (c.R > 220 && c.G > 220 && c.B > 220)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
        }
    }
}
