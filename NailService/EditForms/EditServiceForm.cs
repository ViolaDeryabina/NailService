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
    /// <summary>
    /// Форма для редактирования данных существующей услуги
    /// Позволяет изменять название, описание, цену, категорию и изображение
    /// </summary>
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

        /// <summary>
        /// Конструктор формы редактирования услуги
        /// </summary>
        /// <param name="service">Объект услуги с текущими данными</param>
        /// <param name="imageService">Сервис для работы с изображениями</param>
        public EditServiceForm(ServiceModel service, ImageService imageService = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Service = service;
            _dataService = new EditUserClass();
            _imageService = imageService ?? new ImageService();

            InitializeImagePaths();
            LoadCategory();
            LoadTextBoxs();
            LoadServiceImage();
        }

        #region Инициализация и загрузка данных

        /// <summary>
        /// Инициализация путей для сохранения изображений услуг
        /// </summary>
        private void InitializeImagePaths()
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
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Загрузка изображения услуги или заглушки
        /// </summary>
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

        /// <summary>
        /// Загрузка изображения-заглушки
        /// </summary>
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
        /// Загрузка текстовых данных услуги в поля формы
        /// </summary>
        private void LoadTextBoxs()
        {
            NameService.Text = Service.ServiceName;
            Price.Text = Service.Price.ToString();
            Description.Text = Service.Description;
            UpdateCharCount();
        }

        /// <summary>
        /// Загрузка категорий из базы данных
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
        /// Обновление счетчика символов в описании
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

        /// <summary>
        /// Сохранение изменений и закрытие формы
        /// </summary>
        private void EditService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveServiceData();

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

        /// <summary>
        /// Отмена редактирования и закрытие формы
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
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

        /// <summary>
        /// Сохранение данных из формы в объект Service
        /// </summary>
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
            Service.ServiceImage = _selectedImage;
        }

        /// <summary>
        /// Обновление данных услуги в базе данных
        /// </summary>
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

        #endregion

        #region Управление изображением

        /// <summary>
        /// Загрузка изображения из файла
        /// </summary>
        private void LoadImageFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = $"Выберите изображение услуги (макс. {MAX_IMAGE_SIZE / (1024 * 1024)} МБ)";
                openFileDialog.RestoreDirectory = true;

                openFileDialog.FileOk += OpenFileDialog_FileOk;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

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
        /// Удаление текущего изображения (замена на заглушку)
        /// </summary>
        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            RemoveImage();
        }

        /// <summary>
        /// Удаление текущего изображения (замена на заглушку)
        /// </summary>
        private void RemoveImage()
        {
            LoadDefaultImage();
            _imageChanged = true;
        }

        /// <summary>
        /// Загрузка изображения при клике на PictureBox
        /// </summary>
        private void pictureBoxService_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        /// <summary>
        /// Загрузка изображения через кнопку
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
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

        #endregion

        #region Фильтрация ввода

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

            UpdateCharCount();
        }

        #endregion
    }
}