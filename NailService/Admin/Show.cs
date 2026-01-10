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
        public Show(string FIO)
        {
            InitializeComponent();
            _fio = FIO;
            _connection = Connection.ConnectionString;
        }

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
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

        private void EditSelectedUser()
        {
            if (Users.SelectedRows.Count == 0)
            {
                ShowInfo("Выберите товар для редактирования");
                return;
            }

            var selectedRow = Users.SelectedRows[0];
           OpenEditForm(selectedRow);
        }
        private void OpenEditForm(DataGridViewRow row)
        {
            try
            {
                // Получаем ID из скрытой колонки
                int userId = Convert.ToInt32(row.Cells["ID"].Value);

                // Загружаем полные данные пользователя из базы по ID
                var userModel = LoadUserById(userId);

                if (userModel != null)
                {
                    // Открываем форму редактирования
                    var editForm = new EditUserForm(userModel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновляем данные в базе
                        UpdateUserInDatabase(editForm.User);

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

        // Метод для загрузки полных данных пользователя по ID
        private UserModel LoadUserById(int userId)
        {
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                    u.IDUser,
                    u.LastName,
                    u.FirstName,
                    u.MiddleName,
                    u.Login,
                    u.Password,
                    u.Role as RoleID,
                    r.RoleName
                FROM Users u
                INNER JOIN Role r ON u.Role = r.IDRole
                WHERE u.IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                UserId = reader.GetInt32("IDUser"),
                                LastName = reader["LastName"]?.ToString() ?? "",
                                FirstName = reader["FirstName"]?.ToString() ?? "",
                                MiddleName = reader["MiddleName"]?.ToString() ?? "",
                                Login = reader["Login"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                RoleId = reader.GetInt32("RoleID"),
                                RoleName = reader["RoleName"]?.ToString() ?? ""
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
                    return null;
                }
            }
        }

        private void UpdateUserInDatabase(UserModel user)
        {
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE Users 
                        SET LastName = @LastName, 
                            FirstName = @FirstName, 
                            MiddleName = @MiddleName, 
                            Login = @Login, 
                            Password = @Password, 
                            Role = @Role 
                        WHERE IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName);
                    cmd.Parameters.AddWithValue("@Login", user.Login);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Role", user.RoleId);
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                }
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

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        

        // ---------------------------------------------------------------------------------------------
        private void Show_Load(object sender, EventArgs e)
        {         
            
            LoadUsersData();
            ConfigureDataGridView(dataGridViewClients);
            ConfigureDataGridView(Users);
            ConfigureDataGridView(dataGridViewMasters);
            ConfigureDataGridView(dataGridViewRoles);
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
            name.CellClick += (s, e) => {
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
            switch (tabControl1.SelectedIndex)
            {
                case 0: // Пользователи
                    LoadUsersData();
                    break;
                case 1: // Мастера
                    LoadMastersData();
                    break;
                case 2: // Роли
                    LoadRolesData();
                    break;
                case 3: // Услуги
                    LoadServicesData();
                    break;
                case 4: // Клиенты
                    LoadClientsData();
                    break;

            }
        }

        private void LoadUsersData()
        {
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
                    maskedDt.Columns.Add("Роль", typeof(string));

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
                            phone,
                            row["Роль"]
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
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        s.ServiceName as 'Название услуги',
                        s.Description as 'Описание',
                        s.Price as 'Цена',
                        c.CategoryName as 'Категория'
                    FROM Services s
                    INNER JOIN Category c ON s.Category = c.IDCategory
                    ORDER BY c.CategoryName, s.ServiceName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridViewServices.DataSource = dt;
                    dataGridViewServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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
            if (_fio != "")
            {
                MenuAdmin menuAdmin = new MenuAdmin(_fio);
                menuAdmin.Show();
                this.Hide();
            }
            else
            {
                Schedule menuManager = new Schedule();
                menuManager.Show();
                this.Hide();
            }
            
        }

        private void AddUsers_Click(object sender, EventArgs e)
        {
            AddUserForm addUserForm = new AddUserForm();

            // Показываем форму как модальное окно
            DialogResult result = addUserForm.ShowDialog();

            // После закрытия формы добавления обновляем данные
            if (result == DialogResult.OK)
            {
                LoadUsersData(); // Перезагружаем данные пользователей
                MessageBox.Show("Пользователь успешно добавлен", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
