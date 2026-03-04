using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Сервис для работы с изображениями в приложении
    /// Предоставляет методы для загрузки, сохранения, масштабирования и управления изображениями услуг
    /// </summary>
    public class ImageService
    {
        private string _servicesImagesPath;
        private string _defaultImagePath;

        /// <summary>
        /// Конструктор сервиса изображений
        /// Инициализирует пути для хранения изображений
        /// </summary>
        public ImageService()
        {
            InitializePaths();
        }

        /// <summary>
        /// Инициализация путей к папкам с изображениями
        /// Определяет путь к папке Images/Services в корне проекта
        /// </summary>
        private void InitializePaths()
        {
            try
            {
                string startupPath = Application.StartupPath;

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

        /// <summary>
        /// Получение пути к папке с изображениями услуг
        /// </summary>
        public string GetServicesImagesPath() => _servicesImagesPath;

        /// <summary>
        /// Получение пути к изображению-заглушке
        /// </summary>
        public string GetDefaultImagePath() => _defaultImagePath;

        /// <summary>
        /// Расчет оптимального размера миниатюры для отображения в DataGridView
        /// </summary>
        /// <param name="dataGridView">Таблица, в которой будет отображаться миниатюра</param>
        /// <param name="desiredWidth">Желаемая ширина (по умолчанию 80)</param>
        /// <returns>Размер миниатюры в пределах 60-100 пикселей</returns>
        public Size CalculateOptimalThumbnailSize(DataGridView dataGridView, int desiredWidth = 80)
        {
            int thumbnailWidth = desiredWidth;
            int thumbnailHeight = thumbnailWidth;

            thumbnailWidth = Math.Max(60, Math.Min(thumbnailWidth, 100));
            thumbnailHeight = Math.Max(60, Math.Min(thumbnailHeight, 100));

            return new Size(thumbnailWidth, thumbnailHeight);
        }

        /// <summary>
        /// Сохранение изображения услуги в файл
        /// </summary>
        /// <param name="image">Изображение для сохранения</param>
        /// <param name="serviceName">Название услуги (используется для генерации имени файла)</param>
        /// <param name="oldFileName">Имя предыдущего файла для удаления</param>
        /// <returns>Имя сохраненного файла или null</returns>
        public string SaveServiceImage(Image image, string serviceName, string oldFileName = null)
        {
            try
            {
                if (image == null || IsDefaultImage(image))
                {
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        DeleteOldImage(oldFileName);
                    }
                    return null;
                }

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

                if (!string.IsNullOrEmpty(oldFileName) && oldFileName != fileName)
                {
                    DeleteOldImage(oldFileName);
                }

                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось сохранить изображение: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка изображения услуги по имени файла
        /// </summary>
        /// <param name="photoFileName">Имя файла изображения</param>
        /// <returns>Загруженное изображение или заглушка</returns>
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
            catch
            {
                return LoadDefaultServiceImage();
            }
        }

        /// <summary>
        /// Загрузка изображения-заглушки для услуг без фото
        /// </summary>
        public Image LoadDefaultServiceImage()
        {
            string defaultImagePath = GetDefaultImagePath();

            if (File.Exists(defaultImagePath))
            {
                return Image.FromFile(defaultImagePath);
            }
            else
            {
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

        /// <summary>
        /// Получение миниатюры изображения услуги
        /// </summary>
        /// <param name="photoFileName">Имя файла изображения</param>
        /// <param name="maxWidth">Максимальная ширина миниатюры</param>
        /// <param name="maxHeight">Максимальная высота миниатюры</param>
        /// <returns>Масштабированное изображение</returns>
        public Image GetServiceThumbnail(string photoFileName, int maxWidth, int maxHeight)
        {
            Image originalImage = LoadServiceImage(photoFileName);
            return ScaleImage(originalImage, maxWidth, maxHeight);
        }

        public Image CreateDefaultThumbnail(int width, int height)
        {
            Bitmap defaultImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.DarkGray))
                {
                    string text = "Нет фото";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return defaultImage;
        }

        /// <summary>
        /// Масштабирование изображения с сохранением пропорций
        /// </summary>
        /// <param name="image">Исходное изображение</param>
        /// <param name="maxWidth">Максимальная ширина</param>
        /// <param name="maxHeight">Максимальная высота</param>
        /// <returns>Масштабированное изображение</returns>
        public Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        /// <summary>
        /// Удаление старого файла изображения
        /// </summary>
        /// <param name="fileName">Имя файла для удаления</param>
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

        /// <summary>
        /// Проверка, является ли изображение заглушкой
        /// </summary>
        private bool IsDefaultImage(Image image)
        {
            try
            {
                return image == null || (image.Width == 100 && image.Height == 100);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Проверка, является ли файл заглушкой по умолчанию
        /// </summary>
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

        /// <summary>
        /// Загрузка изображения из файла по пути
        /// </summary>
        /// <param name="filePath">Полный путь к файлу</param>
        /// <returns>Загруженное изображение</returns>
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