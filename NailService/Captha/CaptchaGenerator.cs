using System;
using System.Drawing;
using System.Linq;

namespace NailService
{
    /// <summary>
    /// Генератор CAPTCHA кодов и изображений
    /// </summary>
    public class CaptchaGenerator
    {
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int CodeLength = 4;
        private const int ImageWidth = 200;
        private const int ImageHeight = 70;

        private readonly Random _random = new Random();

        /// <summary>
        /// Генерирует новый CAPTCHA код
        /// </summary>
        public string GenerateCode()
        {
            return new string(Enumerable.Repeat(AllowedChars, CodeLength)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Создает искаженное изображение CAPTCHA
        /// </summary>
        public Bitmap CreateImage(string code)
        {
            Bitmap bitmap = new Bitmap(ImageWidth, ImageHeight);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                AddBackgroundNoise(bitmap);
                DrawCaptchaSymbols(graphics, code);
                AddNoiseLines(graphics);
            }

            return bitmap;
        }

        private void AddBackgroundNoise(Bitmap bitmap)
        {
            for (int y = 0; y < ImageHeight; y++)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    if (_random.Next(100) < 5) // 5% шума
                        bitmap.SetPixel(x, y, Color.LightGray);
                }
            }
        }

        private void DrawCaptchaSymbols(Graphics graphics, string code)
        {
            Font font = new Font("Arial", 24, FontStyle.Bold | FontStyle.Italic);
            float xPos = 10;

            for (int i = 0; i < code.Length; i++)
            {
                float angle = _random.Next(-15, 16);
                string symbol = code[i].ToString();

                using (Bitmap charBitmap = CreateCharacterBitmap(symbol, font, angle))
                {
                    int yOffset = _random.Next(10, 30);
                    graphics.DrawImage(charBitmap, xPos, yOffset, 35, 40);

                    // Добавляем перечеркивание
                    using (Pen pen = new Pen(Color.DarkRed, 2))
                    {
                        graphics.DrawLine(pen, xPos, yOffset + 20, xPos + 30, yOffset + 20);
                    }
                }

                xPos += _random.Next(20, 35);
            }
        }

        private Bitmap CreateCharacterBitmap(string symbol, Font font, float angle)
        {
            Bitmap charBitmap = new Bitmap(40, 50);
            using (Graphics charGraphics = Graphics.FromImage(charBitmap))
            {
                charGraphics.Clear(Color.White);
                charGraphics.DrawString(symbol, font, Brushes.Black, 0, 0);
            }

            return RotateImage(charBitmap, angle);
        }

        private void AddNoiseLines(Graphics graphics)
        {
            using (Pen noisePen = new Pen(Color.LightBlue))
            {
                for (int i = 0; i < 15; i++)
                {
                    int x1 = _random.Next(ImageWidth);
                    int y1 = _random.Next(ImageHeight);
                    int x2 = _random.Next(ImageWidth);
                    int y2 = _random.Next(ImageHeight);
                    graphics.DrawLine(noisePen, x1, y1, x2, y2);
                }
            }
        }

        private Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap rotated = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
                g.DrawImage(bmp, new Point(0, 0));
            }
            return rotated;
        }
    }
}