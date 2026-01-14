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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NailService
{
    public partial class AddClientForm : Form
    {
        private string _connection;
        public ClientModel NewClient { get; private set; }

        public AddClientForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewClient = new ClientModel();
        }

        private void AddClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                if (AddClientToDatabase())
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
            string phoneDigits = new string(Phone.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            // Проверка уникальности телефона
            if (!IsPhoneUnique())
            {
                MessageBox.Show("Клиент с таким номером телефона уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                Phone.SelectAll();
                return false;
            }

            return true;
        }

        private bool IsPhoneUnique()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Client WHERE Phone = @Phone";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", GetPhoneDigits(Phone.Text));

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки телефона: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        private void SaveClientData()
        {
            NewClient.LastName = LastName.Text.Trim();
            NewClient.FirstName = FirstName.Text.Trim();
            NewClient.MiddleName = MiddleName.Text.Trim();
            NewClient.Phone = GetPhoneDigits(Phone.Text); // Сохраняем только цифры
        }

        private bool AddClientToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"INSERT INTO Client 
                                    (LastName, FirstName, MiddleName, Phone) 
                                    VALUES (@LastName, @FirstName, @MiddleName, @Phone)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", NewClient.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", NewClient.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", NewClient.MiddleName);
                    cmd.Parameters.AddWithValue("@Phone", NewClient.Phone);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить клиента", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

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
        }

        private int GetAdjustedCursorPosition(int originalPosition, string oldText, string newText)
        {
            if (originalPosition >= oldText.Length)
                return newText.Length;

            // Считаем, сколько форматирующих символов было добавлено ДО позиции курсора
            int formatCharsBeforeCursor = 0;

            // Форматирующие символы в телефонном номере
            char[] formatChars = { '(', ')', ' ', '-', '+' };

            for (int i = 0; i < originalPosition && i < newText.Length; i++)
            {
                if (formatChars.Contains(newText[i]))
                {
                    formatCharsBeforeCursor++;
                }
            }

            // Корректируем позицию с учетом форматирующих символов
            return originalPosition + formatCharsBeforeCursor;
        }
    }
}