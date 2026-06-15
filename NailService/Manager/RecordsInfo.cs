using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class RecordsInfo : Form
    {
        private int _recordId;
        private string _userFIO;
        private int _roleID;

        private int _currentMasterId;
        private int _currentServiceId;
        private int _currentStatusId;
        private DateTime _currentDateTime;
        private string _currentClientName;
        private string _currentClientPhone;

        private bool _isCancelled;

        private string _masterName;
        private string _serviceName;
        private decimal _servicePrice;
        private string _statusName;

        public RecordsInfo(int recordId, string userFIO, int roleID)
        {
            InitializeComponent();
            _recordId = recordId;
            _userFIO = userFIO;
            _roleID = roleID;

            LoadMasters();
            LoadStatuses();
            LoadServices();
            LoadRecordData();
        }

        #region Загрузка справочников

        private void LoadMasters()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT m.IDMasters, u.LastName, u.FirstName, u.MiddleName
                        FROM Masters m
                        INNER JOIN Users u ON m.User = u.IDUser
                        WHERE u.Role = 3 AND u.IsActive = 1";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    DataTable displayDt = new DataTable();
                    displayDt.Columns.Add("IDMasters", typeof(int));
                    displayDt.Columns.Add("FullName", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        string fullName = NameFormatter.FormatToShortName(
                            row["LastName"].ToString(),
                            row["FirstName"].ToString(),
                            row["MiddleName"].ToString()
                        );
                        displayDt.Rows.Add(row["IDMasters"], fullName);
                    }

                    cmbMaster.DisplayMember = "FullName";
                    cmbMaster.ValueMember = "IDMasters";
                    cmbMaster.DataSource = displayDt;
                    cmbMaster.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT IDServices, ServiceName, Price FROM Services WHERE IsActive = 1 ORDER BY ServiceName";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    cmbService.DisplayMember = "ServiceName";
                    cmbService.ValueMember = "IDServices";
                    cmbService.DataSource = dt;
                    cmbService.Enabled = false;   // Услугу нельзя менять
                    cmbService.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatuses()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT IDStatus, StatusName FROM Status ORDER BY IDStatus";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    cmbStatus.DisplayMember = "StatusName";
                    cmbStatus.ValueMember = "IDStatus";
                    cmbStatus.DataSource = dt;
                    cmbStatus.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Загрузка записи

        private void LoadRecordData()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                SELECT 
                    r.Master,
                    r.Service,
                    r.Status,
                    r.Date,
                    r.ClientName,
                    r.ClientPhone,
                    u_m.LastName as MasterLastName,
                    u_m.FirstName as MasterFirstName,
                    u_m.MiddleName as MasterMiddleName,
                    s.ServiceName,
                    s.Price,
                    stat.StatusName
                FROM Record r
                INNER JOIN Masters m ON r.Master = m.IDMasters
                INNER JOIN Users u_m ON m.User = u_m.IDUser
                INNER JOIN Services s ON r.Service = s.IDServices
                INNER JOIN Status stat ON r.Status = stat.IDStatus
                WHERE r.IDRecord = @recordId";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@recordId", _recordId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _currentMasterId = Convert.ToInt32(reader["Master"]);
                            _currentServiceId = Convert.ToInt32(reader["Service"]);
                            _currentStatusId = Convert.ToInt32(reader["Status"]);
                            _currentDateTime = Convert.ToDateTime(reader["Date"]);
                            _currentClientName = reader["ClientName"].ToString();
                            _currentClientPhone = reader["ClientPhone"].ToString();

                            _masterName = NameFormatter.FormatToShortName(
                                reader["MasterLastName"].ToString(),
                                reader["MasterFirstName"].ToString(),
                                reader["MasterMiddleName"].ToString()
                            );
                            _serviceName = reader["ServiceName"].ToString();
                            _servicePrice = Convert.ToDecimal(reader["Price"]);
                            _statusName = reader["StatusName"].ToString();

                            _isCancelled = (_currentStatusId == 3); // 3 = Отменено

                            // Отображение информации
                            lblClientName.Text = $"{_currentClientName}";
                            lblClientPhone.Text = $"Телефон: {_currentClientPhone}";
                            lblDateTimeInfo.Text = $"Дата и время: {_currentDateTime:dd.MM.yyyy HH:mm}";

                            // Устанавливаем выбранную услугу (только для отображения)
                            SelectServiceItem(_currentServiceId);

                            // Определяем, можно ли устанавливать статус "Выполнено"
                            bool canSetCompleted = _currentDateTime <= DateTime.Now;

                            if (_isCancelled)
                            {
                                lblDateTimeInfo.ForeColor = Color.Red;
                                lblStatusInfo.ForeColor = Color.Red;
                                btnSave.Text = "Восстановить запись";
                                btnPrintReceipt.Visible = false;
                                cmbMaster.Enabled = true;
                                cmbStatus.Enabled = false;
                            }
                            else
                            {
                                lblDateTimeInfo.ForeColor = Color.HotPink;
                                btnSave.Text = "Сохранить изменения";
                                btnPrintReceipt.Visible = true;

                                // Права доступа:
                                // Администратор (2) – может менять и мастера, и статус
                                // Менеджер (4) – может менять только статус (на "Отменено"), но не мастера
                                bool canEditMaster = (_roleID == 2);
                                bool canEditStatus = (_roleID == 2 || _roleID == 4);

                                cmbMaster.Enabled = canEditMaster;
                                cmbStatus.Enabled = canEditStatus;

                                // ЗАПРЕЩАЕМ устанавливать статус "Выполнено", если дата и время ещё не наступили
                                if (!canSetCompleted && cmbStatus.Enabled)
                                {
                                    // Блокируем статус "Выполнено" (IDStatus = 2)
                                    foreach (DataRowView item in cmbStatus.Items)
                                    {
                                        if (Convert.ToInt32(item["IDStatus"]) == 2)
                                        {
                                            item.Delete(); // Удаляем из списка
                                            break;
                                        }
                                    }
                                    // Или можно просто заблокировать выбор этого статуса
                                    // Альтернативный подход: отключаем возможность выбора статуса "Выполнено"
                                }
                            }

                            SelectMasterItem(_currentMasterId);
                            SelectStatusItem(_currentStatusId);
                        }
                        else
                        {
                            MessageBox.Show("Запись не найдена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectServiceItem(int serviceId)
        {
            if (cmbService.Items.Count == 0) return;
            foreach (DataRowView item in cmbService.Items)
            {
                if (Convert.ToInt32(item["IDServices"]) == serviceId)
                {
                    cmbService.SelectedItem = item;
                    return;
                }
            }
        }

        private void SelectMasterItem(int masterId)
        {
            if (cmbMaster.Items.Count == 0) return;
            foreach (DataRowView item in cmbMaster.Items)
            {
                if (Convert.ToInt32(item["IDMasters"]) == masterId)
                {
                    cmbMaster.SelectedItem = item;
                    break;
                }
            }
        }

        private void SelectStatusItem(int statusId)
        {
            if (cmbStatus.Items.Count == 0) return;
            foreach (DataRowView item in cmbStatus.Items)
            {
                if (Convert.ToInt32(item["IDStatus"]) == statusId)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }
        }

        #endregion

        #region Сохранение / Восстановление

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_isCancelled)
                RestoreRecord();
            else
                UpdateRecord();
        }

        private void RestoreRecord()
        {
            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newMasterId = Convert.ToInt32(cmbMaster.SelectedValue);

            if (!IsTimeSlotAvailable(newMasterId, _currentDateTime, _recordId))
            {
                MessageBox.Show("Это время уже занято у выбранного мастера.",
                    "Время занято", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "UPDATE Record SET Master = @Master, Status = 1 WHERE IDRecord = @IDRecord";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Master", newMasterId);
                    cmd.Parameters.AddWithValue("@IDRecord", _recordId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Запись успешно восстановлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateRecord()
        {
            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newMasterId = Convert.ToInt32(cmbMaster.SelectedValue);
            int newStatusId = Convert.ToInt32(cmbStatus.SelectedValue);

            if (newMasterId == _currentMasterId && newStatusId == _currentStatusId)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (newMasterId != _currentMasterId && !IsTimeSlotAvailable(newMasterId, _currentDateTime, _recordId))
            {
                MessageBox.Show("Это время уже занято у выбранного мастера.",
                    "Время занято", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "UPDATE Record SET Master = @Master, Status = @Status WHERE IDRecord = @IDRecord";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Master", newMasterId);
                    cmd.Parameters.AddWithValue("@Status", newStatusId);
                    cmd.Parameters.AddWithValue("@IDRecord", _recordId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Запись обновлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsTimeSlotAvailable(int masterId, DateTime dateTime, int excludeRecordId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT COUNT(*) FROM Record 
                        WHERE Master = @Master 
                        AND Date = @Date 
                        AND Status IN (1, 2)
                        AND IDRecord != @excludeRecordId";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Master", masterId);
                    cmd.Parameters.AddWithValue("@Date", dateTime);
                    cmd.Parameters.AddWithValue("@excludeRecordId", excludeRecordId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Печать чека

        private void btnPrintReceipt_Click(object sender, EventArgs e) => GenerateReceipt();

        private void GenerateReceipt()
        {
            try
            {
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = true;
                var doc = wordApp.Documents.Add();

                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(3f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2f);

                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Word.Paragraph para;

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "ЧЕК";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 24;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Салон красоты NailService";
                para.Range.Font.Size = 16;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 20;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "═══════════════════════════════════════";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Дата: {_currentDateTime:dd.MM.yyyy}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Время: {_currentDateTime:HH:mm}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Клиент: {_currentClientName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                if (!string.IsNullOrEmpty(_currentClientPhone))
                {
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Телефон: {_currentClientPhone}";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.ParagraphFormat.SpaceAfter = 10;
                    para.Range.InsertParagraphAfter();
                }

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Услуга: {_serviceName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Мастер: {_masterName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Статус: {_statusName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 15;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "───────────────────────────────────────";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Стоимость: {_servicePrice:N0} руб.";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                decimal discount = 0;
                if (_currentDateTime.Hour < 12)
                    discount = 5;
                decimal total = _servicePrice;
                if (discount > 0)
                {
                    decimal discountAmount = _servicePrice * discount / 100;
                    total = _servicePrice - discountAmount;
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Скидка: {discount}% (утренняя)";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorRed;
                    para.Range.ParagraphFormat.SpaceAfter = 5;
                    para.Range.InsertParagraphAfter();
                }

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"ИТОГО К ОПЛАТЕ: {total:N0} руб.";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 16;
                para.Range.Font.Name = "Times New Roman";
                para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorDarkGreen;
                para.Range.ParagraphFormat.SpaceAfter = 20;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Спасибо за визит!";
                para.Range.Font.Size = 14;
                para.Range.Font.Italic = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Будем рады видеть вас снова!";
                para.Range.Font.Size = 12;
                para.Range.Font.Italic = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                string fileName = $"Чек_{_currentDateTime:yyyyMMdd_HHmm}.docx";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fullPath = Path.Combine(desktopPath, fileName);
                doc.SaveAs(fullPath);

                MessageBox.Show($"Чек сохранён на рабочий стол:\n{fullPath}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания чека: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void btnBack_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }



        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
        /// </summary>
        private void lblClientPhone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = lblClientPhone.SelectionStart;
            string originalText = lblClientPhone.Text;

            string filteredText = InputValidator.FilterToPhone(originalText);
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                lblClientPhone.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                lblClientPhone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
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

        private void lblClientName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = lblClientName.SelectionStart;
            int selectionLength = lblClientName.SelectionLength;

            // Фильтрация: оставляем только русские буквы, дефис и пробел
            string filteredText = InputValidator.FilterToRussianLetters(lblClientName.Text);

            // Преобразование в формат "С заглавной буквы"
            string properText = CapitalizeRussianName(filteredText);

            if (properText != lblClientName.Text)
            {
                lblClientName.Text = properText;
                // Корректируем позицию курсора
                if (selectionStart > properText.Length)
                    selectionStart = properText.Length;
                lblClientName.SelectionStart = selectionStart;
                lblClientName.SelectionLength = 0;
            }
        }

        /// <summary>
        /// Преобразует строку с русскими буквами в формат: первая буква заглавная, остальные строчные.
        /// Поддерживает имена через дефис (каждая часть начинается с заглавной).
        /// </summary>
        private string CapitalizeRussianName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Разбиваем на части по дефису
            string[] parts = input.Split('-');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    // Первый символ – заглавный, остальные – строчные
                    char first = char.ToUpper(parts[i][0]);
                    string rest = parts[i].Substring(1).ToLower();
                    parts[i] = first + rest;
                }
            }
            return string.Join("-", parts);
        }
    }
}