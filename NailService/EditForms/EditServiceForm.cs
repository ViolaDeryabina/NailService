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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
    public partial class EditServiceForm : Form
    {
        private string _connection;
        public ServiceModel Service { get; private set; }
        private EditUserClass _dataService; // Добавим сервис для работы с БД
        public EditServiceForm(ServiceModel service)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Service = service;
            _dataService = new EditUserClass(); // Инициализируем сервис            
            LoadCategory();
            LoadTextBoxs();
            
        }

        private void LoadTextBoxs()
        {
            NameService.Text = Service.ServiceName;
            CategoryCb.Text = Service.CategoryName;
            Price.Text = ((int)Service.Price).ToString();
            Description.Text = Service.Description;
        }

        private void LoadCategory()
        {
            // Загрузка ролей в комбобокс
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDCategory, CategoryName FROM category";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "CategoryName";
                    CategoryCb.ValueMember = "IDCategory";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }

        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void EditService_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveServiceData();
                // ОБНОВЛЯЕМ ДАННЫЕ В БАЗЕ!
                if (UpdateServiceInDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }               
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameService.Text))
            {
                MessageBox.Show("Введите название услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameService.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите стоимость услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            // Проверка корректности цены
            if (!decimal.TryParse(Price.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную стоимость (число больше 0)", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
                return false;
            }

            // Проверка описания
            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                MessageBox.Show("Введите описание услуги", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Description.Focus();
                return false;
            }

            // Проверка категории
            if (CategoryCb.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CategoryCb.Focus();
                return false;
            }

            

            return true;
        }

        private void SaveServiceData()
        {
            Service.ServiceName = NameService.Text.Trim();
            Service.Price = Convert.ToInt32(Price.Text.Trim());
            Service.Description = Description.Text.Trim();
            Service.Category = (int)CategoryCb.SelectedValue;
        }


        // НОВЫЙ МЕТОД: Обновление в базе данных
        private bool UpdateServiceInDatabase()
        {
            try
            {
                _dataService.UpdateServiceInDatabase(Service);
                return true;
            }
            catch (MySqlException mysqlEx)
            {
                // Детальная информация об ошибке MySQL
                MessageBox.Show($"Ошибка MySQL при обновлении услуги:\nКод: {mysqlEx.Number}\nСообщение: {mysqlEx.Message}",
                              "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении услуги: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
