using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NailServiceApp.Utilities.NameFormatter;

namespace NailService
{
    public partial class Show : Form
    {
        private string _fio;
        private string _login;
        private string _connection;
        private int _roleID;
        private int _currentUserId;
        private string _currentLogin;
        private ImageService _imageService;

        private TabPage _tabUsers;
        private TabPage _tabCategories;
        private TabPage _tabStatuses;

        private EditUserClass _editUserClass;

        private bool _isEditingStatus = false;
        private string _selectedStatusName = "";

        private bool _isEditingCategory = false;
        private string _selectedCategoryName = "";

        /// <summary>
        /// Конструктор формы просмотра и управления данными
        /// </summary>
        public Show(string FIO, int RoleID, string login = null)
        {
            InitializeComponent();
            _fio = FIO;
            _login = login;
            _roleID = RoleID;
            _connection = Connection.ConnectionString;
            _currentLogin = login;

            if (!string.IsNullOrEmpty(login))
            {
                _currentUserId = GetCurrentUserId(login);
            }
            else
            {
                _currentUserId = GetCurrentUserIdByFIO(FIO);
            }

            _editUserClass = new EditUserClass();
            _imageService = new ImageService();

            LabelLoad();
            ConfigureTabsByRole();
            ResetStatusEditingState();
            ResetCategoryEditingState();
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

        private void LabelLoad()
        {
            if (_roleID == 2)
            {
                label3.Text = $"Админ: {_fio}";
            }
            else if (_roleID == 4)
            {
                label3.Text = $"Менеджер: {_fio}";
            }
        }

        private void ConfigureTabsByRole()
        {
            tabControl1.TabPages.Clear();

            _tabCategories = tabPage7;
            _tabStatuses = tabPage6;

            if (_roleID == 2) // Администратор
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabCategories,
                    _tabStatuses
                });
            }
        }

        private bool HasDependencies(string tableName, int id)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    switch (tableName)
                    {
                        case "user":
                            string userQuery = @"SELECT COUNT(*) FROM Record WHERE User = @Id";
                            MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
                            userCmd.Parameters.AddWithValue("@Id", id);
                            return Convert.ToInt32(userCmd.ExecuteScalar()) > 0;

                        default:
                            return false;
                    }
                }
                catch
                {
                    return true;
                }
            }
        }

       

        #region ============ КАТЕГОРИИ ============

        private void LoadCategoriesData()
        {
            if (!tabControl1.TabPages.Contains(_tabCategories))
                return;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        IDCategory as 'ID',
                        CategoryName as 'Название категории',
                        IsActive as 'Активна'
                    FROM Category
                    WHERE IsActive = 1
                    ORDER BY CategoryName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("Название категории", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название категории"]?.ToString() ?? ""
                        );
                    }

                    dataGridViewCategories.DataSource = displayDt;
                    dataGridViewCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewCategories.Columns["ID"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
                }
            }
        }

        private void dataGridViewCategories_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewCategories.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewCategories.Rows.Count)
                {
                    dataGridViewCategories.ClearSelection();
                    dataGridViewCategories.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedCategory();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedCategory();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewCategories, e.Location);
                }
            }
        }

        private void EditSelectedCategory()
        {
            if (dataGridViewCategories.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите категорию для редактирования");
                return;
            }

            var selectedRow = dataGridViewCategories.SelectedRows[0];
            _selectedCategoryName = selectedRow.Cells["Название категории"].Value?.ToString();

            if (!string.IsNullOrEmpty(_selectedCategoryName))
            {
                _isEditingCategory = true;
                CategoryTextBox.Text = _selectedCategoryName;
                UpdateCategoryButtonsState();
            }
        }

        private void DeleteSelectedCategory()
        {
            if (dataGridViewCategories.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите категорию для удаления");
                return;
            }

            var selectedRow = dataGridViewCategories.SelectedRows[0];
            string categoryName = selectedRow.Cells["Название категории"].Value?.ToString();
            int categoryId = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            // Проверка наличия связанных услуг
            if (HasCategoryDependencies(categoryId))
            {
                MessageBox.Show(
                    $"Нельзя удалить категорию '{categoryName}'.\n\n" +
                    "У категории есть связанные услуги. Сначала удалите или перенесите эти услуги.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить категорию '{categoryName}'?\n\n" +
                "Категория будет помечена как неактивная, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                SoftDeleteCategory(categoryId, categoryName);
            }
        }

        private bool HasCategoryDependencies(int categoryId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Services WHERE Category = @CategoryId AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch
                {
                    return true;
                }
            }
        }

        private void SoftDeleteCategory(int categoryId, string categoryName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT IsActive FROM Category WHERE IDCategory = @CategoryId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@CategoryId", categoryId);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo("Категория уже отключена");
                            return;
                        }
                    }

                    string query = "UPDATE Category SET IsActive = 0 WHERE IDCategory = @CategoryId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo($"Категория '{categoryName}' успешно отключена");
                        LoadCategoriesData();
                        ResetCategoryEditingState();
                    }
                    else
                    {
                        ShowInfo("Категория не найдена");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения категории: {ex.Message}");
                }
            }
        }

        private void AddCategory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryTextBox.Text))
            {
                ShowInfo("Введите название категории");
                return;
            }

            string newCategoryName = CategoryTextBox.Text.Trim();

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Category WHERE CategoryName = @CategoryName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@CategoryName", newCategoryName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Категория с таким названием уже существует");
                        return;
                    }

                    string insertQuery = "INSERT INTO Category (CategoryName, IsActive) VALUES (@CategoryName, 1)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@CategoryName", newCategoryName);

                    int affectedRows = insertCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Категория успешно добавлена");
                        LoadCategoriesData();
                        ResetCategoryEditingState();
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления категории: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void EditCategory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryTextBox.Text))
            {
                ShowInfo("Введите новое название категории");
                return;
            }

            if (string.IsNullOrEmpty(_selectedCategoryName))
            {
                ShowInfo("Сначала выберите категорию для редактирования");
                return;
            }

            string newCategoryName = CategoryTextBox.Text.Trim();

            if (_selectedCategoryName == newCategoryName)
            {
                ShowInfo("Название категории не изменилось");
                return;
            }

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Category WHERE CategoryName = @NewCategoryName AND CategoryName != @OldCategoryName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@NewCategoryName", newCategoryName);
                    checkCmd.Parameters.AddWithValue("@OldCategoryName", _selectedCategoryName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Категория с таким названием уже существует");
                        return;
                    }

                    string updateQuery = "UPDATE Category SET CategoryName = @NewCategoryName WHERE CategoryName = @OldCategoryName";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewCategoryName", newCategoryName);
                    updateCmd.Parameters.AddWithValue("@OldCategoryName", _selectedCategoryName);

                    int affectedRows = updateCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Категория успешно обновлена");
                        LoadCategoriesData();
                        ResetCategoryEditingState();
                    }
                    else
                    {
                        ShowInfo("Категория не найдена");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования категории: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateCategoryButtonsState()
        {
            bool hasText = !string.IsNullOrWhiteSpace(CategoryTextBox.Text);

            if (_isEditingCategory)
            {
                AddCategoryButton.Enabled = false;
                EditCategoryButton.Enabled = hasText && !string.IsNullOrEmpty(_selectedCategoryName);
            }
            else
            {
                AddCategoryButton.Enabled = hasText;
                EditCategoryButton.Enabled = false;
            }

            if (!hasText)
            {
                AddCategoryButton.Enabled = false;
                EditCategoryButton.Enabled = false;
            }
        }

        private void ResetCategoryEditingState()
        {
            _isEditingCategory = false;
            _selectedCategoryName = "";
            CategoryTextBox.Clear();

            AddCategoryButton.Enabled = false;
            EditCategoryButton.Enabled = false;

            UpdateCategoryButtonsState();
        }

        private void CategoryTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateCategoryButtonsState();
        }

        private void CategoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isEditingCategory)
            {
                ResetCategoryEditingState();
                e.Handled = true;
            }
        }

        #endregion

        #region ============ СТАТУСЫ ============

        private void LoadStatusesData()
        {
            if (!tabControl1.TabPages.Contains(_tabStatuses))
                return;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        IDStatus as 'ID',
                        StatusName as 'Название статуса'
                    FROM Status
                    ORDER BY IDStatus";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("Название статуса", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название статуса"]?.ToString() ?? ""
                        );
                    }

                    dataGridViewStatuses.DataSource = displayDt;
                    dataGridViewStatuses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewStatuses.Columns["ID"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
                }
            }
        }

        private void dataGridViewStatuses_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewStatuses.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewStatuses.Rows.Count)
                {
                    dataGridViewStatuses.ClearSelection();
                    dataGridViewStatuses.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedStatus();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedStatus();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewStatuses, e.Location);
                }
            }
        }

        private void EditSelectedStatus()
        {
            if (dataGridViewStatuses.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите статус для редактирования");
                return;
            }

            var selectedRow = dataGridViewStatuses.SelectedRows[0];
            _selectedStatusName = selectedRow.Cells["Название статуса"].Value?.ToString();

            if (!string.IsNullOrEmpty(_selectedStatusName))
            {
                _isEditingStatus = true;
                StatusTextBox.Text = _selectedStatusName;
                UpdateStatusButtonsState();
            }
        }

        private void DeleteSelectedStatus()
        {
            if (dataGridViewStatuses.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите статус для удаления");
                return;
            }

            var selectedRow = dataGridViewStatuses.SelectedRows[0];
            string statusName = selectedRow.Cells["Название статуса"].Value?.ToString();

            if (IsSystemStatus(statusName))
            {
                MessageBox.Show("Системные статусы нельзя удалять",
                               "Предупреждение",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить статус '{statusName}'?\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                DeleteStatusFromDatabase(statusName);
            }
        }

        private bool IsSystemStatus(string statusName)
        {
            if (string.IsNullOrEmpty(statusName))
                return false;

            string[] systemStatuses = {
                "Запланирован",
                "Подтвержден",
                "Выполнен",
                "Отменен"
            };

            return systemStatuses.Contains(statusName, StringComparer.OrdinalIgnoreCase);
        }

        private void DeleteStatusFromDatabase(string statusName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Record WHERE Status = " +
                                       "(SELECT IDStatus FROM Status WHERE StatusName = @StatusName)";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@StatusName", statusName);

                    int recordCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (recordCount > 0)
                    {
                        ShowInfo($"Нельзя удалить статус '{statusName}'. Найдено {recordCount} записей с этим статусом.");
                        return;
                    }

                    string deleteQuery = "DELETE FROM Status WHERE StatusName = @StatusName";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@StatusName", statusName);

                    int affectedRows = deleteCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Статус успешно удален");
                        LoadStatusesData();
                        ResetStatusEditingState();
                    }
                    else
                    {
                        ShowInfo("Статус не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления статуса: {ex.Message}");
                }
            }
        }

        private void AddStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StatusTextBox.Text))
            {
                ShowInfo("Введите название статуса");
                return;
            }

            string newStatusName = StatusTextBox.Text.Trim();

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Status WHERE StatusName = @StatusName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@StatusName", newStatusName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Статус с таким названием уже существует");
                        return;
                    }

                    string insertQuery = "INSERT INTO Status (StatusName) VALUES (@StatusName)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@StatusName", newStatusName);

                    int affectedRows = insertCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Статус успешно добавлен");
                        LoadStatusesData();
                        ResetStatusEditingState();
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления статуса: {ex.Message}");
                }
            }
        }

        private void EditStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StatusTextBox.Text))
            {
                ShowInfo("Введите новое название статуса");
                return;
            }

            if (string.IsNullOrEmpty(_selectedStatusName))
            {
                ShowInfo("Сначала выберите статус для редактирования");
                return;
            }

            string newStatusName = StatusTextBox.Text.Trim();

            if (_selectedStatusName == newStatusName)
            {
                ShowInfo("Название статуса не изменилось");
                return;
            }

            if (IsSystemStatus(_selectedStatusName))
            {
                MessageBox.Show("Системные статусы нельзя редактировать",
                               "Предупреждение",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                ResetStatusEditingState();
                return;
            }

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Status WHERE StatusName = @NewStatusName AND StatusName != @OldStatusName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@NewStatusName", newStatusName);
                    checkCmd.Parameters.AddWithValue("@OldStatusName", _selectedStatusName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Статус с таким названием уже существует");
                        return;
                    }

                    string updateQuery = "UPDATE Status SET StatusName = @NewStatusName WHERE StatusName = @OldStatusName";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewStatusName", newStatusName);
                    updateCmd.Parameters.AddWithValue("@OldStatusName", _selectedStatusName);

                    int affectedRows = updateCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Статус успешно обновлен");
                        LoadStatusesData();
                        ResetStatusEditingState();
                    }
                    else
                    {
                        ShowInfo("Статус не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования статуса: {ex.Message}");
                }
            }
        }

        private void UpdateStatusButtonsState()
        {
            bool hasText = !string.IsNullOrWhiteSpace(StatusTextBox.Text);

            if (_isEditingStatus)
            {
                AddStatusButton.Enabled = false;
                EditStatusButton.Enabled = hasText && !string.IsNullOrEmpty(_selectedStatusName);
            }
            else
            {
                AddStatusButton.Enabled = hasText;
                EditStatusButton.Enabled = false;
            }

            if (!hasText)
            {
                AddStatusButton.Enabled = false;
                EditStatusButton.Enabled = false;
            }
        }

        private void ResetStatusEditingState()
        {
            _isEditingStatus = false;
            _selectedStatusName = "";
            StatusTextBox.Clear();

            AddStatusButton.Enabled = false;
            EditStatusButton.Enabled = false;

            UpdateStatusButtonsState();
        }

        private void StatusTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateStatusButtonsState();
        }

        private void StatusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isEditingStatus)
            {
                ResetStatusEditingState();
                e.Handled = true;
            }
        }

        #endregion

        #region ============ ЗАГРУЗКА ДАННЫХ ============

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Show_Load(object sender, EventArgs e)
        {
            LoadCurrentTabData();
            if (tabControl1.TabPages.Contains(_tabCategories))
                ConfigureDataGridView(dataGridViewCategories);
            if (tabControl1.TabPages.Contains(_tabStatuses))
            {
                ConfigureDataGridView(dataGridViewStatuses);
                ResetStatusEditingState();
            }
        }

        private void ConfigureDataGridView(DataGridView name)
        {
            name.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            name.MultiSelect = false;
            name.RowHeadersVisible = false;
            name.ReadOnly = true;

            name.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    name.Rows[e.RowIndex].Selected = true;
                }
            };

            name.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 203, 219);
            name.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCurrentTabData();
        }

        private void LoadCurrentTabData()
        {
            if (tabControl1.SelectedTab == null)
                return;

            string tabName = tabControl1.SelectedTab.Name;

            switch (tabName) { 

                case "tabPage7":
                    if (_roleID == 2)
                        LoadCategoriesData();
                    break;
                case "tabPage6":
                    if (_roleID == 2)
                        LoadStatusesData();
                    break;
            }
        }

        #endregion

        private void InMenu_Click(object sender, EventArgs e)
        {
            MenuAdmin menuAdmin = new MenuAdmin(_fio, _login);
            menuAdmin.Show();
            this.Hide();
        }

        private void Show_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
    }
}