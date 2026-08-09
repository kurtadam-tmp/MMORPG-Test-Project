using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    static void Main(string[] args)
    {
        string imgPath = @"C:\Users\LK-Dell-Laptop\.gemini\antigravity\brain\ad509590-0363-4945-bd0c-19a6e6b056b5\scale_mail_chest_armor_1786275172512.jpg";
        string outDir = @"C:\Projects\Antigravity\MMORPG-Test-Project\src\MMORPG.GodotClient\Assets\Textures\Paperdoll\Armor\ScaleMailChest";

        Directory.CreateDirectory(outDir);

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

                    // Remove white/light background to make transparent
                    MakeTransparent(frameImg);

                    string framePath = Path.Combine(outDir, $"{dirs[i]}.png");
                    frameImg.Save(framePath, ImageFormat.Png);
                    Console.WriteLine($"[SUCCESS] Saved 8-directional texture: {framePath}");
                }
            }
        }

        Console.WriteLine("[COMPLETE] Scale Mail Chest Armor extracted into 8 directions!");
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
