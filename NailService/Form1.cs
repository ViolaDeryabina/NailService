using MySql.Data.MySqlClient;
using NailService.Properties;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

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

        private CaptchaManager _captchaManager;
        private FormSizeManager _sizeManager;

        // Размеры формы
        private readonly Size _smallFormSize = new Size(400, 433);
        private readonly Size _largeFormSize = new Size(748, 433);

        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            InitializeManagers();
            InitializeComponents();
        }

        private void InitializeManagers()
        {
            _passwordToggle = new PasswordVisibilityToggle(
                Eye, Password, Resources.eyeOpen, Resources.eyeClose);

            _sizeManager = new FormSizeManager(this, _smallFormSize, _largeFormSize);

            _captchaManager = new CaptchaManager(
                pictureBoxCaptcha, textBoxCaptcha, buttonRefreshCaptcha,
                labelCaptcha, groupBox1);

            _captchaManager.OnBlockStarted += OnCaptchaBlockStarted;
            _captchaManager.OnBlockEnded += OnCaptchaBlockEnded;

            int timeout = Properties.Settings.Default.inactivityTimeout;
            _inactivityController = new InactivityController(LockSystem, timeout);
            Application.AddMessageFilter(_inactivityController);
        }

        private void InitializeComponents()
        {
            _sizeManager.SetSmallSize();
            _captchaManager.Hide();
        }

        private void OnCaptchaBlockStarted()
        {
            // Блокируем основные элементы управления
            Autorization.Enabled = false;
            Login.Enabled = false;
            Password.Enabled = false;
            Eye.Enabled = false;
        }

        private void OnCaptchaBlockEnded()
        {
            // Разблокируем основные элементы управления
            Autorization.Enabled = true;
            Login.Enabled = true;
            Password.Enabled = true;
            Eye.Enabled = true;
        }

        private void Autorization_Click(object sender, EventArgs e)
        {
            if (_captchaManager.IsBlocked)
            {
                MessageBox.Show($"Подождите {_captchaManager.RemainingSeconds} секунд перед следующей попыткой.",
                    "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка SysAdmin
            if (Login.Text == "SysAdmin" && Password.Text == "SysAdmin")
            {
                OpenSysAdminForm();
                return;
            }

            // Проверка подключения к БД
            if (!Connection.TestConnection())
            {
                HandleConnectionError();
                return;
            }

            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(Login.Text) || string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Заполните логин и пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Проверка CAPTCHA
            if (_captchaManager.FailedAttempts > 0 && !ValidateCaptcha())
            {
                return;
            }

            // Авторизация в БД
            AuthorizeUser();
        }

        private bool ValidateCaptcha()
        {
            if (string.IsNullOrWhiteSpace(textBoxCaptcha.Text))
            {
                MessageBox.Show("Введите код с картинки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!_captchaManager.Validate(textBoxCaptcha.Text))
            {
                _captchaManager.IncrementFailedAttempts();

                if (_captchaManager.FailedAttempts >= 3)
                {
                    MessageBox.Show("Превышено количество попыток. Доступ заблокирован на 10 секунд.",
                        "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _captchaManager.Block(10);
                }
                else
                {
                    MessageBox.Show($"Неверный код CAPTCHA. Попытка {_captchaManager.FailedAttempts} из 3",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _captchaManager.Refresh();
                }
                return false;
            }

            return true;
        }

        private void AuthorizeUser()
        {
            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                try
                {
                    con.Open();
                    string passwordHash = MySQLHelper.GetHash(Password.Text);

                    if (IsActiveUser(con, passwordHash))
                    {
                        HandleSuccessfulLogin(passwordHash);
                    }
                    else
                    {
                        HandleFailedLogin(con, passwordHash);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool IsActiveUser(MySqlConnection con, string passwordHash)
        {
            string query = @"SELECT Count(*) FROM users 
                   WHERE Login = @Login AND Password = @Password AND IsActive = 1";

            MySqlCommand cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Login", Login.Text);
            cmd.Parameters.AddWithValue("@Password", passwordHash);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void HandleSuccessfulLogin(string passwordHash)
        {
            _captchaManager.ResetFailedAttempts();
            _captchaManager.Hide();
            _sizeManager.SetSmallSize();

            var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
            string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

            if (role != null && FIO != null)
            {
                int masterID = EditUserClass.GetMasterId(Login.Text, passwordHash);
                OpenRoleForm(role, FIO, masterID);
            }
        }

        private void HandleFailedLogin(MySqlConnection con, string passwordHash)
        {
            if (IsInactiveUser(con, passwordHash))
            {
                MessageBox.Show("Ваша учетная запись отключена. Обратитесь к администратору.",
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                _captchaManager.IncrementFailedAttempts();
                MessageBox.Show("Неверный логин или пароль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (_captchaManager.FailedAttempts >= 1)
                {
                    _captchaManager.Show();
                    _sizeManager.SetLargeSize();
                    _captchaManager.Refresh();
                }
            }

            Password.Clear();
            _captchaManager.ClearInput();
        }

        private bool IsInactiveUser(MySqlConnection con, string passwordHash)
        {
            string query = @"SELECT Count(*) FROM users 
                   WHERE Login = @Login AND Password = @Password AND IsActive = 0";

            MySqlCommand cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Login", Login.Text);
            cmd.Parameters.AddWithValue("@Password", passwordHash);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private void OpenRoleForm(string role, string fio, int masterID)
        {
            switch (role)
            {
                case "Админ":
                    new MenuAdmin(fio, Login.Text).Show();
                    break;
                case "Мастер":
                    new MenuMaster(fio, masterID).Show();
                    break;
                case "Менеджер":
                    new MenuManager(fio).Show();
                    break;
            }
            this.Hide();
        }

        private void OpenSysAdminForm()
        {
            new SysAdmin().Show();
            this.Hide();
        }

        private void HandleConnectionError()
        {
            MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.",
                "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

            new SettingForm().Show();
            this.Hide();
        }

        private void ButtonRefreshCaptcha_Click(object sender, EventArgs e)
        {
            if (!_captchaManager.IsBlocked)
            {
                _captchaManager.Refresh();
            }
            else
            {
                MessageBox.Show($"Нельзя обновить капчу во время блокировки. Осталось {_captchaManager.RemainingSeconds} сек.",
                    "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void Exit_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => DatabaseBackup.CreateBackup());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Application.Exit();
        }

        private void LockSystem()
        {
            Login.Clear();
            Password.Clear();
            _captchaManager.ClearInput();

            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
            {
                if (f.Name != "Form1")
                    f.Hide();
                else
                    f.Close();
            }

            _sizeManager.Reset();
            _captchaManager.Hide();
            _captchaManager.ResetFailedAttempts();

            this.Show();
            MessageBox.Show("Сессия завершена из-за отсутствия активности.");
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