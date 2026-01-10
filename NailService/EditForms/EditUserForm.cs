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

        public EditUserForm(UserModel user)
        {
            _connection = Connection.ConnectionString;
            InitializeComponent();
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
            Password.Text = User.Password;
            RoleCb.Text = User.RoleName;

            if (IsAdminUser())
            {
                RoleCb.Enabled = false;
            }
            else
            {
                RoleCb.Enabled = true;
            }
            //*****************************
            if (IsMasterUser())
            {
                phoneText.Visible = true;
                phoneTextBox.Visible = true;
            }
            else
            {
                phoneText.Visible = false;
                phoneTextBox.Visible = false;
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
        private bool IsMasterUser()
        {

            // Проверяем, является ли пользователь администратором
            // Можно проверять по RoleId или по названию роли
            return User.RoleName?.ToLower() == "мастер" ||
                   User.RoleName?.ToLower() == "Мастер" ||
                   User.RoleId == GetMasterRoleId(); // если знаете ID роли админа
        }

        private int GetMasterRoleId()
        {
            return 3;
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
            User.Password = Password.Text;
            User.RoleId = (int)RoleCb.SelectedValue;
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
