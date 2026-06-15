using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Форма для редактирования данных существующего мастера
    /// Позволяет изменять ФИО, описание и номер телефона
    /// </summary>
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

        /// <summary>
        /// Загрузка данных мастера в поля формы
        /// </summary>
        private void LoadMasterData()
        {
            LastName.Text = Master.LastName;
            FirstName.Text = Master.FirstName;
            MiddleName.Text = Master.MiddleName;
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
                UpdateUserData();
                UpdateMasterInDatabase();
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

        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон мастера", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            string phoneDigits = GetPhoneDigits(Phone.Text);
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            if (!IsPhoneUnique(phoneDigits))
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
            catch
            {
                return false;
            }
        }

        #endregion

        #region Работа с телефоном

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        private void Phone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = Phone.SelectionStart;
            string originalText = Phone.Text;

            string filteredText = new string(originalText.Where(c => char.IsDigit(c)).ToArray());
            string formattedText = FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                Phone.Text = formattedText;
                Phone.SelectionStart = Math.Min(originalSelectionStart, formattedText.Length);
            }
        }

        private string FormatPhoneNumber(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                return string.Empty;

            if (digits.Length >= 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                string number = digits.Length > 11 ? digits.Substring(0, 11) : digits;
                if (number.Length == 11)
                {
                    return $"+7 ({number.Substring(1, 3)}) {number.Substring(4, 3)}-{number.Substring(7, 2)}-{number.Substring(9, 2)}";
                }
                else if (number.Length > 1)
                {
                    return $"+7 ({number.Substring(1, Math.Min(3, number.Length - 1))})";
                }
            }

            return digits;
        }

        #endregion

        #region Сохранение данных

        private void SaveMasterData()
        {
            Master.LastName = LastName.Text.Trim();
            Master.FirstName = FirstName.Text.Trim();
            Master.MiddleName = MiddleName.Text.Trim();
            Master.Description = Description.Text.Trim();
            Master.Phone = GetPhoneDigits(Phone.Text);
        }

        /// <summary>
        /// Обновление данных пользователя (ФИО) в таблице Users
        /// </summary>
        private void UpdateUserData()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE Users 
                                     SET LastName = @LastName,
                                         FirstName = @FirstName,
                                         MiddleName = @MiddleName
                                     WHERE IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", Master.UserId);
                    cmd.Parameters.AddWithValue("@LastName", Master.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", Master.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", string.IsNullOrWhiteSpace(Master.MiddleName) ? (object)DBNull.Value : Master.MiddleName);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления ФИО: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обновление данных мастера в таблице Masters
        /// </summary>
        private void UpdateMasterInDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE Masters 
                                     SET Description = @Description,
                                         Phone = @Phone
                                     WHERE IDMasters = @MasterId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MasterId", Master.IDMasters);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(Master.Description) ? (object)DBNull.Value : Master.Description);
                    cmd.Parameters.AddWithValue("@Phone", Master.Phone);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении мастера: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Фильтрация ввода (только русские буквы для ФИО)

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

        #endregion

        #region Автоматическое преобразование первой буквы в заглавную

        private void LastName_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LastName.Text))
            {
                string name = LastName.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
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
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
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
                    name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                    MiddleName.Text = name;
                }
            }
        }

        #endregion
    }
}