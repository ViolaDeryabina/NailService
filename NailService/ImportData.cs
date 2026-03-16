using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                MessageBox.Show("Выберите CSV файл!");
                return;
            }

            if (cmbTables == null || cmbTables.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта!");
                return;
            }

            string tableName = cmbTables.SelectedItem.ToString();

            // Вызываем соответствующий метод в зависимости от выбранной таблицы
            switch (tableName)
            {
                case "category":
                    ImportCategory(txtFilePath.Text);
                    break;
                case "client":
                    ImportClient(txtFilePath.Text);
                    break;
                case "masters":
                    ImportMasters(txtFilePath.Text);
                    break;
                case "record":
                    ImportRecord(txtFilePath.Text);
                    break;
                case "role":
                    ImportRole(txtFilePath.Text);
                    break;
                case "services":
                    ImportServices(txtFilePath.Text);
                    break;
                case "status":
                    ImportStatus(txtFilePath.Text);
                    break;
                case "users":
                    ImportUsers(txtFilePath.Text);
                    break;
                default:
                    MessageBox.Show("Неизвестная таблица!");
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

                        // Ожидаемый порядок колонок в CSV: CategoryName, IsActive
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

                        // Ожидаемый порядок: LastName, FirstName, MiddleName, Phone, IsActive
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

                        // Ожидаемый порядок: User, Description, Phone, IsActive
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

                        // Ожидаемый порядок: Master, Client, Date, Status, Service, User, discount
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

                        // Ожидаемый порядок: RoleName
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

                        // Ожидаемый порядок: ServiceName, Description, Price, Category, IsActive
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

                        // Ожидаемый порядок: StatusName
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

                        // Ожидаемый порядок: LastName, FirstName, MiddleName, Login, Password, Role, IsActive
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