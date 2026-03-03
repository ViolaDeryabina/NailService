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
    /// Форма главного меню для пользователей с ролью "Менеджер"
    /// Предоставляет доступ к расписанию и просмотру списков
    /// </summary>
    public partial class MenuManager : Form
    {
        private string _fio;

        /// <summary>
        /// Конструктор формы меню менеджера
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        public MenuManager(string FIO)
        {
            InitializeComponent();
            _fio = FIO;
            FIOlabel.Text = $"Менеджер: {_fio}";
        }

        /// <summary>
        /// Открытие формы расписания (запись клиентов)
        /// </summary>
        private void RecordButton_Click(object sender, EventArgs e)
        {
            Schedule schedule = new Schedule(_fio, 4, 0);
            schedule.Show();
            this.Hide();
        }

        /// <summary>
        /// Открытие формы просмотра списков (клиенты, услуги)
        /// </summary>
        private void ListButton_Click(object sender, EventArgs e)
        {
            Show show = new Show(_fio, 4);
            show.Show();
            this.Hide();
        }

        /// <summary>
        /// Выход из учетной записи и возврат на форму входа
        /// </summary>
        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 show = new Form1();
            show.Show();
            this.Hide();
        }
    }
}