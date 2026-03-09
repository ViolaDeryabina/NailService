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
        private Show _showForm; // Ссылка на форму Show для обновления списка после добавления

        /// <summary>
        /// Конструктор формы добавления мастера
        /// </summary>
        /// <param name="showForm">Ссылка на главную форму для обновления данных</param>
        public AddMasterForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm;
            NewMaster = new MasterModel();
            LoadAvailableMasters();
        }

        /// <summary>
        /// Загрузка доступных пользователей с ролью "Мастер" для добавления или восстановления
        /// </summary>
        private void LoadAvailableMasters()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Запрос для получения пользователей с ролью "Мастер" (Role ID = 3),
                    // которые еще не добавлены в таблицу Masters или являются неактивными мастерами
                    string query = @"
                        SELECT 
                            u.IDUser,
                            u.LastName,
                            u.FirstName,
                            u.MiddleName,
                            m.IsActive as MasterActive
                        FROM Users u
                        INNER JOIN Role r ON u.Role = r.IDRole
                        LEFT JOIN Masters m ON u.IDUser = m.User
                        WHERE r.IDRole = 3 
                          AND u.IsActive = 1
                          AND (m.IDMasters IS NULL OR m.IsActive = 0)
                        ORDER BY 
                            CASE WHEN m.IsActive = 0 THEN 0 ELSE 1 END, -- Сначала неактивные мастера
                            u.LastName, u.FirstName, u.MiddleName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создание таблицы с отформатированными данными для отображения в ComboBox
                    DataTable fioDt = new DataTable();
                    fioDt.Columns.Add("ID", typeof(int));
                    fioDt.Columns.Add("ФИО", typeof(string));
                    fioDt.Columns.Add("Статус", typeof(string));
                    fioDt.Columns.Add("IsInactiveMaster", typeof(bool));

                    // Форматирование ФИО в короткий формат (Фамилия И.О.)
                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = FormatToShortName(
                            row["LastName"]?.ToString(),
                            row["FirstName"]?.ToString(),
                            row["MiddleName"]?.ToString()
                        );

                        bool isInactiveMaster = row["MasterActive"] != DBNull.Value &&
                                                Convert.ToBoolean(row["MasterActive"]) == false;

                        string status = isInactiveMaster ? " (неактивный мастер)" : "";

                        fioDt.Rows.Add(
                            Convert.ToInt32(row["IDUser"]),
                            fullName + status,
                            status,
                            isInactiveMaster
                        );
                    }

                    // Привязка данных к ComboBox
                    FIO.DataSource = fioDt;
                    FIO.DisplayMember = "ФИО";
                    FIO.ValueMember = "ID";

                    // Проверка наличия доступных мастеров
                    if (FIO.Items.Count == 0)
                    {
                        MessageBox.Show("Нет доступных пользователей с ролью 'Мастер' для добавления.",
                            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AddMasterButton.Enabled = false;
                    }
                    else
                    {
                        FIO.SelectedIndex = 0;
                        AddMasterButton.Enabled = true;
                        CheckForInactiveMasterHint();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка мастеров: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Форматирование ФИО в короткий формат (Фамилия И.О.)
        /// </summary>
        /// <returns>ФИО в формате "Фамилия И.О."</returns>
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

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы без сохранения
        /// </summary>
        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - валидация и сохранение мастера
        /// </summary>
        private void AddMasterButton_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                // Проверка возможности восстановления неактивного мастера
                if (CheckAndRestoreInactiveMaster())
                {
                    return; // Мастер восстановлен, форма закрывается
                }

                // Создание нового мастера
                if (AddNewMaster())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        /// <summary>
        /// Валидация введенных данных перед сохранением
        /// </summary>
        /// <returns>true если данные корректны</returns>
        private bool ValidateData()
        {
            // Проверка выбора пользователя
            if (FIO.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите мастера из списка", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FIO.Focus();
                return false;
            }

            // Проверка заполнения телефона
            if (string.IsNullOrWhiteSpace(Phone.Text))
            {
                MessageBox.Show("Введите телефон мастера", "Внимание",
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

            return true;
        }

        /// <summary>
        /// Проверка и восстановление неактивного мастера
        /// </summary>
        /// <returns>true если мастер восстановлен и форма закрыта</returns>
        private bool CheckAndRestoreInactiveMaster()
        {
            try
            {
                DataRowView selectedRow = (DataRowView)FIO.SelectedItem;
                bool isInactiveMaster = Convert.ToBoolean(selectedRow["IsInactiveMaster"]);

                if (isInactiveMaster)
                {
                    int userId = Convert.ToInt32(selectedRow["ID"]);
                    string fullName = selectedRow["ФИО"].ToString();

                    // Предложение восстановить неактивного мастера
                    var result = MessageBox.Show(
                        $"Найден неактивный мастер: {fullName}\n\n" +
                        "Восстановить этого мастера с новыми данными?",
                        "Восстановление мастера",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveMasterData();
                        bool restored = RestoreMasterInDatabase(userId, NewMaster);

                        if (restored)
                        {
                            MessageBox.Show("Мастер успешно восстановлен", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось восстановить мастера", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке мастера: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        /// <summary>
        /// Добавление нового мастера или восстановление неактивного
        /// </summary>
        /// <returns>true если операция успешна</returns>
        private bool AddNewMaster()
        {
            try
            {
                SaveMasterData();
                int userId = Convert.ToInt32(FIO.SelectedValue);

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Поиск неактивной записи мастера для этого пользователя
                    string checkQuery = @"SELECT m.IDMasters, m.IsActive 
                                        FROM Masters m 
                                        INNER JOIN Users u ON m.User = u.IDUser
                                        WHERE u.IDUser = @UserId 
                                          AND u.IsActive = 1
                                          AND m.IsActive = 0";

                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Восстановление неактивного мастера
                            int masterId = reader.GetInt32("IDMasters");
                            reader.Close();

                            string updateQuery = @"UPDATE Masters 
                                                SET Description = @Description,
                                                    Phone = @Phone,
                                                    IsActive = 1
                                                WHERE IDMasters = @MasterId";

                            MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                            updateCmd.Parameters.AddWithValue("@MasterId", masterId);
                            updateCmd.Parameters.AddWithValue("@Description", NewMaster.Description);
                            updateCmd.Parameters.AddWithValue("@Phone", NewMaster.Phone);

                            int updatedRows = updateCmd.ExecuteNonQuery();

                            if (updatedRows > 0)
                            {
                                MessageBox.Show("Мастер успешно восстановлен", "Успех",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return true;
                            }
                        }
                    }

                    // Проверка наличия активного мастера для этого пользователя
                    string checkActiveQuery = @"SELECT COUNT(*) FROM Masters 
                                              WHERE User = @UserId AND IsActive = 1";
                    MySqlCommand checkActiveCmd = new MySqlCommand(checkActiveQuery, connection);
                    checkActiveCmd.Parameters.AddWithValue("@UserId", userId);

                    int activeCount = Convert.ToInt32(checkActiveCmd.ExecuteScalar());

                    if (activeCount > 0)
                    {
                        MessageBox.Show("Этот пользователь уже является активным мастером", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Создание нового мастера
                    string insertQuery = @"INSERT INTO Masters 
                                        (User, Description, Phone, IsActive) 
                                        VALUES (@UserId, @Description, @Phone, 1)";

                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@Description", NewMaster.Description);
                    insertCmd.Parameters.AddWithValue("@Phone", NewMaster.Phone);

                    int result = insertCmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Мастер успешно добавлен", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // Ошибка дублирования уникального ключа
                {
                    MessageBox.Show("Этот пользователь уже является мастером", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Восстановление неактивного мастера в базе данных
        /// </summary>
        /// <returns>true если восстановление успешно</returns>
        private bool RestoreMasterInDatabase(int userId, MasterModel masterData)
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"UPDATE Masters 
                                   SET Description = @Description,
                                       Phone = @Phone,
                                       IsActive = 1
                                   WHERE User = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Description", masterData.Description);
                    cmd.Parameters.AddWithValue("@Phone", masterData.Phone);

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
        /// Сохранение данных из формы в объект NewMaster
        /// </summary>
        private void SaveMasterData()
        {
            NewMaster.UserId = Convert.ToInt32(FIO.SelectedValue);
            NewMaster.Description = Description.Text.Trim();
            NewMaster.Phone = GetPhoneDigits(Phone.Text);
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

        /// <summary>
        /// Проверка наличия подсказки о неактивном мастере при выборе в ComboBox
        /// </summary>
        private void CheckForInactiveMasterHint()
        {
            try
            {
                if (FIO.SelectedIndex == -1)
                    return;

                DataRowView selectedRow = (DataRowView)FIO.SelectedItem;
                bool isInactiveMaster = Convert.ToBoolean(selectedRow["IsInactiveMaster"]);

                if (isInactiveMaster)
                {
                    string fullName = selectedRow["ФИО"].ToString();
                    // Здесь можно добавить визуальную подсказку о найденном неактивном мастере
                    // Например, изменить цвет фона или показать иконку
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        /// <summary>
        /// Обработчик изменения выбора в ComboBox
        /// </summary>
        private void FIO_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInactiveMasterHint();
            LoadMasterDataIfExists();
        }

        /// <summary>
        /// Загрузка существующих данных неактивного мастера при выборе
        /// </summary>
        private void LoadMasterDataIfExists()
        {
            try
            {
                if (FIO.SelectedIndex == -1) return;

                int userId = Convert.ToInt32(FIO.SelectedValue);

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = @"SELECT Description, Phone 
                                   FROM Masters 
                                   WHERE User = @UserId AND IsActive = 0";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Заполнение полей данными из базы для неактивного мастера
                            Description.Text = reader["Description"]?.ToString() ?? "";

                            string phone = reader["Phone"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(phone))
                            {
                                Phone.Text = InputValidator.FormatPhoneNumber(phone);
                            }
                        }
                        else
                        {
                            // Очистка полей для нового мастера
                            Description.Text = "";
                            Phone.Text = "";
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при загрузке данных
            }
        }
    }
}