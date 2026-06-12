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
        private int _userId; // ID пользователя (менеджера)

        public MenuManager(string FIO, int userId)
        {
            InitializeComponent();
            _fio = FIO;
            _userId = userId;
            FIOlabel.Text = $"Менеджер: {FIO}";
        }

        /// <summary>
        /// Открытие формы расписания (запись клиентов)
        /// </summary>
        private void RecordButton_Click(object sender, EventArgs e)
        {
            Schedule schedule = new Schedule(_fio, 4,_userId);
            schedule.Show();
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

        private void MenuManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ServiceForm show = new ServiceForm(_fio,4, _userId);
            show.Show();
            this.Hide();
        }
    }
}