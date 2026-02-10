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
                        try
                        {
                            con.Open();

                            string passwordHash = MySQLHelper.GetHash(Password.Text);

                            // Добавляем проверку IsActive = 1
                            string query = @"SELECT Count(*) FROM users 
                                   WHERE Login = @Login 
                                   AND Password = @Password 
                                   AND IsActive = 1";

                            MySqlCommand cmd = new MySqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@Login", Login.Text);
                            cmd.Parameters.AddWithValue("@Password", passwordHash);

                            int count = Convert.ToInt32(cmd.ExecuteScalar());

                            if (count > 0)
                            {
                                var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                                string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                                if (role != null && FIO != null)
                                {
                                    int roleId = MySQLHelper.GetRoleId(Login.Text, passwordHash);

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
                            }
                            else
                            {
                                // Проверяем, существует ли пользователь, но неактивен
                                string checkInactiveQuery = @"SELECT Count(*) FROM users 
                                                     WHERE Login = @Login 
                                                     AND Password = @Password 
                                                     AND IsActive = 0";

                                MySqlCommand checkCmd = new MySqlCommand(checkInactiveQuery, con);
                                checkCmd.Parameters.AddWithValue("@Login", Login.Text);
                                checkCmd.Parameters.AddWithValue("@Password", passwordHash);

                                int inactiveCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                                if (inactiveCount > 0)
                                {
                                    MessageBox.Show("Ваша учетная запись отключена. Обратитесь к администратору.",
                                                  "Доступ запрещен",
                                                  MessageBoxButtons.OK,
                                                  MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show("Неверный логин или пароль", "Ошибка");
                                }

                                Login.Text = "";
                                Password.Text = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка");
                        }
                        finally
                        {
                            con.Close();
                        }
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
