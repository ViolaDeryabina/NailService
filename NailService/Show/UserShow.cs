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
using static NailServiceApp.Utilities.NameFormatter;

namespace NailService
{
    public partial class UserShow : Form
    {
        private string _connection;
        private int _roleID;
        private int _currentUserId;
        private string _userName;
        private string _currentLogin;
        private EditUserClass _editUserClass;

        public UserShow(string FIO, int RoleID, string login = null)
        {
            InitializeComponent();
            _roleID = RoleID;
            _userName = FIO;
            _connection = Connection.ConnectionString;
            _currentLogin = login;
            _editUserClass = new EditUserClass();

            if (!string.IsNullOrEmpty(login))
            {
                _currentUserId = GetCurrentUserId(login);
            }
            else
            {
                _currentUserId = GetCurrentUserIdByFIO(FIO);
            }

            ConfigureDataGridView();
            LoadUsersData();
        }

        private int GetCurrentUserId(string login)
        {
            try
            {
                using (var connection = GetNewConnection())
                {
                    connection.Open();
                    string query = @"SELECT IDUser FROM Users WHERE Login = @Login";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private int GetCurrentUserIdByFIO(string fio)
        {
            try
            {
                using (var connection = GetNewConnection())
                {
                    connection.Open();
                    string query = @"SELECT IDUser FROM Users 
                           WHERE CONCAT(LastName, ' ', LEFT(FirstName, 1), '.') = @FIO 
                           LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@FIO", fio);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        private void ConfigureDataGridView()
        {
            dataGridViewUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewUsers.MultiSelect = false;
            dataGridViewUsers.RowHeadersVisible = false;
            dataGridViewUsers.ReadOnly = true;
            dataGridViewUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridViewUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 203, 219);
            dataGridViewUsers.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewUsers.MouseClick += DataGridViewUsers_MouseClick;
        }

        private void DataGridViewUsers_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewUsers.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewUsers.Rows.Count)
                {
                    dataGridViewUsers.ClearSelection();
                    dataGridViewUsers.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedUser();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedUser();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewUsers, e.Location);
                }
            }
        }

        private void EditSelectedUser()
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для редактирования");
                return;
            }
            var selectedRow = dataGridViewUsers.SelectedRows[0];
            OpenEditForm(selectedRow);
        }

