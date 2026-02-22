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
        private string _connection;
        private int _roleID;
        private ImageService _imageService;

        private TabPage _tabUsers;
        private TabPage _tabMasters;
        private TabPage _tabServices;
        private TabPage _tabClients;
        private TabPage _tabStatuses;

        private EditUserClass _editUserClass;

        private bool _isEditingStatus = false;
        private string _selectedStatusName = "";

        public Show(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;
            _connection = Connection.ConnectionString;

            _editUserClass = new EditUserClass();
            _imageService = new ImageService();

            _tabUsers = tabPage1;
            _tabMasters = tabPage2;
            _tabServices = tabPage4;
            _tabClients = tabPage5;
            _tabStatuses = tabPage6;

            ConfigureTabsByRole();
            ResetStatusEditingState();
        }

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }


        private void ConfigureTabsByRole()
        {
            tabControl1.TabPages.Clear();

            if (_roleID == 2) // Администратор
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabUsers,
                    _tabMasters,
                    _tabServices,
                    _tabClients,
                    _tabStatuses
                });
                AddUsers.Visible = true;
            }
            else if (_roleID == 4) // Менеджер
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabClients,
                    _tabServices
                });
                AddUsers.Visible = false;
            }
            else if (_roleID == 1) // Директор
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabServices
                });
                AddUsers.Visible = false;
            }

        }

        // РАБОТА С МАСТЕРАМИ (SOFT DELETE)
        private void dataGridViewMasters_MouseClick(object sender, MouseEventArgs e)
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

        private void OpenEditFormMaster(DataGridViewRow row)
        {
            try
            {
                int masterId = Convert.ToInt32(row.Cells["ID"].Value);
                var masterModel = _editUserClass.LoadMasterById(masterId);
                if (masterModel != null)
                {
                    var editForm = new EditMasterForm(masterModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _editUserClass.UpdateMasterInDatabase(editForm.Master);
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

        private void DeleteSelectedMaster()
        {
            if (dataGridViewMasters.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите мастера для удаления");
                return;
            }

            var selectedRow = dataGridViewMasters.SelectedRows[0];
            string masterFullName = selectedRow.Cells["ФИО"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить мастера '{masterFullName}'?\n\n" +
                "Мастер будет скрыт, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int masterId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                SoftDeleteMaster(masterId);
            }
        }

        private void SoftDeleteMaster(int masterId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, активен ли мастер
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

                    // SOFT DELETE: помечаем мастера как неактивного
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

        // РАБОТА С КЛИЕНТАМИ (SOFT DELETE)
        private void dataGridViewClients_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewClients.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewClients.Rows.Count)
                {
                    dataGridViewClients.ClearSelection();
                    dataGridViewClients.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedClient();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedClient();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewClients, e.Location);
                }
            }
        }

        private void EditSelectedClient()
        {
            if (dataGridViewClients.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите клиента для редактирования");
                return;
            }
            var selectedRow = dataGridViewClients.SelectedRows[0];
            OpenEditFormClient(selectedRow);
        }

        private void OpenEditFormClient(DataGridViewRow row)
        {
            try
            {
                int clientId = Convert.ToInt32(row.Cells["ID"].Value);
                var clientModel = _editUserClass.LoadClientById(clientId);
                if (clientModel != null)
                {
                    var editForm = new EditClientForm(clientModel);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _editUserClass.UpdateClientInDatabase(editForm.Client);
                        LoadClientsData();
                        ShowInfo("Клиент успешно обновлен");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные клиента");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void DeleteSelectedClient()
        {
            if (dataGridViewClients.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите клиента для удаления");
                return;
            }

            var selectedRow = dataGridViewClients.SelectedRows[0];
            string clientFullName = selectedRow.Cells["ФИО"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить клиента '{clientFullName}'?\n\n" +
                "Клиент будет скрыт, но останется в истории записей.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int clientId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                SoftDeleteClient(clientId);
            }
        }

        private void SoftDeleteClient(int clientId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, активен ли клиент
                    string checkQuery = "SELECT IsActive FROM client WHERE IDClient = @ClientId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ClientId", clientId);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo("Клиент уже отключен");
                            return;
                        }
                    }

                    // SOFT DELETE: помечаем клиента как неактивного
                    string query = "UPDATE client SET IsActive = 0 WHERE IDClient = @ClientId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Клиент успешно отключен");
                        LoadClientsData();
                    }
                    else
                    {
                        ShowInfo("Клиент не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения клиента: {ex.Message}");
                }
            }
        }

        // РАБОТА С ПОЛЬЗОВАТЕЛЯМИ (SOFT DELETE)
        private void Users_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = Users.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    Users.ClearSelection();
                    Users.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedUser();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedUser();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(Users, e.Location);
                }
            }
        }

        private void EditSelectedUser()
        {
            if (Users.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для редактирования");
                return;
            }
            var selectedRow = Users.SelectedRows[0];
            OpenEditForm(selectedRow);
        }

        private void OpenEditForm(DataGridViewRow row)
        {
            try
            {
                int userId = Convert.ToInt32(row.Cells["ID"].Value);
                var userModel = _editUserClass.LoadUserById(userId);
                if (userModel != null)
                {
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
            if (Users.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите пользователя для удаления");
                return;
            }

            var result = MessageBox.Show(
                "Вы точно хотите удалить пользователя?\n\n" +
                "Пользователь будет скрыт, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                var selectedRow = Users.SelectedRows[0];
                int userId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                SoftDeleteUser(userId);
            }
        }

        private void SoftDeleteUser(int userId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, активен ли пользователь
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

                    // Остальной код проверки администраторов...
                    string userInfoQuery = @"
                SELECT u.LastName, u.FirstName, u.MiddleName, r.RoleName, r.IDRole, u.IsActive
                FROM Users u INNER JOIN Role r ON u.Role = r.IDRole
                WHERE u.IDUser = @UserId";

                    MySqlCommand infoCmd = new MySqlCommand(userInfoQuery, connection);
                    infoCmd.Parameters.AddWithValue("@UserId", userId);

                    string lastName = "", firstName = "", middleName = "", roleName = "";
                    int roleId = 0;
                    bool isActiveUser = true;

                    using (var reader = infoCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastName = reader["LastName"]?.ToString() ?? "";
                            firstName = reader["FirstName"]?.ToString() ?? "";
                            middleName = reader["MiddleName"]?.ToString() ?? "";
                            roleName = reader["RoleName"]?.ToString() ?? "";
                            roleId = reader.GetInt32("IDRole");
                            isActiveUser = reader.GetBoolean("IsActive");
                        }
                    }

                    string userFullName = $"{lastName} {firstName} {middleName}".Trim();

                    // Если пользователь уже неактивен
                    if (!isActiveUser)
                    {
                        ShowInfo($"Пользователь '{userFullName}' уже отключен");
                        return;
                    }

                    // Проверяем, является ли пользователь администратором
                    bool isAdmin = roleId == 2 ||
                                  roleName.ToLower() == "админ" ||
                                  roleName.ToLower() == "admin" ||
                                  roleName.ToLower() == "administrator";

                    if (isAdmin)
                    {
                        // Проверяем, сколько всего активных администраторов
                        string countAdminsQuery = "SELECT COUNT(*) FROM Users WHERE Role = 2 AND IsActive = 1";
                        MySqlCommand countAdminsCmd = new MySqlCommand(countAdminsQuery, connection);
                        int adminCount = Convert.ToInt32(countAdminsCmd.ExecuteScalar());

                        if (adminCount <= 1)
                        {
                            MessageBox.Show($"Нельзя отключить пользователя '{userFullName}'.\n\n" +
                                          "Это последний активный администратор в системе.",
                                          "Ошибка",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Error);
                            connection.Close();
                            return;
                        }
                    }

                    // SOFT DELETE: помечаем пользователя как неактивного
                    string query = "UPDATE Users SET IsActive = 0 WHERE IDUser = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo($"Пользователь '{userFullName}' успешно отключен");
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

        // РАБОТА С УСЛУГАМИ (SOFT DELETE)
        private void dataGridViewServices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewServices.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewServices.RowCount)
                {
                    dataGridViewServices.ClearSelection();
                    dataGridViewServices.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedService();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedService();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewServices, e.Location);
                }
            }
        }

        private void EditSelectedService()
        {
            if (dataGridViewServices.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите услугу для редактирования");
                return;
            }
            var selectedRow = dataGridViewServices.SelectedRows[0];
            OpenEditFormService(selectedRow);
        }

        

        private void DeleteSelectedService()
        {
            if (dataGridViewServices.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите услугу для удаления");
                return;
            }

            var result = MessageBox.Show(
                "Вы точно хотите удалить услугу?\n\n" +
                "Услуга будет скрыта, но останется в базе данных.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                var selectedRow = dataGridViewServices.SelectedRows[0];
                int serviceId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                SoftDeleteService(serviceId);
            }
        }

        private void SoftDeleteService(int serviceId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, активна ли услуга
                    string checkQuery = "SELECT IsActive FROM services WHERE IDServices = @ServiceId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            ShowInfo("Услуга уже отключена");
                            return;
                        }
                    }

                    // SOFT DELETE: помечаем услугу как неактивную
                    string query = "UPDATE services SET IsActive = 0 WHERE IDServices = @ServiceId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Услуга успешно отключена");
                        LoadServicesData();
                    }
                    else
                    {
                        ShowInfo("Услуга не найдена");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отключения услуги: {ex.Message}");
                }
            }
        }

        // ДОБАВЛЕНИЕ С ПРОВЕРКОЙ НА ВОССТАНОВЛЕНИЕ
        private void AddUsers_Click(object sender, EventArgs e)
        {
            if (_roleID != 2)
            {
                MessageBox.Show("У вас нет прав для добавления пользователей",
                              "Доступ запрещен",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            // Передаем текущую форму (this) для доступа к методам проверки
            AddUserForm addUserForm = new AddUserForm(this);
            DialogResult result = addUserForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadUsersData();
            }
        }

        // Метод для проверки и восстановления неактивных пользователей (используется в AddUserForm)
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

        // Метод для восстановления пользователя
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

        private void AddMaster_Click(object sender, EventArgs e)
        {
            if (_roleID != 2)
            {
                MessageBox.Show("У вас нет прав для добавления мастеров",
                              "Доступ запрещен",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            AddMasterForm addMasterForm = new AddMasterForm(this); // Передаем текущую форму
            DialogResult result = addMasterForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadMastersData();
                // MessageBox уже показывается в форме добавления
            }
        }

        private void AddService_Click(object sender, EventArgs e)
        {
            AddServiceForm addServiceForm = new AddServiceForm(this); // Передаем текущую форму
            DialogResult result = addServiceForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadServicesData();
                // MessageBox уже показывается в форме добавления
            }
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            AddClientForm addClientForm = new AddClientForm(this); // Передаем текущую форму
            DialogResult result = addClientForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadClientsData();
                MessageBox.Show("Клиент успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Остальные методы остаются без изменений...

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Show_Load(object sender, EventArgs e)
        {
            LoadCurrentTabData();

            if (tabControl1.TabPages.Contains(_tabUsers))
                ConfigureDataGridView(Users);
            if (tabControl1.TabPages.Contains(_tabClients))
                ConfigureDataGridView(dataGridViewClients);
            if (tabControl1.TabPages.Contains(_tabMasters))
                ConfigureDataGridView(dataGridViewMasters);
            if (tabControl1.TabPages.Contains(_tabServices))
                ConfigureDataGridView(dataGridViewServices);
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
            name.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            name.DefaultCellStyle.SelectionForeColor = Color.Black;
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

            switch (tabName)
            {
                case "tabPage1": // Пользователи
                    if (_roleID == 2)
                        LoadUsersData();
                    break;
                case "tabPage2": // Мастера
                    if (_roleID == 2)
                        LoadMastersData();
                    break;
                case "tabPage4": // Услуги
                    LoadServicesData();
                    break;
                case "tabPage5": // Клиенты
                    LoadClientsData();
                    break;
                case "tabPage6": // Статусы
                    if (_roleID == 2)
                        LoadStatusesData();
                    break;
            }
        }

        // ЗАГРУЗКА ДАННЫХ С ФИЛЬТРАЦИЕЙ ПО IsActive
        private void LoadUsersData()
        {
            if (!tabControl1.TabPages.Contains(_tabUsers))
                return;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    // Добавляем фильтр WHERE IsActive = 1
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

                    Users.DataSource = maskedDt;
                    Users.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    Users.Columns["ID"].Visible = false;
                    Users.Columns["RoleID"].Visible = false;
                    Users.Columns["Пароль"].Visible = false;
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
                }
            }
        }

        private void LoadMastersData()
        {
            if (!tabControl1.TabPages.Contains(_tabMasters))
                return;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    // Добавляем фильтр WHERE m.IsActive = 1 AND u.IsActive = 1
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
                            row["Описание"],
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

        private void LoadServicesData()
        {
            if (!tabControl1.TabPages.Contains(_tabServices))
                return;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                s.IDServices as 'ID',
                s.ServiceName as 'Название услуги',
                s.Description as 'Описание',
                s.Price as 'Цена',
                s.Category as 'CategoryID',
                s.Photo as 'Фото',
                c.CategoryName as 'Категория'
            FROM Services s
            INNER JOIN Category c ON s.Category = c.IDCategory
            WHERE s.IsActive = 1 AND c.IsActive = 1
            ORDER BY c.CategoryName, s.ServiceName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int));
                    maskedDt.Columns.Add("Название услуги", typeof(string));
                    maskedDt.Columns.Add("Описание", typeof(string));
                    maskedDt.Columns.Add("Цена", typeof(decimal));
                    maskedDt.Columns.Add("Категория", typeof(string));
                    maskedDt.Columns.Add("Миниатюра", typeof(Image));
                    maskedDt.Columns.Add("Имя файла", typeof(string));
                    maskedDt.Columns.Add("CategoryID", typeof(int));

                    Size thumbnailSize = _imageService.CalculateOptimalThumbnailSize(dataGridViewServices, 80);

                    foreach (DataRow row in dt.Rows)
                    {
                        Image thumbnail = _imageService.GetServiceThumbnail(
                            row["Фото"]?.ToString(),
                            thumbnailSize.Width,
                            thumbnailSize.Height
                        );

                        maskedDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название услуги"]?.ToString() ?? "",
                            row["Описание"]?.ToString() ?? "",
                            Convert.ToDecimal(row["Цена"]),
                            row["Категория"]?.ToString() ?? "",
                            thumbnail,
                            row["Фото"]?.ToString(),
                            Convert.ToInt32(row["CategoryID"])
                        );
                    }

                    // СОХРАНЯЕМ ВЫДЕЛЕНИЕ (если есть)
                    int selectedIndex = -1;
                    if (dataGridViewServices.SelectedRows.Count > 0)
                    {
                        selectedIndex = dataGridViewServices.SelectedRows[0].Index;
                    }

                    // 1. Сначала очищаем существующие колонки
                    dataGridViewServices.Columns.Clear();

                    // 2. Настраиваем базовые свойства БЕЗ столбцов
                    dataGridViewServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dataGridViewServices.MultiSelect = false;
                    dataGridViewServices.RowHeadersVisible = false;
                    dataGridViewServices.ReadOnly = true;
                    dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewServices.AllowUserToResizeRows = false;

                    // 3. Устанавливаем источник данных
                    dataGridViewServices.DataSource = maskedDt;

                    // 4. Настраиваем колонки ПОСЛЕ установки DataSource
                    DataGridViewConfigurator.ConfigureServicesDataGridView( dataGridViewServices );

                    // 5. Восстанавливаем выделение
                    if (selectedIndex >= 0 && selectedIndex < dataGridViewServices.Rows.Count)
                    {
                        dataGridViewServices.Rows[selectedIndex].Selected = true;
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
                }
            }
        }

        private void OpenEditFormService(DataGridViewRow row)
        {
            try
            {
                int serviceId = Convert.ToInt32(row.Cells["ID"].Value);

                // Получаем имя файла фото
                string photoFileName = null;
                if (dataGridViewServices.Columns.Contains("Имя файла") && row.Cells["Имя файла"].Value != null)
                {
                    photoFileName = row.Cells["Имя файла"].Value.ToString();
                }

                var serviceModel = _editUserClass.LoadServiceById(serviceId);
                if (serviceModel != null)
                {
                    // ЗАГРУЖАЕМ ОРИГИНАЛЬНОЕ ИЗОБРАЖЕНИЕ через ImageService
                    if (!string.IsNullOrEmpty(photoFileName))
                    {
                        try
                        {
                            // Получаем полный путь к изображению через ImageService
                            string imagesPath = _imageService.GetServicesImagesPath();
                            string imagePath = Path.Combine(imagesPath, photoFileName);

                            if (File.Exists(imagePath))
                            {
                                // Загружаем оригинальное изображение
                                serviceModel.ServiceImage = _imageService.LoadImageFromFile(imagePath);
                            }
                            else
                            {
                                // Если файл не найден, загружаем заглушку
                                serviceModel.ServiceImage = _imageService.LoadDefaultServiceImage();
                            }
                        }
                        catch (Exception ex)
                        {
                            // В случае ошибки загрузки используем заглушку
                            serviceModel.ServiceImage = _imageService.LoadDefaultServiceImage();
                        }
                    }
                    else
                    {
                        // Если фото нет, загружаем заглушку
                        serviceModel.ServiceImage = _imageService.LoadDefaultServiceImage();
                    }

                    serviceModel.Photo = photoFileName;

                    // Передаем ImageService в форму редактирования
                    var editForm = new EditServiceForm(serviceModel, _imageService);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        _editUserClass.UpdateServiceInDatabase(editForm.Service);
                        LoadServicesData();
                        ShowInfo("Услуга успешно обновлена");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные услуги");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private void LoadClientsData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    // Добавляем фильтр WHERE IsActive = 1
                    string query = @"SELECT 
                        IDClient as 'ID',
                        LastName as 'Фамилия',
                        FirstName as 'Имя',
                        MiddleName as 'Отчество',
                        Phone as 'Телефон'
                    FROM Client
                    WHERE IsActive = 1
                    ORDER BY LastName, FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int));
                    maskedDt.Columns.Add("ФИО", typeof(string));
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
                            phone
                        );
                    }

                    dataGridViewClients.DataSource = maskedDt;
                    dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewClients.Columns["ID"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
                }
            }
        }

        // Статусы (не изменяем, т.к. это системная таблица)
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

        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2)
            {
                MenuAdmin menuAdmin = new MenuAdmin(_fio);
                menuAdmin.Show();
                this.Hide();
            }
            else if (_roleID == 4)
            {
                Schedule menuManager = new Schedule(_fio,4,0);
                menuManager.Show();
                this.Hide();
            }
            else if (_roleID == 1)
            {
                MenuDirector menuManager = new MenuDirector(_fio);
                menuManager.Show();
                this.Hide();
            }
        }

        // Обработчик правого клика по таблице статусов
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

        // Редактирование выбранного статуса
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
                StatusTextBox.Text = _selectedStatusName; // Предполагаю, что поле называется StatusTextBox
                UpdateStatusButtonsState();
            }
        }

        // Удаление выбранного статуса
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

        // Проверка, является ли статус системным
        private bool IsSystemStatus(string statusName)
        {
            if (string.IsNullOrEmpty(statusName))
                return false;

            // Список системных статусов, которые нельзя удалять
            string[] systemStatuses = {
        "Запланирован",
        "Подтвержден",
        "Выполнен",
        "Отменен",
        "Planned",       // английские версии на всякий случай
        "Confirmed",
        "Completed",
        "Cancelled"
    };

            return systemStatuses.Contains(statusName, StringComparer.OrdinalIgnoreCase);
        }

        // Удаление статуса из базы данных (HARD DELETE - т.к. это справочник)
        private void DeleteStatusFromDatabase(string statusName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли записи с этим статусом
                    string checkQuery = "SELECT COUNT(*) FROM Record WHERE Status = " +
                                       "(SELECT IDStatus FROM Status WHERE StatusName = @StatusName)";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@StatusName", statusName);

                    int recordCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (recordCount > 0)
                    {
                        ShowInfo($"Нельзя удалить статус '{statusName}'. Найдено {recordCount} записей с этим статусом.\n\n" +
                                "Пожалуйста, измените статус в записях или удалите записи сначала.");
                        return;
                    }

                    // Удаляем статус (HARD DELETE - т.к. это справочная таблица)
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
                catch (MySqlException ex)
                {
                    if (ex.Number == 1451) // Ошибка внешнего ключа
                    {
                        MessageBox.Show($"Нельзя удалить статус '{statusName}'.\n\n" +
                                      "Есть связанные записи в расписании. " +
                                      "Сначала измените статус в этих записях или удалите их.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления статуса: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления статуса: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        // Добавление нового статуса
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

                    // Проверяем, существует ли уже такой статус
                    string checkQuery = "SELECT COUNT(*) FROM Status WHERE StatusName = @StatusName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@StatusName", newStatusName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Статус с таким названием уже существует");
                        return;
                    }

                    // Добавляем новый статус
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
                    MessageBox.Show($"Ошибка добавления статуса: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        // Редактирование существующего статуса
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

                    // Проверяем, существует ли уже такой статус (кроме редактируемого)
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

                    // Обновляем статус
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
                catch (MySqlException ex)
                {
                    if (ex.Number == 1451) // Ошибка внешнего ключа
                    {
                        MessageBox.Show($"Нельзя изменить статус '{_selectedStatusName}'.\n\n" +
                                      "Есть связанные записи в расписании. " +
                                      "Сначала измените статус в этих записях.",
                                      "Ошибка редактирования",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка редактирования статуса: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования статуса: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        // Обновление состояния кнопок для статусов
        private void UpdateStatusButtonsState()
        {
            bool hasText = !string.IsNullOrWhiteSpace(StatusTextBox.Text);

            if (_isEditingStatus)
            {
                // В режиме редактирования
                AddStatusButton.Enabled = false; // Добавление недоступно
                EditStatusButton.Enabled = hasText && !string.IsNullOrEmpty(_selectedStatusName);
            }
            else
            {
                // В режиме добавления
                AddStatusButton.Enabled = hasText;
                EditStatusButton.Enabled = false;
            }

            // Если текстовое поле пустое, обе кнопки отключены
            if (!hasText)
            {
                AddStatusButton.Enabled = false;
                EditStatusButton.Enabled = false;
            }
        }

        // Сброс состояния редактирования статусов
        private void ResetStatusEditingState()
        {
            _isEditingStatus = false;
            _selectedStatusName = "";
            StatusTextBox.Clear();

            // Явно отключаем кнопки
            AddStatusButton.Enabled = false;
            EditStatusButton.Enabled = false;

            UpdateStatusButtonsState();
        }

        // Обработчик изменения текста в поле статуса
        private void StatusTextBox_TextChanged(object sender, EventArgs e)
        {
            bool hasText = !string.IsNullOrWhiteSpace(StatusTextBox.Text);

            if (_isEditingStatus)
            {
                // В режиме редактирования
                AddStatusButton.Enabled = false;
                EditStatusButton.Enabled = hasText && !string.IsNullOrEmpty(_selectedStatusName);
            }
            else
            {
                // В режиме добавления
                AddStatusButton.Enabled = hasText;
                EditStatusButton.Enabled = false;
            }
        }

        // Обработчик нажатия Esc в поле статуса (для отмены редактирования)
        private void StatusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isEditingStatus)
            {
                ResetStatusEditingState();
                e.Handled = true;
            }
        }
    }
}