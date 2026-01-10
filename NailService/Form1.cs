using MySql.Data.MySqlClient;
using NailService.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
    public partial class Form1 : Form
    {
        private string _connection;
        private PasswordVisibilityToggle _passwordToggle;
        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            _passwordToggle = new PasswordVisibilityToggle(
            Eye,     // PictureBox на форме
            Password,
            Resources.eyeOpen,
            Resources.eyeClose// TextBox с паролем
        );
        }
        
        private void Autorization_Click(object sender, EventArgs e)
        {
            if (Connection.TestConnection())
            {
                if (Login.Text == "" || Password.Text == "")
                {
                    MessageBox.Show("Заполните все поле!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using (MySqlConnection con = new MySqlConnection(_connection))
                    {
                        con.Open();

                        string passwordHash = MySQLHelper.GetHash(Password.Text);

                        string query = $"SELECT Count(*) FROM users Where Login='{Login.Text}' and Password = '{passwordHash}';";

                        MySqlCommand cmd = new MySqlCommand(query, con);
                        cmd.ExecuteScalar();


                        var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);                       
                        string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                        if(role != null && FIO != null){
                            switch (role)
                            {
                                case "Директор":
                                    {
                                        MenuDirector menuDirector = new MenuDirector(FIO);
                                        menuDirector.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Админ":
                                    {
                                        MenuAdmin menuAdmin = new MenuAdmin(FIO);
                                        menuAdmin.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Мастер":
                                    {
                                        MenuMaster menuMaster = new MenuMaster(FIO);
                                        menuMaster.Show();
                                        this.Hide();
                                        break;
                                    }
                                case "Менеджер":
                                    {
                                        MenuManager menuManager = new MenuManager(FIO);
                                        menuManager.Show();
                                        this.Hide();
                                        break;
                                    }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль", "Ошибка");
                            Login.Text = "";
                            Password.Text = "";
                        }


                         con.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка подключения: Измените настройки подключения", "Ошибка");

                SettingForm settingForm = new SettingForm();
                settingForm.Show();
                this.Hide();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
