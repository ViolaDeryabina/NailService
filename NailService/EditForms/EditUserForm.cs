using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Форма для редактирования данных существующего пользователя
    /// </summary>
    public partial class EditUserForm : Form
    {
        private string _connection;
        public UserModel User { get; private set; }
        public bool IsEditMode { get; private set; }
        private bool _isPasswordChanged = false;

        public EditUserForm(UserModel user)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            User = user;
            IsEditMode = true;

            LoadRoles();
            LoadTextBoxs();
        }

        #region Загрузка данных

        private void LoadTextBoxs()
        {
            LastName.Text = User.LastName;
            FirstName.Text = User.FirstName;
            MiddleName.Text = User.MiddleName;
            Login.Text = User.Login;
            Password.Text = "";
            Password.PasswordChar = '*';

            if (RoleCb != null)
            {
                RoleCb.Text = User.RoleName;
                RoleCb.Enabled = !IsCurrentUserAdmin();
            }
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        #endregion

        #region Валидация и сохранение

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                UpdateUserInDatabase();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                return false;
            }

            if (Login.Text != User.Login && IsLoginExists())
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            return true;
        }

        private bool IsLoginExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Login = @Login AND IDUser != @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", Login.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserId", User.UserId);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
                return true;
            }
        }

        private void SaveUserData()
        {
            User.LastName = LastName.Text.Trim();
            User.FirstName = FirstName.Text.Trim();
            User.MiddleName = MiddleName.Text.Trim();
            User.Login = Login.Text.Trim();
            User.RoleId = (int)RoleCb.SelectedValue;
            User.RoleName = RoleCb.Text;

            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                User.Password = MySQLHelper.GetHash(Password.Text);
            }
        }

        private void UpdateUserInDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query;
                    MySqlCommand cmd;

                    if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
                    {
                        query = @"UPDATE Users 
                                  SET LastName = @LastName,
                                      FirstName = @FirstName,
                                      MiddleName = @MiddleName,
                                      Login = @Login,
                                      Password = @Password,
                                      Role = @Role
                                  WHERE IDUser = @UserId";
                        cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@Password", User.Password);
                    }
                    else
                    {
                        query = @"UPDATE Users 
                                  SET LastName = @LastName,
                                      FirstName = @FirstName,
                                      MiddleName = @MiddleName,
                                      Login = @Login,
                                      Role = @Role
                                  WHERE IDUser = @UserId";
                        cmd = new MySqlCommand(query, connection);
                    }

                    cmd.Parameters.AddWithValue("@UserId", User.UserId);
                    cmd.Parameters.AddWithValue("@LastName", User.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", User.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(User.MiddleName) ? (object)DBNull.Value : User.MiddleName);
                    cmd.Parameters.AddWithValue("@Login", User.Login);
                    cmd.Parameters.AddWithValue("@Role", User.RoleId);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        #endregion

        #region Проверка прав

        private bool IsCurrentUserAdmin()
        {
            return User.RoleName?.ToLower() == "админ";
        }

        #endregion

        #region Фильтрация ввода

        private void LastName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(LastName.Text);

            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = FirstName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(FirstName.Text);

            if (filteredText != FirstName.Text)
            {
                FirstName.Text = filteredText;
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
            }
        }

        private void MiddleName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = MiddleName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(MiddleName.Text);

            if (filteredText != MiddleName.Text)
            {
                MiddleName.Text = filteredText;
                MiddleName.SelectionStart = Math.Min(selectionStart, MiddleName.Text.Length);
            }
        }

        private void Login_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Login.SelectionStart;
            string filteredText = new string(Login.Text
                .Where(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                           char.IsDigit(c) || c == '_' || c == '.')
                .ToArray());

            if (filteredText != Login.Text)
            {
                Login.Text = filteredText;
                Login.SelectionStart = Math.Min(selectionStart, Login.Text.Length);
            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                _isPasswordChanged = true;
            }
        }

        private void LastName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                string name = LastName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                    LastName.Text = name;
                }
            }
        }

        private void FirstName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                string name = FirstName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                    FirstName.Text = name;
                }
            }
        }

        private void MiddleName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MiddleName.Text))
            {
                string name = MiddleName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                    MiddleName.Text = name;
                }
            }
        }

        #endregion
    }
}