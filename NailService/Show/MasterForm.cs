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
    public partial class MasterForm : Form
    {
        private string _connection;
        private int _roleID;
        private string _userName;
        private int _currentUserId;
        private string _currentLogin;
        private EditUserClass _editUserClass;

        public MasterForm(string FIO, int RoleID, string login = null)
        {
            InitializeComponent();
            _roleID = RoleID;
            _userName = FIO;
            txtFIO.Text = $"Админ: {FIO}";
            _connection = Connection.ConnectionString;
            _currentLogin = login;
            _editUserClass = new EditUserClass();

            // Получаем ID текущего пользователя
            if (!string.IsNullOrEmpty(login))
            {
                _currentUserId = GetCurrentUserId(login);
            }

            ConfigureDataGridView();
            LoadMastersData();
        }

        /// <summary>
        /// Получение ID текущего пользователя по логину
        /// </summary>
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

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        /// <summary>
        /// Настройка DataGridView
        /// </summary>
        private void ConfigureDataGridView()
        {
            dataGridViewMasters.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMasters.MultiSelect = false;
            dataGridViewMasters.RowHeadersVisible = false;
            dataGridViewMasters.ReadOnly = true;
            dataGridViewMasters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridViewMasters.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 203, 219);
            dataGridViewMasters.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Подписываемся на событие MouseClick для контекстного меню
            dataGridViewMasters.MouseClick += DataGridViewMasters_MouseClick;
        }

        /// <summary>
        /// Обработчик правого клика по таблице мастеров
        /// </summary>
        private void DataGridViewMasters_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewMasters.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewMasters.Rows.Count)
                {
                    dataGridViewMasters.ClearSelection();
                    dataGridViewMasters.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedMaster();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedMaster();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewMasters, e.Location);
                }
            }
        }

        /// <summary>
        /// Редактирование выбранного мастера
        /// </summary>
        private void EditSelectedMaster()
        {
            if (dataGridViewMasters.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите мастера для редактирования");
                return;
            }
            var selectedRow = dataGridViewMasters.SelectedRows[0];
            OpenEditFormMaster(selectedRow);
        }

        /// <summary>
        /// Открытие формы редактирования мастера
        /// </summary>
        private void OpenEditFormMaster(DataGridViewRow row)
        {
            try
            {
                int masterId = Convert.ToInt32(row.Cells["ID"].Value);
                var masterModel = _editUserClass.LoadMasterById(masterId);
                if (masterModel != null)
                {
                    // Передаём MasterModel напрямую в EditMasterForm
                    var editForm = new EditMasterForm(masterModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadMastersData();
                        ShowInfo("Мастер успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные мастера");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаление выбранного мастера (soft delete)
        /// </summary>
        private void DeleteSelectedMaster()
        {
            if (dataGridViewMasters.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите мастера для удаления");
                return;
            }

            var selectedRow = dataGridViewMasters.SelectedRows[0];
            string masterFullName = selectedRow.Cells["ФИО"].Value?.ToString();
            int masterId = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            // Проверка наличия зависимостей
            if (HasDependencies("master", masterId))
            {
                MessageBox.Show(
                    $"Невозможно удалить мастера '{masterFullName}'.\n\n" +
                    "У мастера есть связанные записи в расписании. Сначала удалите или перенесите эти записи.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить мастера '{masterFullName}'?\n\n" +
                "Мастер будет помечен как неактивный, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                SoftDeleteMaster(masterId);
            }
        }

        /// <summary>
        /// Проверка наличия зависимостей
        /// </summary>
        private bool HasDependencies(string tableName, int id)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    if (tableName == "master")
                    {
                        string masterQuery = "SELECT COUNT(*) FROM Record WHERE Master = @Id";
                        MySqlCommand masterCmd = new MySqlCommand(masterQuery, connection);
                        masterCmd.Parameters.AddWithValue("@Id", id);
                        return Convert.ToInt32(masterCmd.ExecuteScalar()) > 0;
                    }
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Мягкое удаление мастера (установка IsActive = 0)
        /// </summary>
        private void SoftDeleteMaster(int masterId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT IsActive FROM masters WHERE IDMasters = @MasterId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@MasterId", masterId);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo("Мастер уже отключен");
                            return;
                        }
                    }

                    string query = "UPDATE masters SET IsActive = 0 WHERE IDMasters = @MasterId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Мастер успешно отключен");
                        LoadMastersData();
                    }
                    else
                    {
                        ShowInfo("Мастер не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения мастера: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Загрузка данных мастеров
        /// </summary>
        private void LoadMastersData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        m.IDMasters as 'ID',
                        u.LastName as 'Фамилия',
                        u.FirstName as 'Имя',
                        u.MiddleName as 'Отчество',
                        m.Description as 'Описание',
                        m.Phone as 'Телефон',
                        r.RoleName as 'Роль'
                    FROM Masters m
                    INNER JOIN Users u ON m.User = u.IDUser
                    INNER JOIN Role r ON u.Role = r.IDRole
                    WHERE m.IsActive = 1 AND u.IsActive = 1
                    ORDER BY u.LastName, u.FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int));
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Описание", typeof(string));
                    maskedDt.Columns.Add("Телефон", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = FormatToShortName(
                            row["Фамилия"]?.ToString(),
                            row["Имя"]?.ToString(),
                            row["Отчество"]?.ToString()
                        );

                        string phone = row["Телефон"]?.ToString();
                        if (!string.IsNullOrEmpty(phone))
                        {
                            phone = MaskPhone(phone);
                        }

                        maskedDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            fullName,
                            row["Описание"]?.ToString() ?? "",
                            phone
                        );
                    }

                    dataGridViewMasters.DataSource = maskedDt;
                    dataGridViewMasters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewMasters.Columns["ID"].Visible = false;

                    if (dataGridViewMasters.Columns.Contains("Описание"))
                    {
                        dataGridViewMasters.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        dataGridViewMasters.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Маскирование номера телефона
        /// </summary>
        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length >= 11 && (digitsOnly.StartsWith("7") || digitsOnly.StartsWith("8")))
            {
                string lastFour = digitsOnly.Length >= 4 ? digitsOnly.Substring(digitsOnly.Length - 4) : digitsOnly;
                return $"+7(***)***{lastFour}";
            }
            else if (digitsOnly.Length > 4)
            {
                string prefix = phone.Length >= 2 ? phone.Substring(0, 2) : phone;
                string lastFour = digitsOnly.Length >= 4 ? digitsOnly.Substring(digitsOnly.Length - 4) : digitsOnly;
                return $"{prefix} *** *** {lastFour}";
            }
            else
            {
                return phone;
            }
        }

        /// <summary>
        /// Добавление нового мастера (используем единую форму AddUserForm)
        /// </summary>
        private void BtnAddMaster_Click(object sender, EventArgs e)
        {
            // Используем ту же форму AddUserForm, но в режиме мастера
            using (AddUserForm addMasterForm = new AddUserForm(this, true))
            {
                DialogResult result = addMasterForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    LoadMastersData();
                    ShowInfo("Мастер успешно добавлен");
                }
            }
        }

        /// <summary>
        /// Проверка существования пользователя (для AddUserForm)
        /// </summary>
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
                catch
                {
                    return (false, false, 0);
                }
            }
        }

        /// <summary>
        /// Восстановление неактивного пользователя (для AddUserForm)
        /// </summary>
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
                    cmd.Parameters.AddWithValue("@MiddleName", userData.MiddleName);

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch
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
            this.Close();
        }

        private void MasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Close();
            }
        }
    }
}