        /// <summary>
        /// Открытие формы редактирования пользователя
        /// </summary>
        private void OpenEditForm(DataGridViewRow row)
        {
            try
            {
                int userId = Convert.ToInt32(row.Cells["ID"].Value);
                var userModel = _editUserClass.LoadUserById(userId);

                if (userModel != null)
                {
                    // ПРАВИЛЬНО: передаем данные пользователя в EditUserForm
                    var editForm = new EditUserForm(userModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _editUserClass.UpdateUserInDatabase(editForm.User);
                        LoadUsersData();
                        ShowInfo("Пользователь успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные пользователя");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void DeleteSelectedUser()
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для удаления");
                return;
            }

            var selectedRow = dataGridViewUsers.SelectedRows[0];
            int userId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
            string userName = selectedRow.Cells["ФИО"].Value?.ToString();
            string userLogin = selectedRow.Cells["Логин"].Value?.ToString();

            if (userId == _currentUserId || (!string.IsNullOrEmpty(userLogin) && userLogin == _currentLogin))
            {
                MessageBox.Show("Вы не можете удалить свой собственный аккаунт!\n\n" +
                               "Для безопасности системы нельзя удалить учётную запись, под которой вы вошли.",
                               "Ошибка",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
                return;
            }

            if (HasDependencies("user", userId))
            {
                MessageBox.Show(
                    $"Невозможно удалить пользователя '{userName}'.\n\n" +
                    "У пользователя есть связанные записи.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            using (var connection = GetNewConnection())
            {
                connection.Open();
                string roleQuery = "SELECT Role FROM Users WHERE IDUser = @UserId";
                MySqlCommand roleCmd = new MySqlCommand(roleQuery, connection);
                roleCmd.Parameters.AddWithValue("@UserId", userId);
                int roleId = Convert.ToInt32(roleCmd.ExecuteScalar());

                if (roleId == 2)
                {
                    string countAdminsQuery = "SELECT COUNT(*) FROM Users WHERE Role = 2 AND IsActive = 1";
                    MySqlCommand countAdminsCmd = new MySqlCommand(countAdminsQuery, connection);
                    int adminCount = Convert.ToInt32(countAdminsCmd.ExecuteScalar());

                    if (adminCount <= 1)
                    {
                        MessageBox.Show($"Нельзя удалить пользователя '{userName}'.\n\n" +
                                      "Это последний активный администратор в системе.",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить пользователя '{userName}'?\n\n" +
                "Пользователь будет помечен как неактивный, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                SoftDeleteUser(userId, userName);
            }
        }

        private bool HasDependencies(string tableName, int id)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    if (tableName == "user")
                    {
                        string userQuery = @"SELECT COUNT(*) FROM Record WHERE User = @Id";
                        MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
                        userCmd.Parameters.AddWithValue("@Id", id);

                        return Convert.ToInt32(userCmd.ExecuteScalar()) > 0;
                    }
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        private void SoftDeleteUser(int userId, string userName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT IsActive FROM Users WHERE IDUser = @UserId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@UserId", userId);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo("Пользователь уже отключен");
                            return;
                        }
                    }

                    string query = "UPDATE Users SET IsActive = 0 WHERE IDUser = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo($"Пользователь '{userName}' успешно отключен");
                        LoadUsersData();
                    }
                    else
                    {
                        ShowInfo("Пользователь не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения пользователя: {ex.Message}");
                }
            }
        }

        private void LoadUsersData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        u.IDUser as 'ID',
                        u.LastName as 'Фамилия',
                        u.FirstName as 'Имя',
                        u.MiddleName as 'Отчество',
                        u.Login as 'Логин',
                        u.Password as 'Пароль',
                        u.Role as 'RoleID',
                        r.RoleName as 'Роль',
                        u.IsActive as 'Активен'
                    FROM Users u
                    INNER JOIN Role r ON u.Role = r.IDRole
                    WHERE u.IsActive = 1
                    ORDER BY u.LastName, u.FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int));
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Логин", typeof(string));
                    maskedDt.Columns.Add("Пароль", typeof(string));
                    maskedDt.Columns.Add("RoleID", typeof(int));
                    maskedDt.Columns.Add("Роль", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = FormatToShortName(
                            row["Фамилия"]?.ToString(),
                            row["Имя"]?.ToString(),
                            row["Отчество"]?.ToString()
                        );

                        maskedDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            fullName,
                            row["Логин"],
                            row["Пароль"],
                            Convert.ToInt32(row["RoleID"]),
                            row["Роль"]
                        );
                    }

                    dataGridViewUsers.DataSource = maskedDt;
                    dataGridViewUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewUsers.Columns["ID"].Visible = false;
                    dataGridViewUsers.Columns["RoleID"].Visible = false;
                    dataGridViewUsers.Columns["Пароль"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
                }
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            AddUserForm addUserForm = new AddUserForm();
            DialogResult result = addUserForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadUsersData();
            }
        }

        public (bool exists, bool isActive, int userId) CheckUserExists(string lastName, string firstName, string login)
        {
            using (var connection = GetNewConnection())
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
                    MessageBox.Show($"Ошибка проверки пользователя: {ex.Message}");
                    return (false, false, 0);
                }
            }
        }

        public bool RestoreUser(int userId, UserModel userData)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query = @"
                        UPDATE users 
                        SET IsActive = 1,
                            Login = @Login,
                            Password = @Password,
                            Role = @Role
                        WHERE IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Login", userData.Login);
                    cmd.Parameters.AddWithValue("@Password", userData.Password);
                    cmd.Parameters.AddWithValue("@Role", userData.RoleId);

                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            MenuAdmin menu = new MenuAdmin(_userName);
            menu.Show();
            this.Hide();
        }

        private void UserShow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}