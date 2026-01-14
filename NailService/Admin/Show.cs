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
using static NailServiceApp.Utilities.NameFormatter;

namespace NailService
{
    public partial class Show : Form
    {
        private string _fio;
        private string _connection;
        private int _roleID;

        private TabPage _tabUsers;
        private TabPage _tabMasters;
        private TabPage _tabServices;
        private TabPage _tabClients;
        private TabPage _tabRoles;
        private TabPage _tabStatuses;

        private EditUserClass _editUserClass;
        private bool _isEditingRole = false; // Флаг режима редактирования
        private string _selectedRoleName = ""; // Сохраняем выбранное имя роли

        private bool _isEditingStatus = false;
        private string _selectedStatusName = "";

        public Show(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;
            _connection = Connection.ConnectionString;
            _editUserClass = new EditUserClass();

            _tabUsers = tabPage1;
            _tabMasters = tabPage2;
            _tabServices = tabPage4;
            _tabClients = tabPage5;
            _tabRoles = tabPage3;
            _tabStatuses = tabPage6;

            ConfigureTabsByRole();

            ResetStatusEditingState(); // Добавьте эту строку
            ResetRoleEditingState();
        }



        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        private void ConfigureTabsByRole()
        {
            // Очищаем все вкладки
            tabControl1.TabPages.Clear();

            if (_roleID == 2) // Администратор - все вкладки
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabUsers,
                    _tabMasters,
                    _tabServices,
                    _tabClients,
                    _tabRoles,
                    _tabStatuses
                });

