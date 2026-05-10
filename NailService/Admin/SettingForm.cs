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
        private void Connection_Click(object sender, EventArgs e)
        {
            try
            {
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

                MessageBox.Show("Настройки сохранены успешно!");
                Clean();
                LoadCurrentSettings();
            }
            catch (Exception ex)
            {
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
    }
}