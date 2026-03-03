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
    /// <summary>
    /// Форма для редактирования данных существующего мастера
    /// Позволяет изменять описание и номер телефона (ФИО только для просмотра)
    /// </summary>
    public partial class EditMasterForm : Form
    {
        private string _connection;
        public MasterModel Master { get; private set; }

        /// <summary>
        /// Конструктор формы редактирования мастера
        /// </summary>
        /// <param name="master">Объект мастера с текущими данными для редактирования</param>
        public EditMasterForm(MasterModel master)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Master = master;
            LoadMasterData();
        }

        /// <summary>
        /// Загрузка данных мастера в поля формы
        /// </summary>
        private void LoadMasterData()
        {
            FIO.Text = Master.FullName;
            FIO.Enabled = false; // ФИО нельзя редактировать (привязано к пользователю)
            Description.Text = Master.Description;
            Phone.Text = Master.Phone;
        }

        /// <summary>
        /// Сохранение изменений и закрытие формы
        /// </summary>
        private void EditMaster_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveMasterData();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Отмена редактирования и закрытие формы
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #region Валидация данных

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        /// <returns>true если данные корректны</returns>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон мастера", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            string phoneDigits = new string(Phone.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

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

        /// <summary>
        /// Проверка уникальности номера телефона (исключая текущего мастера)
        /// </summary>
        /// <param name="phone">Номер телефона для проверки</param>
        /// <returns>true если номер уникален</returns>
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

        #endregion

        #region Работа с телефоном

        /// <summary>
        /// Извлечение только цифр из строки телефона
        /// </summary>
        /// <returns>Строка, содержащая только цифры</returns>
        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
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
        /// Сохранение данных из формы в объект Master
        /// </summary>
        private void SaveMasterData()
        {
            Master.Description = Description.Text.Trim();
            Master.Phone = GetPhoneDigits(Phone.Text);
        }
    }
}