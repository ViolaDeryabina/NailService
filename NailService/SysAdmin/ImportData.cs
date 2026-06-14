using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace NailService
{
    public partial class ImportData : Form
    {
        private string _connection;
        private OpenFileDialog openFileDialog;

        public ImportData()
        {
            InitializeComponent();

            // Исправленная строка подключения с явным указанием кодировки
            string baseConnection = Connection.ConnectionString;

            // Удаляем возможные старые параметры кодировки и добавляем правильные
            baseConnection = Regex.Replace(baseConnection, ";?Charset=[^;]+", "", RegexOptions.IgnoreCase);
            baseConnection = Regex.Replace(baseConnection, ";?Character Set=[^;]+", "", RegexOptions.IgnoreCase);

            // Добавляем правильную кодировку
            _connection = baseConnection + ";Charset=utf8;Convert Zero Datetime=True;Allow Zero Datetime=True;";

            openFileDialog = new OpenFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                Title = "Выберите CSV файл для импорта"
            };

            LoadTables();
            LoadTablesExport();
        }

        private void LoadTables()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    DataTable tables = con.GetSchema("Tables");

                    cmbTables.Items.Clear();

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("__") && tableName != "sysdiagrams")
                        {
                            cmbTables.Items.Add(tableName);
                        }
                    }

                    if (cmbTables.Items.Count > 0)
                        cmbTables.SelectedIndex = 0;

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка таблиц: {ex.Message}");
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TextBox txtFilePath = FindControlByName(this, "txtFilePath") as TextBox;
                if (txtFilePath != null)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
                else
                {
                    MessageBox.Show("Не найден текстовое поле txtFilePath");
                }
            }
        }

        /// <summary>
        /// Рекурсивный поиск контрола по имени
        /// </summary>
        private Control FindControlByName(Control parent, string name)
        {
            if (parent.Name == name)
                return parent;

            foreach (Control child in parent.Controls)
            {
                Control found = FindControlByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void btnImportCSV_Click(object sender, EventArgs e)
        {
            TextBox txtFilePath = FindControlByName(this, "txtFilePath") as TextBox;
            ComboBox cmbTables = FindControlByName(this, "cmbTables") as ComboBox;

            if (txtFilePath == null)
            {
                MessageBox.Show("Не найден элемент txtFilePath!");
                return;
            }

            if (string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Выберите файл для импорта!");
                return;
            }

            if (cmbTables == null)
            {
                MessageBox.Show("Не найден элемент cmbTables!");
                return;
            }

            if (cmbTables.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта!");
                return;
            }

            string tableName = cmbTables.SelectedItem.ToString();
            string filePath = txtFilePath.Text;
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".csv")
            {
                ImportTableFromCSV(tableName, filePath);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите CSV файл для импорта!");
            }
        }

        /// <summary>
        /// Импорт таблицы из CSV файла
        /// </summary>
        private void ImportTableFromCSV(string tableName, string filePath)
        {
            // Добавьте проверку существования файла
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Файл не найден: {filePath}");
                return;
            }

            // Покажем пользователю, что импорт начался
            Cursor = Cursors.WaitCursor;

            try
            {
                switch (tableName)
                {
                    case "category":
                        ImportCategory(filePath);
                        break;
                    case "client":
                        ImportClient(filePath);
                        break;
                    case "masters":
                        ImportMasters(filePath);
                        break;
                    case "record":
                        ImportRecord(filePath);
                        break;
                    case "role":
                        ImportRole(filePath);
                        break;
                    case "services":
                        ImportServices(filePath);
                        break;
                    case "status":
                        ImportStatus(filePath);
                        break;
                    case "users":
                        ImportUsers(filePath);
                        break;
                    default:
                        MessageBox.Show($"Импорт для таблицы '{tableName}' не реализован!");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}\n\n{ex.StackTrace}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // Импорт в таблицу category
        private void ImportCategory(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    // Получаем заголовки, чтобы определить индексы колонок
                    string[] headers = lines[0].Split(';');
                    int colCategoryName = Array.IndexOf(headers, "CategoryName");
                    int colIsActive = Array.IndexOf(headers, "IsActive");
                    int colId = Array.IndexOf(headers, "IDCategory");

                    if (colCategoryName == -1)
                    {
                        MessageBox.Show("В CSV файле не найден обязательный столбец CategoryName");
                        return;
                    }

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO category (IDCategory, CategoryName, IsActive) 
                        VALUES (@IDCategory, @CategoryName, @IsActive)
                        ON DUPLICATE KEY UPDATE 
                        CategoryName = VALUES(CategoryName), 
                        IsActive = VALUES(IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            // Добавляем ID, если он есть
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDCategory", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDCategory", 0); // Автоинкремент
                            }

                            cmd.Parameters.AddWithValue("@CategoryName", values[colCategoryName]);

                            bool isActive = true;
                            if (colIsActive != -1 && values.Length > colIsActive && !string.IsNullOrEmpty(values[colIsActive]))
                            {
                                bool.TryParse(values[colIsActive], out isActive);
                            }
                            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу category завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта category:\n{ex.Message}");
            }
        }

        // Импорт в таблицу client
        private void ImportClient(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO client (LastName, FirstName, MiddleName, Phone, IsActive) 
                                VALUES (@LastName, @FirstName, @MiddleName, @Phone, @IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@LastName", values[0]);
                            cmd.Parameters.AddWithValue("@FirstName", values[1]);
                            cmd.Parameters.AddWithValue("@MiddleName", values.Length > 2 ? values[2] : "");
                            cmd.Parameters.AddWithValue("@Phone", values.Length > 3 ? values[3] : "");
                            cmd.Parameters.AddWithValue("@IsActive", values.Length > 4 ? Convert.ToInt32(values[4]) : 1);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу client завершен!\nДобавлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта client:\n{ex.Message}");
            }
        }

        // Импорт в таблицу masters
        private void ImportMasters(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDMaster");
                    int colUser = Array.IndexOf(headers, "User");
                    int colDescription = Array.IndexOf(headers, "Description");
                    int colPhone = Array.IndexOf(headers, "Phone");
                    int colIsActive = Array.IndexOf(headers, "IsActive");

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO masters (IDMaster, User, Description, Phone, IsActive) 
                        VALUES (@IDMaster, @User, @Description, @Phone, @IsActive)
                        ON DUPLICATE KEY UPDATE 
                        User = VALUES(User),
                        Description = VALUES(Description),
                        Phone = VALUES(Phone),
                        IsActive = VALUES(IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDMaster", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDMaster", 0);
                            }

                            cmd.Parameters.AddWithValue("@User", colUser != -1 && values.Length > colUser ? Convert.ToInt32(values[colUser]) : 0);
                            cmd.Parameters.AddWithValue("@Description", colDescription != -1 && values.Length > colDescription ? values[colDescription] : "");
                            cmd.Parameters.AddWithValue("@Phone", colPhone != -1 && values.Length > colPhone ? values[colPhone] : "");

                            bool isActive = true;
                            if (colIsActive != -1 && values.Length > colIsActive && !string.IsNullOrEmpty(values[colIsActive]))
                            {
                                bool.TryParse(values[colIsActive], out isActive);
                            }
                            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу masters завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта masters:\n{ex.Message}");
            }
        }

        // Импорт в таблицу record
        private void ImportRecord(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDRecord");
                    int colMaster = Array.IndexOf(headers, "Master");
                    int colClient = Array.IndexOf(headers, "Client");
                    int colDate = Array.IndexOf(headers, "Date");
                    int colStatus = Array.IndexOf(headers, "Status");
                    int colService = Array.IndexOf(headers, "Service");
                    int colUser = Array.IndexOf(headers, "User");
                    int colDiscount = Array.IndexOf(headers, "discount");

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO record (IDRecord, Master, Client, Date, Status, Service, User, discount) 
                        VALUES (@IDRecord, @Master, @Client, @Date, @Status, @Service, @User, @discount)
                        ON DUPLICATE KEY UPDATE 
                        Master = VALUES(Master),
                        Client = VALUES(Client),
                        Date = VALUES(Date),
                        Status = VALUES(Status),
                        Service = VALUES(Service),
                        User = VALUES(User),
                        discount = VALUES(discount)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDRecord", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDRecord", 0);
                            }

                            cmd.Parameters.AddWithValue("@Master", colMaster != -1 && values.Length > colMaster ? Convert.ToInt32(values[colMaster]) : 0);
                            cmd.Parameters.AddWithValue("@Client", colClient != -1 && values.Length > colClient ? Convert.ToInt32(values[colClient]) : 0);

                            DateTime date = DateTime.Now;
                            if (colDate != -1 && values.Length > colDate && DateTime.TryParse(values[colDate], out DateTime d))
                                date = d;
                            cmd.Parameters.AddWithValue("@Date", date);

                            cmd.Parameters.AddWithValue("@Status", colStatus != -1 && values.Length > colStatus ? Convert.ToInt32(values[colStatus]) : 1);
                            cmd.Parameters.AddWithValue("@Service", colService != -1 && values.Length > colService ? Convert.ToInt32(values[colService]) : 0);
                            cmd.Parameters.AddWithValue("@User", colUser != -1 && values.Length > colUser ? Convert.ToInt32(values[colUser]) : 0);

                            int discount = 0;
                            if (colDiscount != -1 && values.Length > colDiscount && int.TryParse(values[colDiscount], out int dsc))
                                discount = dsc;
                            cmd.Parameters.AddWithValue("@discount", discount);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу record завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта record:\n{ex.Message}");
            }
        }

        // Импорт в таблицу role
        private void ImportRole(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDRole");
                    int colRoleName = Array.IndexOf(headers, "RoleName");

                    if (colRoleName == -1)
                    {
                        MessageBox.Show("В CSV файле не найден обязательный столбец RoleName");
                        return;
                    }

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO role (IDRole, RoleName) 
                        VALUES (@IDRole, @RoleName)
                        ON DUPLICATE KEY UPDATE 
                        RoleName = VALUES(RoleName)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDRole", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDRole", 0);
                            }

                            cmd.Parameters.AddWithValue("@RoleName", values[colRoleName]);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу role завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта role:\n{ex.Message}");
            }
        }

        // Импорт в таблицу services
        private void ImportServices(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDService");
                    int colServiceName = Array.IndexOf(headers, "ServiceName");
                    int colDescription = Array.IndexOf(headers, "Description");
                    int colPrice = Array.IndexOf(headers, "Price");
                    int colCategory = Array.IndexOf(headers, "Category");
                    int colIsActive = Array.IndexOf(headers, "IsActive");

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO services (IDService, ServiceName, Description, Price, Category, IsActive) 
                        VALUES (@IDService, @ServiceName, @Description, @Price, @Category, @IsActive)
                        ON DUPLICATE KEY UPDATE 
                        ServiceName = VALUES(ServiceName),
                        Description = VALUES(Description),
                        Price = VALUES(Price),
                        Category = VALUES(Category),
                        IsActive = VALUES(IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDService", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDService", 0);
                            }

                            cmd.Parameters.AddWithValue("@ServiceName", values[colServiceName]);
                            cmd.Parameters.AddWithValue("@Description", colDescription != -1 && values.Length > colDescription ? values[colDescription] : "");

                            decimal price = 0;
                            if (colPrice != -1 && values.Length > colPrice && decimal.TryParse(values[colPrice], out decimal p))
                                price = p;
                            cmd.Parameters.AddWithValue("@Price", price);

                            cmd.Parameters.AddWithValue("@Category", colCategory != -1 && values.Length > colCategory ? Convert.ToInt32(values[colCategory]) : 0);

                            bool isActive = true;
                            if (colIsActive != -1 && values.Length > colIsActive && !string.IsNullOrEmpty(values[colIsActive]))
                            {
                                bool.TryParse(values[colIsActive], out isActive);
                            }
                            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу services завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта services:\n{ex.Message}");
            }
        }

        // Вспомогательный метод для правильного парсинга CSV с кавычками
        private string[] ParseCSVLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new string[0];

            List<string> result = new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Экранированная кавычка
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    result.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString().Trim());

            // Обработка пустых полей в конце строки
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] == "")
                    result[i] = null;
            }

            return result.ToArray();
        }

        private string CleanValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Trim().Trim('"', '\'');
        }


        // Импорт в таблицу status
        private void ImportStatus(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDStatus");
                    int colStatusName = Array.IndexOf(headers, "StatusName");

                    if (colStatusName == -1)
                    {
                        MessageBox.Show("В CSV файле не найден обязательный столбец StatusName");
                        return;
                    }

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO status (IDStatus, StatusName) 
                        VALUES (@IDStatus, @StatusName)
                        ON DUPLICATE KEY UPDATE 
                        StatusName = VALUES(StatusName)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDStatus", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDStatus", 0);
                            }

                            cmd.Parameters.AddWithValue("@StatusName", values[colStatusName]);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу status завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта status:\n{ex.Message}");
            }
        }

        // Импорт в таблицу users
        private void ImportUsers(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст");
                    return;
                }

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;

                    string[] headers = lines[0].Split(';');
                    int colId = Array.IndexOf(headers, "IDUser");
                    int colLastName = Array.IndexOf(headers, "LastName");
                    int colFirstName = Array.IndexOf(headers, "FirstName");
                    int colMiddleName = Array.IndexOf(headers, "MiddleName");
                    int colLogin = Array.IndexOf(headers, "Login");
                    int colPassword = Array.IndexOf(headers, "Password");
                    int colRole = Array.IndexOf(headers, "Role");
                    int colIsActive = Array.IndexOf(headers, "IsActive");

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = ParseCSVLine(lines[i]);

                        string query = @"INSERT INTO users (IDUser, LastName, FirstName, MiddleName, Login, Password, Role, IsActive) 
                        VALUES (@IDUser, @LastName, @FirstName, @MiddleName, @Login, @Password, @Role, @IsActive)
                        ON DUPLICATE KEY UPDATE 
                        LastName = VALUES(LastName),
                        FirstName = VALUES(FirstName),
                        MiddleName = VALUES(MiddleName),
                        Login = VALUES(Login),
                        Password = VALUES(Password),
                        Role = VALUES(Role),
                        IsActive = VALUES(IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            if (colId != -1 && values.Length > colId && !string.IsNullOrEmpty(values[colId]))
                            {
                                cmd.Parameters.AddWithValue("@IDUser", Convert.ToInt32(values[colId]));
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@IDUser", 0);
                            }

                            cmd.Parameters.AddWithValue("@LastName", colLastName != -1 && values.Length > colLastName ? values[colLastName] : "");
                            cmd.Parameters.AddWithValue("@FirstName", colFirstName != -1 && values.Length > colFirstName ? values[colFirstName] : "");
                            cmd.Parameters.AddWithValue("@MiddleName", colMiddleName != -1 && values.Length > colMiddleName ? values[colMiddleName] : "");
                            cmd.Parameters.AddWithValue("@Login", colLogin != -1 && values.Length > colLogin ? values[colLogin] : "");
                            cmd.Parameters.AddWithValue("@Password", colPassword != -1 && values.Length > colPassword ? values[colPassword] : "");
                            cmd.Parameters.AddWithValue("@Role", colRole != -1 && values.Length > colRole ? Convert.ToInt32(values[colRole]) : 1);

                            bool isActive = true;
                            if (colIsActive != -1 && values.Length > colIsActive && !string.IsNullOrEmpty(values[colIsActive]))
                            {
                                bool.TryParse(values[colIsActive], out isActive);
                            }
                            cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу users завершен!\nДобавлено/обновлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта users:\n{ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SysAdmin show = new SysAdmin();
            show.Show();
            this.Hide();
        }

        private void LoadTablesExport()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    DataTable tables = con.GetSchema("Tables");

                    cmbTablesCSV.Items.Clear();
                    cmbTablesCSV.Items.Add("-- Все таблицы --");

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("__") && tableName != "sysdiagrams")
                        {
                            cmbTablesCSV.Items.Add(tableName);
                        }
                    }

                    if (cmbTablesCSV.Items.Count > 0)
                        cmbTablesCSV.SelectedIndex = 0;

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки таблиц: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Экспорт в CSV
        /// </summary>
        private async void BtnExportCSV_Click(object sender, EventArgs e)
        {
            if (cmbTablesCSV.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для экспорта!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedTable = cmbTablesCSV.SelectedItem.ToString();

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку для сохранения CSV файлов";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string exportFolder = folderDialog.SelectedPath;

                    try
                    {
                        Cursor = Cursors.WaitCursor;

                        if (selectedTable == "-- Все таблицы --")
                        {
                            await ExportAllTablesToCSV(exportFolder);
                        }
                        else
                        {
                            await ExportSingleTableToCSV(selectedTable, exportFolder);
                        }

                        Cursor = Cursors.Default;

                        if (MessageBox.Show("Экспорт завершен!\n\nОткрыть папку с файлами?",
                            "Успех", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", exportFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        Cursor = Cursors.Default;
                        MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Экспорт одной таблицы в CSV
        /// </summary>
        private async Task ExportSingleTableToCSV(string tableName, string exportFolder)
        {
            string csvPath = Path.Combine(exportFolder, $"{tableName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                await con.OpenAsync();

                string query = $"SELECT * FROM `{tableName}`";
                MySqlCommand cmd = new MySqlCommand(query, con);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                using (StreamWriter writer = new StreamWriter(csvPath, false, Encoding.UTF8))
                {
                    // Заголовки
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (i > 0) writer.Write(";");
                        writer.Write(reader.GetName(i));
                    }
                    writer.WriteLine();

                    // Данные
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (i > 0) writer.Write(";");

                            if (!reader.IsDBNull(i))
                            {
                                string value = reader.GetValue(i).ToString();
                                if (value.Contains(";") || value.Contains("\""))
                                {
                                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                                }
                                writer.Write(value);
                            }
                        }
                        writer.WriteLine();
                    }
                }
            }

            MessageBox.Show($"Таблица '{tableName}' экспортирована!\n\nФайл: {csvPath}",
                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Экспорт всех таблиц в CSV
        /// </summary>
        private async Task ExportAllTablesToCSV(string exportFolder)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string exportSubFolder = Path.Combine(exportFolder, $"Export_{timestamp}");
            Directory.CreateDirectory(exportSubFolder);

            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                await con.OpenAsync();
                DataTable tables = con.GetSchema("Tables");
                int exportedCount = 0;

                foreach (DataRow row in tables.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();

                    if (tableName.StartsWith("__") || tableName == "sysdiagrams")
                        continue;

                    string csvPath = Path.Combine(exportSubFolder, $"{tableName}.csv");

                    string query = $"SELECT * FROM `{tableName}`";
                    MySqlCommand cmd = new MySqlCommand(query, con);

                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    using (StreamWriter writer = new StreamWriter(csvPath, false, Encoding.UTF8))
                    {
                        // Заголовки
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (i > 0) writer.Write(";");
                            writer.Write(reader.GetName(i));
                        }
                        writer.WriteLine();

                        // Данные
                        while (await reader.ReadAsync())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (i > 0) writer.Write(";");

                                if (!reader.IsDBNull(i))
                                {
                                    string value = reader.GetValue(i).ToString();
                                    if (value.Contains(";") || value.Contains("\""))
                                    {
                                        value = "\"" + value.Replace("\"", "\"\"") + "\"";
                                    }
                                    writer.Write(value);
                                }
                            }
                            writer.WriteLine();
                        }
                    }
                    exportedCount++;
                }
            }

            MessageBox.Show($"Таблицы экспортированы!\n\nПапка: {exportSubFolder}",
                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}