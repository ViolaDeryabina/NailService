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
                    cmbTables.Items.Add("-- Весь дамп (полный импорт) --");

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
        private async void ImportFullDump(string filePath)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                string sqlContent = File.ReadAllText(filePath, Encoding.UTF8);

                // Предлагаем очистить таблицы перед импортом
                DialogResult clearResult = MessageBox.Show(
                    "Очистить существующие таблицы перед импортом?\n\n" +
                    "Да - таблицы будут удалены и созданы заново\n" +
                    "Нет - данные будут добавлены к существующим\n" +
                    "Внимание! При выборе 'Да' все существующие данные будут потеряны!",
                    "Очистка таблиц", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (clearResult == DialogResult.Cancel)
                {
                    Cursor = Cursors.Default;
                    return;
                }

                bool clearTables = (clearResult == DialogResult.Yes);

                using (MySqlConnection con = new MySqlConnection(_connection))
                {
                    await con.OpenAsync();

                    if (clearTables)
                    {
                        // Правильный порядок удаления таблиц (сначала те, у которых есть внешние ключи)
                        string[] dropOrder = {
                    "record", "masters", "services", "client", "users", "status", "category", "role"
                };

                        foreach (var table in dropOrder)
                        {
                            try
                            {
                                // Отключаем проверку внешних ключей
                                using (var cmd = new MySqlCommand($"SET FOREIGN_KEY_CHECKS = 0", con))
                                    await cmd.ExecuteNonQueryAsync();

                                using (var cmd = new MySqlCommand($"DROP TABLE IF EXISTS `{table}`", con))
                                    await cmd.ExecuteNonQueryAsync();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ошибка удаления {table}: {ex.Message}");
                            }
                        }

                        // Включаем проверку внешних ключей обратно
                        using (var cmd = new MySqlCommand($"SET FOREIGN_KEY_CHECKS = 1", con))
                            await cmd.ExecuteNonQueryAsync();

                        MessageBox.Show("Таблицы очищены.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Разбираем SQL на запросы
                    var queries = ParseSqlQueries(sqlContent);

                    int totalQueries = queries.Count;
                    int successCount = 0;
                    int errorCount = 0;
                    var errors = new List<string>();

                    // Сортируем запросы: сначала CREATE TABLE, потом INSERT, потом остальное
                    var sortedQueries = SortQueriesByType(queries);

                    // Создаем прогресс бар
                    using (var progressForm = new Form())
                    {
                        progressForm.Text = "Импорт данных";
                        progressForm.Size = new System.Drawing.Size(500, 120);
                        progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        progressForm.StartPosition = FormStartPosition.CenterParent;
                        progressForm.ControlBox = false;

                        var progressBar = new ProgressBar();
                        progressBar.Dock = DockStyle.Top;
                        progressBar.Height = 30;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = sortedQueries.Count;
                        progressBar.Value = 0;

                        var lblStatus = new Label();
                        lblStatus.Dock = DockStyle.Fill;
                        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        lblStatus.Text = "Выполнение запросов...";
                        lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10);

                        progressForm.Controls.Add(lblStatus);
                        progressForm.Controls.Add(progressBar);
                        progressForm.Height = 150;

                        progressForm.Show();

                        // Отключаем проверку внешних ключей на время импорта
                        using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0", con))
                            await cmd.ExecuteNonQueryAsync();

                        try
                        {
                            for (int i = 0; i < sortedQueries.Count; i++)
                            {
                                string query = sortedQueries[i];
                                progressBar.Value = i + 1;
                                lblStatus.Text = $"Выполнение запроса {i + 1} из {sortedQueries.Count}...";
                                Application.DoEvents();

                                if (string.IsNullOrWhiteSpace(query))
                                    continue;

                                try
                                {
                                    using (var cmd = new MySqlCommand(query, con))
                                    {
                                        await cmd.ExecuteNonQueryAsync();
                                        successCount++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    string shortQuery = query.Length > 100 ? query.Substring(0, 100) + "..." : query;
                                    errors.Add($"Ошибка: {ex.Message}\nЗапрос: {shortQuery}");
                                }
                            }
                        }
                        finally
                        {
                            // Включаем проверку внешних ключей обратно
                            using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1", con))
                                await cmd.ExecuteNonQueryAsync();
                        }

                        progressForm.Close();
                    }

                    Cursor = Cursors.Default;

                    string message = $"Импорт дампа завершен!\n\n" +
                        $"✅ Успешно выполнено: {successCount} запросов\n" +
                        $"❌ Ошибок: {errorCount}";

                    if (errors.Count > 0 && errors.Count <= 5)
                    {
                        message += $"\n\nОшибки:\n{string.Join("\n", errors)}";
                    }
                    else if (errors.Count > 5)
                    {
                        message += $"\n\nПервые 5 ошибок:\n{string.Join("\n", errors.Take(5))}";
                    }

                    MessageBox.Show(message, "Импорт завершен",
                        MessageBoxButtons.OK, errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка импорта дампа:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Сортировка запросов: сначала CREATE TABLE, потом INSERT, потом остальное
        /// </summary>
        private List<string> SortQueriesByType(List<string> queries)
        {
            var createQueries = new List<string>();
            var insertQueries = new List<string>();
            var otherQueries = new List<string>();

            foreach (var query in queries)
            {
                string upperQuery = query.Trim().ToUpper();

                if (upperQuery.StartsWith("CREATE TABLE"))
                {
                    createQueries.Add(query);
                }
                else if (upperQuery.StartsWith("INSERT INTO"))
                {
                    insertQueries.Add(query);
                }
                else if (upperQuery.StartsWith("DROP TABLE") ||
                         upperQuery.StartsWith("LOCK TABLES") ||
                         upperQuery.StartsWith("UNLOCK TABLES") ||
                         upperQuery.StartsWith("ALTER TABLE"))
                {
                    // Пропускаем, так как таблицы уже созданы
                    continue;
                }
                else
                {
                    otherQueries.Add(query);
                }
            }

            var result = new List<string>();
            result.AddRange(createQueries);
            result.AddRange(insertQueries);
            result.AddRange(otherQueries);

            return result;
        }


        /// <summary>
        /// Парсинг SQL скрипта на отдельные запросы
        /// </summary>
        private List<string> ParseSqlQueries(string sql)
        {
            var queries = new List<string>();

            // Убираем BOM и лишние символы
            if (sql.Length > 0 && sql[0] == 0xFEFF)
                sql = sql.Substring(1);

            // Убираем комментарии
            sql = RemoveComments(sql);

            // Обрабатываем построчно
            var currentQuery = new StringBuilder();
            bool inString = false;
            bool inEscape = false;
            char stringDelimiter = '\'';

            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];

                // Обработка экранирования
                if (inEscape)
                {
                    currentQuery.Append(c);
                    inEscape = false;
                    continue;
                }

                // Обработка начала экранирования
                if (c == '\\' && inString)
                {
                    currentQuery.Append(c);
                    inEscape = true;
                    continue;
                }

                // Обработка строк
                if ((c == '\'' || c == '"') && !inEscape)
                {
                    if (!inString)
                    {
                        inString = true;
                        stringDelimiter = c;
                    }
                    else if (c == stringDelimiter)
                    {
                        // Проверяем, не экранирован ли кавычка (двойная кавычка)
                        if (i + 1 < sql.Length && sql[i + 1] == stringDelimiter)
                        {
                            currentQuery.Append(c);
                            i++; // Пропускаем следующую кавычку
                            currentQuery.Append(sql[i]);
                        }
                        else
                        {
                            inString = false;
                        }
                    }
                }

                currentQuery.Append(c);

                // Если встретили точку с запятой и не внутри строки
                if (c == ';' && !inString)
                {
                    string query = currentQuery.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(query) && query.Length > 3)
                    {
                        queries.Add(query);
                    }
                    currentQuery.Clear();
                }
            }

            // Добавляем последний запрос, если есть
            string lastQuery = currentQuery.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastQuery) && lastQuery.Length > 3)
            {
                queries.Add(lastQuery);
            }

            return queries;
        }

        /// <summary>
        /// Удаление комментариев из SQL
        /// </summary>
        private string RemoveComments(string sql)
        {
            // Удаляем однострочные комментарии --
            sql = Regex.Replace(sql, @"--[^\r\n]*", "", RegexOptions.Multiline);

            // Удаляем многострочные комментарии /* */
            sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Удаляем пустые строки
            sql = Regex.Replace(sql, @"^\s*$\r?\n", "", RegexOptions.Multiline);

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

            MessageBox.Show($"Экспортировано таблиц: \n\nПапка: {exportSubFolder}",
                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Экспорт всей базы данных в SQL дамп
        /// </summary>
        private async void BtnExportSQL_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Экспорт базы данных в SQL";
            saveDialog.Filter = "SQL файлы (*.sql)|*.sql|Все файлы (*.*)|*.*";
            saveDialog.DefaultExt = "sql";
            saveDialog.FileName = $"db86_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    await ExportDatabaseToSQL(saveDialog.FileName);

                    Cursor = Cursors.Default;

                    MessageBox.Show($"База данных успешно экспортирована!\n\nФайл: {saveDialog.FileName}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (MessageBox.Show("Открыть папку с файлом?", "Открыть папку",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(saveDialog.FileName));
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

        /// <summary>
        /// Экспорт базы данных в SQL дамп
        /// </summary>
        private async Task ExportDatabaseToSQL(string outputPath)
        {
            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                await con.OpenAsync();

                using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    writer.WriteLine("-- MySQL Database Export");
                    writer.WriteLine($"-- Database: {Connection.Database}");
                    writer.WriteLine($"-- Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine("-- -----------------------------------------------------");
                    writer.WriteLine();

                    DataTable tables = con.GetSchema("Tables");

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();

                        if (tableName.StartsWith("__") || tableName == "sysdiagrams")
                            continue;

                        // Структура таблицы
                        string createTableQuery = $"SHOW CREATE TABLE `{tableName}`";
                        using (MySqlCommand cmd = new MySqlCommand(createTableQuery, con))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                writer.WriteLine($"-- Table structure for `{tableName}`");
                                writer.WriteLine(reader.GetString(1) + ";");
                                writer.WriteLine();
                            }
                        }

                        // Данные
                        string selectQuery = $"SELECT * FROM `{tableName}`";
                        using (MySqlCommand cmd = new MySqlCommand(selectQuery, con))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            bool hasRows = false;
                            while (await reader.ReadAsync())
                            {
                                if (!hasRows)
                                {
                                    writer.WriteLine($"-- Dumping data for `{tableName}`");
                                    hasRows = true;
                                }

                                writer.Write($"INSERT INTO `{tableName}` VALUES (");
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (i > 0) writer.Write(", ");

                                    if (reader.IsDBNull(i))
                                    {
                                        writer.Write("NULL");
                                    }
                                    else
                                    {
                                        string value = reader.GetValue(i).ToString();
                                        value = value.Replace("\\", "\\\\").Replace("'", "''");
                                        writer.Write($"'{value}'");
                                    }
                                }
                                writer.WriteLine(");");
                            }
                            if (hasRows) writer.WriteLine();
                        }
                    }
                }
            }
        }
    }
}