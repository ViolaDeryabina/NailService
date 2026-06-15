using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class SettingForm : Form
    {
        private InactivityController _inactivityController;
        private PasswordVisibilityToggle _passwordToggle;

        /// <summary>
        /// Конструктор формы настроек подключения к базе данных
        /// </summary>
        public SettingForm(InactivityController inactivityController = null)
        {
            InitializeComponent();
            _inactivityController = inactivityController;

            // Инициализируем переключатель видимости пароля
            InitializePasswordToggle();

            LoadCurrentSettings();
            numericTimeout.Maximum = 180;
            numericTimeout.Minimum = 1;

            // Дополнительная проверка при изменении значения
            numericTimeout.ValueChanged += (s, e) =>
            {
                if (numericTimeout.Value > 180)
                    numericTimeout.Value = 180;
                if (numericTimeout.Value < 1)
                    numericTimeout.Value = 1;
            };
        }

        /// <summary>
        /// Инициализация переключателя видимости пароля
        /// </summary>
        private void InitializePasswordToggle()
        {
            // Предполагаем, что на форме есть:
            // - TextBox с именем Password (поле для пароля)
            // - PictureBox с именем Eye (глазик для показа/скрытия)
            // - Ресурсы eyeOpen и eyeClose (иконки открытого и закрытого глаза)

            _passwordToggle = new PasswordVisibilityToggle(
                Eye, Password, Properties.Resources.eyeOpen, Properties.Resources.eyeClose);
        }

        /// <summary>
        /// Загрузка текущих настроек из файла конфигурации
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                Server.Text = Properties.Settings.Default.host;
                NameUser.Text = Properties.Settings.Default.uid;
                Password.Text = Properties.Settings.Default.pwd;
                DB.Text = Properties.Settings.Default.database;
                numericTimeout.Value = Properties.Settings.Default.inactivityTimeout;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохранение введенных настроек в файл конфигурации
        /// </summary>
        private async void ConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, заполнены ли обязательные поля
                if (string.IsNullOrWhiteSpace(Server.Text) ||
                    string.IsNullOrWhiteSpace(NameUser.Text) ||
                    string.IsNullOrWhiteSpace(DB.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните все обязательные поля:\n- Сервер\n- Имя пользователя\n- База данных",
                        "Не все поля заполнены",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Получаем введённые значения
                string newHost = Server.Text.Trim();
                string newUid = NameUser.Text.Trim();
                string newPwd = Password.Text;
                string newDatabase = DB.Text.Trim();
                int newTimeout = (int)numericTimeout.Value;

                // Проверяем, изменились ли настройки (сравниваем с текущими сохранёнными)
                bool settingsChanged =
                    newHost != Properties.Settings.Default.host ||
                    newUid != Properties.Settings.Default.uid ||
                    newPwd != Properties.Settings.Default.pwd ||
                    newDatabase != Properties.Settings.Default.database ||
                    newTimeout != Properties.Settings.Default.inactivityTimeout;

                // Если настройки не изменились, просто выходим
                if (!settingsChanged)
                {
                    DialogResult noChanges = MessageBox.Show(
                        "Настройки не были изменены.\n\nЗакрыть окно?",
                        "Информация",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (noChanges == DialogResult.Yes)
                    {
                        this.Close();
                    }
                    return;
                }

                // Пробуем подключиться с новыми параметрами
                Cursor = Cursors.WaitCursor;

                // Создаём временную строку подключения для проверки
                string testConnectionString = $"server={newHost};user={newUid};password={newPwd};database={newDatabase};charset=utf8;Convert Zero Datetime=True;";
                bool connectionSuccess = await Task.Run(() => TestConnection(testConnectionString));

                Cursor = Cursors.Default;

                // Если подключение не удалось - НЕ СОХРАНЯЕМ настройки
                if (!connectionSuccess)
                {
                    MessageBox.Show(
                        "Не удалось подключиться к базе данных с указанными параметрами.\n\n" +
                        "Проверьте правильность введенных данных.\n\n" +
                        "Настройки НЕ БЫЛИ СОХРАНЕНЫ.",
                        "Ошибка подключения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Подключение успешно - СОХРАНЯЕМ настройки
                Properties.Settings.Default.host = newHost;
                Properties.Settings.Default.uid = newUid;
                Properties.Settings.Default.pwd = newPwd;
                Properties.Settings.Default.database = newDatabase;
                Properties.Settings.Default.inactivityTimeout = newTimeout;

                Properties.Settings.Default.Save();

                // Обновляем статические свойства класса Connection
                Connection.Host = newHost;
                Connection.Database = newDatabase;
                Connection.UserId = newUid;
                Connection.Password = newPwd;

                // Обновляем таймаут неактивности
                if (_inactivityController != null)
                {
                    _inactivityController.UpdateTimeout(newTimeout);
                }

                // Спрашиваем о перезапуске
                DialogResult restart = MessageBox.Show(
                    "Настройки успешно сохранены!\n\nДля применения всех изменений необходимо перезапустить приложение.\n\nПерезапустить сейчас?",
                    "Сохранение настроек",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (restart == DialogResult.Yes)
                {
                    Application.Restart();
                }
                else
                {
                    Clean();
                    LoadCurrentSettings();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Тест подключения к базе данных с указанными параметрами
        /// </summary>
        private bool TestConnection(string connectionString)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Очистка всех полей ввода
        /// </summary>
        private void Clean()
        {
            Server.Text = "";
            NameUser.Text = "";
            Password.Text = "";
            DB.Text = "";
            numericTimeout.Value = 30;
        }

        /// <summary>
        /// Перезапуск приложения
        /// </summary>
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Проверяем, заполнены ли обязательные поля
            if (string.IsNullOrWhiteSpace(Server.Text) ||
                string.IsNullOrWhiteSpace(NameUser.Text) ||
                string.IsNullOrWhiteSpace(DB.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля:\n- Сервер\n- Имя пользователя\n- База данных",
                    "Не все поля заполнены",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;

            // Временно сохраняем текущие настройки
            string oldHost = Connection.Host;
            string oldDatabase = Connection.Database;
            string oldUserId = Connection.UserId;
            string oldPassword = Connection.Password;

            // Устанавливаем новые настройки для проверки
            Connection.Host = Server.Text;
            Connection.Database = DB.Text;
            Connection.UserId = NameUser.Text;
            Connection.Password = Password.Text;

            bool success = Connection.TestConnection();

            // Восстанавливаем старые настройки
            Connection.Host = oldHost;
            Connection.Database = oldDatabase;
            Connection.UserId = oldUserId;
            Connection.Password = oldPassword;

            Cursor = Cursors.Default;

            if (success)
            {
                MessageBox.Show("✅ Подключение к базе данных успешно установлено!",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("❌ Не удалось подключиться к базе данных.\n\nПроверьте правильность введенных данных:\n" +
                    $"- Сервер: {Server.Text}\n" +
                    $"- База данных: {DB.Text}\n" +
                    $"- Имя пользователя: {NameUser.Text}",
                    "Ошибка подключения",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new SysAdmin().Show();
            this.Close();
        }
    }
}