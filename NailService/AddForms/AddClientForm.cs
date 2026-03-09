using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class AddClientForm : Form
    {
        private string _connection;
        public ClientModel NewClient { get; private set; }
        public int AddedClientId { get; private set; }
        private Show _showForm; // Ссылка на форму Show для обновления списка после добавления

        /// <summary>
        /// Конструктор формы добавления клиента
        /// </summary>
        /// <param name="showForm">Ссылка на главную форму для обновления данных</param>
        public AddClientForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewClient = new ClientModel();
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - валидация и сохранение клиента
        /// </summary>
        private void AddClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                // Проверяем наличие неактивного клиента для восстановления
                if (_showForm != null && CheckAndRestoreInactiveClient())
                {
                    return; // Клиент восстановлен, форма закрывается
                }

                // Создание нового клиента
                if (AddNewClient())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы без сохранения
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        /// <returns>true если данные корректны</returns>
        private bool ValidateData()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            // Проверка корректности номера телефона (минимум 10 цифр)
            string phoneDigits = GetPhoneDigits(Phone.Text);
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            // Проверка уникальности телефона среди активных клиентов
            if (IsActiveClientExists(phoneDigits))
            {
                MessageBox.Show("Клиент с таким номером телефона уже существует и активен",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                Phone.SelectAll();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка существования активного клиента с указанным телефоном
        /// </summary>
        /// <param name="phoneDigits">Очищенный номер телефона (только цифры)</param>
        /// <returns>true если активный клиент существует</returns>
        private bool IsActiveClientExists(string phoneDigits)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Client WHERE Phone = @Phone AND IsActive = 1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", phoneDigits);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки телефона: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // При ошибке блокируем добавление для безопасности
            }
        }

        /// <summary>
        /// Поиск и восстановление неактивного клиента с такими же данными
        /// </summary>
        /// <returns>true если клиент восстановлен и форма закрыта</returns>
        private bool CheckAndRestoreInactiveClient()
        {
            try
            {
                string lastName = LastName.Text.Trim();
                string firstName = FirstName.Text.Trim();
                string phoneDigits = GetPhoneDigits(Phone.Text);

                var (exists, isActive, clientId) = CheckClientExists(lastName, firstName, phoneDigits);

                if (exists && !isActive)
                {
                    // Найден неактивный клиент - предлагаем восстановить
                    var result = MessageBox.Show(
                        $"Найден неактивный клиент с такими данными:\n" +
                        $"ФИО: {lastName} {firstName}\n" +
                        $"Телефон: {FormatPhoneForDisplay(phoneDigits)}\n\n" +
                        "Восстановить этого клиента с новыми данными?",
                        "Восстановление клиента",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveClientData();
                        bool restored = RestoreClientInDatabase(clientId, NewClient);

                        if (restored)
                        {
                            MessageBox.Show("Клиент успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось восстановить клиента", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке клиента: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        /// <summary>
        /// Проверка существования клиента в базе по телефону или ФИО
        /// </summary>
        /// <returns>Кортеж (существует, активен, ID клиента)</returns>
        private (bool exists, bool isActive, int clientId) CheckClientExists(string lastName, string firstName, string phone)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"SELECT IDClient, IsActive 
                                   FROM Client 
                                   WHERE Phone = @Phone 
                                      OR (LastName = @LastName AND FirstName = @FirstName)
                                   LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int clientId = reader.GetInt32("IDClient");
                            bool isActive = reader.GetBoolean("IsActive");
                            return (true, isActive, clientId);
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

        /// <summary>
        /// Восстановление неактивного клиента (активация и обновление данных)
        /// </summary>
        /// <returns>true если восстановление успешно</returns>
        private bool RestoreClientInDatabase(int clientId, ClientModel clientData)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE Client 
                                   SET IsActive = 1,
                                       LastName = @LastName,
                                       FirstName = @FirstName,
                                       MiddleName = @MiddleName,
                                       Phone = @Phone
                                   WHERE IDClient = @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);
                    cmd.Parameters.AddWithValue("@LastName", clientData.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", clientData.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", clientData.MiddleName ?? "");
                    cmd.Parameters.AddWithValue("@Phone", clientData.Phone);

                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Добавление нового клиента в базу данных
        /// </summary>
        /// <returns>true если добавление успешно</returns>
        private bool AddNewClient()
        {
            try
            {
                SaveClientData();
                string phoneDigits = GetPhoneDigits(Phone.Text);

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Поиск неактивного клиента по телефону
                    string checkQuery = "SELECT IDClient FROM Client WHERE Phone = @Phone AND IsActive = 0";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Phone", phoneDigits);

                    object inactiveClientId = checkCmd.ExecuteScalar();

                    if (inactiveClientId != null && inactiveClientId != DBNull.Value)
                    {
                        // Восстановление неактивного клиента по телефону
                        int clientId = Convert.ToInt32(inactiveClientId);

                        string updateQuery = @"UPDATE Client 
                                    SET LastName = @LastName,
                                        FirstName = @FirstName,
                                        MiddleName = @MiddleName,
                                        IsActive = 1
                                    WHERE IDClient = @ClientId";

                        MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                        updateCmd.Parameters.AddWithValue("@ClientId", clientId);
                        updateCmd.Parameters.AddWithValue("@LastName", NewClient.LastName);
                        updateCmd.Parameters.AddWithValue("@FirstName", NewClient.FirstName);
                        updateCmd.Parameters.AddWithValue("@MiddleName", NewClient.MiddleName ?? "");

                        int updatedRows = updateCmd.ExecuteNonQuery();

                        if (updatedRows > 0)
                        {
                            AddedClientId = clientId;
                            MessageBox.Show("Клиент успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                    else
                    {
                        // Поиск неактивного клиента по ФИО
                        string checkByNameQuery = @"SELECT IDClient FROM Client 
                                          WHERE LastName = @LastName 
                                            AND FirstName = @FirstName 
                                            AND IsActive = 0";
                        MySqlCommand checkByNameCmd = new MySqlCommand(checkByNameQuery, connection);
                        checkByNameCmd.Parameters.AddWithValue("@LastName", NewClient.LastName);
                        checkByNameCmd.Parameters.AddWithValue("@FirstName", NewClient.FirstName);

                        object inactiveByNameClientId = checkByNameCmd.ExecuteScalar();

                        if (inactiveByNameClientId != null && inactiveByNameClientId != DBNull.Value)
                        {
                            // Предложение восстановить клиента с другим телефоном
                            int clientId = Convert.ToInt32(inactiveByNameClientId);

                            var results = MessageBox.Show(
                                "Найден неактивный клиент с таким ФИО, но другим телефоном.\n" +
                                "Восстановить клиента и обновить телефон?",
                                "Восстановление клиента",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (results == DialogResult.Yes)
                            {
                                string updateQuery = @"UPDATE Client 
                                            SET Phone = @Phone,
                                                MiddleName = @MiddleName,
                                                IsActive = 1
                                            WHERE IDClient = @ClientId";

                                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                                updateCmd.Parameters.AddWithValue("@ClientId", clientId);
                                updateCmd.Parameters.AddWithValue("@Phone", phoneDigits);
                                updateCmd.Parameters.AddWithValue("@MiddleName", NewClient.MiddleName ?? "");

                                int updatedRows = updateCmd.ExecuteNonQuery();

                                if (updatedRows > 0)
                                {
                                    AddedClientId = clientId;
                                    MessageBox.Show("Клиент успешно восстановлен", "Успех",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return true;
                                }
                            }
                        }

                        // Создание нового клиента
                        string insertQuery = @"INSERT INTO Client 
                                    (LastName, FirstName, MiddleName, Phone, IsActive) 
                                    VALUES (@LastName, @FirstName, @MiddleName, @Phone, 1);
                                    SELECT LAST_INSERT_ID();";

                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@LastName", NewClient.LastName);
                        insertCmd.Parameters.AddWithValue("@FirstName", NewClient.FirstName);
                        insertCmd.Parameters.AddWithValue("@MiddleName", NewClient.MiddleName ?? "");
                        insertCmd.Parameters.AddWithValue("@Phone", phoneDigits);

                        object result = insertCmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            AddedClientId = Convert.ToInt32(result);
                            return true;
                        }
                    }

                    MessageBox.Show("Не удалось добавить клиента", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Ошибка дублирования уникального ключа
                {
                    MessageBox.Show("Клиент с таким номером телефона уже существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Извлечение только цифр из строки телефона
        /// </summary>
        /// <returns>Строка, содержащая только цифры</returns>
        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Форматирование номера телефона для отображения
        /// </summary>
        /// <returns>Отформатированный номер в формате +7 (XXX) XXX-XX-XX</returns>
        private string FormatPhoneForDisplay(string phoneDigits)
        {
            if (phoneDigits.Length == 11 && (phoneDigits.StartsWith("7") || phoneDigits.StartsWith("8")))
            {
                return $"+7 ({phoneDigits.Substring(1, 3)}) {phoneDigits.Substring(4, 3)}-{phoneDigits.Substring(7, 2)}-{phoneDigits.Substring(9, 2)}";
            }
            else if (phoneDigits.Length == 10)
            {
                return $"+7 ({phoneDigits.Substring(0, 3)}) {phoneDigits.Substring(3, 3)}-{phoneDigits.Substring(6, 2)}-{phoneDigits.Substring(8, 2)}";
            }

            return phoneDigits;
        }

        /// <summary>
        /// Сохранение данных из формы в объект NewClient
        /// </summary>
        private void SaveClientData()
        {
            NewClient.LastName = LastName.Text.Trim();
            NewClient.FirstName = FirstName.Text.Trim();
            NewClient.MiddleName = MiddleName.Text.Trim();
            NewClient.Phone = GetPhoneDigits(Phone.Text);
        }

        /// <summary>
        /// Фильтрация ввода в поле фамилии (только русские буквы)
        /// </summary>
        private void LastName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = LastName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(LastName.Text);

            if (filteredText != LastName.Text)
            {
                LastName.Text = filteredText;
                LastName.SelectionStart = Math.Min(selectionStart, LastName.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле имени (только русские буквы)
        /// </summary>
        private void FirstName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = FirstName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(FirstName.Text);

            if (filteredText != FirstName.Text)
            {
                FirstName.Text = filteredText;
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле отчества (только русские буквы)
        /// </summary>
        private void MiddleName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = MiddleName.SelectionStart;
            string filteredText = InputValidator.FilterToRussianLetters(MiddleName.Text);

            if (filteredText != MiddleName.Text)
            {
                MiddleName.Text = filteredText;
                MiddleName.SelectionStart = Math.Min(selectionStart, MiddleName.Text.Length);
            }
        }

        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
        /// </summary>
        private void Phone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = Phone.SelectionStart;
            string originalText = Phone.Text;

            string filteredText = InputValidator.FilterToPhone(originalText);
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                Phone.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                Phone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }

            // Проверка наличия неактивного клиента с таким телефоном
            if (!string.IsNullOrWhiteSpace(Phone.Text))
            {
                CheckForInactiveClientHint();
            }
        }

        /// <summary>
        /// Корректировка позиции курсора после форматирования телефона
        /// </summary>
        private int GetAdjustedCursorPosition(int originalPosition, string oldText, string newText)
        {
            if (originalPosition >= oldText.Length)
                return newText.Length;

            int formatCharsBeforeCursor = 0;
            char[] formatChars = { '(', ')', ' ', '-', '+' };

            for (int i = 0; i < originalPosition && i < newText.Length; i++)
            {
                if (formatChars.Contains(newText[i]))
                {
                    formatCharsBeforeCursor++;
                }
            }

            return originalPosition + formatCharsBeforeCursor;
        }

        /// <summary>
        /// Проверка наличия неактивного клиента с введенным номером телефона
        /// </summary>
        private void CheckForInactiveClientHint()
        {
            try
            {
                string phoneDigits = GetPhoneDigits(Phone.Text);

                if (string.IsNullOrWhiteSpace(phoneDigits) || phoneDigits.Length < 10)
                    return;

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT IDClient, LastName, FirstName, MiddleName, IsActive
                                    FROM Client 
                                    WHERE Phone = @Phone AND IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", phoneDigits);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Здесь можно добавить визуальную подсказку о найденном неактивном клиенте
                            // Например, изменить цвет фона или показать иконку
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }


        private void LastName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                string name = LastName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    LastName.Text = name;
                }
            }
        }

        private void FirstName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FirstName.Text))
            {
                string name = FirstName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    FirstName.Text = name;
                }
            }
        }

        private void MiddleName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MiddleName.Text))
            {
                string name = MiddleName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    MiddleName.Text = name;
                }
            }
        }
    }
}