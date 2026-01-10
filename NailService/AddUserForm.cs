using MySql.Data.MySqlClient;
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
    public partial class AddUserForm : Form
    {
        private string _connection;
        public UserModel NewUser { get; private set; }
        public AddUserForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewUser = new UserModel();
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDRole, RoleName FROM Role";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    RoleCb.DataSource = dt;
                    RoleCb.DisplayMember = "RoleName";
                    RoleCb.ValueMember = "IDRole";

                    // Устанавливаем значение по умолчанию (например, первую роль)
                    if (RoleCb.Items.Count > 0)
                    {
                        RoleCb.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                if (AddUserToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            // Проверка логина
            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                return false;
            }

            // Проверка пароля
            if (string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Введите пароль пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password.Focus();
                return false;
            }

            // Проверка уникальности логина
            if (!IsLoginUnique(Login.Text.Trim()))
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            return true;
        }

        private bool IsLoginUnique(string login)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Login = @Login";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки логина: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveUserData()
        {
            string passwordHash = MySQLHelper.GetHash(Password.Text);

            NewUser.LastName = LastName.Text.Trim();
            NewUser.FirstName = FirstName.Text.Trim();
            NewUser.MiddleName = MiddleName.Text.Trim();
            NewUser.Login = Login.Text.Trim();
            NewUser.Password = passwordHash;
            NewUser.RoleId = (int)RoleCb.SelectedValue;
        }



        private bool AddUserToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"INSERT INTO Users 
                                    (LastName, FirstName, MiddleName, Login, Password, Role) 
                                    VALUES (@LastName, @FirstName, @MiddleName, @Login, @Password, @Role)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", NewUser.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", NewUser.MiddleName);
                    cmd.Parameters.AddWithValue("@Login", NewUser.Login);
                    cmd.Parameters.AddWithValue("@Password", NewUser.Password);
                    cmd.Parameters.AddWithValue("@Role", NewUser.RoleId);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить пользователя", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void LastName_TextChanged(object sender, EventArgs e)
        {
            // Запоминаем позицию курсора
            int selectionStart = LastName.SelectionStart;

            // Фильтруем текст, оставляя только русские буквы
            string filteredText = RussianLettersValidator.FilterToRussianLetters(LastName.Text);

            // Если текст изменился после фильтрации
            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;

                // Восстанавливаем позицию курсора
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            // Запоминаем позицию курсора
            int selectionStart = FirstName.SelectionStart;

            // Фильтруем текст, оставляя только русские буквы
            string filteredText = RussianLettersValidator.FilterToRussianLetters(FirstName.Text);

            // Если текст изменился после фильтрации
            if (filteredText != FirstName.Text)
            {
                FirstName.Text = filteredText;

                // Восстанавливаем позицию курсора
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
            }
        }

        private void MiddleName_TextChanged(object sender, EventArgs e)
        {
            // Запоминаем позицию курсора
            int selectionStart = MiddleName.SelectionStart;

            // Фильтруем текст, оставляя только русские буквы
            string filteredText = RussianLettersValidator.FilterToRussianLetters(MiddleName.Text);

            // Если текст изменился после фильтрации
            if (filteredText != MiddleName.Text)
            {
                MiddleName.Text = filteredText;

                // Восстанавливаем позицию курсора
                MiddleName.SelectionStart = Math.Min(selectionStart, MiddleName.Text.Length);
            }
        }
    }
}
