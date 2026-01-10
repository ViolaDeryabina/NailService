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
        public SettingForm()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                Server.Text = Properties.Settings.Default.host;
                Name.Text = Properties.Settings.Default.uid;
                Password.Text = Properties.Settings.Default.pwd;
                DB.Text = Properties.Settings.Default.database;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}");
            }
        }
        private void Connection_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default["host"] = Server.Text;
                Properties.Settings.Default["uid"] = Name.Text;
                Properties.Settings.Default["pwd"] = Password.Text;
                Properties.Settings.Default["database"] = DB.Text;

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
        private void Clean()
        {
            Server.Text = "";
            Name.Text = "";
            Password.Text = "";
            DB.Text = "";
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
