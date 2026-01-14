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
    public partial class AddMasterForm : Form
    {
        private string _connection;
        public MasterModel NewMaster { get; private set; }

        public AddMasterForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewMaster = new MasterModel();
            LoadAvailableMasters();
        }

        private void LoadAvailableMasters()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Запрос для получения пользователей с ролью "Мастер" (IDRole = 3),
                    // которых еще нет в таблице Masters
                    string query = @"
                        SELECT 
                            u.IDUser,
                            u.LastName,
                            u.FirstName,
                            u.MiddleName
                        FROM Users u
                        INNER JOIN Role r ON u.Role = r.IDRole
                        WHERE r.IDRole = 3 
                          AND u.IDUser NOT IN (SELECT User FROM Masters)
                        ORDER BY u.LastName, u.FirstName, u.MiddleName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с объединенным столбцом ФИО
                    DataTable fioDt = new DataTable();
                    fioDt.Columns.Add("ID", typeof(int));
                    fioDt.Columns.Add("ФИО", typeof(string));

                    // Объединение ФИО в один столбец
                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = FormatToShortName(
                            row["LastName"]?.ToString(),
                            row["FirstName"]?.ToString(),
                            row["MiddleName"]?.ToString()
                        );

                        fioDt.Rows.Add(
                            Convert.ToInt32(row["IDUser"]),
                            fullName
                        );
                    }

                    // Привязываем данные к ComboBox
                    FIO.DataSource = fioDt;
                    FIO.DisplayMember = "ФИО";
                    FIO.ValueMember = "ID";

                    // Проверяем, есть ли доступные мастера
                    if (FIO.Items.Count == 0)
                    {
                        MessageBox.Show("Нет доступных пользователей с ролью 'Мастер' для добавления.",
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AddMaster.Enabled = false;

                    }
                    else
                    {
                        FIO.SelectedIndex = 0;
                        AddMaster.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка мастеров: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatToShortName(string lastName, string firstName, string middleName)
        {
            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                return "";

            string result = $"{lastName} {firstName[0]}.";

            if (!string.IsNullOrEmpty(middleName))
            {
                result += $"{middleName[0]}.";
            }

            return result;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddMaster_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveMasterData();
                if (AddMasterToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private bool ValidateData()
        {
            // Проверка выбора мастера
            if (FIO.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите мастера из списка", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FIO.Focus();
                return false;
            }

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

            return true;
        }

        private void SaveMasterData()
        {
            NewMaster.UserId = Convert.ToInt32(FIO.SelectedValue);
            NewMaster.Description = Description.Text.Trim();
            NewMaster.Phone = GetPhoneDigits(Phone.Text); // Сохраняем только цифры
        }

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        private bool AddMasterToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"INSERT INTO Masters 
                                    (User, Description, Phone) 
                                    VALUES (@UserId, @Description, @Phone)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", NewMaster.UserId);
                    cmd.Parameters.AddWithValue("@Description", NewMaster.Description);
                    cmd.Parameters.AddWithValue("@Phone", NewMaster.Phone);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить мастера", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
