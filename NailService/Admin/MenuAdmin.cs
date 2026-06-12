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
        private string _login;

        /// <summary>
        /// Конструктор формы главного меню администратора
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        public MenuAdmin(string FIO, string login=null)
        {
            InitializeComponent();
            FIOlabel.Text = $"Админ: {FIO}";
            _fio = FIO;
            _login = login;
        }

        /// <summary>
        /// Открытие формы со списком записей
        /// </summary>
        private void ListButton_Click(object sender, EventArgs e)
        {
            Show show = new Show(_fio, 2, _login);
            show.Show();
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

        private void MenuAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UserShow form1 = new UserShow(_fio, 2);
            form1.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MasterForm form1 = new MasterForm(_fio, 2);
            form1.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ServiceForm form1 = new ServiceForm(_fio, 2, 3);
            form1.Show();
            this.Hide();
        }
    }
}