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
    public partial class EditMasterForm : Form
    {
        private string _connection;
        public MasterModel Master { get; private set; }

        public EditMasterForm(MasterModel master)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Master = master;
            LoadMasterData();
        }

        private void LoadMasterData()
        {
            // Заполняем поля данными мастера
            FIO.Text = Master.FullName; // Поле только для чтения
            FIO.Enabled = false; // ФИО нельзя редактировать
            Description.Text = Master.Description;
            Phone.Text = Master.Phone;
        }

        private void EditMaster_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveMasterData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ValidateData()
        {
            // Проверка телефона
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон мастера", "Внимание",
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

            // Проверка уникальности телефона (если изменился)
            string currentPhoneDigits = GetPhoneDigits(Phone.Text);
            if (currentPhoneDigits != Master.Phone && !IsPhoneUnique(currentPhoneDigits))
            {
                MessageBox.Show("Мастер с таким номером телефона уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Phone.Focus();
                Phone.SelectAll();
                return false;
            }

            return true;
        }

        private bool IsPhoneUnique(string phone)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Masters WHERE Phone = @Phone AND IDMasters != @MasterId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@MasterId", Master.IDMasters);

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

        private void SaveMasterData()
        {
            Master.Description = Description.Text.Trim();
            Master.Phone = GetPhoneDigits(Phone.Text);
        }

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
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