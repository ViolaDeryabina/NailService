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
        private Show _showForm; // Ссылка на главную форму

        public AddClientForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm; // Сохраняем ссылку на главную форму
            NewClient = new ClientModel();
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                // Проверяем, есть ли неактивный клиент для восстановления
                if (_showForm != null && CheckAndRestoreInactiveClient())
                {
                    return; // Клиент восстановлен, форма закрывается
                }

                // Иначе создаем нового клиента
                if (AddNewClient())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            // Проверка телефона
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон клиента", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            // Проверка формата телефона
            string phoneDigits = GetPhoneDigits(Phone.Text);
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            // Проверка, что телефон не занят активным клиентом
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

        // Проверяет, есть ли активный клиент с таким телефоном
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
                return true; // В случае ошибки считаем, что клиент существует
            }
        }

        // Проверяет и восстанавливает неактивного клиента
        private bool CheckAndRestoreInactiveClient()
        {
            try
            {
                string lastName = LastName.Text.Trim();
                string firstName = FirstName.Text.Trim();
                string phoneDigits = GetPhoneDigits(Phone.Text);

                // Проверяем через базу данных
                var (exists, isActive, clientId) = CheckClientExists(lastName, firstName, phoneDigits);

                if (exists && !isActive)
                {
                    // Нашли неактивного клиента - предлагаем восстановить
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
                        // Сохраняем данные из формы
                        SaveClientData();

                        // Восстанавливаем клиента
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
                    else
                    {
                        // Пользователь отказался от восстановления
                        return false;
                    }
                }
                else if (exists && isActive)
                {
                    // Активный клиент уже существует - проверка уже была в ValidateData
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке клиента: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Проверяет существование клиента в базе
        private (bool exists, bool isActive, int clientId) CheckClientExists(string lastName, string firstName, string phone)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Ищем по телефону и ФИО
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

        // Восстанавливает клиента в базе данных
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

        // Создает нового клиента или восстанавливает неактивного
        private bool AddNewClient()
        {
            try
            {
                SaveClientData();
                string phoneDigits = GetPhoneDigits(Phone.Text);

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Сначала проверяем, нет ли неактивного клиента с таким телефоном
                    string checkQuery = "SELECT IDClient FROM Client WHERE Phone = @Phone AND IsActive = 0";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Phone", phoneDigits);

                    object inactiveClientId = checkCmd.ExecuteScalar();

                    if (inactiveClientId != null && inactiveClientId != DBNull.Value)
                    {
                        // Нашли неактивного клиента с таким телефоном - восстанавливаем
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
                            MessageBox.Show("Клиент успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                    else
                    {
                        // Проверяем по ФИО (может быть другой телефон, но тот же человек)
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
                            // Предлагаем восстановить по ФИО
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
                                    MessageBox.Show("Клиент успешно восстановлен", "Успех",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return true;
                                }
                            }
                        }

                        // Создаем нового клиента
                        string insertQuery = @"INSERT INTO Client 
                                            (LastName, FirstName, MiddleName, Phone, IsActive) 
                                            VALUES (@LastName, @FirstName, @MiddleName, @Phone, 1)";

                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@LastName", NewClient.LastName);
                        insertCmd.Parameters.AddWithValue("@FirstName", NewClient.FirstName);
                        insertCmd.Parameters.AddWithValue("@MiddleName", NewClient.MiddleName ?? "");
                        insertCmd.Parameters.AddWithValue("@Phone", phoneDigits);

                        int result = insertCmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Клиент успешно добавлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

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

        private void SaveClientData()
        {
            NewClient.LastName = LastName.Text.Trim();
            NewClient.FirstName = FirstName.Text.Trim();
            NewClient.MiddleName = MiddleName.Text.Trim();
            NewClient.Phone = GetPhoneDigits(Phone.Text);
            // NewClient.Email = EmailTextBox.Text.Trim(); // если есть поле email
        }

        // Обработчики фильтрации ввода (остаются без изменений)
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

        private void Phone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = Phone.SelectionStart;
            string originalText = Phone.Text;

            // 1. Фильтруем текст
            string filteredText = InputValidator.FilterToPhone(originalText);

            // 2. Форматируем номер
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            // Если текст изменился
            if (formattedText != originalText)
            {
                // Сохраняем текст
                Phone.Text = formattedText;

                // Корректируем позицию курсора с учетом добавленных символов форматирования
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                Phone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
            }

            // Проверяем подсказку о неактивном клиенте
            if (!string.IsNullOrWhiteSpace(Phone.Text))
            {
                CheckForInactiveClientHint();
            }
        }

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

        // Проверка подсказки о неактивном клиенте
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
                            string lastName = reader["LastName"]?.ToString() ?? "";
                            string firstName = reader["FirstName"]?.ToString() ?? "";
                            string middleName = reader["MiddleName"]?.ToString() ?? "";

                            
                        }
                        
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        // Также проверяем при уходе с поля телефона
        private void Phone_Leave(object sender, EventArgs e)
        {
            CheckForInactiveClientHint();
        }
    }
}