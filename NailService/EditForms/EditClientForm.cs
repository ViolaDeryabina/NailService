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
    /// <summary>
    /// Форма для редактирования данных существующего клиента
    /// Позволяет изменять ФИО и номер телефона с валидацией уникальности
    /// </summary>
    public partial class EditClientForm : Form
    {
        private string _connection;
        public ClientModel Client { get; private set; }

        /// <summary>
        /// Конструктор формы редактирования клиента
        /// </summary>
        /// <param name="client">Объект клиента с текущими данными для редактирования</param>
        public EditClientForm(ClientModel client)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Client = client;
            LoadClientData();
        }

        /// <summary>
        /// Загрузка данных клиента в поля формы
        /// </summary>
        private void LoadClientData()
        {
            LastName.Text = Client.LastName;
            FirstName.Text = Client.FirstName;
            MiddleName.Text = Client.MiddleName;
            Phone.Text = Client.Phone;
        }

        /// <summary>
        /// Отмена редактирования и закрытие формы
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Сохранение изменений и закрытие формы
        /// </summary>
        private void EditClient_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveClientData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        #region Валидация данных

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        /// <returns>true если данные корректны</returns>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон клиента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            string phoneDigits = new string(Phone.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            if (!IsPhoneUnique())
            {
                MessageBox.Show("Клиент с таким номером телефона уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка уникальности номера телефона (исключая текущего клиента)
        /// </summary>
        /// <returns>true если номер уникален</returns>
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

        #endregion

        #region Фильтрация и форматирование ввода

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
                FirstName.Text = filteredText;
                FirstName.SelectionStart = Math.Min(selectionStart, FirstName.Text.Length);
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

        #endregion

        /// <summary>
        /// Сохранение данных из формы в объект Client
        /// </summary>
        private void SaveClientData()
        {
            Client.LastName = LastName.Text.Trim();
            Client.FirstName = FirstName.Text.Trim();
            Client.MiddleName = MiddleName.Text.Trim();
            Client.Phone = Phone.Text.Trim();
        }
    }
}