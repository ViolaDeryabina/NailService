using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class AddServiceForm : Form
    {
        private string _connection;
        public ServiceModel NewService { get; private set; }
        private Image _selectedImage;
        private byte[] _selectedImageBytes = null;
        private bool _imageChanged = false;
        private bool _isEditMode = false;
        private Form _parentForm;
        private int _editingServiceId = 0;


        private const long MAX_IMAGE_SIZE = 5 * 1024 * 1024;

        /// <summary>
        /// Конструктор формы редактирования услуги
        /// </summary>
        public AddServiceForm(Form parentForm)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _parentForm = parentForm;
            _isEditMode = true;
            _imageChanged = false;
            NewService = new ServiceModel();

            LoadCategory();
            LoadServiceData();
        }
        /// <summary>
        /// Загрузка данных услуги в форму для редактирования
        /// </summary>
        private void LoadServiceData()
        {
            NameService.Text = NewService.ServiceName;
            Price.Text = NewService.Price.ToString();
            Description.Text = NewService.Description;

            // Выбираем категорию
            for (int i = 0; i < Category.Items.Count; i++)
            {
                DataRowView row = (DataRowView)Category.Items[i];
                if (Convert.ToInt32(row["IDCategory"]) == NewService.Category)
                {
                    Category.SelectedIndex = i;
                    break;
                }
            }

            // Загружаем изображение
            if (_selectedImageBytes != null && _selectedImageBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(_selectedImageBytes))
                {
                    _selectedImage = Image.FromStream(ms);
                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                    _imageChanged = false;
                }
            }
            else
            {
                LoadDefaultImage();
            }

            this.Text = "Редактирование услуги";
            button1.Text = "Сохранить";
        }
        /// <summary>
        /// Загрузка изображения-заглушки для новых услуг
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
                pictureBoxService.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // Проверка размера файла
                if (imageBytes.Length > MAX_IMAGE_SIZE)
                {
                    // Сжимаем изображение
                    Cursor = Cursors.WaitCursor;
                    imageBytes = CompressImageBytes(imageBytes);
                    Cursor = Cursors.Default;
                }

                // Загружаем изображение из байтов
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    _selectedImage = Image.FromStream(ms);
                    _selectedImageBytes = imageBytes;
                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                    _imageChanged = true;

                    FileInfo fileInfo = new FileInfo(filePath);
                    ShowImageInfo(fileInfo.Name, fileInfo.Length, imageBytes.Length, _selectedImage.Width, _selectedImage.Height);
                }
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Файл поврежден или не является корректным изображением.",
                              "Ошибка загрузки",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Не удалось загрузить изображение: неверный формат файла.\n{ex.Message}",
                              "Ошибка",
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
        /// Сжатие изображения из байтов
        /// </summary>
        private byte[] CompressImageBytes(byte[] imageBytes)
        {
            try
            {
                using (MemoryStream inputMs = new MemoryStream(imageBytes))
                using (Image originalImage = Image.FromStream(inputMs))
                {
                    // Вычисляем новые размеры
                    int targetWidth = originalImage.Width;
                    int targetHeight = originalImage.Height;
                    int maxDimension = 1200; // Уменьшил до 1200 для лучшей производительности

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
                // Если сжатие не удалось, возвращаем исходные байты
                return imageBytes;
            }
        }

        /// <summary>
        /// Сжатие изображения до допустимых размеров
        /// </summary>
        private Image CompressImage(Image originalImage)
        {
            int targetWidth = originalImage.Width;
            int targetHeight = originalImage.Height;
            int maxDimension = 1600;

            if (targetWidth > maxDimension || targetHeight > maxDimension)
            {
                float ratio = Math.Min((float)maxDimension / targetWidth, (float)maxDimension / targetHeight);
                targetWidth = (int)(targetWidth * ratio);
                targetHeight = (int)(targetHeight * ratio);
            }

            Bitmap resizedImage = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, targetWidth, targetHeight);
            }

            int quality = 85;
            byte[] imageBytes = ImageToBytesWithQuality(resizedImage, quality);
            long currentSize = imageBytes.Length;

            while (currentSize > MAX_IMAGE_SIZE && quality > 30)
            {
                quality -= 10;
                imageBytes = ImageToBytesWithQuality(resizedImage, quality);
                currentSize = imageBytes.Length;
            }

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Конвертация Image в массив байтов с указанным качеством
        /// </summary>
        private byte[] ImageToBytesWithQuality(Image image, int quality)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var jpegCodec = ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                if (jpegCodec != null)
                {
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                    image.Save(ms, jpegCodec, encoderParams);
                }
                else
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }

                return ms.ToArray();
            }
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
        /// Отображение информации о загруженном изображении
        /// </summary>
        private void ShowImageInfo(string fileName, long originalSize, long compressedSize, int width, int height)
        {
            string info = $"Файл: {fileName}\n" +
                         $"Исходный размер: {FormatFileSize(originalSize)}\n" +
                         $"Размер после обработки: {FormatFileSize(compressedSize)}\n" +
                         $"Разрешение: {width}x{height} пикселей";

            toolTip1.SetToolTip(pictureBoxService, info);
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

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
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
                    string query = "SELECT IDCategory, CategoryName FROM Category WHERE IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    Category.DataSource = dt;
                    Category.DisplayMember = "CategoryName";
                    Category.ValueMember = "IDCategory";

                    if (Category.Items.Count > 0)
                    {
                        Category.SelectedIndex = 0;
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
        /// Добавление новой услуги
        /// </summary>
        private bool AddNewService()
        {
            try
            {
                SaveServiceData();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string checkQuery = @"SELECT IDServices FROM services 
                                        WHERE ServiceName = @ServiceName AND IsActive = 0";

                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceName", NewService.ServiceName);

                    object inactiveServiceId = checkCmd.ExecuteScalar();

                    if (inactiveServiceId != null && inactiveServiceId != DBNull.Value)
                    {
                        int serviceId = Convert.ToInt32(inactiveServiceId);
                        string updateQuery = _imageChanged && _selectedImageBytes != null ?
                            @"UPDATE services SET Description = @Description, Price = @Price, Category = @Category, Photo = @Photo, IsActive = 1 WHERE IDServices = @ServiceId" :
                            @"UPDATE services SET Description = @Description, Price = @Price, Category = @Category, Photo = NULL, IsActive = 1 WHERE IDServices = @ServiceId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        updateCmd.Parameters.AddWithValue("@Description", NewService.Description);
                        updateCmd.Parameters.AddWithValue("@Price", NewService.Price);
                        updateCmd.Parameters.AddWithValue("@Category", NewService.Category);
                        if (_imageChanged && _selectedImageBytes != null)
                            updateCmd.Parameters.AddWithValue("@Photo", _selectedImageBytes);

                        int updatedRows = updateCmd.ExecuteNonQuery();
                        if (updatedRows > 0)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }

                    string insertQuery = _imageChanged && _selectedImageBytes != null ?
                        @"INSERT INTO services (ServiceName, Description, Price, Category, Photo, IsActive) VALUES (@ServiceName, @Description, @Price, @Category, @Photo, 1)" :
                        @"INSERT INTO services (ServiceName, Description, Price, Category, Photo, IsActive) VALUES (@ServiceName, @Description, @Price, @Category, NULL, 1)";

                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@ServiceName", NewService.ServiceName);
                    insertCmd.Parameters.AddWithValue("@Description", NewService.Description);
                    insertCmd.Parameters.AddWithValue("@Price", NewService.Price);
                    insertCmd.Parameters.AddWithValue("@Category", NewService.Category);
                    if (_imageChanged && _selectedImageBytes != null)
                        insertCmd.Parameters.AddWithValue("@Photo", _selectedImageBytes);

                    int result = insertCmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно добавлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить услугу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show("Услуга с таким названием уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Сохранение данных из формы
        /// </summary>
        private void SaveServiceData()
        {
            NewService.ServiceName = NameService.Text.Trim();
            NewService.Description = Description.Text.Trim();
            NewService.Price = decimal.TryParse(Price.Text.Trim(), out decimal priceValue) ? Convert.ToInt32(priceValue) : 0;
            NewService.Category = (int)Category.SelectedValue;
            NewService.PhotoBytes = (_imageChanged && _selectedImageBytes != null) ? _selectedImageBytes : null;
        }

        /// <summary>
        /// Валидация данных
        /// </summary>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену услуги", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            if (!decimal.TryParse(Price.Text, out decimal priceValue) || priceValue <= 0)
            {
                MessageBox.Show("Введите корректную цену (положительное число)", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            if (IsActiveServiceExists())
            {
                MessageBox.Show("Активная услуга с таким названием уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameService.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка существования активной услуги
        /// </summary>
        private bool IsActiveServiceExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM services WHERE ServiceName = @ServiceName AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", NameService.Text.Trim());
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить"
        /// </summary>
        private void AddService_Click(object sender, EventArgs e)
        {
            if (ValidateData() && AddNewService())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Назад"
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Проверка существования услуги
        /// </summary>
        private (bool exists, bool isActive, int serviceId) CheckServiceExists(string serviceName)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDServices, IsActive FROM services WHERE ServiceName = @ServiceName LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (true, reader.GetBoolean("IsActive"), reader.GetInt32("IDServices"));
                        }
                    }
                }
            }
            catch { }

            return (false, false, 0);
        }

        /// <summary>
        /// Проверка и восстановление неактивной услуги
        /// </summary>
        private bool CheckAndRestoreInactiveService()
        {
            try
            {
                string serviceName = NameService.Text.Trim();
                var (exists, isActive, serviceId) = CheckServiceExists(serviceName);

                if (exists && !isActive)
                {
                    var result = MessageBox.Show($"Найдена неактивная услуга с таким названием:\n\nНазвание: {serviceName}\n\nВосстановить эту услугу с новыми данными?",
                        "Восстановление услуги", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveServiceData();
                        bool restored = RestoreServiceInDatabase(serviceId, NewService);
                        if (restored)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке услуги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        /// <summary>
        /// Восстановление неактивной услуги
        /// </summary>
        private bool RestoreServiceInDatabase(int serviceId, ServiceModel serviceData)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = _imageChanged && _selectedImageBytes != null ?
                        @"UPDATE services SET IsActive = 1, Description = @Description, Price = @Price, Category = @Category, Photo = @Photo WHERE IDServices = @ServiceId" :
                        @"UPDATE services SET IsActive = 1, Description = @Description, Price = @Price, Category = @Category, Photo = NULL WHERE IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    cmd.Parameters.AddWithValue("@Description", serviceData.Description);
                    cmd.Parameters.AddWithValue("@Price", serviceData.Price);
                    cmd.Parameters.AddWithValue("@Category", serviceData.Category);
                    if (_imageChanged && _selectedImageBytes != null)
                        cmd.Parameters.AddWithValue("@Photo", _selectedImageBytes);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Фильтрация ввода названия
        /// </summary>
        private void NameService_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameService.Text))
            {
                CheckForInactiveServiceHint();
            }
        }

        private void Price_TextChanged(object sender, EventArgs e) { }
        private void Description_TextChanged(object sender, EventArgs e)
        {
            int charCount = Description.Text.Length;
            lblCharCount.Text = $"{charCount}/500";
            lblCharCount.ForeColor = charCount > 450 ? Color.Orange : (charCount > 500 ? Color.Red : Color.Green);
        }

        private void CheckForInactiveServiceHint()
        {
            try
            {
                string serviceName = NameService.Text.Trim();
                if (string.IsNullOrWhiteSpace(serviceName)) return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT Price, Description, Category FROM services WHERE ServiceName = @ServiceName AND IsActive = 0";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Price.Text = reader.GetDecimal("Price").ToString();
                            Description.Text = reader["Description"]?.ToString() ?? "";
                            SetCategory(reader.GetInt32("Category"));
                            NameService.BackColor = Color.LightYellow;
                        }
                        else
                        {
                            NameService.BackColor = Color.White;
                        }
                    }
                }
            }
            catch { }
        }

        private void SetCategory(int categoryId)
        {
            for (int i = 0; i < Category.Items.Count; i++)
            {
                DataRowView row = (DataRowView)Category.Items[i];
                if (Convert.ToInt32(row["IDCategory"]) == categoryId)
                {
                    Category.SelectedIndex = i;
                    break;
                }
            }
        }

        private void NameService_Leave(object sender, EventArgs e) => CheckForInactiveServiceHint();

        private void ClearButton_Click(object sender, EventArgs e)
        {
            NameService.Text = "";
            Price.Text = "";
            Description.Text = "";
            NameService.BackColor = Color.White;
            LoadDefaultImage();
            _selectedImageBytes = null;
            _imageChanged = false;
            NameService.Focus();
        }

        private void pictureBoxService_Click(object sender, EventArgs e) => LoadImage();
        private void btnLoadImage_Click(object sender, EventArgs e) => LoadImage();
        private void btnRemoveImage_Click(object sender, EventArgs e) => RemoveImage();

        private void RemoveImage()
        {
            LoadDefaultImage();
            _selectedImageBytes = null;
            _imageChanged = true;
        }

        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Выберите изображение услуги";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImageFromFile(openFileDialog.FileName);
                }
            }
        }

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

        private void pictureBoxService_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxService,
                "Кликните для выбора изображения\n" +
                "Или перетащите файл сюда\n" +
                $"Максимальный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ\n" +
                "Поддерживаемые форматы: JPG, JPEG, PNG, BMP, GIF");
        }

        private void NameService_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameService.Text))
            {
                string name = NameService.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    NameService.Text = name;
                }
            }
        }
    }
}