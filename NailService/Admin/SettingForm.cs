using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
    public partial class SettingForm : Form
    {
        private InactivityController _inactivityController;

        /// <summary>
        /// Конструктор формы настроек подключения к базе данных
        /// </summary>
        public SettingForm(InactivityController inactivityController = null)
        {
            InitializeComponent();
            _inactivityController = inactivityController;
            LoadCurrentSettings();
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
                // Проверяем, изменились ли настройки
                bool settingsChanged =
                    Server.Text != Properties.Settings.Default.host ||
                    NameUser.Text != Properties.Settings.Default.uid ||
                    Password.Text != Properties.Settings.Default.pwd ||
                    DB.Text != Properties.Settings.Default.database ||
                    (int)numericTimeout.Value != Properties.Settings.Default.inactivityTimeout;

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

                // Проверяем соединение с новыми параметрами
                Cursor = Cursors.WaitCursor;
                bool connectionSuccess = await Task.Run(() => Connection.TestConnection());
                Cursor = Cursors.Default;

                if (!connectionSuccess)
                {
                    DialogResult retry = MessageBox.Show(
                        "Не удалось подключиться к базе данных с указанными параметрами.\n\n" +
                        "Проверьте правильность введенных данных.\n\n" +
                        "Сохранить настройки все равно?",
                        "Ошибка подключения",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (retry != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // Сохраняем настройки
                Properties.Settings.Default["host"] = Server.Text;
                Properties.Settings.Default["uid"] = NameUser.Text;
                Properties.Settings.Default["pwd"] = Password.Text;
                Properties.Settings.Default["database"] = DB.Text;

                int newTimeout = (int)numericTimeout.Value;
                Properties.Settings.Default["inactivityTimeout"] = newTimeout;
                if (_inactivityController != null)
                {
                    _inactivityController.UpdateTimeout(newTimeout);
                }

                Properties.Settings.Default.Save();

                // Обновляем статические свойства класса Connection
                Connection.Host = Server.Text;
                Connection.Database = DB.Text;
                Connection.UserId = NameUser.Text;
                Connection.Password = Password.Text;

                // Спрашиваем о перезапуске
                DialogResult restart = MessageBox.Show(
                    connectionSuccess
                        ? "Настройки успешно сохранены!\n\nДля применения всех изменений необходимо перезапустить приложение.\n\nПерезапустить сейчас?"
                        : "Настройки сохранены, но подключение не удалось.\n\nДля применения изменений необходимо перезапустить приложение.\n\nПерезапустить сейчас?",
                    "Сохранение настроек",
                    MessageBoxButtons.YesNo,
                    connectionSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

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
            this.Hide();
        }
    }
}