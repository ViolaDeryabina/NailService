using MySql.Data.MySqlClient;
using NailService.Properties;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Главная форма авторизации в приложении
    /// Обрабатывает вход пользователей с проверкой учетных данных, ролей и CAPTCHA
    /// </summary>
    public partial class Form1 : Form
    {
        private string _connection;
        private PasswordVisibilityToggle _passwordToggle;
        private InactivityController _inactivityController;

        // Переменные для CAPTCHA
        private int _failedAttempts = 0; // Счетчик неудачных попыток (без учета капчи)
        private string _currentCaptchaCode; // Текущий сгенерированный код капчи
        private Timer _blockTimer; // Таймер для разблокировки после 10 секунд
        private bool _isBlocked = false; // Флаг блокировки формы
        private int _remainingSeconds = 10; // Оставшиеся секунды блокировки

        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            _passwordToggle = new PasswordVisibilityToggle(
                Eye,
                Password,
                Resources.eyeOpen,
                Resources.eyeClose
            );

            _inactivityController = new InactivityController(LockSystem);
            Application.AddMessageFilter(_inactivityController);

            // Инициализация таймера для разблокировки
            _blockTimer = new Timer();
            _blockTimer.Interval = 1000; // 1 секунда для обновления счетчика
            _blockTimer.Tick += BlockTimer_Tick;

            // Изначально скрываем элементы CAPTCHA
            HideCaptchaElements();
            labelCaptcha.Text = ""; // Очищаем текст подписи
        }

        // --- Методы для работы с CAPTCHA ---

        /// <summary>
        /// Показывает элементы CAPTCHA на форме
        /// </summary>
        private void ShowCaptchaElements()
        {
            labelCaptcha.Visible = true;
            pictureBoxCaptcha.Visible = true;
            textBoxCaptcha.Visible = true;
            buttonRefreshCaptcha.Visible = true;
            labelCaptcha.Text = "Введите код с картинки:"; // Восстанавливаем обычный текст
        }

        /// <summary>
        /// Скрывает элементы CAPTCHA
        /// </summary>
        private void HideCaptchaElements()
        {
            labelCaptcha.Visible = false;
            pictureBoxCaptcha.Visible = false;
            textBoxCaptcha.Visible = false;
            buttonRefreshCaptcha.Visible = false;
            textBoxCaptcha.Clear();
            labelCaptcha.Text = ""; // Очищаем текст
        }

        /// <summary>
        /// Генерирует новый код CAPTCHA (4 символа: цифры или латинские буквы)
        /// </summary>
        private string GenerateCaptchaCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Создает искаженное изображение CAPTCHA с наложением символов и шумом
        /// </summary>
        private Bitmap CreateDistortedCaptchaImage(string code)
        {
            int width = 200;
            int height = 70;
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            Random random = new Random();

            // Рисуем фон с легким шумом
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (random.Next(100) < 5) // 5% шума
                        bitmap.SetPixel(x, y, Color.LightGray);
                }
            }

            // Рисуем символы с наложением и искажением
            Font font = new Font("Arial", 24, FontStyle.Bold | FontStyle.Italic);
            float xPos = 10;

            for (int i = 0; i < code.Length; i++)
            {
                // Случайный поворот символа
                float angle = random.Next(-15, 16);
                string symbol = code[i].ToString();

                // Создаем временный bitmap для поворота символа
                Bitmap charBitmap = new Bitmap(40, 50);//266; 170
                Graphics charGraphics = Graphics.FromImage(charBitmap);
                charGraphics.Clear(Color.White);
                charGraphics.DrawString(symbol, font, Brushes.Black, 0, 0);

                // Поворачиваем символ
                charBitmap = RotateImage(charBitmap, angle);

                // Случайное смещение по Y для эффекта "не в одной линии"
                int yOffset = random.Next(10, 30);

                // Рисуем символ с возможным наложением (xPos может перекрываться)
                graphics.DrawImage(charBitmap, xPos, yOffset, 35, 40);

                // Добавляем перечеркивание (линию через символ)
                Pen pen = new Pen(Color.DarkRed, 2);
                graphics.DrawLine(pen, xPos, yOffset + 20, xPos + 30, yOffset + 20);

                // Смещение для следующего символа с возможным наложением
                xPos += random.Next(20, 35); // Интервал меньше ширины символа для наложения
            }

            // Добавляем дополнительные линии шума
            Pen noisePen = new Pen(Color.LightBlue);
            for (int i = 0; i < 15; i++)
            {
                int x1 = random.Next(width);
                int y1 = random.Next(height);
                int x2 = random.Next(width);
                int y2 = random.Next(height);
                graphics.DrawLine(noisePen, x1, y1, x2, y2);
            }

            return bitmap;
        }

        /// <summary>
        /// Вспомогательный метод для поворота изображения
        /// </summary>
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

        /// <summary>
        /// Обновляет изображение CAPTCHA
        /// </summary>
        private void RefreshCaptcha()
        {
            if (!_isBlocked)
            {
                _currentCaptchaCode = GenerateCaptchaCode();
                pictureBoxCaptcha.Image = CreateDistortedCaptchaImage(_currentCaptchaCode);
                textBoxCaptcha.Clear();
            }
        }

        /// <summary>
        /// Блокирует элементы формы на 10 секунд
        /// </summary>
        private void BlockFormFor10Seconds()
        {
            _isBlocked = true;
            _remainingSeconds = 10;

            // Блокируем все интерактивные элементы
            Autorization.Enabled = false;
            buttonRefreshCaptcha.Enabled = false;
            Login.Enabled = false;
            Password.Enabled = false;
            textBoxCaptcha.Enabled = false;
            Eye.Enabled = false;

            // Показываем сообщение о блокировке в labelCaptcha
            labelCaptcha.Visible = true;
            labelCaptcha.Text = $"Доступ заблокирован!\nОсталось: {_remainingSeconds} сек.";
            labelCaptcha.ForeColor = Color.Red; // Делаем текст красным для акцента

            // Картинку капчи тоже можно оставить видимой или скрыть - оставим видимой
            pictureBoxCaptcha.Visible = true;
            textBoxCaptcha.Visible = true;
            buttonRefreshCaptcha.Visible = true;

            _blockTimer.Start();
        }

        /// <summary>
        /// Разблокирует форму после таймера
        /// </summary>
        private void UnblockForm()
        {
            _isBlocked = false;

            // Разблокируем все элементы
            Autorization.Enabled = true;
            buttonRefreshCaptcha.Enabled = true;
            Login.Enabled = true;
            Password.Enabled = true;
            textBoxCaptcha.Enabled = true;
            Eye.Enabled = true;

            // Восстанавливаем обычный текст labelCaptcha
            labelCaptcha.Text = "Введите код с картинки:";
            labelCaptcha.ForeColor = SystemColors.ControlText; // Возвращаем обычный цвет

            // Генерируем новую капчу после разблокировки
            RefreshCaptcha();
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds > 0)
            {
                // Обновляем текст с оставшимся временем
                labelCaptcha.Text = $"Доступ заблокирован!\nОсталось: {_remainingSeconds} сек.";
            }
            else
            {
                // Время вышло - останавливаем таймер и разблокируем форму
                _blockTimer.Stop();
                UnblockForm();
            }
        }

        // --- Основная логика авторизации ---

        private void Autorization_Click(object sender, EventArgs e)
        {
            // Если форма заблокирована, ничего не делаем
            if (_isBlocked)
            {
                MessageBox.Show($"Подождите {_remainingSeconds} секунд перед следующей попыткой.", "Блокировка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Особая проверка для SysAdmin
            if (Login.Text == "SysAdmin" && Password.Text == "SysAdmin")
            {
                SysAdmin sysAdmin = new SysAdmin();
                sysAdmin.Show();
                this.Hide();
                return;
            }

            // Проверка подключения к базе данных
            if (!Connection.TestConnection())
            {
                MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                SettingForm settingForm = new SettingForm();
                settingForm.Show();
                this.Hide();
                return;
            }

            // Проверка заполнения обязательных полей
            if (string.IsNullOrWhiteSpace(Login.Text) || string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            

            // Проверка CAPTCHA, если она активна
            if (_failedAttempts > 0)
            {
                if (string.IsNullOrWhiteSpace(textBoxCaptcha.Text))
                {
                    MessageBox.Show("Введите код с картинки!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (textBoxCaptcha.Text != _currentCaptchaCode)
                {
                    // Неверная капча
                    _failedAttempts++;

                    if (_failedAttempts >= 3) // После двух неудач с капчей (всего 3 неудачи)
                    {
                        MessageBox.Show("Превышено количество попыток. Доступ заблокирован на 10 секунд.",
                            "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        BlockFormFor10Seconds();
                    }
                    else
                    {
                        MessageBox.Show($"Неверный код CAPTCHA. Попытка {_failedAttempts} из 3",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        RefreshCaptcha(); // Обновляем капчу
                    }
                    return;
                }
            }

            // Основная проверка логина и пароля в БД
            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                try
                {
                    con.Open();
                    string passwordHash = MySQLHelper.GetHash(Password.Text);

                    // Проверка активного пользователя
                    string query = @"SELECT Count(*) FROM users 
                           WHERE Login = @Login 
                           AND Password = @Password 
                           AND IsActive = 1";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Login", Login.Text);
                    cmd.Parameters.AddWithValue("@Password", passwordHash);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Успешная авторизация - сбрасываем счетчик попыток и скрываем капчу
                        _failedAttempts = 0;
                        HideCaptchaElements();

                        // Получение роли и ФИО пользователя
                        var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                        string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                        if (role != null && FIO != null)
                        {
                            int masterID = EditUserClass.GetMasterId(Login.Text, passwordHash);

                            // Перенаправление на соответствующую форму
                            switch (role)
                            {
                                case "Директор":
                                    {
                                        MenuDirector menuDirector = new MenuDirector(FIO);
                                        menuDirector.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Админ":
                                    {
                                        MenuAdmin menuAdmin = new MenuAdmin(FIO, Login.Text);
                                        menuAdmin.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Мастер":
                                    {
                                        MenuMaster menuMaster = new MenuMaster(FIO, masterID);
                                        menuMaster.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Менеджер":
                                    {
                                        MenuManager menuManager = new MenuManager(FIO);
                                        menuManager.Show();
                                        this.Hide();
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        // Проверка на неактивного пользователя
                        string checkInactiveQuery = @"SELECT Count(*) FROM users 
                                             WHERE Login = @Login 
                                             AND Password = @Password 
                                             AND IsActive = 0";

                        MySqlCommand checkCmd = new MySqlCommand(checkInactiveQuery, con);
                        checkCmd.Parameters.AddWithValue("@Login", Login.Text);
                        checkCmd.Parameters.AddWithValue("@Password", passwordHash);

                        int inactiveCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (inactiveCount > 0)
                        {
                            MessageBox.Show("Ваша учетная запись отключена. Обратитесь к администратору.",
                                          "Доступ запрещен",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // Неудачная попытка входа - увеличиваем счетчик
                            _failedAttempts++;
                            MessageBox.Show("Неверный логин или пароль", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                            // После первой неудачной попытки показываем CAPTCHA
                            if (_failedAttempts >= 1)
                            {
                                ShowCaptchaElements();
                                RefreshCaptcha();
                            }
                        }

                        // Очистка полей ввода
                        Password.Clear();
                        textBoxCaptcha.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки обновления CAPTCHA
        /// </summary>
        private void ButtonRefreshCaptcha_Click(object sender, EventArgs e)
        {
            if (!_isBlocked)
            {
                RefreshCaptcha();
            }
            else
            {
                MessageBox.Show($"Нельзя обновить капчу во время блокировки. Осталось {_remainingSeconds} сек.",
                    "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LockSystem()
        {
            Login.Clear();
            Password.Clear();
            textBoxCaptcha.Clear();

            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
            {
                if (f.Name != "Form1")
                    f.Close();
            }

            this.Show();
            MessageBox.Show("Сессия завершена из-за отсутствия активности.");

            // Сбрасываем состояние CAPTCHA при блокировке неактивности
            _failedAttempts = 0;
            HideCaptchaElements();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
    }
}