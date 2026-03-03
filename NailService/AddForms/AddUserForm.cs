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
        private Show _showForm; // Ссылка на форму Show для вызова методов проверки/восстановления

        /// <summary>
        /// Конструктор формы добавления пользователя
        /// </summary>
        /// <param name="showForm">Ссылка на главную форму для проверки существующих пользователей</param>
        public AddUserForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewUser = new UserModel();
            LoadRoles();
        }

        /// <summary>
        /// Загрузка доступных ролей из базы данных
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

        /// <summary>
        /// Обработчик кнопки "Сохранить" - валидация и сохранение пользователя
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                if (_showForm != null && CheckAndRestoreInactiveUser())
                {
                    return; // Пользователь восстановлен, форма закрывается
                }

                if (AddNewUser())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы без сохранения
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        /// <returns>true если данные корректны</returns>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Введите пароль пользователя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password.Focus();
                return false;
            }

            if (IsActiveUserExists())
            {
                MessageBox.Show("Пользователь с таким логином уже существует и активен", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                Login.Focus();
                Login.SelectAll();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка существования активного пользователя с таким логином
        /// </summary>
        private bool IsActiveUserExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Login = @Login AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", Login.Text.Trim());

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки логина: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // При ошибке блокируем добавление для безопасности
            }
        }

        /// <summary>
        /// Проверка и восстановление неактивного пользователя
        /// </summary>
        /// <returns>true если пользователь восстановлен и форма закрыта</returns>
        private bool CheckAndRestoreInactiveUser()
        {
            try
            {
                string lastName = LastName.Text.Trim();
                string firstName = FirstName.Text.Trim();
                string login = Login.Text.Trim();

                var (exists, isActive, userId) = _showForm.CheckUserExists(lastName, firstName, login);

                if (exists && !isActive)
                {
                    var result = MessageBox.Show(
                        $"Найден неактивный пользователь с такими данными:\n" +
                        $"ФИО: {lastName} {firstName}\n" +
                        $"Логин: {login}\n\n" +
                        "Восстановить этого пользователя с новыми данными?",
                        "Восстановление пользователя",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveUserData();
                        bool restored = _showForm.RestoreUser(userId, NewUser);

                        if (restored)
                        {
                            MessageBox.Show("Пользователь успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось восстановить пользователя", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        /// <summary>
        /// Добавление нового пользователя или восстановление неактивного
        /// </summary>
        private bool AddNewUser()
        {
            try
            {
                SaveUserData();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Поиск неактивного пользователя с таким логином
                    string checkQuery = "SELECT IDUser FROM Users WHERE Login = @Login AND IsActive = 0";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Login", NewUser.Login);

                    object inactiveUserId = checkCmd.ExecuteScalar();

                    if (inactiveUserId != null && inactiveUserId != DBNull.Value)
                    {
                        // Восстановление неактивного пользователя
                        int userId = Convert.ToInt32(inactiveUserId);

                        string updateQuery = @"UPDATE Users 
                                            SET LastName = @LastName,
                                                FirstName = @FirstName,
                                                MiddleName = @MiddleName,
                                                Password = @Password,
                                                Role = @Role,
                                                IsActive = 1
                                            WHERE IDUser = @UserId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                        updateCmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                        updateCmd.Parameters.AddWithValue("@FirstName", NewUser.FirstName);
                        updateCmd.Parameters.AddWithValue("@MiddleName", NewUser.MiddleName);
                        updateCmd.Parameters.AddWithValue("@Password", NewUser.Password);
                        updateCmd.Parameters.AddWithValue("@Role", NewUser.RoleId);

                        int updatedRows = updateCmd.ExecuteNonQuery();

                        if (updatedRows > 0)
                        {
                            MessageBox.Show("Пользователь успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                    else
                    {
                        // Создание нового пользователя
                        string insertQuery = @"INSERT INTO Users 
                                            (LastName, FirstName, MiddleName, Login, Password, Role, IsActive) 
                                            VALUES (@LastName, @FirstName, @MiddleName, @Login, @Password, @Role, 1)";

                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                        insertCmd.Parameters.AddWithValue("@FirstName", NewUser.FirstName);
                        insertCmd.Parameters.AddWithValue("@MiddleName", NewUser.MiddleName);
                        insertCmd.Parameters.AddWithValue("@Login", NewUser.Login);
                        insertCmd.Parameters.AddWithValue("@Password", NewUser.Password);
                        insertCmd.Parameters.AddWithValue("@Role", NewUser.RoleId);

                        int result = insertCmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Пользователь успешно добавлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }

                    MessageBox.Show("Не удалось добавить пользователя", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Ошибка дублирования уникального ключа
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Сохранение данных из формы в объект NewUser
        /// </summary>
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
        /// Проверка при потере фокуса поля логина
        /// </summary>
        private void Login_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Login.Text) && _showForm != null)
            {
                CheckForInactiveUserHint();
            }
        }

        /// <summary>
        /// Проверка наличия неактивного пользователя с таким логином
        /// </summary>
        private void CheckForInactiveUserHint()
        {
            try
            {
                string login = Login.Text.Trim();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT u.IDUser, u.LastName, u.FirstName, u.MiddleName, 
                                           u.IsActive, r.RoleName
                                    FROM Users u 
                                    INNER JOIN Role r ON u.Role = r.IDRole
                                    WHERE u.Login = @Login AND u.IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32("IDUser");
                            string lastName = reader["LastName"]?.ToString() ?? "";
                            string firstName = reader["FirstName"]?.ToString() ?? "";
                            string middleName = reader["MiddleName"]?.ToString() ?? "";
                            string roleName = reader["RoleName"]?.ToString() ?? "";

                            // Здесь можно добавить визуальную подсказку
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }
    }
}