using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace NailService
{
    public class ImageService
    {
        private string _servicesImagesPath;
        private string _defaultImagePath;

        public ImageService()
        {
            InitializePaths();
        }

        private void InitializePaths()
        {
            try
            {
                string startupPath = Application.StartupPath;

                // Если запущено из bin\Debug или bin\Release
                if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
                {
                    string projectRoot = Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
                    _servicesImagesPath = Path.Combine(projectRoot, "Images", "Services");
                }
                else
                {
                    _servicesImagesPath = Path.Combine(startupPath, "Images", "Services");
                }

                _defaultImagePath = Path.Combine(_servicesImagesPath, "Default.jpg");

                // Создаем папку если ее нет
                if (!Directory.Exists(_servicesImagesPath))
                {
                    Directory.CreateDirectory(_servicesImagesPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации путей для изображений: {ex.Message}");
            }
        }

        public string GetServicesImagesPath() => _servicesImagesPath;

        public string GetDefaultImagePath() => _defaultImagePath;


        // Расчет оптимального размера миниатюр
        public Size CalculateOptimalThumbnailSize(DataGridView dataGridView, int desiredWidth = 80)
        {
            // Фиксированный размер для миниатюр
            int thumbnailWidth = desiredWidth;
            int thumbnailHeight = thumbnailWidth;

            // Ограничиваем минимальный и максимальный размер
            thumbnailWidth = Math.Max(60, Math.Min(thumbnailWidth, 100));  // от 60 до 100 пикселей
            thumbnailHeight = Math.Max(60, Math.Min(thumbnailHeight, 100));

            return new Size(thumbnailWidth, thumbnailHeight);
        }

        // Сохранение изображения услуги
        public string SaveServiceImage(Image image, string serviceName, string oldFileName = null)
        {
            try
            {
                if (image == null || IsDefaultImage(image))
                {
                    // Если изображение является заглушкой
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        DeleteOldImage(oldFileName);
                    }
                    return null;
                }

                // Генерируем имя файла
                string cleanName = serviceName.Trim().ToLower()
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(":", "")
                    .Replace("*", "")
                    .Replace("?", "")
                    .Replace("\"", "")
                    .Replace("<", "")
                    .Replace(">", "")
                    .Replace("|", "");

                string fileName = $"service_{cleanName}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string filePath = Path.Combine(_servicesImagesPath, fileName);

                // Удаляем старое изображение если оно существует
                if (!string.IsNullOrEmpty(oldFileName) && oldFileName != fileName)
                {
                    DeleteOldImage(oldFileName);
                }

                // Сохраняем изображение
                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось сохранить изображение: {ex.Message}");
            }
        }
        public Image LoadServiceImage(string photoFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(photoFileName))
                {
                    return LoadDefaultServiceImage();
                }

                string imagePath = Path.Combine(GetServicesImagesPath(), photoFileName);

                if (File.Exists(imagePath))
                {
                    // Загружаем оригинальное изображение
                    using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(stream);
                    }
                }
                else
                {
                    return LoadDefaultServiceImage();
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем заглушку
                return LoadDefaultServiceImage();
            }
        }

        public Image LoadDefaultServiceImage()
        {
            // Возвращаем заглушку для изображения
            string defaultImagePath = GetDefaultImagePath();

            if (File.Exists(defaultImagePath))
            {
                return Image.FromFile(defaultImagePath);
            }
            else
            {
                // Создаем программную заглушку
                Bitmap defaultImage = new Bitmap(300, 200);
                using (Graphics g = Graphics.FromImage(defaultImage))
                {
                    g.Clear(Color.LightGray);
                    using (Font font = new Font("Arial", 14, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.DarkGray))
                    {
                        string text = "Нет изображения";
                        SizeF textSize = g.MeasureString(text, font);
                        float x = (defaultImage.Width - textSize.Width) / 2;
                        float y = (defaultImage.Height - textSize.Height) / 2;
                        g.DrawString(text, font, brush, x, y);
                    }
                }
                return defaultImage;
            }
        }

        public Image GetServiceThumbnail(string photoFileName, int maxWidth, int maxHeight)
        {
            // Загружаем оригинальное изображение
            Image originalImage = LoadServiceImage(photoFileName);

            // Создаем миниатюру
            return ScaleImage(originalImage, maxWidth, maxHeight);
        }

        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        // Удаление старого изображения
        private void DeleteOldImage(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string filePath = Path.Combine(_servicesImagesPath, fileName);
                    if (File.Exists(filePath) && !IsDefaultImageFile(fileName))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch { }
        }

        // Проверка, является ли изображение заглушкой
        private bool IsDefaultImage(Image image)
        {
            try
            {
                // Простая проверка - можно расширить при необходимости
                return image == null || image.Width == 100 && image.Height == 100;
            }
            catch
            {
                return true;
            }
        }

        // Проверка, является ли файл заглушкой по умолчанию
        private bool IsDefaultImageFile(string fileName)
        {
            try
            {
                return fileName == "Default.jpg" || fileName == "default_service.jpg";
            }
            catch
            {
                return false;
            }
        }

        // Загрузка изображения из файла
        public Image LoadImageFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return Image.FromFile(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось загрузить изображение: {ex.Message}");
            }
            return null;
        }
    }
}