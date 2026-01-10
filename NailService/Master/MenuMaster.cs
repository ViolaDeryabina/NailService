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
    public partial class MenuMaster : Form
    {
        private string _fio;
        public MenuMaster(string FIO)
        {
            InitializeComponent();
            FIOlabel.Text = $"Мастер: {FIO}";

        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Close();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
