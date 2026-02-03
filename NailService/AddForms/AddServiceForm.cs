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
        private Show _showForm; // Ссылка на главную форму

        public AddServiceForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm; // Сохраняем ссылку на главную форму
            NewService = new ServiceModel();
            LoadCategory();
        }

        private void LoadCategory()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT IDCategory, CategoryName FROM Category WHERE IsActive = 1";
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
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
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

            // Проверка что цена - валидное число
            if (!decimal.TryParse(Price.Text, out decimal priceValue) || priceValue <= 0)
            {
                MessageBox.Show("Введите корректную цену (положительное число)", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                Price.SelectAll();
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

            // Проверка, что услуга с таким названием не активна
            if (IsActiveServiceExists())
            {
                MessageBox.Show("Активная услуга с таким названием уже существует", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameService.Focus();
                NameService.SelectAll();
                return false;
            }

            return true;
        }

        // Проверяет, есть ли активная услуга с таким названием
        private bool IsActiveServiceExists()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM services 
                                   WHERE ServiceName = @ServiceName 
                                   AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", NameService.Text.Trim());

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // В случае ошибки считаем, что услуга существует
            }
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
                // Проверяем, есть ли неактивная услуга для восстановления
                if (CheckAndRestoreInactiveService())
                {
                    return; // Услуга восстановлена, форма закрывается
                }

                // Иначе создаем новую услугу
                if (AddNewService())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        // Проверяет и восстанавливает неактивную услугу
        private bool CheckAndRestoreInactiveService()
        {
            try
            {
                string serviceName = NameService.Text.Trim();

                // Проверяем через базу данных
                var (exists, isActive, serviceId) = CheckServiceExists(serviceName);

                if (exists && !isActive)
                {
                    // Нашли неактивную услугу - предлагаем восстановить
                    var result = MessageBox.Show(
                        $"Найдена неактивная услуга с таким названием:\n\n" +
                        $"Название: {serviceName}\n\n" +
                        "Восстановить эту услугу с новыми данными?",
                        "Восстановление услуги",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Сохраняем данные из формы
                        SaveServiceData();

                        // Восстанавливаем услугу
                        bool restored = RestoreServiceInDatabase(serviceId, NewService);

                        if (restored)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось восстановить услугу", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Пользователь отказался от восстановления
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Проверяет существование услуги в базе
        private (bool exists, bool isActive, int serviceId) CheckServiceExists(string serviceName)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"SELECT IDServices, IsActive 
                                   FROM services 
                                   WHERE ServiceName = @ServiceName
                                   LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int serviceId = reader.GetInt32("IDServices");
                            bool isActive = reader.GetBoolean("IsActive");
                            return (true, isActive, serviceId);
                        }
                    }

                    return (false, false, 0);
                }
            }
            catch
            {
                return (false, false, 0);
            }
        }

        // Восстанавливает услугу в базе данных
        private bool RestoreServiceInDatabase(int serviceId, ServiceModel serviceData)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE services 
                                   SET IsActive = 1,
                                       Description = @Description,
                                       Price = @Price,
                                       Category = @Category
                                   WHERE IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                    cmd.Parameters.AddWithValue("@Description", serviceData.Description);
                    cmd.Parameters.AddWithValue("@Price", serviceData.Price);
                    cmd.Parameters.AddWithValue("@Category", serviceData.Category);

                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        // Создает новую услугу или восстанавливает неактивную
        private bool AddNewService()
        {
            try
            {
                SaveServiceData();
                string serviceName = NameService.Text.Trim();

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Сначала проверяем, нет ли неактивной услуги с таким названием
                    string checkQuery = @"SELECT IDServices FROM services 
                                        WHERE ServiceName = @ServiceName 
                                          AND IsActive = 0";

                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    object inactiveServiceId = checkCmd.ExecuteScalar();

                    if (inactiveServiceId != null && inactiveServiceId != DBNull.Value)
                    {
                        // Нашли неактивную услугу с таким названием - восстанавливаем
                        int serviceId = Convert.ToInt32(inactiveServiceId);

                        string updateQuery = @"UPDATE services 
                                            SET Description = @Description,
                                                Price = @Price,
                                                Category = @Category,
                                                IsActive = 1
                                            WHERE IDServices = @ServiceId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        updateCmd.Parameters.AddWithValue("@Description", NewService.Description);
                        updateCmd.Parameters.AddWithValue("@Price", NewService.Price);
                        updateCmd.Parameters.AddWithValue("@Category", NewService.Category);

                        int updatedRows = updateCmd.ExecuteNonQuery();

                        if (updatedRows > 0)
                        {
                            MessageBox.Show("Услуга успешно восстановлена", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }

                    // Создаем новую услугу
                    string insertQuery = @"INSERT INTO services 
                                        (ServiceName, Description, Price, Category, IsActive) 
                                        VALUES (@ServiceName, @Description, @Price, @Category, 1)";

                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@ServiceName", NewService.ServiceName);
                    insertCmd.Parameters.AddWithValue("@Description", NewService.Description);
                    insertCmd.Parameters.AddWithValue("@Price", NewService.Price);
                    insertCmd.Parameters.AddWithValue("@Category", NewService.Category);

                    int result = insertCmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно добавлена", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Ошибка дублирования уникального ключа
                {
                    // Дополнительная проверка для дублирования
                    string errorMessage = "Услуга с таким названием уже существует";

                    // Проверяем статус существующей услуги
                    try
                    {
                        using (var connection = new MySqlConnection(_connection))
                        {
                            connection.Open();
                            string checkQuery = @"SELECT IsActive FROM services 
                                               WHERE ServiceName = @ServiceName";
                            MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                            checkCmd.Parameters.AddWithValue("@ServiceName", NameService.Text.Trim());

                            object result = checkCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                bool isActive = Convert.ToBoolean(result);
                                if (!isActive)
                                {
                                    errorMessage += " (но неактивна). Попробуйте снова.";
                                }
                            }
                        }
                    }
                    catch { }

                    MessageBox.Show(errorMessage, "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveServiceData()
        {
            NewService.ServiceName = NameService.Text.Trim();
            NewService.Description = Description.Text.Trim();

            // Парсим цену как decimal для поддержки копеек
            if (decimal.TryParse(Price.Text.Trim(), out decimal priceValue))
            {
                NewService.Price = Convert.ToInt32(priceValue);
            }
            else
            {
                NewService.Price = 0;
            }

            NewService.Category = (int)Category.SelectedValue;
        }

        // Обработчики фильтрации ввода
        private void NameService_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameService.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(NameService.Text, true);

            if (filteredText != NameService.Text)
            {
                NameService.Text = filteredText;
                NameService.SelectionStart = Math.Min(selectionStart, NameService.Text.Length);
            }

            // Проверяем подсказку о неактивной услуге
            if (!string.IsNullOrWhiteSpace(NameService.Text))
            {
                CheckForInactiveServiceHint();
            }
        }

        private void Price_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Price.SelectionStart;
            // Разрешаем десятичную точку для цены
            bool allowDecimal = true;
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

            // Счетчик символов
            int charCount = Description.Text.Length;
            int maxChars = 500; // Максимальное количество символов
            lblCharCount.Text = $"{charCount}/{maxChars}";

            if (charCount > maxChars * 0.9) // 90% от лимита
            {
                lblCharCount.ForeColor = Color.Orange;
            }
            else if (charCount > maxChars)
            {
                lblCharCount.ForeColor = Color.Red;
            }
            else
            {
                lblCharCount.ForeColor = Color.Green;
            }
        }

        // Проверка подсказки о неактивной услуге
        private void CheckForInactiveServiceHint()
        {
            try
            {
                string serviceName = NameService.Text.Trim();

                if (string.IsNullOrWhiteSpace(serviceName))
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT IDServices, Price, Description, Category
                                    FROM services 
                                    WHERE ServiceName = @ServiceName AND IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", serviceName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal price = reader.GetDecimal("Price");
                            string description = reader["Description"]?.ToString() ?? "";
                            int categoryId = reader.GetInt32("Category");

                            // Получаем название категории
                            string categoryName = GetCategoryName(categoryId);

                            

                            // Можно предзаполнить поля
                            Price.Text = price.ToString();
                            Description.Text = description;

                            // Устанавливаем категорию если она есть в списке
                            SetCategory(categoryId);
                        }
                        
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        // Получает название категории по ID
        private string GetCategoryName(int categoryId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT CategoryName FROM Category WHERE IDCategory = @CategoryId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Неизвестно";
                }
            }
            catch
            {
                return "Неизвестно";
            }
        }

        // Устанавливает категорию в ComboBox
        private void SetCategory(int categoryId)
        {
            for (int i = 0; i < Category.Items.Count; i++)
            {
                DataRowView row = (DataRowView)Category.Items[i];
                if (Convert.ToInt32(row["IDCategory"]) == categoryId)
                {
                    Category.SelectedIndex = i;
                    break;
                }
            }
        }

        // При уходе с поля названия услуги
        private void NameService_Leave(object sender, EventArgs e)
        {
            CheckForInactiveServiceHint();
        }

        // При нажатии на кнопку "Очистить"
        private void ClearButton_Click(object sender, EventArgs e)
        {
            NameService.Text = "";
            Price.Text = "";
            Description.Text = "";
            
            NameService.Focus();
        }

        // При нажатии на кнопку "Обновить категории"
        private void RefreshCategoriesButton_Click(object sender, EventArgs e)
        {
            LoadCategory();
        }
    }
}