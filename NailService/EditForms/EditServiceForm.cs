using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Encoder = System.Drawing.Imaging.Encoder;


namespace NailService
{
    /// <summary>
    /// Форма для редактирования данных существующей услуги
    /// Позволяет изменять название, описание, цену, категорию и изображение
    /// </summary>
    public partial class EditServiceForm : Form
    {
        private string _connection;
        public ServiceModel Service { get; private set; }
        private Image _selectedImage;
        private byte[] _selectedImageBytes = null;
        private bool _imageChanged = false;
        private const long MAX_IMAGE_SIZE = 5 * 1024 * 1024; // 5 МБ

        /// <summary>
        /// Конструктор формы редактирования услуги
        /// </summary>
        public EditServiceForm(ServiceModel service)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Service = service;

            LoadCategory();
            LoadTextBoxs();
            LoadServiceImage();
        }

        #region Инициализация и загрузка данных

        /// <summary>
        /// Загрузка изображения услуги или заглушки
        /// </summary>
        private void LoadServiceImage()
        {
            try
            {
                if (Service.PhotoBytes != null && Service.PhotoBytes.Length > 0)
                {
                    _selectedImageBytes = Service.PhotoBytes;
                    _selectedImage = BytesToImage(Service.PhotoBytes);
                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                    Service.ServiceImage = _selectedImage;
                }
                else
                {
                    LoadDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                LoadDefaultImage();
            }
        }

        /// <summary>
        /// Загрузка изображения-заглушки
        /// </summary>
        private void LoadDefaultImage()
        {
            try
            {
                Bitmap defaultImage = new Bitmap(pictureBoxService.Width, pictureBoxService.Height);
                using (Graphics g = Graphics.FromImage(defaultImage))
                {
                    g.Clear(Color.LightGray);
                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.DarkGray))
                    {
                        string text = "Изображение услуги";
                        SizeF textSize = g.MeasureString(text, font);
                        float x = (defaultImage.Width - textSize.Width) / 2;
                        float y = (defaultImage.Height - textSize.Height) / 2;
                        g.DrawString(text, font, brush, x, y);
                    }
                }

                _selectedImage = defaultImage;
                _selectedImageBytes = null;
                pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                _imageChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}");
            }
        }

        /// <summary>
        /// Масштабирование изображения
        /// </summary>
        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null) return null;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            if (newWidth < 1) newWidth = 1;
            if (newHeight < 1) newHeight = 1;

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        /// <summary>
        /// Конвертация Image в массив байтов
        /// </summary>
        private byte[] ImageToBytes(Image image)
        {
            if (image == null) return null;

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Конвертация массива байтов в Image
        /// </summary>
        private Image BytesToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;

            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Загрузка текстовых данных услуги
        /// </summary>
        private void LoadTextBoxs()
        {
            NameService.Text = Service.ServiceName;
            Price.Text = Service.Price.ToString();
            Description.Text = Service.Description;
            UpdateCharCount();
        }

        /// <summary>
        /// Загрузка категорий
        /// </summary>
        private void LoadCategory()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDCategory, CategoryName FROM category WHERE IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "CategoryName";
                    CategoryCb.ValueMember = "IDCategory";

                    if (CategoryCb.Items.Count > 0)
                    {
                        for (int i = 0; i < CategoryCb.Items.Count; i++)
                        {
                            DataRowView row = (DataRowView)CategoryCb.Items[i];
                            if (Convert.ToInt32(row["IDCategory"]) == Service.Category)
                            {
                                CategoryCb.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обновление счетчика символов
        /// </summary>
        private void UpdateCharCount()
        {
            int charCount = Description.Text.Length;
            int maxChars = 500;
            lblCharCount.Text = $"{charCount}/{maxChars}";

            if (charCount > maxChars * 0.9)
            {
                lblCharCount.ForeColor = Color.Orange;
            }
            else if (charCount > maxChars)
            {
                lblCharCount.ForeColor = Color.Red;
            }
            else
            {
                lblCharCount.ForeColor = Color.Green;
            }
        }

        #endregion

        #region Сохранение данных

        private void EditService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveServiceData();

                if (_imageChanged && _selectedImage != null)
                {
                    _selectedImageBytes = ImageToBytes(_selectedImage);
                    Service.PhotoBytes = _selectedImageBytes;
                }

                if (UpdateServiceInDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите стоимость услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            if (!decimal.TryParse(Price.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную стоимость (число больше 0)", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            if (CategoryCb.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CategoryCb.Focus();
                return false;
            }

            return true;
        }

        private void SaveServiceData()
        {
            Service.ServiceName = NameService.Text.Trim();
            Service.Price = decimal.TryParse(Price.Text.Trim(), out decimal priceValue) ? Convert.ToInt32(priceValue) : 0;
            Service.Description = Description.Text.Trim();
            Service.Category = (int)CategoryCb.SelectedValue;
            Service.ServiceImage = _selectedImage;
        }

        private bool UpdateServiceInDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query;
                    MySqlCommand cmd;

                    if (_imageChanged && _selectedImageBytes != null)
                    {
                        query = @"UPDATE services 
                                 SET ServiceName = @ServiceName,
                                     Description = @Description,
                                     Price = @Price,
                                     Category = @Category,
                                     Photo = @Photo
                                 WHERE IDServices = @ServiceId";

                        cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@Photo", _selectedImageBytes);
                    }
                    else
                    {
                        query = @"UPDATE services 
                                 SET ServiceName = @ServiceName,
                                     Description = @Description,
                                     Price = @Price,
                                     Category = @Category
                                 WHERE IDServices = @ServiceId";

                        cmd = new MySqlCommand(query, connection);
                    }

                    cmd.Parameters.AddWithValue("@ServiceId", Service.IDServices);
                    cmd.Parameters.AddWithValue("@ServiceName", Service.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", Service.Description);
                    cmd.Parameters.AddWithValue("@Price", Service.Price);
                    cmd.Parameters.AddWithValue("@Category", Service.Category);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить услугу", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Управление изображением

        /// <summary>
        /// Загрузка изображения из файла с автоматическим сжатием
        /// </summary>
        private void LoadImageFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Выберите изображение услуги";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImageFromFile(openFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// Загрузка изображения из файла с автоматическим сжатием
        /// </summary>
        private void LoadImageFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                string extension = Path.GetExtension(filePath).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    MessageBox.Show("Выберите файл с поддерживаемым форматом:\nJPG, JPEG, PNG, BMP или GIF",
                                   "Неверный формат файла",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    return;
                }

                byte[] imageBytes = File.ReadAllBytes(filePath);
                FileInfo fileInfo = new FileInfo(filePath);

                // Если файл слишком большой - сжимаем
                if (imageBytes.Length > MAX_IMAGE_SIZE)
                {
                    Cursor = Cursors.WaitCursor;
                    imageBytes = CompressImageBytes(imageBytes);
                    Cursor = Cursors.Default;
                }

                // Загружаем изображение
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    _selectedImage = new Bitmap(Image.FromStream(ms));
                    _selectedImageBytes = imageBytes;
                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                    pictureBoxService.SizeMode = PictureBoxSizeMode.Zoom;
                    _imageChanged = true;

                    // Показываем информацию
                    string info = $"Файл: {fileInfo.Name}\n" +
                                 $"Размер: {FormatFileSize(imageBytes.Length)}\n" +
                                 $"Разрешение: {_selectedImage.Width}x{_selectedImage.Height}";
                    toolTip1.SetToolTip(pictureBoxService, info);
                }
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Файл поврежден или не является корректным изображением.",
                              "Ошибка загрузки",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Сжатие изображения
        /// </summary>
        private byte[] CompressImageBytes(byte[] imageBytes)
        {
            try
            {
                using (MemoryStream inputMs = new MemoryStream(imageBytes))
                using (Image originalImage = Image.FromStream(inputMs))
                {
                    int targetWidth = originalImage.Width;
                    int targetHeight = originalImage.Height;
                    int maxDimension = 1200;

                    if (targetWidth > maxDimension || targetHeight > maxDimension)
                    {
                        float ratio = Math.Min((float)maxDimension / targetWidth, (float)maxDimension / targetHeight);
                        targetWidth = (int)(targetWidth * ratio);
                        targetHeight = (int)(targetHeight * ratio);
                        if (targetWidth < 1) targetWidth = 1;
                        if (targetHeight < 1) targetHeight = 1;
                    }

                    // Создаем уменьшенное изображение
                    using (Bitmap resizedImage = new Bitmap(targetWidth, targetHeight))
                    {
                        using (Graphics g = Graphics.FromImage(resizedImage))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.DrawImage(originalImage, 0, 0, targetWidth, targetHeight);
                        }

                        // Сохраняем с качеством 80%
                        using (MemoryStream outputMs = new MemoryStream())
                        {
                            var jpegCodec = ImageCodecInfo.GetImageEncoders()
                                .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                            if (jpegCodec != null)
                            {
                                var encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 80L);
                                resizedImage.Save(outputMs, jpegCodec, encoderParams);
                            }
                            else
                            {
                                resizedImage.Save(outputMs, ImageFormat.Jpeg);
                            }

                            return outputMs.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return imageBytes;
            }
        }

        /// <summary>
        /// Форматирование размера файла
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Удаление изображения
        /// </summary>
        private void RemoveImage()
        {
            LoadDefaultImage();
            _selectedImageBytes = null;
            _imageChanged = true;
        }

        private void pictureBoxService_Click(object sender, EventArgs e) => LoadImageFromFile();
        private void btnLoadImage_Click(object sender, EventArgs e) => LoadImageFromFile();
        private void btnRemoveImage_Click(object sender, EventArgs e) => RemoveImage();

        private void pictureBoxService_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void pictureBoxService_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string extension = Path.GetExtension(files[0]).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (allowedExtensions.Contains(extension))
                {
                    LoadImageFromFile(files[0]);
                }
                else
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        #region Фильтрация ввода

        private void NameService_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameService.Text))
            {
                string name = NameService.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + (name.Length > 1 ? name.Substring(1) : "");
                    if (NameService.Text != name)
                        NameService.Text = name;
                }
            }
        }

        private void Price_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Price.Text))
            {
                string filtered = new string(Price.Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
                if (filtered != Price.Text)
                {
                    int selectionStart = Price.SelectionStart;
                    Price.Text = filtered;
                    Price.SelectionStart = Math.Min(selectionStart, Price.Text.Length);
                }
            }
        }

        private void Description_TextChanged(object sender, EventArgs e) => UpdateCharCount();

        #endregion

        private void EditServiceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
    }
}