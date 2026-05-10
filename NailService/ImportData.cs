using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace NailService
{
    public partial class ImportData : Form
    {
        private string _connection;
        private OpenFileDialog openFileDialog;
        private DataTable databaseTables;

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
                Filter = "SQL файлы (*.sql)|*.sql|CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
                Title = "Выберите файл для импорта"
            };

            LoadTables();
        }

        private void LoadTables()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    databaseTables = con.GetSchema("Tables");

                    ComboBox cmbTables = this.Controls["cmbTables"] as ComboBox;
                    if (cmbTables != null)
                    {
                        cmbTables.Items.Clear();
                        cmbTables.Items.Add("-- Весь дамп (полный импорт) --");

                        foreach (DataRow row in databaseTables.Rows)
                        {
                            string tableName = row["TABLE_NAME"].ToString();
                            // Исключаем системные таблицы
                            if (!tableName.StartsWith("__") && tableName != "sysdiagrams")
                            {
                                cmbTables.Items.Add(tableName);
                            }
                        }

                        if (cmbTables.Items.Count > 0)
                            cmbTables.SelectedIndex = 0;
                    }
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
                TextBox txtFilePath = this.Controls["txtFilePath"] as TextBox;
                if (txtFilePath != null)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnImportCSV_Click(object sender, EventArgs e)
        {
            TextBox txtFilePath = this.Controls["txtFilePath"] as TextBox;
            ComboBox cmbTables = this.Controls["cmbTables"] as ComboBox;

            if (txtFilePath == null || string.IsNullOrEmpty(txtFilePath.Text))
            {
                MessageBox.Show("Выберите файл для импорта!");
                return;
            }

            if (cmbTables == null || cmbTables.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта!");
                return;
            }

            string selectedItem = cmbTables.SelectedItem.ToString();
            string filePath = txtFilePath.Text;
            string extension = Path.GetExtension(filePath).ToLower();

            // Проверяем, выбран ли полный дамп
            if (selectedItem == "-- Весь дамп (полный импорт) --")
            {
                if (extension == ".sql")
                {
                    ImportFullDump(filePath);
                }
                else
                {
                    MessageBox.Show("Для полного импорта выберите SQL файл дампа!");
                }
                return;
            }

            // Импорт отдельных таблиц
            if (extension == ".csv")
            {
                string tableName = selectedItem;
                ImportTableFromCSV(tableName, filePath);
            }
            else
            {
                MessageBox.Show("Для импорта отдельных таблиц выберите CSV файл!");
            }
        }

        /// <summary>
        /// Импорт полного дампа базы данных из SQL файла
        /// </summary>
        private void ImportFullDump(string filePath)
        {
            try
            {
                string sqlContent = File.ReadAllText(filePath, Encoding.UTF8);
                string[] queries = SplitSqlQueries(sqlContent);

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    con.Open();
                    int successCount = 0;
                    int errorCount = 0;

                    foreach (string query in queries)
                    {
                        // Дополнительная проверка перед выполнением
                        if (string.IsNullOrWhiteSpace(query))
                            continue;

                        if (query.Trim().Length < 3)
                            continue;

                        // Проверка на осмысленный SQL запрос
                        string upperQuery = query.Trim().ToUpper();
                        if (!upperQuery.StartsWith("CREATE") &&
                            !upperQuery.StartsWith("DROP") &&
                            !upperQuery.StartsWith("LOCK") &&
                            !upperQuery.StartsWith("UNLOCK") &&
                            !upperQuery.StartsWith("INSERT") &&
                            !upperQuery.StartsWith("ALTER") &&
                            !upperQuery.StartsWith("SET"))
                            continue;

                        try
                        {
                            using (MySqlCommand cmd = new MySqlCommand(query, con))
                            {
                                cmd.ExecuteNonQuery();
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}\nЗапрос: {query.Substring(0, Math.Min(200, query.Length))}");
                        }
                    }

                    MessageBox.Show($"Импорт дампа завершен!\n\n" +
                        $"Успешно выполнено: {successCount} запросов\n" +
                        $"Пропущено/ошибок: {errorCount}",
                        "Импорт завершен",
                        MessageBoxButtons.OK,
                        errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта дампа:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Разделение SQL скрипта на отдельные запросы
        /// </summary>
        private string[] SplitSqlQueries(string sql)
        {
            // Убираем комментарии
            sql = RemoveComments(sql);

            // Разделяем по точке с запятой, но учитываем строки и процедуры
            List<string> queries = new List<string>();
            StringBuilder currentQuery = new StringBuilder();
            bool inString = false;
            char stringChar = '\'';

            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];

                // Обработка строк
                if ((c == '\'' || c == '"') && (i == 0 || sql[i - 1] != '\\'))
                {
                    if (!inString)
                    {
                        inString = true;
                        stringChar = c;
                    }
                    else if (c == stringChar)
                    {
                        inString = false;
                    }
                }

                currentQuery.Append(c);

                // Если не в строке и встретили точку с запятой
                if (!inString && c == ';')
                {
                    string query = currentQuery.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        queries.Add(query);
                    }
                    currentQuery.Clear();
                }
            }

            // Добавляем последний запрос если есть
            string lastQuery = currentQuery.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastQuery))
            {
                queries.Add(lastQuery);
            }

            return queries.ToArray();
        }

        /// <summary>
        /// Удаление комментариев из SQL
        /// </summary>
        private string RemoveComments(string sql)
        {
            // Удаляем однострочные комментарии --
            sql = Regex.Replace(sql, @"--[^\r\n]*", "");
            // Удаляем многострочные комментарии /* */
            sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);
            return sql;
        }

        /// <summary>
        /// Импорт таблицы из CSV файла
        /// </summary>
        private void ImportTableFromCSV(string tableName, string filePath)
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

                    for (int i = 1; i < lines.Length; i++) // пропускаем заголовок
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        // Очищаем значения
                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO category (CategoryName, IsActive) 
                                VALUES (@CategoryName, @IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@CategoryName", values[0]);
                            cmd.Parameters.AddWithValue("@IsActive", string.IsNullOrEmpty(values[1]) ? 1 : Convert.ToInt32(values[1]));

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу category завершен!\nДобавлено записей: {successCount}");
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO masters (User, Description, Phone, IsActive) 
                                VALUES (@User, @Description, @Phone, @IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@User", Convert.ToInt32(values[0]));
                            cmd.Parameters.AddWithValue("@Description", values.Length > 1 ? values[1] : "");
                            cmd.Parameters.AddWithValue("@Phone", values.Length > 2 ? values[2] : "");
                            cmd.Parameters.AddWithValue("@IsActive", values.Length > 3 ? Convert.ToInt32(values[3]) : 1);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу masters завершен!\nДобавлено записей: {successCount}");
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO record (Master, Client, Date, Status, Service, User, discount) 
                                VALUES (@Master, @Client, @Date, @Status, @Service, @User, @discount)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Master", Convert.ToInt32(values[0]));
                            cmd.Parameters.AddWithValue("@Client", Convert.ToInt32(values[1]));
                            cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(values[2]));
                            cmd.Parameters.AddWithValue("@Status", Convert.ToInt32(values[3]));
                            cmd.Parameters.AddWithValue("@Service", Convert.ToInt32(values[4]));
                            cmd.Parameters.AddWithValue("@User", Convert.ToInt32(values[5]));
                            cmd.Parameters.AddWithValue("@discount", values.Length > 6 ? Convert.ToInt32(values[6]) : 0);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу record завершен!\nДобавлено записей: {successCount}");
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO role (RoleName) VALUES (@RoleName)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@RoleName", values[0]);
                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу role завершен!\nДобавлено записей: {successCount}");
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO services (ServiceName, Description, Price, Category, IsActive) 
                                VALUES (@ServiceName, @Description, @Price, @Category, @IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ServiceName", values[0]);
                            cmd.Parameters.AddWithValue("@Description", values.Length > 1 ? values[1] : "");
                            cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(values[2]));
                            cmd.Parameters.AddWithValue("@Category", Convert.ToInt32(values[3]));
                            cmd.Parameters.AddWithValue("@IsActive", values.Length > 4 ? Convert.ToInt32(values[4]) : 1);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу services завершен!\nДобавлено записей: {successCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта services:\n{ex.Message}");
            }
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO status (StatusName) VALUES (@StatusName)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@StatusName", values[0]);
                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу status завершен!\nДобавлено записей: {successCount}");
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

                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        string[] values = lines[i].Split(';');

                        for (int j = 0; j < values.Length; j++)
                        {
                            values[j] = values[j].Trim().Trim('"', '\'');
                        }

                        string query = @"INSERT INTO users (LastName, FirstName, MiddleName, Login, Password, Role, IsActive) 
                                VALUES (@LastName, @FirstName, @MiddleName, @Login, @Password, @Role, @IsActive)";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@LastName", values[0]);
                            cmd.Parameters.AddWithValue("@FirstName", values[1]);
                            cmd.Parameters.AddWithValue("@MiddleName", values.Length > 2 ? values[2] : "");
                            cmd.Parameters.AddWithValue("@Login", values.Length > 3 ? values[3] : "");
                            cmd.Parameters.AddWithValue("@Password", values.Length > 4 ? values[4] : "");
                            cmd.Parameters.AddWithValue("@Role", values.Length > 5 ? Convert.ToInt32(values[5]) : 1);
                            cmd.Parameters.AddWithValue("@IsActive", values.Length > 6 ? Convert.ToInt32(values[6]) : 1);

                            cmd.ExecuteNonQuery();
                            successCount++;
                        }
                    }

                    MessageBox.Show($"Импорт в таблицу users завершен!\nДобавлено записей: {successCount}");
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
    }
}