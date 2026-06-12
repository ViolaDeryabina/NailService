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

                if (UpdateMasterInDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
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
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон мастера", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Phone.Focus();
                return false;
            }

            string phoneDigits = GetPhoneDigits(Phone.Text);
            if (phoneDigits.Length < 10)
            {
                MessageBox.Show("Номер телефона должен содержать не менее 10 цифр",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки телефона: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        /// <summary>
        /// Сохранение данных из формы в объект Master
        /// </summary>
        private void SaveMasterData()
        {
            Master.Description = Description.Text.Trim();
            Master.Phone = GetPhoneDigits(Phone.Text);
        }

        /// <summary>
        /// Обновление данных мастера в базе данных
        /// </summary>
        private bool UpdateMasterInDatabase()
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

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Данные мастера успешно обновлены", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные мастера", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении мастера: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}