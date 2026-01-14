using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
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
            LoadRoles();
            User = user;
            IsEditMode = true;
            LoadTextBoxs();
        }

        private void LoadTextBoxs() {
            LastName.Text = User.LastName;
            FirstName.Text = User.FirstName;
            MiddleName.Text = User.MiddleName;
            Login.Text = User.Login;
            Password.Text = "";
            Password.PasswordChar = '*';
            RoleCb.Text = User.RoleName;

            if (IsAdminUser())
            {
                RoleCb.Enabled = false;
            }
            else
            {
                RoleCb.Enabled = true;
            }
            
        }


        private void LoadRoles()
        {
            // Загрузка ролей в комбобокс
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

        private bool IsAdminUser()
        {
            // Проверяем, является ли пользователь администратором
            // Можно проверять по RoleId или по названию роли
            return User.RoleName?.ToLower() == "админ" ||
                   User.RoleName?.ToLower() == "administrator" ||
                   User.RoleName?.ToLower() == "admin" ||
                   User.RoleId == GetAdminRoleId(); // если знаете ID роли админа
        }

        // Метод для получения ID роли администратора (если известно)
        private int GetAdminRoleId()
        {
            return 2; 
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
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
                MessageBox.Show("Введите фамилию");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин");
                return false;
            }

            return true;
        }

        private void SaveUserData()
        {
            User.LastName = LastName.Text.Trim();
            User.FirstName = FirstName.Text.Trim();
            User.MiddleName = MiddleName.Text.Trim();
            User.Login = Login.Text.Trim();
            User.RoleId = (int)RoleCb.SelectedValue;

            // Обновляем пароль только если он был изменен
            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                // Хешируем новый пароль

                User.Password = MySQLHelper.GetHash(Password.Text);
                
            }
        }

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
    }
}