                // Показываем кнопку добавления пользователей для админа
                AddUsers.Visible = true;
                // Показываем кнопки для работы с ролями
                AddRole.Visible = true;
                EditRole.Visible = true;
            }
            else if (_roleID == 4) // Менеджер - только Клиенты и Услуги
            {
                tabControl1.TabPages.AddRange(new TabPage[]
                {
                    _tabClients,
                    _tabServices
                });

                // Скрываем кнопку добавления пользователей для менеджера
                AddUsers.Visible = false;
                AddRole.Visible = false;
                EditRole.Visible = false;
            }
            else if (_roleID == 3) // Мастер 
            {
                /*
                tabControl1.TabPages.Add(_tabClients);
                AddUsers.Visible = false;*/
            }

        }


        // РАБОТА С МАСТЕРАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ)

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
                // Получаем ID мастера
                int masterId = Convert.ToInt32(row.Cells["ID"].Value);

                // Загружаем полные данные мастера из базы по ID
                var masterModel = _editUserClass.LoadMasterById(masterId);

                if (masterModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditMasterForm(masterModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editUserClass.UpdateMasterInDatabase(editForm.Master);

                        // Перезагружаем данные
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
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
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
                $"Вы точно хотите удалить мастера '{masterFullName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int masterId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteMasterFromDatabase(masterId);
            }
        }

        private void DeleteMasterFromDatabase(int masterId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли записи в расписании для этого мастера
                    string checkAppointmentsQuery = "SELECT COUNT(*) FROM Record WHERE Master = @MasterId";
                    MySqlCommand checkCmd = new MySqlCommand(checkAppointmentsQuery, connection);
                    checkCmd.Parameters.AddWithValue("@MasterId", masterId);
                    int appointmentCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (appointmentCount > 0)
                    {
                        var confirmResult = MessageBox.Show(
                            $"У мастера есть {appointmentCount} записи(ей) в расписании. Удалить мастера и все его записи?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи в расписании
                        string deleteAppointmentsQuery = "DELETE FROM Record WHERE Master = @MasterId";
                        MySqlCommand deleteAppointmentsCmd = new MySqlCommand(deleteAppointmentsQuery, connection);
                        deleteAppointmentsCmd.Parameters.AddWithValue("@MasterId", masterId);
                        deleteAppointmentsCmd.ExecuteNonQuery();
                    }

                    // Удаляем мастера
                    string query = "DELETE FROM Masters WHERE IDMasters = @MasterId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Мастер успешно удален");
                        LoadMastersData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Мастер не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления мастера: {ex.Message}");
                }
            }
        }
        // РАБОТА С МАСТЕРАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ)

        // РАБОТА С КЛИЕНАТМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ)

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
                // Получаем ID клиента (предполагается, что у вас есть скрытая колонка с ID)
                int clientId = Convert.ToInt32(row.Cells["ID"].Value);

                // Загружаем полные данные клиента из базы по ID
                var clientModel = _editUserClass.LoadClientById(clientId);

                if (clientModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditClientForm(clientModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editUserClass.UpdateClientInDatabase(editForm.Client);

                        // Перезагружаем данные
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
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
            }
        }
        private void DeleteSelectedClient()
        {
            if (dataGridViewClients.SelectedRows.Count == 0) // Исправлено: должно быть dataGridViewClients
            {
                ShowInfo("Выберите клиента для удаления");
                return;
            }

            var selectedRow = dataGridViewClients.SelectedRows[0];
            string clientFullName = selectedRow.Cells["ФИО"].Value?.ToString();

            var result = MessageBox.Show(
                $"Вы точно хотите удалить клиента '{clientFullName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int clientId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteClientFromDatabase(clientId);
            }
        }

        private void DeleteClientFromDatabase(int clientId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли записи в расписании для этого клиента
                    string checkAppointmentsQuery = "SELECT COUNT(*) FROM Record WHERE Client = @ClientId";
                    MySqlCommand checkCmd = new MySqlCommand(checkAppointmentsQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ClientId", clientId);
                    int appointmentCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (appointmentCount > 0)
                    {
                        // Получаем имя клиента для сообщения
                        string clientNameQuery = "SELECT LastName, FirstName, MiddleName FROM Client WHERE IDClient = @ClientId";
                        MySqlCommand nameCmd = new MySqlCommand(clientNameQuery, connection);
                        nameCmd.Parameters.AddWithValue("@ClientId", clientId);

                        string clientName = "";
                        using (var reader = nameCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string lastName = reader["LastName"]?.ToString() ?? "";
                                string firstName = reader["FirstName"]?.ToString() ?? "";
                                string middleName = reader["MiddleName"]?.ToString() ?? "";
                                clientName = $"{lastName} {firstName} {middleName}".Trim();
                            }
                        }

                        var confirmResult = MessageBox.Show(
                            $"У клиента '{clientName}' есть {appointmentCount} записи(ей) в расписании.\n\n" +
                            "Удалить клиента и все его записи?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи в расписании (Record)
                        string deleteAppointmentsQuery = "DELETE FROM Record WHERE Client = @ClientId";
                        MySqlCommand deleteAppointmentsCmd = new MySqlCommand(deleteAppointmentsQuery, connection);
                        deleteAppointmentsCmd.Parameters.AddWithValue("@ClientId", clientId);
                        deleteAppointmentsCmd.ExecuteNonQuery();
                    }

                    // Удаляем клиента
                    string query = "DELETE FROM Client WHERE IDClient = @ClientId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Клиент успешно удален");
                        LoadClientsData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Клиент не найден");
                    }

                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок внешнего ключа
                    if (ex.Number == 1451) // Ошибка внешнего ключа (нельзя удалить из-за зависимостей)
                    {
                        MessageBox.Show("Нельзя удалить клиента, так как есть связанные записи в расписании.\n" +
                                      "Пожалуйста, сначала удалите все записи клиента.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления клиента: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления клиента: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
        // РАБОТА С КЛИЕНАТМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) Конец----



        // РАБОТА С ПОЛЬЗОВАТЕЛЯМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ)
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
                    editMenuItem.Image = Properties.Resources.edit_icon; // Или другая системная иконка
                    editMenuItem.Click += (s, args) => EditSelectedUser();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon; // Или другая системная иконка
                    deleteMenuItem.Click += (s, args) => DeleteSelectedUser();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(Users, e.Location);
                }
            }
        }

        //Выбранный пользователь
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

        //Открытие формы для редактирования 
        private void OpenEditForm(DataGridViewRow row)
        {
            try
            {
                // Получаем ID из скрытой колонки
                int userId = Convert.ToInt32(row.Cells["ID"].Value);
                // Загружаем полные данные пользователя из базы по ID
                var userModel = _editUserClass.LoadUserById(userId);

                if (userModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditUserForm(userModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editUserClass.UpdateUserInDatabase(editForm.User);

                        // Перезагружаем данные
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
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
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
                "Вы точно хотите удалить пользователя?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                var selectedRow = Users.SelectedRows[0];
                int userId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteUserFromDatabase(userId);
            }
        }

        private void DeleteUserFromDatabase(int userId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Получаем информацию о пользователе для сообщений
                    string userInfoQuery = @"
                SELECT 
                    u.LastName,
                    u.FirstName,
                    u.MiddleName,
                    r.RoleName,
                    r.IDRole
                FROM Users u
                INNER JOIN Role r ON u.Role = r.IDRole
                WHERE u.IDUser = @UserId";

                    MySqlCommand infoCmd = new MySqlCommand(userInfoQuery, connection);
                    infoCmd.Parameters.AddWithValue("@UserId", userId);

                    string lastName = "", firstName = "", middleName = "", roleName = "";
                    int roleId = 0;

                    using (var reader = infoCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastName = reader["LastName"]?.ToString() ?? "";
                            firstName = reader["FirstName"]?.ToString() ?? "";
                            middleName = reader["MiddleName"]?.ToString() ?? "";
                            roleName = reader["RoleName"]?.ToString() ?? "";
                            roleId = reader.GetInt32("IDRole");
                        }
                    }

                    string userFullName = $"{lastName} {firstName} {middleName}".Trim();

                    // Проверяем, является ли пользователь администратором (ID роли 2 = Админ)
                    bool isAdmin = roleId == 2 ||
                                  roleName.ToLower() == "админ" ||
                                  roleName.ToLower() == "admin" ||
                                  roleName.ToLower() == "administrator";

                    if (isAdmin)
                    {
                        // Проверяем, сколько всего администраторов в системе
                        string countAdminsQuery = "SELECT COUNT(*) FROM Users WHERE Role = 2";
                        MySqlCommand countAdminsCmd = new MySqlCommand(countAdminsQuery, connection);
                        int adminCount = Convert.ToInt32(countAdminsCmd.ExecuteScalar());

                        // Если это последний администратор - нельзя удалять
                        if (adminCount <= 1)
                        {
                            MessageBox.Show($"Нельзя удалить пользователя '{userFullName}'.\n\n" +
                                          "Это последний администратор в системе. В системе должен остаться хотя бы один администратор.",
                                          "Ошибка удаления",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Error);
                            connection.Close();
                            return;
                        }
                        else
                        {
                            // Предупреждение о том, что это администратор, но не последний
                            var adminConfirm = MessageBox.Show(
                                $"Пользователь '{userFullName}' является администратором.\n" +
                                $"В системе останется {adminCount - 1} администратор(ов).\n\n" +
                                "Вы уверены, что хотите удалить этого администратора?",
                                "Подтверждение удаления администратора",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

                            if (adminConfirm == DialogResult.No)
                            {
                                connection.Close();
                                return;
                            }
                        }
                    }

                    // Проверяем, является ли пользователь мастером
                    bool isMaster = roleId == 3 ||
                                   roleName.ToLower() == "мастер" ||
                                   roleName.ToLower() == "master";

                    if (isMaster)
                    {
                        // Проверяем, есть ли пользователь в таблице Masters
                        string checkMasterQuery = "SELECT COUNT(*) FROM Masters WHERE User = @UserId";
                        MySqlCommand checkMasterCmd = new MySqlCommand(checkMasterQuery, connection);
                        checkMasterCmd.Parameters.AddWithValue("@UserId", userId);
                        int masterCount = Convert.ToInt32(checkMasterCmd.ExecuteScalar());

                        if (masterCount > 0)
                        {
                            var result = MessageBox.Show(
                                $"Пользователь '{userFullName}' является мастером и имеет {masterCount} связанных записей(ь) в таблице мастеров.\n\n" +
                                "Выберите действие:\n" +
                                "1. Сначала удалить мастера\n" +
                                "2. Удалить всё сразу\n" +
                                "3. Отменить удаление",
                                "Пользователь является мастером",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Warning
                            );

                            if (result == DialogResult.Cancel)
                            {
                                connection.Close();
                                return;
                            }
                            else if (result == DialogResult.Yes)
                            {
                                MessageBox.Show("Перейдите на вкладку 'Мастера' для удаления мастера.",
                                              "Информация",
                                              MessageBoxButtons.OK,
                                              MessageBoxIcon.Information);
                                connection.Close();
                                return;
                            }
                            else if (result == DialogResult.No)
                            {
                                // Сначала удаляем записи мастера в таблице Masters
                                string deleteMasterQuery = "DELETE FROM Masters WHERE User = @UserId";
                                MySqlCommand deleteMasterCmd = new MySqlCommand(deleteMasterQuery, connection);
                                deleteMasterCmd.Parameters.AddWithValue("@UserId", userId);
                                int deletedMasterRows = deleteMasterCmd.ExecuteNonQuery();

                                if (deletedMasterRows > 0)
                                {
                                    MessageBox.Show($"Удалено {deletedMasterRows} записей(ь) мастера.",
                                                  "Информация",
                                                  MessageBoxButtons.OK,
                                                  MessageBoxIcon.Information);
                                }
                            }
                        }
                    }

                    // Проверяем, есть ли записи в Record для этого пользователя
                    string checkRecordQuery = "SELECT COUNT(*) FROM Record WHERE User = @UserId";
                    MySqlCommand checkRecordCmd = new MySqlCommand(checkRecordQuery, connection);
                    checkRecordCmd.Parameters.AddWithValue("@UserId", userId);
                    int recordCount = Convert.ToInt32(checkRecordCmd.ExecuteScalar());

                    if (recordCount > 0)
                    {
                        var confirmResult = MessageBox.Show(
                            $"Пользователь '{userFullName}' имеет {recordCount} записей(ь) в расписании.\n\n" +
                            "Удалить пользователя и все его записи?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (confirmResult == DialogResult.No)
                        {
                            connection.Close();
                            return;
                        }

                        // Удаляем записи в расписании
                        string deleteRecordsQuery = "DELETE FROM Record WHERE User = @UserId";
                        MySqlCommand deleteRecordsCmd = new MySqlCommand(deleteRecordsQuery, connection);
                        deleteRecordsCmd.Parameters.AddWithValue("@UserId", userId);
                        deleteRecordsCmd.ExecuteNonQuery();
                    }

                    // Удаляем пользователя
                    string query = "DELETE FROM Users WHERE IDUser = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo($"Пользователь '{userFullName}' успешно удален");
                        LoadUsersData(); // Перезагружаем данные
                    }
                    else
                    {
                        ShowInfo("Пользователь не найден");
                    }

                    connection.Close();
                }
                catch (MySqlException ex)
                {
                    // Обработка ошибок MySQL
                    if (ex.Number == 1451) // Ошибка внешнего ключа
                    {
                        // Более детальная проверка, какие таблицы содержат ссылки
                        string checkConstraintsQuery = @"
                    SELECT 
                        TABLE_NAME,
                        COLUMN_NAME,
                        CONSTRAINT_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE REFERENCED_TABLE_NAME = 'Users' 
                      AND REFERENCED_COLUMN_NAME = 'IDUser'
                      AND TABLE_SCHEMA = DATABASE()";

                        try
                        {
                            MySqlCommand constraintsCmd = new MySqlCommand(checkConstraintsQuery, connection);
                            using (var reader = constraintsCmd.ExecuteReader())
                            {
                                List<string> referencingTables = new List<string>();
                                while (reader.Read())
                                {
                                    referencingTables.Add($"{reader["TABLE_NAME"]}.{reader["COLUMN_NAME"]}");
                                }

                                if (referencingTables.Count > 0)
                                {
                                    MessageBox.Show($"Нельзя удалить пользователя из-за связанных записей в таблицах:\n" +
                                                  string.Join("\n", referencingTables) + "\n\n" +
                                                  "Пожалуйста, сначала удалите все связанные записи.",
                                                  "Ошибка удаления",
                                                  MessageBoxButtons.OK,
                                                  MessageBoxIcon.Error);
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Нельзя удалить пользователя из-за связанных записей в других таблицах.\n" +
                                          "Пожалуйста, сначала удалите все связанные записи.",
                                          "Ошибка удаления",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
        // РАБОТА С ПОЛЬЗОВАТЕЛЯМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) Конец----


        // РАБОТА С УСЛУГАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) 
        private void dataGridViewServices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewServices.HitTest(e.X, e.Y);

                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewServices.RowCount)
                {
                    // Снимаем выделение и выделяем текущую строку
                    dataGridViewServices.ClearSelection();
                    dataGridViewServices.Rows[hitTest.RowIndex].Selected = true;

                    // Создаем контекстное меню
                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedService();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedService();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    // Показываем меню в правильном месте
                    contextMenu.Show(dataGridViewServices, e.Location);
                }
            }
        }

        //Выбранная услуга
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

        //Открытие формы для редактирования 
        private void OpenEditFormService(DataGridViewRow row)
        {
            try
            {
                // Получаем ID из скрытой колонки
                int serivceId = Convert.ToInt32(row.Cells["ID"].Value);
                // Загружаем полные данные пользователя из базы по ID
                var serivceModel = _editUserClass.LoadServiceById(serivceId);

                if (serivceModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditServiceForm(serivceModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        _editUserClass.UpdateServiceInDatabase(editForm.Service);

                        // Перезагружаем данные
                        LoadServicesData();

                        ShowInfo("Услуга успешно обновлена");
                    }
                }
                else
                {
                    ShowInfo("Не удалось загрузить данные пользователя");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}");
            }
        }

        private void DeleteSelectedService()
        {
            if (dataGridViewServices.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите услугу для удаления");
                return;
            }

            var result = MessageBox.Show(
                "Вы точно хотите удалить услугу?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                var selectedRow = dataGridViewServices.SelectedRows[0];
                int serviceId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
                DeleteServiceFromDatabaseSafe(serviceId);
            }
        }

        // Альтернативный вариант с выбором действия
        private void DeleteServiceFromDatabaseSafe(int serviceId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем использование услуги
                    string checkUsageQuery = "SELECT COUNT(*) FROM Record WHERE Service = @ServiceId";
                    MySqlCommand checkCmd = new MySqlCommand(checkUsageQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    int usageCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    // Получаем название услуги
                    string serviceNameQuery = "SELECT ServiceName FROM Services WHERE IDServices = @ServiceId";
                    MySqlCommand nameCmd = new MySqlCommand(serviceNameQuery, connection);
                    nameCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    string serviceName = nameCmd.ExecuteScalar()?.ToString() ?? "эта услуга";

                    if (usageCount > 0)
                    {
                        var result = MessageBox.Show(
                            $"Услуга '{serviceName}' используется в {usageCount} записях.\n\n" +
                            "Выберите действие:\n" +
                            "1. Удалить услугу и все записи\n" +
                            "2. Сначала заменить услугу в записях, затем удалить\n" +
                            "3. Отменить удаление",
                            "Услуга используется в записях",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.Cancel)
                        {
                            connection.Close();
                            return;
                        }
                        else if (result == DialogResult.Yes)
                        {
                            // Удаляем записи и услугу
                            string deleteRecordsQuery = "DELETE FROM Record WHERE Service = @ServiceId";
                            MySqlCommand deleteRecordsCmd = new MySqlCommand(deleteRecordsQuery, connection);
                            deleteRecordsCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                            deleteRecordsCmd.ExecuteNonQuery();
                        }
                        else if (result == DialogResult.No)
                        {
                            // Здесь можно вызвать форму для замены услуги в записях
                            MessageBox.Show("Функция замены услуги в записях будет реализована позже.",
                                          "Информация",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Information);
                            connection.Close();
                            return;
                        }
                    }

                    // Удаляем услугу
                    string query = "DELETE FROM Services WHERE IDServices = @ServiceId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Услуга успешно удалена");
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
                    MessageBox.Show($"Ошибка удаления услуги: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }
        // РАБОТА С УСЛУГАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) Конец----

        // РАБОТА С РОЛЯМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ)

        private void dataGridViewRoles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridViewRoles.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0 && hitTest.RowIndex < dataGridViewRoles.RowCount)
                {
                    dataGridViewRoles.ClearSelection();
                    dataGridViewRoles.Rows[hitTest.RowIndex].Selected = true;

                    var contextMenu = new ContextMenuStrip();

                    var editMenuItem = new ToolStripMenuItem("Редактировать");
                    editMenuItem.Image = Properties.Resources.edit_icon;
                    editMenuItem.Click += (s, args) => EditSelectedRole();

                    var deleteMenuItem = new ToolStripMenuItem("Удалить");
                    deleteMenuItem.Image = Properties.Resources.delete_icon;
                    deleteMenuItem.Click += (s, args) => DeleteSelectedRole();

                    contextMenu.Items.Add(editMenuItem);
                    contextMenu.Items.Add(deleteMenuItem);

                    contextMenu.Show(dataGridViewRoles, e.Location);
                }
            }
        }

        private void EditSelectedRole()
        {
            if (dataGridViewRoles.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите роль для редактирования");
                return;
            }

            var selectedRow = dataGridViewRoles.SelectedRows[0];
            _selectedRoleName = selectedRow.Cells["Название роли"].Value?.ToString();

            if (!string.IsNullOrEmpty(_selectedRoleName))
            {
                // Включаем режим редактирования
                _isEditingRole = true;
                RoleTextBox.Text = _selectedRoleName;

                // Настраиваем кнопки
                UpdateRoleButtonsState();
            }
        }

        private void DeleteSelectedRole()
        {
            if (dataGridViewRoles.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите роль для удаления");
                return;
            }

            var selectedRow = dataGridViewRoles.SelectedRows[0];
            string roleName = selectedRow.Cells["Название роли"].Value?.ToString();

            if (IsSystemRole(roleName))
            {
                ShowInfo("Системные роли нельзя удалять");
                return;
            }

            var result = MessageBox.Show(
                $"Вы точно хотите удалить роль '{roleName}'?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                DeleteRoleFromDatabase(roleName);
            }
        }

        private bool IsSystemRole(string roleName)
        {
            // Проверяем, является ли роль системной (нельзя удалять)
            string[] systemRoles = { "Администратор", "Админ", "Administrator", "Admin",
                                   "Мастер", "Менеджер", "Клиент" };

            return systemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        private void DeleteRoleFromDatabase(string roleName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли пользователи с этой ролью
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Role = " +
                                       "(SELECT IDRole FROM Role WHERE RoleName = @RoleName)";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@RoleName", roleName);

                    int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userCount > 0)
                    {
                        ShowInfo($"Нельзя удалить роль '{roleName}'. Найдено {userCount} пользователей с этой ролью.");
                        return;
                    }

                    // Удаляем роль
                    string deleteQuery = "DELETE FROM Role WHERE RoleName = @RoleName";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@RoleName", roleName);

                    int affectedRows = deleteCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Роль успешно удалена");
                        LoadRolesData();

                        // Сбрасываем состояние редактирования
                        ResetRoleEditingState();
                    }
                    else
                    {
                        ShowInfo("Роль не найдена");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления роли: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void AddRole_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoleTextBox.Text))
            {
                ShowInfo("Введите название роли");
                return;
            }

            string newRoleName = RoleTextBox.Text.Trim();

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, существует ли уже такая роль
                    string checkQuery = "SELECT COUNT(*) FROM Role WHERE RoleName = @RoleName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@RoleName", newRoleName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Роль с таким названием уже существует");
                        return;
                    }

                    // Добавляем новую роль
                    string insertQuery = "INSERT INTO Role (RoleName) VALUES (@RoleName)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@RoleName", newRoleName);

                    int affectedRows = insertCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Роль успешно добавлена");
                        LoadRolesData();

                        // Сбрасываем состояние
                        ResetRoleEditingState();
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления роли: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void EditRole_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoleTextBox.Text))
            {
                ShowInfo("Введите новое название роли");
                return;
            }

            if (string.IsNullOrEmpty(_selectedRoleName))
            {
                ShowInfo("Сначала выберите роль для редактирования");
                return;
            }

            string newRoleName = RoleTextBox.Text.Trim();

            if (_selectedRoleName == newRoleName)
            {
                ShowInfo("Название роли не изменилось");
                return;
            }

            if (IsSystemRole(_selectedRoleName))
            {
                MessageBox.Show("Системные роли нельзя редактировать",
                               "Предупреждение",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                ResetRoleEditingState();
                return;
            }

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, существует ли уже такая роль
                    string checkQuery = "SELECT COUNT(*) FROM Role WHERE RoleName = @NewRoleName AND RoleName != @OldRoleName";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@NewRoleName", newRoleName);
                    checkCmd.Parameters.AddWithValue("@OldRoleName", _selectedRoleName);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowInfo("Роль с таким названием уже существует");
                        return;
                    }

                    // Обновляем роль
                    string updateQuery = "UPDATE Role SET RoleName = @NewRoleName WHERE RoleName = @OldRoleName";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@NewRoleName", newRoleName);
                    updateCmd.Parameters.AddWithValue("@OldRoleName", _selectedRoleName);

                    int affectedRows = updateCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Роль успешно обновлена");
                        LoadRolesData();

                        // Сбрасываем состояние редактирования
                        ResetRoleEditingState();
                    }
                    else
                    {
                        ShowInfo("Роль не найдена");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования роли: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        
        // Обновление состояния кнопок
        private void UpdateRoleButtonsState()
        {
            bool hasText = !string.IsNullOrWhiteSpace(RoleTextBox.Text);

            if (_isEditingRole)
            {
                // В режиме редактирования
                AddRole.Enabled = false; // Добавление недоступно
                EditRole.Enabled = hasText && !string.IsNullOrEmpty(_selectedRoleName); // Редактирование доступно только если есть текст и выбрана роль
            }
            else
            {
                // В режиме добавления
                AddRole.Enabled = hasText; // Добавление доступно если есть текст
                EditRole.Enabled = false; // Редактирование недоступно
            }
        }

        // Сброс состояния редактирования
        private void ResetRoleEditingState()
        {
            _isEditingRole = false;
            _selectedRoleName = "";
            RoleTextBox.Clear();
            UpdateRoleButtonsState();
        }
        // РАБОТА С УСЛУГАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) Конец----


        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        

        // ---------------------------------------------------------------------------------------------
        private void Show_Load(object sender, EventArgs e)
        {
            // Загружаем данные для активных вкладок
            LoadCurrentTabData();

            // Настраиваем DataGridView только для видимых вкладок
            if (tabControl1.TabPages.Contains(_tabUsers))
                ConfigureDataGridView(Users);
            if (tabControl1.TabPages.Contains(_tabClients))
                ConfigureDataGridView(dataGridViewClients);
            if (tabControl1.TabPages.Contains(_tabMasters))
                ConfigureDataGridView(dataGridViewMasters);
            if (tabControl1.TabPages.Contains(_tabRoles))
                ConfigureDataGridView(dataGridViewRoles);
            if (tabControl1.TabPages.Contains(_tabServices))
                ConfigureDataGridView(dataGridViewServices);
            if (tabControl1.TabPages.Contains(_tabStatuses))
            {
                ConfigureDataGridView(dataGridViewStatuses);
                ResetStatusEditingState(); // Добавьте эту строку
            } // Добавляем настройку для статусов


            // Инициализация кнопок для ролей
            ResetRoleEditingState();
        }

        private void ConfigureDataGridView(DataGridView name)
        {
            // Настройка выделения всей строки
            name.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            name.MultiSelect = false; // Запрещаем множественное выделение
            name.RowHeadersVisible = false; // Скрываем заголовки строк

            // Настройка внешнего вида выделения
            name.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            name.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Включаем возможность выделения строки
            name.ReadOnly = true; // Если данные только для чтения

            // Обработчик клика по ячейке для выделения строки
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

            // При переключении вкладок сбрасываем состояние редактирования ролей
            if (tabControl1.SelectedTab != _tabRoles)
            {
                ResetRoleEditingState();
            }
        }

        private void LoadCurrentTabData()
        {
            // Загружаем данные только для активной вкладки
            if (tabControl1.SelectedTab == null)
                return;

            string tabName = tabControl1.SelectedTab.Name;

            switch (tabName)
            {
                case "tabPage1": // Пользователи
                    if (_roleID == 2) // Только для админа
                        LoadUsersData();
                    break;
                case "tabPage2": // Мастера
                    if (_roleID == 2) // Только для админа
                        LoadMastersData();
                    break;
                case "tabPage3": // Роли
                    if (_roleID == 2)
                        LoadRolesData();
                    break;
                case "tabPage4": // Услуги
                    LoadServicesData();
                    break;
                case "tabPage5": // Клиенты
                    LoadClientsData();
                    break;
                case "tabPage6": // Статусы (новая вкладка)
                    if (_roleID == 2) // Только для админа
                        LoadStatusesData();
                    break;
            }

        }
        // СТАТУСЫ
        private void LoadStatusesData()
        {
            // Проверяем, доступна ли вкладка
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

                    // Создаем DataTable для отображения
                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("ID", typeof(int));
                    displayDt.Columns.Add("Название статуса", typeof(string));

                    // Копируем данные
                    foreach (DataRow row in dt.Rows)
                    {
                        displayDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название статуса"]?.ToString() ?? ""
                        );
                    }

                    dataGridViewStatuses.DataSource = displayDt;
                    dataGridViewStatuses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Скрываем ID колонку
                    dataGridViewStatuses.Columns["ID"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
                }
            }
        }

        private void LoadUsersData()
        {
            // Проверяем, доступна ли вкладка
            if (!tabControl1.TabPages.Contains(_tabUsers))
                return;

            using (var connection = GetNewConnection()) // Используем новое соединение
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
                r.RoleName as 'Роль'
            FROM Users u
            INNER JOIN Role r ON u.Role = r.IDRole
            ORDER BY u.LastName, u.FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с объединенным столбцом ФИО
                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int)); // Изменил на int
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Логин", typeof(string));
                    maskedDt.Columns.Add("Пароль", typeof(string));
                    maskedDt.Columns.Add("RoleID", typeof(int)); // Изменил на int
                    maskedDt.Columns.Add("Роль", typeof(string));

                    // Объединение ФИО в один столбец
                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = FormatToShortName(
                            row["Фамилия"]?.ToString(),
                            row["Имя"]?.ToString(),
                            row["Отчество"]?.ToString()
                        );

                        maskedDt.Rows.Add(
                            Convert.ToInt32(row["ID"]), // Явное преобразование
                            fullName,
                            row["Логин"],
                            row["Пароль"],
                            Convert.ToInt32(row["RoleID"]), // Явное преобразование
                            row["Роль"]
                        );
                    }

                    Users.DataSource = maskedDt;
                    Users.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Скрываем служебные колонки
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
            // Проверяем, доступна ли вкладка
            if (!tabControl1.TabPages.Contains(_tabMasters))
                return;

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
            ORDER BY u.LastName, u.FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с объединенным столбцом ФИО
                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int)); // Добавляем колонку ID
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Описание", typeof(string));
                    maskedDt.Columns.Add("Телефон", typeof(string));

                    // Маскировка телефона и объединение ФИО в один столбец
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
                            Convert.ToInt32(row["ID"]), // Добавляем ID
                            fullName,
                            row["Описание"],
                            phone
                        );
                    }

                    dataGridViewMasters.DataSource = maskedDt;
                    dataGridViewMasters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Скрываем ID колонку (но она доступна для получения данных)
                    dataGridViewMasters.Columns["ID"].Visible = false;

                    // Настройка отображения длинного текста
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

            // Убираем все нецифровые символы для упрощения обработки
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            // Если номер начинается с 7 или 8 (российский формат)
            if (digitsOnly.Length >= 11 && (digitsOnly.StartsWith("7") || digitsOnly.StartsWith("8")))
            {
                string lastFour = digitsOnly.Length >= 4 ? digitsOnly.Substring(digitsOnly.Length - 4) : digitsOnly;
                return $"+7(***)***{lastFour}";
            }
            // Для других форматов оставляем первые 2 символа и последние 4
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

        // Загрузка ролей
        private void LoadRolesData()
        {
            // Проверяем, доступна ли вкладка
            if (!tabControl1.TabPages.Contains(_tabRoles))
                return;

            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        RoleName as 'Название роли'
                    FROM Role
                    ORDER BY IDRole";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridViewRoles.DataSource = dt;
                    dataGridViewRoles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
                }
            }
        }

        // Загрузка услуг
        private void LoadServicesData()
        {
            // Проверяем, доступна ли вкладка
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
                c.CategoryName as 'Категория'
            FROM Services s
            INNER JOIN Category c ON s.Category = c.IDCategory
            ORDER BY c.CategoryName, s.ServiceName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с маскированными данными
                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int));
                    maskedDt.Columns.Add("Название услуги", typeof(string));
                    maskedDt.Columns.Add("Описание", typeof(string));
                    maskedDt.Columns.Add("Цена", typeof(decimal));
                    maskedDt.Columns.Add("Категория", typeof(string));
                    maskedDt.Columns.Add("CategoryID", typeof(int)); // Скрытая колонка

                    // Обработка каждой строки
                    foreach (DataRow row in dt.Rows)
                    {
                        maskedDt.Rows.Add(
                            Convert.ToInt32(row["ID"]),
                            row["Название услуги"]?.ToString() ?? "",
                            row["Описание"]?.ToString() ?? "",
                            Convert.ToDecimal(row["Цена"]),
                            row["Категория"]?.ToString() ?? "",
                            Convert.ToInt32(row["CategoryID"])
                        );
                    }

                    dataGridViewServices.DataSource = maskedDt;
                    dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Скрываем служебные колонки
                    dataGridViewServices.Columns["ID"].Visible = false;
                    dataGridViewServices.Columns["CategoryID"].Visible = false;

                    // Форматирование цены
                    dataGridViewServices.Columns["Цена"].DefaultCellStyle.Format = "C2";
                    dataGridViewServices.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    // Настройка отображения длинного текста
                    dataGridViewServices.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridViewServices.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
                }
            }
        }

        // Загрузка клиентов
        private void LoadClientsData()
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                IDClient as 'ID',
                LastName as 'Фамилия',
                FirstName as 'Имя',
                MiddleName as 'Отчество',
                Phone as 'Телефон'
            FROM Client
            ORDER BY LastName, FirstName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с объединенным столбцом ФИО
                    DataTable maskedDt = new DataTable();
                    maskedDt.Columns.Add("ID", typeof(int)); // Добавляем колонку ID
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Телефон", typeof(string));

                    // Маскировка телефона и объединение ФИО в один столбец
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
                            Convert.ToInt32(row["ID"]), // Добавляем ID
                            fullName,
                            phone
                        );
                    }

                    dataGridViewClients.DataSource = maskedDt;
                    dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Скрываем ID колонку (но она доступна для получения данных)
                    dataGridViewClients.Columns["ID"].Visible = false;

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
                }
            }
        }

        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2) // Администратор
            {
                MenuAdmin menuAdmin = new MenuAdmin(_fio);
                menuAdmin.Show();
                this.Hide();
            }
            else if (_roleID == 4) // Менеджер
            {
                Schedule menuManager = new Schedule();
                menuManager.Show();
                this.Hide();
            }
            else // Другие роли
            {
                // Начальная форма или форма входа

            }

        }

        private void AddUsers_Click(object sender, EventArgs e)
        {
            // Проверяем, доступна ли функция для текущей роли
            if (_roleID != 2) // Только для админа
            {
                MessageBox.Show("У вас нет прав для добавления пользователей",
                              "Доступ запрещен",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            AddUserForm addUserForm = new AddUserForm();

            DialogResult result = addUserForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadUsersData();
                MessageBox.Show("Пользователь успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddMaster_Click(object sender, EventArgs e)
        {
            // Проверяем, доступна ли функция для текущей роли
            if (_roleID != 2) // Только для админа
            {
                MessageBox.Show("У вас нет прав для добавления пользователей",
                              "Доступ запрещен",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            AddMasterForm addMasterForm = new AddMasterForm();

            DialogResult result = addMasterForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadMastersData();
                MessageBox.Show("Мастер успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddService_Click(object sender, EventArgs e)
        {
            AddServiceForm addServiceForm = new AddServiceForm();

            DialogResult result = addServiceForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadServicesData();
                MessageBox.Show("Услуга успешно добавлена", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            AddClientForm addClientForm = new AddClientForm();

            DialogResult result = addClientForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                LoadClientsData();
                MessageBox.Show("Клиент успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RoleTextBox_TextChanged_1(object sender, EventArgs e)
        {
            bool hasText = !string.IsNullOrWhiteSpace(RoleTextBox.Text);

            if (_isEditingRole)
            {
                // В режиме редактирования
                AddRole.Enabled = false; // Добавление недоступно
                EditRole.Enabled = hasText && !string.IsNullOrEmpty(_selectedRoleName); // Редактирование доступно только если есть текст и выбрана роль
            }
            else
            {
                // В режиме добавления
                AddRole.Enabled = hasText; // Добавление доступно если есть текст
                EditRole.Enabled = false; // Редактирование недоступно
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
                // Включаем режим редактирования
                _isEditingStatus = true;
                Status.Text = _selectedStatusName;

                // Настраиваем кнопки
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
                $"Вы точно хотите удалить статус '{statusName}'?",
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
            // Проверяем, является ли статус системным (нельзя удалять)
            string[] systemStatuses = { "Запланирован", "Подтвержден", "Выполнен", "Отменен" };
            return systemStatuses.Contains(statusName, StringComparer.OrdinalIgnoreCase);
        }

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
                        ShowInfo($"Нельзя удалить статус '{statusName}'. Найдено {recordCount} записей с этим статусом.");
                        return;
                    }

                    // Удаляем статус
                    string deleteQuery = "DELETE FROM Status WHERE StatusName = @StatusName";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@StatusName", statusName);

                    int affectedRows = deleteCmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Статус успешно удален");
                        LoadStatusesData();

                        // Сбрасываем состояние редактирования
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
                    MessageBox.Show($"Ошибка удаления статуса: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void AddStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Status.Text))
            {
                ShowInfo("Введите название статуса");
                return;
            }

            string newStatusName = Status.Text.Trim();

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

                        // Сбрасываем состояние
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

            private void EditStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Status.Text))
            {
                ShowInfo("Введите новое название статуса");
                return;
            }

            if (string.IsNullOrEmpty(_selectedStatusName))
            {
                ShowInfo("Сначала выберите статус для редактирования");
                return;
            }

            string newStatusName = Status.Text.Trim();

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

                    // Проверяем, существует ли уже такой статус
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

                        // Сбрасываем состояние редактирования
                        ResetStatusEditingState();
                    }
                    else
                    {
                        ShowInfo("Статус не найдена");
                    }

                    connection.Close();
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
            bool hasText = !string.IsNullOrWhiteSpace(Status.Text);

            if (_isEditingStatus)
            {
                // В режиме редактирования
                AddStatus.Enabled = false; // Добавление недоступно
                EditStatus.Enabled = hasText && !string.IsNullOrEmpty(_selectedStatusName);
            }
            else
            {
                // В режиме добавления (или по умолчанию)
                AddStatus.Enabled = hasText; // Добавление доступно только если есть текст
                EditStatus.Enabled = false; // Редактирование всегда недоступно, пока не выбран статус
            }

            // Если текстовое поле пустое, обе кнопки должны быть отключены
            if (!hasText)
            {
                AddStatus.Enabled = false;
                EditStatus.Enabled = false;
            }
        }

        // Сброс состояния редактирования статусов
        private void ResetStatusEditingState()
        {
            _isEditingStatus = false;
            _selectedStatusName = "";
            Status.Clear();

            // Явно отключаем кнопки
            AddStatus.Enabled = false;
            EditStatus.Enabled = false;

            UpdateStatusButtonsState();
        }

        // Обработчик изменения текста в поле статуса
        private void Status_TextChanged(object sender, EventArgs e)
        {
            bool hasText = !string.IsNullOrWhiteSpace(Status.Text);

            if (_isEditingStatus)
            {
                // В режиме редактирования
                AddStatus.Enabled = false; // Добавление недоступно
                EditStatus.Enabled = hasText && !string.IsNullOrEmpty(_selectedStatusName); // Редактирование доступно только если есть текст и выбран статус
            }
            else
            {
                // В режиме добавления
                AddStatus.Enabled = hasText; // Добавление доступно если есть текст
                EditStatus.Enabled = false; // Редактирование недоступно
            }
        }

        // Обработчик нажатия Esc в поле статуса
        private void StatusTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}