using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
        private int _selectedStatusId = 0;

        private bool _isEditingCategory = false;
        private string _selectedCategoryName = "";
        private int _selectedCategoryId = 0;

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

            // Подписываемся на события загрузки вкладок
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
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

                    ConfigureDataGridView(dataGridViewCategories);

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
            _selectedCategoryId = Convert.ToInt32(selectedRow.Cells["ID"].Value);

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
            string newCategoryName = CategoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                ShowInfo("Введите название категории");
                return;
            }

            // Валидация только русские буквы
            if (!IsRussianText(newCategoryName))
            {
                ShowInfo("Название категории должно содержать только русские буквы, пробелы и дефис");
                return;
            }

            newCategoryName = CapitalizeFirstLetter(newCategoryName);

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Category WHERE CategoryName = @CategoryName AND IsActive = 1";
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
            string newCategoryName = CategoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                ShowInfo("Введите новое название категории");
                return;
            }

            if (string.IsNullOrEmpty(_selectedCategoryName))
            {
                ShowInfo("Сначала выберите категорию для редактирования");
                return;
            }

            // Валидация только русские буквы
            if (!IsRussianText(newCategoryName))
            {
                ShowInfo("Название категории должно содержать только русские буквы, пробелы и дефис");
                return;
            }

            newCategoryName = CapitalizeFirstLetter(newCategoryName);

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

                    string checkQuery = "SELECT COUNT(*) FROM Category WHERE CategoryName = @NewCategoryName AND IDCategory != @CategoryId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@NewCategoryName", newCategoryName);
                    checkCmd.Parameters.AddWithValue("@CategoryId", _selectedCategoryId);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Категория с таким названием уже существует");
                        return;
                    }

                    string updateQuery = "UPDATE Category SET CategoryName = @NewCategoryName WHERE IDCategory = @CategoryId";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewCategoryName", newCategoryName);
                    updateCmd.Parameters.AddWithValue("@CategoryId", _selectedCategoryId);

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
                EditCategoryButton.Enabled = hasText;
            }
            else
            {
                AddCategoryButton.Enabled = hasText;
                EditCategoryButton.Enabled = false;
            }
        }

        private void ResetCategoryEditingState()
        {
            _isEditingCategory = false;
            _selectedCategoryName = "";
            _selectedCategoryId = 0;
            CategoryTextBox.Clear();
            UpdateCategoryButtonsState();
        }

        private void CategoryTextBox_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = CategoryTextBox.SelectionStart;
            int selectionLength = CategoryTextBox.SelectionLength;

            // Оставляем только русские буквы, пробелы и дефис
            string filteredText = new string(CategoryTextBox.Text
                .Where(c => (c >= 'а' && c <= 'я') ||
                            (c >= 'А' && c <= 'Я') ||
                            c == 'ё' ||
                            c == 'Ё' ||
                            c == ' ' ||
                            c == '-')
                .ToArray());

            if (filteredText != CategoryTextBox.Text)
            {
                CategoryTextBox.Text = filteredText;

                // Корректируем позицию курсора
                if (selectionStart > CategoryTextBox.Text.Length)
                    selectionStart = CategoryTextBox.Text.Length;

                CategoryTextBox.SelectionStart = selectionStart;
                CategoryTextBox.SelectionLength = 0;
            }

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

        // Валидация только русских букв, пробелов и дефиса
        private bool IsRussianText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            return Regex.IsMatch(text, @"^[а-яА-ЯёЁ\s\-]+$");
        }

        // Преобразование первой буквы в заглавную
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.ToLower();
            char[] chars = text.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);

            // Обработка слов после пробела
            for (int i = 1; i < chars.Length - 1; i++)
            {
                if (chars[i] == ' ' && i + 1 < chars.Length)
                    chars[i + 1] = char.ToUpper(chars[i + 1]);
            }

            return new string(chars);
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

                    ConfigureDataGridView(dataGridViewStatuses);

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
            _selectedStatusId = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            if (IsSystemStatus(_selectedStatusName))
            {
                ShowInfo("Системные статусы нельзя редактировать");
                return;
            }

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
            int statusId = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            if (IsSystemStatus(statusName))
            {
                ShowInfo("Системные статусы нельзя удалять");
                return;
            }

            if (HasStatusDependencies(statusId))
            {
                MessageBox.Show(
                    $"Нельзя удалить статус '{statusName}'.\n\n" +
                    "Есть записи с этим статусом. Сначала измените статус у этих записей.",
                    "Ошибка удаления",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить статус '{statusName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                DeleteStatusFromDatabase(statusId);
            }
        }

        private bool IsSystemStatus(string statusName)
        {
            if (string.IsNullOrEmpty(statusName))
                return false;

            string[] systemStatuses = { "Занято", "Выполнено", "Отменено" };
            return systemStatuses.Contains(statusName);
        }

        private bool HasStatusDependencies(int statusId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Record WHERE Status = @StatusId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@StatusId", statusId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch
                {
                    return true;
                }
            }
        }

        private void DeleteStatusFromDatabase(int statusId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM Status WHERE IDStatus = @StatusId";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@StatusId", statusId);

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
            string newStatusName = StatusTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newStatusName))
            {
                ShowInfo("Введите название статуса");
                return;
            }

            // Валидация только русские буквы
            if (!IsRussianText(newStatusName))
            {
                ShowInfo("Название статуса должно содержать только русские буквы, пробелы и дефис");
                return;
            }

            newStatusName = CapitalizeFirstLetter(newStatusName);

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
            string newStatusName = StatusTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newStatusName))
            {
                ShowInfo("Введите новое название статуса");
                return;
            }

            if (string.IsNullOrEmpty(_selectedStatusName))
            {
                ShowInfo("Сначала выберите статус для редактирования");
                return;
            }

            if (IsSystemStatus(_selectedStatusName))
            {
                ShowInfo("Системные статусы нельзя редактировать");
                ResetStatusEditingState();
                return;
            }

            if (!IsRussianText(newStatusName))
            {
                ShowInfo("Название статуса должно содержать только русские буквы, пробелы и дефис");
                return;
            }

            newStatusName = CapitalizeFirstLetter(newStatusName);

            if (_selectedStatusName == newStatusName)
            {
                ShowInfo("Название статуса не изменилось");
                return;
            }

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Status WHERE StatusName = @NewStatusName AND IDStatus != @StatusId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@NewStatusName", newStatusName);
                    checkCmd.Parameters.AddWithValue("@StatusId", _selectedStatusId);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Статус с таким названием уже существует");
                        return;
                    }

                    string updateQuery = "UPDATE Status SET StatusName = @NewStatusName WHERE IDStatus = @StatusId";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewStatusName", newStatusName);
                    updateCmd.Parameters.AddWithValue("@StatusId", _selectedStatusId);

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
                EditStatusButton.Enabled = hasText;
            }
            else
            {
                AddStatusButton.Enabled = hasText;
                EditStatusButton.Enabled = false;
            }
        }

        private void ResetStatusEditingState()
        {
            _isEditingStatus = false;
            _selectedStatusName = "";
            _selectedStatusId = 0;
            StatusTextBox.Clear();
            UpdateStatusButtonsState();
        }

        private void StatusTextBox_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = CategoryTextBox.SelectionStart;
            int selectionLength = CategoryTextBox.SelectionLength;

            // Оставляем только русские буквы, пробелы и дефис
            string filteredText = new string(CategoryTextBox.Text
                .Where(c => (c >= 'а' && c <= 'я') ||
                            (c >= 'А' && c <= 'Я') ||
                            c == 'ё' ||
                            c == 'Ё' ||
                            c == ' ' ||
                            c == '-')
                .ToArray());

            if (filteredText != CategoryTextBox.Text)
            {
                CategoryTextBox.Text = filteredText;

                // Корректируем позицию курсора
                if (selectionStart > CategoryTextBox.Text.Length)
                    selectionStart = CategoryTextBox.Text.Length;

                CategoryTextBox.SelectionStart = selectionStart;
                CategoryTextBox.SelectionLength = 0;
            }
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
            ConfigureDataGridView(dataGridViewCategories);
            ConfigureDataGridView(dataGridViewStatuses);
            ResetStatusEditingState();
            ResetCategoryEditingState();
        }

        private void ConfigureDataGridView(DataGridView grid)
        {
            if (grid == null) return;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.ReadOnly = true;

            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 203, 219);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
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

            switch (tabName)
            {
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