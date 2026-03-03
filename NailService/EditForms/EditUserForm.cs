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
    /// <summary>
    /// Форма для редактирования данных существующего пользователя
    /// Позволяет изменять ФИО, логин, пароль и роль (с ограничениями для администраторов)
    /// </summary>
    public partial class EditUserForm : Form
    {
        private string _connection;
        public UserModel User { get; private set; }
        public bool IsEditMode { get; private set; }
        private bool _isPasswordChanged = false;

        /// <summary>
        /// Конструктор формы редактирования пользователя
        /// </summary>
        /// <param name="user">Объект пользователя с текущими данными</param>
        public EditUserForm(UserModel user)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            LoadRoles();
            User = user;
            IsEditMode = true;
            LoadTextBoxs();
        }

        #region Загрузка данных

        /// <summary>
        /// Загрузка данных пользователя в поля формы
        /// </summary>
        private void LoadTextBoxs()
        {
            LastName.Text = User.LastName;
            FirstName.Text = User.FirstName;
            MiddleName.Text = User.MiddleName;
            Login.Text = User.Login;
            Password.Text = "";
            Password.PasswordChar = '*';
            RoleCb.Text = User.RoleName;

            // Блокировка изменения роли для администраторов
            RoleCb.Enabled = !IsAdminUser();
        }

        /// <summary>
        /// Загрузка списка ролей из базы данных
        /// </summary>
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

        /// <summary>
        /// Сохранение изменений и закрытие формы
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Отмена редактирования и закрытие формы
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
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

            return true;
        }

        /// <summary>
        /// Сохранение данных из формы в объект User
        /// </summary>
        private void SaveUserData()
        {
            User.LastName = LastName.Text.Trim();
            User.FirstName = FirstName.Text.Trim();
            User.MiddleName = MiddleName.Text.Trim();
            User.Login = Login.Text.Trim();
            User.RoleId = (int)RoleCb.SelectedValue;

            if (_isPasswordChanged && !string.IsNullOrWhiteSpace(Password.Text))
            {
                User.Password = MySQLHelper.GetHash(Password.Text);
            }
        }

        #endregion

        #region Проверка прав

        /// <summary>
        /// Проверка, является ли редактируемый пользователь администратором
        /// </summary>
        /// <returns>true если пользователь администратор</returns>
        private bool IsAdminUser()
        {
            return User.RoleName?.ToLower() == "админ" ||
                   User.RoleName?.ToLower() == "administrator" ||
                   User.RoleName?.ToLower() == "admin" ||
                   User.RoleId == GetAdminRoleId();
        }

        /// <summary>
        /// Получение ID роли администратора (фиксированное значение)
        /// </summary>
        private int GetAdminRoleId()
        {
            return 2;
        }

        #endregion

        #region Фильтрация ввода

        /// <summary>
        /// Фильтрация ввода в поле фамилии (только русские буквы)
        /// </summary>
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

        /// <summary>
        /// Фильтрация ввода в поле имени (только русские буквы)
        /// </summary>
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

        /// <summary>
        /// Фильтрация ввода в поле отчества (только русские буквы)
        /// </summary>
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

        /// <summary>
        /// Фильтрация ввода в поле логина (латиница, цифры, _, .)
        /// </summary>
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

        /// <summary>
        /// Отслеживание изменения пароля
        /// </summary>
        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Password.Text))
            {
                _isPasswordChanged = true;
            }
        }

        #endregion
    }
}