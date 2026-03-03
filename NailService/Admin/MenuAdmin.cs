using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class MenuAdmin : Form
    {
        private string _fio;

        /// <summary>
        /// Конструктор формы главного меню администратора
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        public MenuAdmin(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text = $"Админ: {FIO}";
            _fio = FIO;
        }

        /// <summary>
        /// Открытие формы со списком записей
        /// </summary>
        private void ListButton_Click(object sender, EventArgs e)
        {
            Show show = new Show(_fio, 2);
            show.Show();
            this.Hide();
        }

        /// <summary>
        /// Открытие формы настроек приложения
        /// </summary>
        private void Settings_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            settingForm.Show();
            this.Hide();
        }

        /// <summary>
        /// Открытие формы отчетов
        /// </summary>
        private void Orders_Click(object sender, EventArgs e)
        {
            ShowReports showReports = new ShowReports(_fio, 2);
            showReports.Show();
            this.Hide();
        }

        /// <summary>
        /// Выход из учетной записи и возврат на форму входа
        /// </summary>
        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}