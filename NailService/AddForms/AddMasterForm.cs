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
        private Show _showForm; // Ссылка на главную форму

        public AddMasterForm(Show showForm = null)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            _showForm = showForm; // Сохраняем ссылку на главную форму
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

                    // Запрос для получения активных пользователей с ролью "Мастер" (IDRole = 3),
                    // которых еще нет в таблице Masters или которые неактивны в Masters
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
                            CASE WHEN m.IsActive = 0 THEN 0 ELSE 1 END, -- сначала неактивные мастера
                            u.LastName, u.FirstName, u.MiddleName";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Создаем новый DataTable с объединенным столбцом ФИО
                    DataTable fioDt = new DataTable();
                    fioDt.Columns.Add("ID", typeof(int));
                    fioDt.Columns.Add("ФИО", typeof(string));
                    fioDt.Columns.Add("Статус", typeof(string));
                    fioDt.Columns.Add("IsInactiveMaster", typeof(bool));

                    // Объединение ФИО в один столбец
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
                            fullName,
                            status,
                            isInactiveMaster
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
                        AddMasterButton.Enabled = false;
                    }
                    else
                    {
                        FIO.SelectedIndex = 0;
                        AddMasterButton.Enabled = true;

                        // Показываем подсказку если выбран неактивный мастер
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

        private void AddMasterButton_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                // Проверяем, является ли выбранный мастер неактивным
                if (CheckAndRestoreInactiveMaster())
                {
                    return; // Мастер восстановлен, форма закрывается
                }

                // Иначе создаем нового мастера
                if (AddNewMaster())
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

        // Проверяет и восстанавливает неактивного мастера
        private bool CheckAndRestoreInactiveMaster()
        {
            try
            {
                // Получаем данные о выбранном мастере
                DataRowView selectedRow = (DataRowView)FIO.SelectedItem;
                bool isInactiveMaster = Convert.ToBoolean(selectedRow["IsInactiveMaster"]);

                if (isInactiveMaster)
                {
                    int userId = Convert.ToInt32(selectedRow["ID"]);
                    string fullName = selectedRow["ФИО"].ToString();

                    // Нашли неактивного мастера - предлагаем восстановить
                    var result = MessageBox.Show(
                        $"Найден неактивный мастер: {fullName}\n\n" +
                        "Восстановить этого мастера с новыми данными?",
                        "Восстановление мастера",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Сохраняем данные из формы
                        SaveMasterData();

                        // Восстанавливаем мастера
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

        // Создает нового мастера или восстанавливает неактивного
        private bool AddNewMaster()
        {
            try
            {
                SaveMasterData();
                int userId = Convert.ToInt32(FIO.SelectedValue);

                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    // Сначала проверяем, есть ли неактивная запись мастера для этого пользователя
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
                            // Нашли неактивного мастера - восстанавливаем
                            int masterId = reader.GetInt32("IDMasters");
                            reader.Close(); // Закрываем reader перед выполнением другого запроса

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

                    // Проверяем, есть ли активный мастер для этого пользователя
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

                    // Создаем нового мастера
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

        // Восстанавливает мастера в базе данных
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

        private void SaveMasterData()
        {
            NewMaster.UserId = Convert.ToInt32(FIO.SelectedValue);
            NewMaster.Description = Description.Text.Trim();
            NewMaster.Phone = GetPhoneDigits(Phone.Text);
        }

        private string GetPhoneDigits(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        // Обработчики событий для телефона (без изменений)
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

        // Проверка подсказки о неактивном мастере
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
                    
                }
                
            }
            catch
            {
                // Игнорируем ошибки при проверке подсказки
            }
        }

        // При изменении выбора в ComboBox
        private void FIO_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInactiveMasterHint();

            // Если выбран неактивный мастер, можно предварительно загрузить его данные
            if (FIO.SelectedIndex != -1)
            {
                LoadMasterDataIfExists();
            }
        }

        // Загружает данные мастера, если он существует (но неактивен)
        private void LoadMasterDataIfExists()
        {
            try
            {
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
                            // Заполняем поля данными из базы
                            Description.Text = reader["Description"]?.ToString() ?? "";

                            string phone = reader["Phone"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(phone))
                            {
                                Phone.Text = InputValidator.FormatPhoneNumber(phone);
                            }
                        }
                        else
                        {
                            // Очищаем поля если мастер новый
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

        // При нажатии на кнопку "Обновить список"
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadAvailableMasters();
        }
    }
}