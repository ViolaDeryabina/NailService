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
    /// <summary>
    /// Главная форма авторизации в приложении
    /// Обрабатывает вход пользователей с проверкой учетных данных и ролей
    /// </summary>
    public partial class Form1 : Form
    {
        private string _connection;
        private PasswordVisibilityToggle _passwordToggle;

        /// <summary>
        /// Конструктор формы авторизации
        /// Инициализирует компоненты и настраивает переключатель видимости пароля
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;

            _passwordToggle = new PasswordVisibilityToggle(
                Eye,                    // PictureBox для переключения
                Password,               // TextBox с паролем
                Resources.eyeOpen,      // Иконка открытого глаза
                Resources.eyeClose      // Иконка закрытого глаза
            );
        }

        /// <summary>
        /// Обработчик кнопки авторизации
        /// Проверяет подключение к БД, валидирует учетные данные и перенаправляет на соответствующую форму
        /// </summary>
        private void Autorization_Click(object sender, EventArgs e)
        {
            // Проверка подключения к базе данных
            if (Connection.TestConnection())
            {
                // Проверка заполнения полей
                if (Login.Text == "" || Password.Text == "")
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }else if (Login.Text == "SysAdmin" || Password.Text == "SysAdmin")
                {
                    SysAdminForm menuDirector = new SysAdminForm();
                    menuDirector.Show();
                    this.Hide();
                }
                else
                {
                    // Попытка авторизации
                    using (MySqlConnection con = new MySqlConnection(_connection))
                    {
                        try
                        {
                            con.Open();

                            string passwordHash = MySQLHelper.GetHash(Password.Text);

                            // Проверка наличия активного пользователя с указанными логином и паролем
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
                                // Получение роли и ФИО пользователя
                                var role = MySQLHelper.GetRoleName(Login.Text, passwordHash);
                                string FIO = MySQLHelper.GetLastNameWithInitials(Login.Text, passwordHash);

                                if (role != null && FIO != null)
                                {
                                    int roleId = MySQLHelper.GetRoleId(Login.Text, passwordHash);
                                    int masterID = EditUserClass.GetMasterId(Login.Text, passwordHash);

                                    // Перенаправление на соответствующую форму в зависимости от роли
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
                                                MenuAdmin menuAdmin = new MenuAdmin(FIO, Login.Text);
                                                menuAdmin.Show();
                                                this.Hide();
                                                break;
                                            }
                                        case "Мастер":
                                            {
                                                MenuMaster menuMaster = new MenuMaster(FIO, masterID);
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
                                // Проверка на неактивного пользователя
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
                                    MessageBox.Show("Неверный логин или пароль", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                // Очистка полей ввода
                                Login.Text = "";
                                Password.Text = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Ошибка подключения к базе данных
                MessageBox.Show("Ошибка подключения к базе данных. Проверьте настройки подключения.",
                    "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Открытие формы настроек
                SettingForm settingForm = new SettingForm();
                settingForm.Show();
                this.Hide();
            }
        }

        /// <summary>
        /// Обработчик кнопки выхода из приложения
        /// </summary>
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }
    }
}