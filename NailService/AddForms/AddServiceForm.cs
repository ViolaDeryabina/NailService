using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
    public partial class AddServiceForm : Form
    {
        private string _connection;
        public ServiceModel NewService { get; private set; }
        private Show _showForm;
        private Image _selectedImage;
        private string _defaultImagePath;
        private string _servicesImagesPath;

        /// <summary>
        /// Максимальный размер файла изображения (3 МБ)
        /// </summary>
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024;

        /// <summary>
        /// Конструктор формы добавления услуги
        /// </summary>
        /// <param name="showForm">Ссылка на главную форму для обновления данных</param>
        public AddServiceForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewService = new ServiceModel();
            _selectedImage = null;
            lblCharCount.Text = "0/500";

            InitializeImagePaths();
            LoadCategory();
            LoadDefaultImage();
        }

        /// <summary>
        /// Инициализация путей для сохранения изображений услуг
        /// </summary>
        private void InitializeImagePaths()
        {
            try
            {
                string projectRoot = GetProjectRootDirectory();
                _servicesImagesPath = Path.Combine(projectRoot, "Images", "Services");
                _defaultImagePath = Path.Combine(_servicesImagesPath, "Default.jpg");

                if (!Directory.Exists(_servicesImagesPath))
                {
                    Directory.CreateDirectory(_servicesImagesPath);
                }

                if (!File.Exists(_defaultImagePath))
                {
                    CreateDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Получение корневой директории проекта (выход из папки bin)
        /// </summary>
        private string GetProjectRootDirectory()
        {
            string startupPath = Application.StartupPath;

            if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
            {
                return Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
            }

            return startupPath;
        }

        /// <summary>
        /// Загрузка изображения-заглушки для новых услуг
        /// </summary>
        private void LoadDefaultImage()
        {
            try
            {
                if (File.Exists(_defaultImagePath))
                {
                    pictureBoxService.Image = Image.FromFile(_defaultImagePath);
                    pictureBoxService.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    CreateDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Создание изображения-заглушки с текстом
        /// </summary>
        private void CreateDefaultImage()
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

                pictureBoxService.Image = defaultImage;

                if (!Directory.Exists(_servicesImagesPath))
                {
                    Directory.CreateDirectory(_servicesImagesPath);
                }
                defaultImage.Save(_defaultImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Загрузка изображения из файла с проверкой размера и формата
        /// </summary>
        private void LoadImageFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);

                    // Проверка размера файла
                    if (fileInfo.Length > MAX_IMAGE_SIZE)
                    {
                        MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                       $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.\n\n" +
                                       "Пожалуйста, выберите файл меньшего размера или сожмите изображение.",
                                       "Ошибка размера файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        return;
                    }

                    // Проверка формата файла
                    string extension = Path.GetExtension(filePath).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        MessageBox.Show("Выберите файл с поддерживаемым форматом:\n" +
                                       "JPG, JPEG, PNG, BMP или GIF",
                                       "Неверный формат файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        return;
                    }

                    _selectedImage = Image.FromFile(filePath);

                    // Проверка разрешения изображения
                    if (_selectedImage.Width > 4000 || _selectedImage.Height > 4000)
                    {
                        var result = MessageBox.Show($"Разрешение изображения очень большое ({_selectedImage.Width}x{_selectedImage.Height}).\n" +
                                                   "Рекомендуется использовать изображения до 2000x2000 пикселей.\n\n" +
                                                   "Хотите продолжить загрузку? (изображение будет сжато)",
                                                   "Большое разрешение",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Question);

                        if (result == DialogResult.No)
                        {
                            _selectedImage.Dispose();
                            _selectedImage = null;
                            return;
                        }
                    }

                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                    ShowImageInfo(fileInfo, _selectedImage);
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
        /// Отображение информации о загруженном изображении в подсказке
        /// </summary>
        private void ShowImageInfo(FileInfo fileInfo, Image image)
        {
            string info = $"Файл: {fileInfo.Name}\n" +
                         $"Размер: {FormatFileSize(fileInfo.Length)}\n" +
                         $"Разрешение: {image.Width}x{image.Height} пикселей\n" +
                         $"Формат: {image.RawFormat}";

            toolTip1.SetToolTip(pictureBoxService, info);
        }

        /// <summary>
        /// Форматирование размера файла в читаемый вид
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
        /// Масштабирование изображения с сохранением пропорций
        /// </summary>
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
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        /// <summary>
        /// Сохранение изображения услуги в файл с уникальным именем
        /// </summary>
        /// <returns>Имя сохраненного файла или null</returns>
        private string SaveServiceImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                    return null;

                string serviceName = NameService.Text.Trim().ToLower()
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

                string fileName = $"service_{serviceName}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string filePath = Path.Combine(_servicesImagesPath, fileName);

                SaveOptimizedImage(_selectedImage, filePath);

                return fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// Сохранение изображения с оптимизацией (сжатие JPEG)
        /// </summary>
        private void SaveOptimizedImage(Image image, string filePath)
        {
            var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

            if (encoder != null)
            {
                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 85L);

                image.Save(filePath, encoder, encoderParams);
            }
            else
            {
                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        /// <summary>
        /// Загрузка категорий услуг из базы данных
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
        /// Проверка, является ли текущее изображение заглушкой
        /// </summary>
        private bool IsDefaultImage()
        {
            try
            {
                return _selectedImage == null ||
                       (_defaultImagePath != null &&
                        pictureBoxService.ImageLocation == _defaultImagePath);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Добавление новой услуги или восстановление неактивной
        /// </summary>
        private bool AddNewService()
        {
            try
            {
                SaveServiceData();
                string serviceName = NameService.Text.Trim();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Сначала проверяем, нет ли неактивной услуги с таким названием
                    string checkQuery = @"SELECT IDServices FROM services 
                                        WHERE ServiceName = @ServiceName 
                                          AND IsActive = 0";

                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    object inactiveServiceId = checkCmd.ExecuteScalar();

                    if (inactiveServiceId != null && inactiveServiceId != DBNull.Value)
                    {
                        // Нашли неактивную услугу - восстанавливаем
                        int serviceId = Convert.ToInt32(inactiveServiceId);

                        string updateQuery = @"UPDATE services 
                                            SET Description = @Description,
                                                Price = @Price,
                                                Category = @Category,
                                                Photo = @Photo,
                                                IsActive = 1
                                            WHERE IDServices = @ServiceId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        updateCmd.Parameters.AddWithValue("@Description", NewService.Description);
                        updateCmd.Parameters.AddWithValue("@Price", NewService.Price);
                        updateCmd.Parameters.AddWithValue("@Category", NewService.Category);

                        // Сохраняем изображение
                        string imageFileName = SaveServiceImage();
                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            updateCmd.Parameters.AddWithValue("@Photo", imageFileName);
                        }
                        else
                        {
                            updateCmd.Parameters.AddWithValue("@Photo", DBNull.Value);
                        }

                        int updatedRows = updateCmd.ExecuteNonQuery();

                        if (updatedRows > 0)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }

                    // Создаем новую услугу
                    string insertQuery = @"INSERT INTO services 
                                        (ServiceName, Description, Price, Category, Photo, IsActive) 
                                        VALUES (@ServiceName, @Description, @Price, @Category, @Photo, 1)";

                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@ServiceName", NewService.ServiceName);
                    insertCmd.Parameters.AddWithValue("@Description", NewService.Description);
                    insertCmd.Parameters.AddWithValue("@Price", NewService.Price);
                    insertCmd.Parameters.AddWithValue("@Category", NewService.Category);

                    // Сохраняем изображение
                    string imageFileNameForInsert = SaveServiceImage();
                    if (!string.IsNullOrEmpty(imageFileNameForInsert))
                    {
                        insertCmd.Parameters.AddWithValue("@Photo", imageFileNameForInsert);
                    }
                    else
                    {
                        insertCmd.Parameters.AddWithValue("@Photo", DBNull.Value);
                    }

                    int result = insertCmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно добавлена", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить услугу", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Ошибка дублирования уникального ключа
                {
                    string errorMessage = "Услуга с таким названием уже существует";

                    // Проверяем статус существующей услуги
                    try
                    {
                        using (var connection = new MySqlConnection(_connection))
                        {
                            connection.Open();
                            string checkQuery = @"SELECT IsActive FROM services 
                                               WHERE ServiceName = @ServiceName";
                            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                            checkCmd.Parameters.AddWithValue("@ServiceName", NameService.Text.Trim());

                            object result = checkCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                bool isActive = Convert.ToBoolean(result);
                                if (!isActive)
                                {
                                    errorMessage += " (но неактивна). Попробуйте снова.";
                                }
                            }
                        }
                    }
                    catch { }

                    MessageBox.Show(errorMessage, "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        /// <summary>
        /// Сохранение данных из формы в объект NewService
        /// </summary>
        private void SaveServiceData()
        {
            NewService.ServiceName = NameService.Text.Trim();
            NewService.Description = Description.Text.Trim();

            if (decimal.TryParse(Price.Text.Trim(), out decimal priceValue))
            {
                NewService.Price = Convert.ToInt32(priceValue);
            }
            else
            {
                NewService.Price = 0;
            }

            NewService.Category = (int)Category.SelectedValue;
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену услуги", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            if (!decimal.TryParse(Price.Text, out decimal priceValue) || priceValue <= 0)
            {
                MessageBox.Show("Введите корректную цену (положительное число)", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            if (IsActiveServiceExists())
            {
                MessageBox.Show("Активная услуга с таким названием уже существует", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameService.Focus();
                NameService.SelectAll();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка существования активной услуги с таким названием
        /// </summary>
        private bool IsActiveServiceExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM services 
                                   WHERE ServiceName = @ServiceName 
                                   AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", NameService.Text.Trim());

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - валидация и сохранение услуги
        /// </summary>
        private void AddService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                if (CheckAndRestoreInactiveService())
                {
                    return;
                }

                if (AddNewService())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы без сохранения
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
                    var result = MessageBox.Show(
                        $"Найдена неактивная услуга с таким названием:\n\n" +
                        $"Название: {serviceName}\n\n" +
                        "Восстановить эту услугу с новыми данными?",
                        "Восстановление услуги",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveServiceData();
                        bool restored = RestoreServiceInDatabase(serviceId, NewService);

                        if (restored)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось восстановить услугу", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        /// <summary>
        /// Проверка существования услуги в базе данных
        /// </summary>
        private (bool exists, bool isActive, int serviceId) CheckServiceExists(string serviceName)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"SELECT IDServices, IsActive 
                                   FROM services 
                                   WHERE ServiceName = @ServiceName
                                   LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int serviceId = reader.GetInt32("IDServices");
                            bool isActive = reader.GetBoolean("IsActive");
                            return (true, isActive, serviceId);
                        }
                    }

                    return (false, false, 0);
                }
            }
            catch
            {
                return (false, false, 0);
            }
        }

        /// <summary>
        /// Восстановление неактивной услуги в базе данных
        /// </summary>
        private bool RestoreServiceInDatabase(int serviceId, ServiceModel serviceData)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE services 
                                   SET IsActive = 1,
                                       Description = @Description,
                                       Price = @Price,
                                       Category = @Category,
                                       Photo = @Photo
                                   WHERE IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    cmd.Parameters.AddWithValue("@Description", serviceData.Description);
                    cmd.Parameters.AddWithValue("@Price", serviceData.Price);
                    cmd.Parameters.AddWithValue("@Category", serviceData.Category);

                    if (_selectedImage != null && !IsDefaultImage())
                    {
                        string imagePath = SaveServiceImage();
                        cmd.Parameters.AddWithValue("@Photo", imagePath ?? (object)DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Photo", DBNull.Value);
                    }

                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле названия услуги (только русские буквы)
        /// </summary>
        private void NameService_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameService.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(NameService.Text, true);

            if (filteredText != NameService.Text)
            {
                NameService.Text = filteredText;
                NameService.SelectionStart = Math.Min(selectionStart, NameService.Text.Length);
            }

            if (!string.IsNullOrWhiteSpace(NameService.Text))
            {
                CheckForInactiveServiceHint();
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле цены (только цифры и точка)
        /// </summary>
        private void Price_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Price.SelectionStart;
            bool allowDecimal = true;
            string filteredText = InputValidator.FilterToDigitsOnly(Price.Text, allowDecimal);

            if (filteredText != Price.Text)
            {
                Price.Text = filteredText;
                Price.SelectionStart = Math.Min(selectionStart, Price.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле описания и счетчик символов
        /// </summary>
        private void Description_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Description.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(Description.Text, true);

            if (filteredText != Description.Text)
            {
                Description.Text = filteredText;
                Description.SelectionStart = Math.Min(selectionStart, Description.Text.Length);
            }

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

        /// <summary>
        /// Проверка наличия неактивной услуги при вводе названия
        /// </summary>
        private void CheckForInactiveServiceHint()
        {
            try
            {
                string serviceName = NameService.Text.Trim();

                if (string.IsNullOrWhiteSpace(serviceName))
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT IDServices, Price, Description, Category
                                    FROM services 
                                    WHERE ServiceName = @ServiceName AND IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal price = reader.GetDecimal("Price");
                            string description = reader["Description"]?.ToString() ?? "";
                            int categoryId = reader.GetInt32("Category");

                            Price.Text = price.ToString();
                            Description.Text = description;
                            SetCategory(categoryId);
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        /// <summary>
        /// Получение названия категории по ID
        /// </summary>
        private string GetCategoryName(int categoryId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT CategoryName FROM Category WHERE IDCategory = @CategoryId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Неизвестно";
                }
            }
            catch
            {
                return "Неизвестно";
            }
        }

        /// <summary>
        /// Установка выбранной категории в ComboBox
        /// </summary>
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

        /// <summary>
        /// Проверка при потере фокуса поля названия услуги
        /// </summary>
        private void NameService_Leave(object sender, EventArgs e)
        {
            CheckForInactiveServiceHint();
        }

        /// <summary>
        /// Очистка всех полей формы
        /// </summary>
        private void ClearButton_Click(object sender, EventArgs e)
        {
            NameService.Text = "";
            Price.Text = "";
            Description.Text = "";

            LoadDefaultImage();
            _selectedImage = null;

            NameService.Focus();
        }

        /// <summary>
        /// Загрузка изображения при клике на PictureBox
        /// </summary>
        private void pictureBoxService_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        /// <summary>
        /// Загрузка изображения через кнопку
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        /// <summary>
        /// Открытие диалога выбора файла и загрузка изображения
        /// </summary>
        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Выберите изображение услуги (макс. 3 МБ)";
                openFileDialog.RestoreDirectory = true;

                openFileDialog.FileOk += OpenFileDialog_FileOk;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImageFromFile(openFileDialog.FileName);
                }

                openFileDialog.FileOk -= OpenFileDialog_FileOk;
            }
        }

        /// <summary>
        /// Проверка файла перед загрузкой (размер, формат)
        /// </summary>
        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var openFileDialog = sender as OpenFileDialog;
            if (openFileDialog != null)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                    if (fileInfo.Length > MAX_IMAGE_SIZE)
                    {
                        MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                       $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.",
                                       "Ошибка размера файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        e.Cancel = true;
                        return;
                    }

                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        MessageBox.Show("Выберите файл с поддерживаемым форматом:\n" +
                                       "JPG, JPEG, PNG, BMP или GIF",
                                       "Неверный формат файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки файла: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Обработка перетаскивания файла на PictureBox
        /// </summary>
        private void pictureBoxService_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Обработка сброса файла на PictureBox
        /// </summary>
        private void pictureBoxService_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string filePath = files[0];
                string extension = Path.GetExtension(filePath).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (allowedExtensions.Contains(extension))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > MAX_IMAGE_SIZE)
                        {
                            MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                           $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.",
                                           "Ошибка размера файла",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                            return;
                        }

                        LoadImageFromFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка проверки файла: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Показ подсказки при наведении на PictureBox
        /// </summary>
        private void pictureBoxService_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxService,
                "Кликните для выбора изображения\n" +
                "Или перетащите файл сюда\n" +
                $"Максимальный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ\n" +
                "Поддерживаемые форматы: JPG, JPEG, PNG, BMP, GIF");
        }
    }
}