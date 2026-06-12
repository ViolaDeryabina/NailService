using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class SysAdmin : Form
    {
        private string _connection;
        public SysAdmin()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
        }

       

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 show = new Form1();
            show.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImportData importForm = new ImportData();
            importForm.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            settingForm.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CopyBD settingForm = new CopyBD();
            settingForm.Show();
            this.Hide();
        }
    }
}
