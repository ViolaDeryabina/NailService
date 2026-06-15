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
        private Show _showForm;
        private bool _isMasterMode;
        private Form _parentForm;

        public AddUserForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            _isMasterMode = false;
            NewUser = new UserModel();
            LoadRoles();
            ShowMasterFields(false);
            this.Text = "Добавление пользователя";
            label1.Text = "Добавление пользователя";
        }

        public AddUserForm(Form parentForm, bool isMasterMode)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _parentForm = parentForm;
            _isMasterMode = isMasterMode;
            NewUser = new UserModel();
            LoadRolesForMaster();
            ShowMasterFields(true);
            this.Text = "Добавление мастера";
            label1.Text = "Добавление мастера";
        }

        private void ShowMasterFields(bool show)
        {
            if (txtDescription != null)
                txtDescription.Visible = show;
            if (lblDescription != null)
                lblDescription.Visible = show;
            if (txtPhone != null)
                txtPhone.Visible = show;
            if (lblPhone != null)
                lblPhone.Visible = show;

            if (show)
            {
                this.Size = new Size(624, 609);
                if (button4 != null)
                    button4.Location = new Point(366, 511);
                if (btnCancel != null)
                    btnCancel.Location = new Point(12, 511);
            }
            else
            {
                this.Size = new Size(624, 463);
                if (button4 != null)
                    button4.Location = new Point(366, 356);
                if (btnCancel != null)
                    btnCancel.Location = new Point(12, 356);
            }
        }

        private void LoadRoles()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDRole, RoleName FROM Role WHERE RoleName != 'Мастер'";
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

        private void LoadRolesForMaster()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDRole, RoleName FROM Role WHERE RoleName = 'Мастер'";
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                if (_showForm != null && CheckAndRestoreInactiveUser())
                {
                    return;
                }

                if (_isMasterMode)
                {
                    AddNewMaster();
                }
                else
                {
                    AddNewUser();
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
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text))
            {
                MessageBox.Show("Введите логин", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Login.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password.Text))
            {
                MessageBox.Show("Введите пароль", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password.Focus();
                return false;
            }

            if (_isMasterMode)
            {
                if (string.IsNullOrWhiteSpace(txtPhone?.Text))
                {
                    MessageBox.Show("Введите телефон мастера", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Focus();
                    return false;
                }

                string phoneDigits = new string(txtPhone.Text.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 10)
                {
                    MessageBox.Show("Введите корректный номер телефона (минимум 10 цифр)", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Focus();
                    return false;
                }
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
                return true;
            }
        }

        /// <summary>
        /// Проверка существования пользователя для восстановления
        /// </summary>
        public (bool exists, bool isActive, int userId) CheckUserExists(string lastName, string firstName, string login)
        {
            using (var connection = Connection.GetConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"
                SELECT IDUser, IsActive 
                FROM users 
                WHERE (Login = @Login 
                       OR (LastName = @LastName AND FirstName = @FirstName))
                ORDER BY IsActive DESC
                LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32("IDUser");
                            bool isActive = reader.GetBoolean("IsActive");
                            return (true, isActive, userId);
                        }
                    }

                    return (false, false, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки пользователя: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return (false, false, 0);
                }
            }
        }

        private bool CheckAndRestoreInactiveUser()
        {
            if (_showForm == null) return false;

            try
            {
                string lastName = LastName.Text.Trim();
                string firstName = FirstName.Text.Trim();
                string login = Login.Text.Trim();

                var (exists, isActive, userId) = CheckUserExists(lastName, firstName, login);

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
                        bool restored = RestoreUser(userId, NewUser);

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
        /// Восстановление неактивного пользователя
        /// </summary>
        public bool RestoreUser(int userId, UserModel userData)
        {
            using (var connection = Connection.GetConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"
                UPDATE users 
                SET IsActive = 1,
                    Login = @Login,
                    Password = @Password,
                    Role = @Role,
                    LastName = @LastName,
                    FirstName = @FirstName,
                    MiddleName = @MiddleName
                WHERE IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Login", userData.Login);
                    cmd.Parameters.AddWithValue("@Password", userData.Password);
                    cmd.Parameters.AddWithValue("@Role", userData.RoleId);
                    cmd.Parameters.AddWithValue("@LastName", userData.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", userData.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(userData.MiddleName) ? (object)DBNull.Value : userData.MiddleName);

                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка восстановления пользователя: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private bool AddNewUser()
        {
            try
            {
                SaveUserData();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string checkQuery = "SELECT IDUser FROM Users WHERE Login = @Login AND IsActive = 0";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Login", NewUser.Login);

                    object inactiveUserId = checkCmd.ExecuteScalar();

                    if (inactiveUserId != null && inactiveUserId != DBNull.Value)
                    {
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
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                            return true;
                        }
                    }
                    else
                    {
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
                            MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                                     MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
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
                if (ex.Number == 1062)
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
        /// Добавление нового мастера (без транзакций)
        /// </summary>
        private bool AddNewMaster()
        {
            try
            {
                SaveUserData();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    int newUserId = 0;
                    bool userCreated = false;
                    bool masterCreated = false;

                    // 1. Создаем или восстанавливаем пользователя
                    string checkQuery = "SELECT IDUser FROM Users WHERE Login = @Login AND IsActive = 0";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Login", NewUser.Login);
                    object inactiveUserId = checkCmd.ExecuteScalar();

                    if (inactiveUserId != null && inactiveUserId != DBNull.Value)
                    {
                        // Восстанавливаем существующего пользователя
                        newUserId = Convert.ToInt32(inactiveUserId);
                        string updateQuery = @"UPDATE Users 
                                            SET LastName = @LastName,
                                                FirstName = @FirstName,
                                                MiddleName = @MiddleName,
                                                Password = @Password,
                                                Role = @Role,
                                                IsActive = 1
                                            WHERE IDUser = @UserId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@UserId", newUserId);
                        updateCmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                        updateCmd.Parameters.AddWithValue("@FirstName", NewUser.FirstName);
                        updateCmd.Parameters.AddWithValue("@MiddleName", NewUser.MiddleName);
                        updateCmd.Parameters.AddWithValue("@Password", NewUser.Password);
                        updateCmd.Parameters.AddWithValue("@Role", NewUser.RoleId);

                        int updatedRows = updateCmd.ExecuteNonQuery();
                        userCreated = updatedRows > 0;
                    }
                    else
                    {
                        // Создаем нового пользователя
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

                        insertCmd.ExecuteNonQuery();
                        newUserId = (int)insertCmd.LastInsertedId;
                        userCreated = true;
                    }

                    if (!userCreated)
                    {
                        MessageBox.Show("Не удалось создать пользователя для мастера", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // 2. Создаем запись мастера
                    string masterQuery = @"INSERT INTO Masters (User, Description, Phone, IsActive) 
                                           VALUES (@UserId, @Description, @Phone, 1)";

                    MySqlCommand masterCmd = new MySqlCommand(masterQuery, connection);
                    masterCmd.Parameters.AddWithValue("@UserId", newUserId);
                    masterCmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(txtDescription?.Text) ? (object)DBNull.Value : txtDescription.Text.Trim());
                    masterCmd.Parameters.AddWithValue("@Phone", txtPhone?.Text.Trim());

                    masterCmd.ExecuteNonQuery();
                    masterCreated = true;

                    if (masterCreated)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить мастера", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}", "Ошибка",
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

        private void Login_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Login.Text) && _showForm != null)
            {
                CheckForInactiveUserHint();
            }
        }

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
                            string lastName = reader["LastName"]?.ToString() ?? "";
                            string firstName = reader["FirstName"]?.ToString() ?? "";
                            string middleName = reader["MiddleName"]?.ToString() ?? "";
                            string roleName = reader["RoleName"]?.ToString() ?? "";

                            Login.BackColor = Color.LightYellow;
                            errorProvider1.SetError(Login, $"Найден неактивный пользователь: {lastName} {firstName}\nПри сохранении будет предложено восстановление");
                        }
                        else
                        {
                            Login.BackColor = Color.White;
                            errorProvider1.SetError(Login, "");
                        }
                    }
                }
            }
            catch
            {
                Login.BackColor = Color.White;
                errorProvider1.SetError(Login, "");
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

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = txtPhone.SelectionStart;
            string originalText = txtPhone.Text;

            string filteredText = InputValidator.FilterToPhone(originalText);
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                txtPhone.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                txtPhone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }

            // Проверка наличия неактивного клиента с таким телефоном
            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                CheckForInactiveClientHint();
            }
        }

        /// <summary>
        /// Корректировка позиции курсора после форматирования телефона
        /// </summary>
        private int GetAdjustedCursorPosition(int originalPosition, string oldText, string newText)
        {
            if (originalPosition >= oldText.Length)
                return newText.Length;

            int formatCharsBeforeCursor = 0;
            char[] formatChars = { '(', ')', ' ', '-', '+' };

            for (int i = 0; i < originalPosition && i < newText.Length; i++)
            {
                if (formatChars.Contains(newText[i]))
                {
                    formatCharsBeforeCursor++;
                }
            }

            return originalPosition + formatCharsBeforeCursor;
        }

        /// <summary>
        /// Проверка наличия неактивного клиента с введенным номером телефона
        /// </summary>
        private void CheckForInactiveClientHint()
        {
            try
            {
                string phoneDigits = GetPhoneDigits(txtPhone.Text);

                if (string.IsNullOrWhiteSpace(phoneDigits) || phoneDigits.Length < 10)
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT IDMaster, IsActive
                                    FROM Master 
                                    WHERE Phone = @Phone AND IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", phoneDigits);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Здесь можно добавить визуальную подсказку о найденном неактивном клиенте
                            // Например, изменить цвет фона или показать иконку
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}