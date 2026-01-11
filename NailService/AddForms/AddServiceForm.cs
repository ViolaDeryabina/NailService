using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class AddServiceForm : Form
    {
        private string _connection;
        public ServiceModel NewService { get; private set; }
        public AddServiceForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            LoadCategory();
            NewService = new ServiceModel();
        }
        private void LoadCategory()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDCategory, CategoryName FROM Category";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    Category.DataSource = dt;
                    Category.DisplayMember = "CategoryName";
                    Category.ValueMember = "IDCategory";

                    if (Category.Items.Count > 0)
                    {
                        Category.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену услуги", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            // Проверка описания
            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            // Проверка уникальности услуги
            if (!IsNameServiceUnique(NameService.Text.Trim()))
            {
                MessageBox.Show("Услуга с таким названием уже существует", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameService.Focus();
                NameService.SelectAll();
                return false;
            }
            return true;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveServiceData();
                if (AddServiceToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private bool IsNameServiceUnique(string service)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM services WHERE ServiceName = @ServiceName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", service);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveServiceData()
        {
            NewService.ServiceName = NameService.Text.Trim();
            NewService.Description = Description.Text.Trim();
            NewService.Price = Convert.ToInt32(Price.Text.Trim());
            NewService.Category = (int)Category.SelectedValue;
        }

        private bool AddServiceToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"INSERT INTO services 
                                    (ServiceName, Description,Price,Category) 
                                    VALUES (@ServiceName, @Description, @Price, @Category)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", NewService.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", NewService.Description);
                    cmd.Parameters.AddWithValue("@Price", NewService.Price);
                    cmd.Parameters.AddWithValue("@Category", NewService.Category);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить услугу", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void NameService_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameService.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(NameService.Text, true);

            if (filteredText != NameService.Text)
            {
                NameService.Text = filteredText;
                NameService.SelectionStart = Math.Min(selectionStart, NameService.Text.Length);
            }
        }

        private void Price_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Price.SelectionStart;
            // Разрешаем десятичную точку для цены
            bool allowDecimal = Price.Name == "Price" || Price.Name == "Count";
            string filteredText = InputValidator.FilterToDigitsOnly(Price.Text, allowDecimal);

            if (filteredText != Price.Text)
            {
                Price.Text = filteredText;
                Price.SelectionStart = Math.Min(selectionStart, Price.Text.Length);
            }
        }

        private void Description_TextChanged(object sender, EventArgs e)
        {

            int selectionStart = Description.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(Description.Text, true);

            if (filteredText != Description.Text)
            {
                Description.Text = filteredText;
                Description.SelectionStart = Math.Min(selectionStart, Description.Text.Length);
            }
        }

        
    }
}
