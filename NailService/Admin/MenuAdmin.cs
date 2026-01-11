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
        public MenuAdmin(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text =$"Админ: {FIO}";
            _fio = FIO;
        }

        private void ListButton_Click(object sender, EventArgs e)
        {
            Show show = new Show(_fio, 2);
            show.Show();
            this.Hide();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            settingForm.Show();
            this.Hide();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
