using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Форма для просмотра и редактирования информации о записи
    /// Позволяет изменять мастера и статус, а также восстанавливать отмененные записи
    /// </summary>
    public partial class RecordsInfo : Form
    {
        private int _recordId;
        private string _userFIO;
        private int _roleID;

        // Текущие значения записи для отслеживания изменений
        private int _currentClientId;
        private int _currentMasterId;
        private int _currentServiceId;
        private int _currentStatusId;
        private DateTime _currentDateTime;

        private bool _isCancelled; // Флаг отмененной записи (статус 4)

        // Данные для чека
        private string _clientFullName;
        private string _clientPhone;
        private string _masterName;
        private string _serviceName;
        private decimal _servicePrice;
        private string _statusName;

        /// <summary>
        /// Конструктор формы информации о записи
        /// </summary>
        /// <param name="recordId">ID записи</param>
        /// <param name="userFIO">ФИО текущего пользователя</param>
        /// <param name="roleID">ID роли пользователя</param>
        public RecordsInfo(int recordId, string userFIO, int roleID)
        {
            InitializeComponent();
            _recordId = recordId;
            _userFIO = userFIO;
            _roleID = roleID;

            LoadComboBoxData();
            LoadRecordData();
        }

        #region Загрузка данных

        /// <summary>
        /// Загрузка данных для комбобоксов (клиенты, мастера, услуги, статусы)
        /// </summary>
        private void LoadComboBoxData()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    // Загрузка клиентов
                    string clientQuery = @"
                        SELECT IDClient, LastName, FirstName, MiddleName, Phone 
                        FROM Client 
                        WHERE IsActive = 1
                        ORDER BY LastName, FirstName";

                    MySqlCommand clientCmd = new MySqlCommand(clientQuery, con);
                    DataTable clientDt = new DataTable();
                    clientDt.Load(clientCmd.ExecuteReader());

                    List<ClientItem> clients = new List<ClientItem>();
                    foreach (DataRow row in clientDt.Rows)
                    {
                        string fullName = NameFormatter.FormatToShortName(
                            row["LastName"].ToString(),
                            row["FirstName"].ToString(),
                            row["MiddleName"].ToString()
                        );

                        clients.Add(new ClientItem
                        {
                            ID = Convert.ToInt32(row["IDClient"]),
                            FullName = fullName,
                            Phone = row["Phone"].ToString(),
                            LastName = row["LastName"].ToString(),
                            FirstName = row["FirstName"].ToString(),
                            MiddleName = row["MiddleName"].ToString()
                        });
                    }

                    cmbClient.DisplayMember = "FullName";
                    cmbClient.ValueMember = "ID";
                    cmbClient.DataSource = clients;
                    cmbClient.Enabled = false; // Клиента нельзя изменить
                    cmbClient.SelectedIndex = -1;

                    // Загрузка мастеров
                    string masterQuery = @"
                        SELECT m.IDMasters, u.LastName, u.FirstName, u.MiddleName
                        FROM Masters m
                        INNER JOIN Users u ON m.User = u.IDUser
                        WHERE u.Role = 3 AND m.IsActive = 1";

                    MySqlCommand masterCmd = new MySqlCommand(masterQuery, con);
                    DataTable masterDt = new DataTable();
                    masterDt.Load(masterCmd.ExecuteReader());

                    List<MasterItem> masters = new List<MasterItem>();
                    foreach (DataRow row in masterDt.Rows)
                    {
                        string fullName = NameFormatter.FormatToShortName(
                            row["LastName"].ToString(),
                            row["FirstName"].ToString(),
                            row["MiddleName"].ToString()
                        );

                        masters.Add(new MasterItem
                        {
                            ID = Convert.ToInt32(row["IDMasters"]),
                            FullName = fullName
                        });
                    }

                    cmbMaster.DisplayMember = "FullName";
                    cmbMaster.ValueMember = "ID";
                    cmbMaster.DataSource = masters;
                    cmbMaster.Enabled = true; // Мастера можно менять
                    cmbMaster.SelectedIndex = -1;

                    // Загрузка услуг
                    string serviceQuery = "SELECT IDServices, ServiceName, Price FROM Services WHERE IsActive = 1 ORDER BY ServiceName";

                    MySqlCommand serviceCmd = new MySqlCommand(serviceQuery, con);
                    DataTable serviceDt = new DataTable();
                    serviceDt.Load(serviceCmd.ExecuteReader());

                    cmbService.DisplayMember = "ServiceName";
                    cmbService.ValueMember = "IDServices";
                    cmbService.DataSource = serviceDt;
                    cmbService.Enabled = false; // Услугу нельзя изменить
                    cmbService.SelectedIndex = -1;

                    // Загрузка статусов
                    string statusQuery = "SELECT IDStatus, StatusName FROM Status ORDER BY IDStatus";

                    MySqlCommand statusCmd = new MySqlCommand(statusQuery, con);
                    DataTable statusDt = new DataTable();
                    statusDt.Load(statusCmd.ExecuteReader());

                    cmbStatus.DisplayMember = "StatusName";
                    cmbStatus.ValueMember = "IDStatus";
                    cmbStatus.DataSource = statusDt;
                    cmbStatus.Enabled = true; // Статус можно менять
                    cmbStatus.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загрузка данных конкретной записи
        /// </summary>
        private void LoadRecordData()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            r.Client,
                            r.Master,
                            r.Service,
                            r.Status,
                            r.Date,
                            c.LastName as ClientLastName,
                            c.FirstName as ClientFirstName,
                            c.MiddleName as ClientMiddleName,
                            c.Phone as ClientPhone,
                            u_m.LastName as MasterLastName,
                            u_m.FirstName as MasterFirstName,
                            u_m.MiddleName as MasterMiddleName,
                            s.ServiceName,
                            s.Price,
                            stat.StatusName
                        FROM Record r
                        INNER JOIN Client c ON r.Client = c.IDClient
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
                            _currentClientId = Convert.ToInt32(reader["Client"]);
                            _currentMasterId = Convert.ToInt32(reader["Master"]);
                            _currentServiceId = Convert.ToInt32(reader["Service"]);
                            _currentStatusId = Convert.ToInt32(reader["Status"]);
                            _currentDateTime = Convert.ToDateTime(reader["Date"]);

                            // Сохраняем данные для чека
                            _clientFullName = NameFormatter.FormatToFullName(
                                reader["ClientLastName"].ToString(),
                                reader["ClientFirstName"].ToString(),
                                reader["ClientMiddleName"].ToString()
                            );
                            _clientPhone = reader["ClientPhone"]?.ToString() ?? "";
                            _masterName = NameFormatter.FormatToShortName(
                                reader["MasterLastName"].ToString(),
                                reader["MasterFirstName"].ToString(),
                                reader["MasterMiddleName"].ToString()
                            );
                            _serviceName = reader["ServiceName"]?.ToString() ?? "";
                            _servicePrice = Convert.ToDecimal(reader["Price"]);
                            _statusName = reader["StatusName"]?.ToString() ?? "";

                            _isCancelled = (_currentStatusId == 4);

                            // Отображение даты и времени
                            lblDateTimeInfo.Text = $"Дата и время: {_currentDateTime:dd.MM.yyyy HH:mm}";
                            lblDateTimeInfo.Font = new Font("MS Reference Sans Serif", 10, FontStyle.Bold);

                            if (_isCancelled)
                            {
                                lblDateTimeInfo.ForeColor = Color.Red;
                                lblStatusInfo.Text = "⚠ ЗАПИСЬ ОТМЕНЕНА";
                                lblStatusInfo.ForeColor = Color.Red;
                                lblStatusInfo.Font = new Font("MS Reference Sans Serif", 12, FontStyle.Bold);
                                lblStatusInfo.Visible = true;
                                btnSave.Text = "Восстановить запись";
                                btnPrintReceipt.Visible = false; // Скрываем кнопку печати для отмененных
                            }
                            else
                            {
                                lblDateTimeInfo.ForeColor = Color.HotPink;
                                lblStatusInfo.Visible = false;
                                btnSave.Text = "Изменить";
                                btnPrintReceipt.Visible = true; // Показываем кнопку печати
                            }

                            // Установка выбранных значений в комбобоксах
                            SelectComboBoxItem(cmbClient, _currentClientId);
                            SelectComboBoxItem(cmbMaster, _currentMasterId);
                            SelectServiceItem(_currentServiceId);
                            SelectStatusItem(_currentStatusId);
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

        #endregion

        #region Выбор элементов в ComboBox

        /// <summary>
        /// Выбор элемента в ComboBox по ID (для списков с объектами)
        /// </summary>
        private void SelectComboBoxItem(ComboBox comboBox, int value)
        {
            if (comboBox.Items.Count == 0) return;

            foreach (var item in comboBox.Items)
            {
                if (item == null) continue;

                var property = item.GetType().GetProperty(comboBox.ValueMember);
                if (property != null)
                {
                    object propValue = property.GetValue(item);
                    if (propValue != null)
                    {
                        int itemValue = Convert.ToInt32(propValue);
                        if (itemValue == value)
                        {
                            comboBox.SelectedItem = item;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выбор услуги в ComboBox по ID (для DataTable)
        /// </summary>
        private void SelectServiceItem(int serviceId)
        {
            if (cmbService.Items.Count == 0) return;

            foreach (DataRowView item in cmbService.Items)
            {
                if (item != null && Convert.ToInt32(item["IDServices"]) == serviceId)
                {
                    cmbService.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Выбор статуса в ComboBox по ID (для DataTable)
        /// </summary>
        private void SelectStatusItem(int statusId)
        {
            if (cmbStatus.Items.Count == 0) return;

            foreach (DataRowView item in cmbStatus.Items)
            {
                if (item != null && Convert.ToInt32(item["IDStatus"]) == statusId)
                {
                    cmbStatus.SelectedItem = item;
                    return;
                }
            }
        }

        #endregion

        #region Обработка сохранения

        /// <summary>
        /// Обработчик кнопки сохранения/восстановления
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_isCancelled)
            {
                RestoreCancelledRecord();
            }
            else
            {
                EditRecord();
            }
        }

        /// <summary>
        /// Восстановление отмененной записи
        /// </summary>
        private void RestoreCancelledRecord()
        {
            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Ошибка загрузки клиента!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbService.SelectedItem == null)
            {
                MessageBox.Show("Ошибка загрузки услуги!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newMasterId = (int)cmbMaster.SelectedValue;

            if (!IsTimeSlotAvailable(newMasterId, _currentDateTime, _recordId))
            {
                DialogResult result = MessageBox.Show(
                    "Это время уже занято у выбранного мастера.\n\n" +
                    "Хотите перенести запись на другое время?",
                    "Время занято",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SelectNewDateTime();
                }
                return;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    string query = @"
                        UPDATE Record 
                        SET Master = @Master,
                            Status = 1
                        WHERE IDRecord = @IDRecord";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IDRecord", _recordId);
                    cmd.Parameters.AddWithValue("@Master", newMasterId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно восстановлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Редактирование существующей записи
        /// </summary>
        private void EditRecord()
        {
            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Ошибка загрузки клиента!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbService.SelectedItem == null)
            {
                MessageBox.Show("Ошибка загрузки услуги!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int newClientId = (int)cmbClient.SelectedValue;
            int newMasterId = (int)cmbMaster.SelectedValue;
            int newServiceId = (int)cmbService.SelectedValue;
            int newStatusId = (int)cmbStatus.SelectedValue;

            if (newClientId != _currentClientId)
            {
                MessageBox.Show("Нельзя изменить клиента!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newServiceId != _currentServiceId)
            {
                MessageBox.Show("Нельзя изменить услугу!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newMasterId == _currentMasterId && newStatusId == _currentStatusId)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            if (newMasterId != _currentMasterId)
            {
                if (!IsTimeSlotAvailable(newMasterId, _currentDateTime, _recordId))
                {
                    DialogResult result = MessageBox.Show(
                        "Это время уже занято у выбранного мастера.\n\n" +
                        "Хотите перенести запись на другое время?",
                        "Время занято",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SelectNewDateTime();
                    }
                    return;
                }
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        UPDATE Record 
                        SET Master = @Master,
                            Status = @Status
                        WHERE IDRecord = @IDRecord";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IDRecord", _recordId);
                    cmd.Parameters.AddWithValue("@Master", newMasterId);
                    cmd.Parameters.AddWithValue("@Status", newStatusId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно обновлена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Проверка доступности времени

        /// <summary>
        /// Проверка доступности временного слота для мастера
        /// </summary>
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

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Выбор нового времени для переноса записи
        /// </summary>
        private void SelectNewDateTime()
        {
            MessageBox.Show("Функция переноса времени будет доступна в следующей версии.",
                "В разработке", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Формирование чека

        /// <summary>
        /// Обработчик кнопки печати чека
        /// </summary>
        private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            GenerateReceipt();
        }

        /// <summary>
        /// Генерация чека в формате Word
        /// </summary>
        private void GenerateReceipt()
        {
            try
            {
                Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = true;

                Microsoft.Office.Interop.Word.Document doc = wordApp.Documents.Add();

                // Настройка страницы
                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(3f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2f);

                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Word.Paragraph para;

                // ЗАГОЛОВОК
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "ЧЕК";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 24;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // НАЗВАНИЕ САЛОНА
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Салон красоты NailService";
                para.Range.Font.Size = 16;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 20;
                para.Range.InsertParagraphAfter();

                // РАЗДЕЛИТЕЛЬ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "═══════════════════════════════════════";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // ДАТА И ВРЕМЯ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Дата: {_currentDateTime:dd.MM.yyyy}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Время: {_currentDateTime:HH:mm}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // ИНФОРМАЦИЯ О КЛИЕНТЕ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Клиент: {_clientFullName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                if (!string.IsNullOrEmpty(_clientPhone))
                {
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Телефон: {_clientPhone}";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 10;
                    para.Range.InsertParagraphAfter();
                }

                // УСЛУГА
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Услуга: {_serviceName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                // МАСТЕР
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Мастер: {_masterName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                // СТАТУС
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Статус: {_statusName}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 15;
                para.Range.InsertParagraphAfter();

                // РАЗДЕЛИТЕЛЬ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "───────────────────────────────────────";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // ЦЕНЫ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Стоимость: {_servicePrice:N0} руб.";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                // Проверяем наличие скидки (утренняя скидка до 12 часов)
                if (_currentDateTime.Hour < 12)
                {
                    decimal discountAmount = _servicePrice * 5 / 100;
                    decimal totalPrice = _servicePrice - discountAmount;

                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Скидка: 5% (утренняя)";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorRed;
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 5;
                    para.Range.InsertParagraphAfter();

                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"ИТОГО К ОПЛАТЕ: {totalPrice:N0} руб.";
                    para.Range.Font.Bold = 1;
                    para.Range.Font.Size = 16;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorDarkGreen;
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 20;
                    para.Range.InsertParagraphAfter();
                }
                else
                {
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"ИТОГО К ОПЛАТЕ: {_servicePrice:N0} руб.";
                    para.Range.Font.Bold = 1;
                    para.Range.Font.Size = 16;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorDarkGreen;
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 20;
                    para.Range.InsertParagraphAfter();
                }

                // ПОДПИСЬ
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Спасибо за визит!";
                para.Range.Font.Size = 14;
                para.Range.Font.Italic = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Будем рады видеть вас снова!";
                para.Range.Font.Size = 12;
                para.Range.Font.Italic = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                // Сохраняем документ
                string fileName = $"Чек_{_currentDateTime:yyyyMMdd_HHmm}.docx";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fullPath = System.IO.Path.Combine(desktopPath, fileName);

                doc.SaveAs(fullPath);

                MessageBox.Show($"Чек сохранен на рабочий стол:\n{fullPath}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        /// <summary>
        /// Закрытие формы без сохранения
        /// </summary>
        private void btnBack_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}