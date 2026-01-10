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
    public partial class MenuDirector : Form
    {
        private string _fio;
        public MenuDirector(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text = $"Директор: {FIO}";
            _fio = FIO;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, EventArgs e)
        {
            ShowReports showReports = new ShowReports(_fio);
            showReports.Show();
            this.Hide();
        }
    }
}
