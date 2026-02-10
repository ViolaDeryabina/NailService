using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class EditServiceForm : Form
    {
        private string _connection;
        public ServiceModel Service { get; private set; }
        private EditUserClass _dataService;
        private Image _selectedImage;
        private string _servicesImagesPath;
        private string _defaultImagePath;
        private bool _imageChanged = false;
        private ImageService _imageService;
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ

        public EditServiceForm(ServiceModel service, ImageService imageService = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Service = service;
            _dataService = new EditUserClass();
            _imageService = imageService ?? new ImageService();

            // Инициализация путей для изображений
            InitializeImagePaths();

            LoadCategory();
            LoadTextBoxs();
            LoadServiceImage();
        }

        private void InitializeImagePaths()
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
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadServiceImage()
        {
            try
            {
                if (Service.ServiceImage != null)
                {
                    _selectedImage = Service.ServiceImage;
                    pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                }
                else if (!string.IsNullOrEmpty(Service.Photo))
                {
                    // Получаем путь к изображениям через ImageService
                    string imagesPath = _imageService.GetServicesImagesPath();
                    string imagePath = Path.Combine(imagesPath, Service.Photo);

                    if (File.Exists(imagePath))
                    {
                        _selectedImage = _imageService.LoadImageFromFile(imagePath);
                        pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                        Service.ServiceImage = _selectedImage;
                    }
                    else
                    {
                        LoadDefaultImage();
                    }
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

        private void LoadDefaultImage()
        {
            try
            {
                string defaultImagePath = _imageService.GetDefaultImagePath();
                if (File.Exists(defaultImagePath))
                {
                    _selectedImage = _imageService.LoadImageFromFile(defaultImagePath);
                }
                else
                {
                    // Создаем заглушку
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
                }

                pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}");
            }
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
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void LoadTextBoxs()
        {
            NameService.Text = Service.ServiceName;
            Price.Text = Service.Price.ToString();
            Description.Text = Service.Description;

            // Счетчик символов для описания
            UpdateCharCount();
        }

        private void UpdateCharCount()
        {
            int charCount = Description.Text.Length;
            int maxChars = 500;
            lblCharCount.Text = $"{charCount}/{maxChars}";

            if (charCount > maxChars * 0.9) // 90% от лимита
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

                    // Устанавливаем выбранную категорию
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

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void EditService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveServiceData();

                // Сохраняем изображение через ImageService
                if (_imageChanged && _selectedImage != null)
                {
                    try
                    {
                        string imageFileName = _imageService.SaveServiceImage(
                            _selectedImage,
                            Service.ServiceName,
                            Service.Photo
                        );

                        if (!string.IsNullOrEmpty(imageFileName))
                        {
                            Service.Photo = imageFileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}");
                    }
                }

                if (UpdateServiceInDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }


        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите стоимость услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            // Проверка корректности цены
            if (!decimal.TryParse(Price.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную стоимость (число больше 0)", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            // Проверка описания
            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            // Проверка категории
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

            if (decimal.TryParse(Price.Text.Trim(), out decimal priceValue))
            {
                Service.Price = Convert.ToInt32(priceValue);
            }
            else
            {
                Service.Price = 0;
            }

            Service.Description = Description.Text.Trim();
            Service.Category = (int)CategoryCb.SelectedValue;
            Service.ServiceImage = _selectedImage; // Сохраняем изображение в модель
        }

        private string SaveServiceImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                {
                    // Если выбрана заглушка, удаляем старое фото
                    if (!string.IsNullOrEmpty(Service.Photo))
                    {
                        string oldFilePath = Path.Combine(_servicesImagesPath, Service.Photo);
                        if (File.Exists(oldFilePath) && !IsDefaultImageFile(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    return null; // Возвращаем null для заглушки
                }

                // Генерируем уникальное имя файла
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

                // Удаляем старое изображение если оно существует и это не заглушка
                if (!string.IsNullOrEmpty(Service.Photo) && Service.Photo != fileName)
                {
                    string oldFilePath = Path.Combine(_servicesImagesPath, Service.Photo);
                    if (File.Exists(oldFilePath) && !IsDefaultImageFile(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // Сохраняем новое изображение
                _selectedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                return fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Service.Photo; // Возвращаем старое имя файла в случае ошибки
            }
        }

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

        private bool IsDefaultImageFile(string filePath)
        {
            try
            {
                return Path.GetFileName(filePath) == "Default.jpg" ||
                       Path.GetFileName(filePath) == "default_service.jpg";
            }
            catch
            {
                return false;
            }
        }

        private bool UpdateServiceInDatabase()
        {
            try
            {
                _dataService.UpdateServiceInDatabase(Service);
                MessageBox.Show("Услуга успешно обновлена", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (MySqlException mysqlEx)
            {
                MessageBox.Show($"Ошибка MySQL при обновлении услуги:\nКод: {mysqlEx.Number}\nСообщение: {mysqlEx.Message}",
                              "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении услуги: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Обработчики для кнопок управления изображением
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            RemoveImage();
        }

        private void LoadImageFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = $"Выберите изображение услуги (макс. {MAX_IMAGE_SIZE / (1024 * 1024)} МБ)";
                openFileDialog.RestoreDirectory = true;

                // Добавляем обработчик для проверки размера файла
                openFileDialog.FileOk += OpenFileDialog_FileOk;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        // Проверяем размер файла
                        FileInfo fileInfo = new FileInfo(filePath);
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

                        // Проверяем расширение файла
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

                        // Загружаем изображение
                        _selectedImage = Image.FromFile(filePath);

                        // Дополнительная проверка размера изображения в пикселях
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

                        // Масштабируем изображение для PictureBox
                        pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                        _imageChanged = true;
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

                // Отписываемся от события
                openFileDialog.FileOk -= OpenFileDialog_FileOk;
            }
        }

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var openFileDialog = sender as OpenFileDialog;
            if (openFileDialog != null)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                    // Проверяем размер файла
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

                    // Проверяем расширение
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

        private void RemoveImage()
        {
            LoadDefaultImage();
            _imageChanged = true;
        }

        // Обработчики фильтрации ввода
        private void NameService_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameService.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(NameService.Text, true);

            if (filteredText != NameService.Text)
            {
                NameService.Text = filteredText;
                NameService.SelectionStart = Math.Min(selectionStart, NameService.Text.Length);
            }
        }

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

        private void Description_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Description.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(Description.Text, true);

            if (filteredText != Description.Text)
            {
                Description.Text = filteredText;
                Description.SelectionStart = Math.Min(selectionStart, Description.Text.Length);
            }

            UpdateCharCount();
        }

        // Drag & Drop для PictureBox
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

        private void pictureBoxService_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string filePath = files[0];

                // Проверяем расширение файла
                string extension = Path.GetExtension(filePath).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (allowedExtensions.Contains(extension))
                {
                    // Проверяем размер файла перед загрузкой
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

                        // Проверяем размер изображения в пикселях
                        using (Image tempImage = Image.FromFile(filePath))
                        {
                            if (tempImage.Width > 4000 || tempImage.Height > 4000)
                            {
                                var result = MessageBox.Show($"Разрешение изображения очень большое ({tempImage.Width}x{tempImage.Height}).\n" +
                                                           "Рекомендуется использовать изображения до 2000x2000 пикселей.\n\n" +
                                                           "Хотите продолжить загрузку? (изображение будет сжато)",
                                                           "Большое разрешение",
                                                           MessageBoxButtons.YesNo,
                                                           MessageBoxIcon.Question);

                                if (result == DialogResult.No)
                                {
                                    return;
                                }
                            }
                        }

                        // Загружаем изображение
                        _selectedImage = Image.FromFile(filePath);
                        pictureBoxService.Image = ScaleImage(_selectedImage, pictureBoxService.Width, pictureBoxService.Height);
                        _imageChanged = true;
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
                else
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Клик по PictureBox для загрузки изображения
        private void pictureBoxService_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }
    }
}