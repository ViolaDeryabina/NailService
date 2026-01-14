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
    public partial class EditClientForm : Form
    {
        private string _connection;
        public ClientModel Client { get; private set; }

        public EditClientForm(ClientModel client)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Client = client;
            LoadClientData();
        }
        private void LoadClientData()
        {
            LastName.Text = Client.LastName;
            FirstName.Text = Client.FirstName;
            MiddleName.Text = Client.MiddleName;
            Phone.Text = Client.Phone;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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

        private void EditClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateData()
        {
            // Проверка фамилии
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LastName.Focus();
                return false;
            }

            // Проверка имени
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                FirstName.Focus();
                return false;
            }

            // Проверка телефона
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            // Проверка формата телефона (базовая проверка)
            string phoneDigits = new string(Phone.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            // Проверка на уникальность телефона
            if (!IsPhoneUnique())
            {
                MessageBox.Show("Клиент с таким номером телефона уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            return true;
        }

        private bool IsPhoneUnique()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM Client 
                                   WHERE Phone = @Phone AND IDClient != @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", Phone.Text.Trim());
                    cmd.Parameters.AddWithValue("@ClientId", Client.IDClient);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки телефона: {ex.Message}");
                    return false;
                }
            }
        }

        private void SaveClientData()
        {
            Client.LastName = LastName.Text.Trim();
            Client.FirstName = FirstName.Text.Trim();
            Client.MiddleName = MiddleName.Text.Trim();
            Client.Phone = Phone.Text.Trim();
        }
    }
}
