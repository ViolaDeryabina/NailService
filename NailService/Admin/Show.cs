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

        private EditUserClass _editUserClass;
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

            ConfigureTabsByRole();
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
                    _tabRoles
                });

                // Показываем кнопку добавления пользователей для админа
                AddUsers.Visible = true;
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
            }
            else if (_roleID == 3) // Мастер (если нужно)
            {
                // Пример для мастера - только клиенты
                tabControl1.TabPages.Add(_tabClients);
                AddUsers.Visible = false;
            }
            else // Гость или другие роли
            {
                tabControl1.TabPages.Add(_tabServices); // Только просмотр услуг
                AddUsers.Visible = false;
            }
        }


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
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Users WHERE IDUser = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Пользователь успешно удален");
                        LoadUsersData(); // Перезагружаем данные
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}");
                }
            }
        }
        // РАБОТА С ПОЛЬЗОВАТЕЛЯМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) Конец----


        // РАБОТА С УСЛУГАМИ (УДАЛЕНИЕ, РЕДАКТИРОВАНИЕ И ДОБАВЛЕНИЕ) 
        private void dataGridViewServices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Исправлено: используем dataGridViewServices, а не Users
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
                DeleteServiceFromDatabase(serviceId);
            }
        }

        private void DeleteServiceFromDatabase(int serviceId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // ИСПРАВЛЕНО: Правильное имя таблицы и столбца
                    string query = "DELETE FROM Services WHERE IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Услуга успешно удалена");
                        LoadServicesData(); // Перезагружаем данные услуг
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
                     // Доступно для всех
                    break;
                case "tabPage4": // Услуги
                    LoadServicesData();                  
                    break;
                case "tabPage5": // Клиенты
                    LoadClientsData();
                    break;
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

            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
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
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Описание", typeof(string));
                    maskedDt.Columns.Add("Телефон", typeof(string));

                    // Маскировка ФИО и телефона, объединение ФИО в один столбец
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
                            fullName,
                            row["Описание"],
                            phone
                        );
                    }

                    dataGridViewMasters.DataSource = maskedDt;
                    dataGridViewMasters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Настройка отображения длинного текста
                    dataGridViewMasters.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridViewMasters.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
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
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
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
                    maskedDt.Columns.Add("ФИО", typeof(string));
                    maskedDt.Columns.Add("Телефон", typeof(string));

                    // Маскировка ФИО и телефона, объединение ФИО в один столбец
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
                           fullName,
                           phone
                       );
                    }

                    dataGridViewMasters.DataSource = maskedDt;
                    dataGridViewMasters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    dataGridViewClients.DataSource = maskedDt;
                    dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        
    }
}
