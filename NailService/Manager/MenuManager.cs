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
    public partial class MenuManager : Form
    {
        private string _fio;
        public MenuManager(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text = $"Менеджер: {FIO}";
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            Schedule schedule = new Schedule();
            schedule.Show();
            this.Hide();
        }
    }
}
