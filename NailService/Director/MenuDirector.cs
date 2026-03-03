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
    /// <summary>
    /// Форма главного меню для пользователей с ролью "Директор"
    /// Предоставляет доступ к отчетам и просмотру услуг
    /// </summary>
    public partial class MenuDirector : Form
    {
        private string _fio;

        /// <summary>
        /// Конструктор формы меню директора
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        public MenuDirector(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text = $"Директор: {FIO}";
            _fio = FIO;
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

        /// <summary>
        /// Открытие формы отчетов
        /// </summary>
        private void Reports_Click(object sender, EventArgs e)
        {
            ShowReports showReports = new ShowReports(_fio, 1);
            showReports.Show();
            this.Hide();
        }

        /// <summary>
        /// Открытие формы просмотра услуг
        /// </summary>
        private void Seervices_Click(object sender, EventArgs e)
        {
            Show show = new Show(_fio, 1);
            show.Show();
            this.Hide();
        }
    }
}