using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class RecordingClients : Form
    {
        private DateTime? _selectedDateTime;
        private decimal _selectedServicePrice = 0;
        private int _selectedMasterId = 0;
        private int _selectedServiceId = 0;
        private int _selectedStatusId = 1;
        private decimal _discount = 0;
        private decimal _totalPrice = 0;
        private int _currentUserId;

        /// <summary>
        /// Конструктор формы записи клиента
        /// </summary>
        /// <param name="managerFIO">ФИО менеджера</param>
        public RecordingClients(int managerId)
        {
            InitializeComponent();
            _currentUserId = managerId;
            if (_currentUserId == 0)
            {
                MessageBox.Show("Не удалось определить менеджера. Запись не может быть создана.\nОбратитесь к администратору.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            LoadMasters();
            LoadServices();
            LoadStatuses();
            SetupEventHandlers();

            txtClientName.Text = "";
            txtClientPhone.Text = "";
        }

        private int GetManagerIdByFIO(string fio)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT IDUser FROM users 
                        WHERE CONCAT(LastName, ' ', FirstName, ' ', COALESCE(MiddleName, '')) = @fio 
                        AND Role = 4";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@fio", fio.Trim());
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private void SetupEventHandlers()
        {
            cmbService.SelectedIndexChanged += CmbService_SelectedIndexChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
        }

        #region Загрузка данных
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
                    string query = "SELECT IDStatus, StatusName FROM Status WHERE IDStatus IN (1, 2)";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    cmbStatus.DisplayMember = "StatusName";
                    cmbStatus.ValueMember = "IDStatus";
                    cmbStatus.DataSource = dt;
                    cmbStatus.SelectedValue = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Установка даты и времени
        public void SetSelectedDateTime(DateTime dateTime)
        {
            _selectedDateTime = dateTime;
            lblSelectedTime.Text = dateTime.ToString("dd.MM.yyyy HH:mm");

            if (dateTime.Hour < 12)
                _discount = 5;
            else
                _discount = 0;

            UpdatePriceDisplay();
        }
        #endregion

        #region Обработчики
        private void CmbService_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbService.SelectedItem != null)
            {
                DataRowView row = cmbService.SelectedItem as DataRowView;
                if (row != null)
                {
                    _selectedServiceId = Convert.ToInt32(row["IDServices"]);
                    _selectedServicePrice = Convert.ToDecimal(row["Price"]);
                    UpdatePriceDisplay();
                }
            }
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedValue != null)
                _selectedStatusId = Convert.ToInt32(cmbStatus.SelectedValue);
        }
        #endregion

        #region Расчет стоимости
        private void UpdatePriceDisplay()
        {
            decimal price = _selectedServicePrice;
            decimal discountAmount = price * _discount / 100;
            _totalPrice = price - discountAmount;

            lblPrice.Text = $"Стоимость: {price:N0} руб.";
            lblDiscountPercent.Text = $"Скидка: {_discount:F0}%";
            lblTotalPrice.Text = $"С учётом скидки: {_totalPrice:N0} руб.";
        }
        #endregion

        #region Сохранение записи
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    if (!IsTimeSlotAvailable())
                    {
                        MessageBox.Show("Это время уже занято у данного мастера! Выберите другое время.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string query = @"
                        INSERT INTO Record 
                        (Master, ClientName, ClientPhone, Date, Service, Status, User, discount) 
                        VALUES 
                        (@Master, @ClientName, @ClientPhone, @Date, @Service, @Status, @User, @discount)";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Master", _selectedMasterId);
                    cmd.Parameters.AddWithValue("@ClientName", CapitalizeRussianName(txtClientName.Text.Trim()));
                    cmd.Parameters.AddWithValue("@ClientPhone", txtClientPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Date", _selectedDateTime);
                    cmd.Parameters.AddWithValue("@Service", _selectedServiceId);
                    cmd.Parameters.AddWithValue("@Status", _selectedStatusId);
                    cmd.Parameters.AddWithValue("@User", _currentUserId);
                    cmd.Parameters.AddWithValue("@discount", _discount > 0 ? 1 : 0);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно создана!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        DialogResult result = MessageBox.Show("Создать чек?", "Чек",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                            GenerateReceipt();

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Введите имя клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtClientPhone.Text))
            {
                MessageBox.Show("Введите номер телефона клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            _selectedMasterId = Convert.ToInt32(cmbMaster.SelectedValue);
            if (_selectedServiceId == 0)
            {
                MessageBox.Show("Выберите услугу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (_selectedDateTime == null)
            {
                MessageBox.Show("Выберите дату и время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool IsTimeSlotAvailable()
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
                        AND Status IN (1, 2)";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Master", _selectedMasterId);
                    cmd.Parameters.AddWithValue("@Date", _selectedDateTime);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
                }
            }
            catch { return false; }
        }
        #endregion

        #region Генерация чека
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

                var para = doc.Content.Paragraphs.Add(missing);
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
                para.Range.Text = $"Дата: {_selectedDateTime:dd.MM.yyyy}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Время: {_selectedDateTime:HH:mm}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Клиент: {CapitalizeRussianName(txtClientName.Text.Trim())}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Телефон: {txtClientPhone.Text.Trim()}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Услуга: {cmbService.Text}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Мастер: {cmbMaster.Text}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Статус: {cmbStatus.Text}";
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
                para.Range.Text = $"Стоимость: {_selectedServicePrice:N0} руб.";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                if (_discount > 0)
                {
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Скидка: {_discount}%";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorRed;
                    para.Range.ParagraphFormat.SpaceAfter = 5;
                    para.Range.InsertParagraphAfter();
                }

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"ИТОГО К ОПЛАТЕ: {_totalPrice:N0} руб.";
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

                string fileName = $"Чек_{_selectedDateTime:yyyyMMdd_HHmm}.docx";
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

        #region Навигация
        private void button3_Click(object sender, EventArgs e) => this.Close();
        private void button1_Click(object sender, EventArgs e) => btnSave_Click(sender, e);
        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion


        /// <summary>
        /// Автоматическое форматирование номера телефона при вводе
        /// </summary>
        private void txtClientPhone_TextChanged(object sender, EventArgs e)
        {
            int originalSelectionStart = txtClientPhone.SelectionStart;
            string originalText = txtClientPhone.Text;

            string filteredText = InputValidator.FilterToPhone(originalText);
            string formattedText = InputValidator.FormatPhoneNumber(filteredText);

            if (formattedText != originalText)
            {
                txtClientPhone.Text = formattedText;
                int adjustedPosition = GetAdjustedCursorPosition(originalSelectionStart, originalText, formattedText);
                txtClientPhone.SelectionStart = Math.Min(adjustedPosition, formattedText.Length);
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

        private void txtClientName_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = txtClientName.SelectionStart;
            int selectionLength = txtClientName.SelectionLength;

            // Фильтрация: оставляем только русские буквы, дефис и пробел
            string filteredText = InputValidator.FilterToRussianLetters(txtClientName.Text);

            // Преобразование в формат "С заглавной буквы"
            string properText = CapitalizeRussianName(filteredText);

            if (properText != txtClientName.Text)
            {
                txtClientName.Text = properText;
                // Корректируем позицию курсора
                if (selectionStart > properText.Length)
                    selectionStart = properText.Length;
                txtClientName.SelectionStart = selectionStart;
                txtClientName.SelectionLength = 0;
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

    public static class NameFormatter
    {
        public static string FormatToShortName(string lastName, string firstName, string middleName)
        {
            string result = lastName;
            if (!string.IsNullOrEmpty(firstName))
                result += " " + firstName[0] + ".";
            if (!string.IsNullOrEmpty(middleName))
                result += " " + middleName[0] + ".";
            return result;
        }
    }
}