using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Форма для создания новой записи клиента на услугу
    /// Позволяет выбрать клиента, мастера, услугу, применить скидку и создать чек
    /// </summary>
    public partial class RecordingClients : Form
    {
        private DateTime? _selectedDateTime;
        private decimal _selectedServicePrice = 0;
        private int _selectedClientId = 0;
        private int _selectedMasterId = 0;
        private int _selectedServiceId = 0;
        private int _selectedStatusId = 1;
        private decimal _discount = 0;
        private decimal _totalPrice = 0;
        private bool _isUpdatingFromComboBox = false; // Предотвращает зацикливание событий

        private string _fio;

        /// <summary>
        /// Конструктор формы записи клиента
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя (менеджера)</param>
        public RecordingClients(string FIO)
        {
            _fio = FIO;
            InitializeComponent();
            LoadMasters();
            LoadServices();
            LoadStatuses();
            SetupEventHandlers();
            LoadInitialClients();

            cmbClient.DisplayMember = "FullName";
            cmbClient.ValueMember = "ID";
            cmbClient.SelectedIndexChanged += CmbClient_SelectedIndexChanged;
        }

        /// <summary>
        /// Подписка на события изменения выбора в комбобоксах
        /// </summary>
        private void SetupEventHandlers()
        {
            cmbService.SelectedIndexChanged += CmbService_SelectedIndexChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
        }

        #region Загрузка данных

        /// <summary>
        /// Загрузка начального списка клиентов (первые 20)
        /// </summary>
        private void LoadInitialClients()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT IDClient, LastName, FirstName, MiddleName, Phone 
                        FROM Client 
                        ORDER BY LastName, FirstName
                        LIMIT 20";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    if (dt.Rows.Count > 0)
                    {
                        List<ClientItem> clients = new List<ClientItem>();
                        foreach (DataRow row in dt.Rows)
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

                        cmbClient.DataSource = clients;
                        cmbClient.DisplayMember = "FullName";
                        cmbClient.ValueMember = "ID";

                        if (_selectedClientId != 0)
                        {
                            foreach (ClientItem client in clients)
                            {
                                if (client.ID == _selectedClientId)
                                {
                                    cmbClient.SelectedItem = client;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            cmbClient.SelectedIndex = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка списка мастеров
        /// </summary>
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
                        WHERE u.Role = 3";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    List<MasterItem> masters = new List<MasterItem>();
                    foreach (DataRow row in dt.Rows)
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
                    cmbMaster.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка списка услуг
        /// </summary>
        private void LoadServices()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT IDServices, ServiceName, Price FROM Services ORDER BY ServiceName";

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
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка статусов записи (Запланирован, Подтвержден)
        /// </summary>
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
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
            }
        }

        #endregion

        #region Установка даты и времени

        /// <summary>
        /// Установка выбранной даты и времени из формы расписания
        /// </summary>
        /// <param name="dateTime">Выбранная дата и время</param>
        public void SetSelectedDateTime(DateTime dateTime)
        {
            _selectedDateTime = dateTime;
            lblSelectedTime.Text = dateTime.ToString("dd.MM.yyyy HH:mm");

            if (dateTime.Hour < 12)
            {
                _discount = 5;
                lblDiscountPercent.Text = "5% (утренняя скидка)";
            }
            else
            {
                _discount = 0;
                lblDiscountPercent.Text = "0%";
            }

            UpdatePriceDisplay();
        }

        #endregion

        #region Обработчики событий ComboBox

        /// <summary>
        /// Оценка точности совпадения номера телефона при поиске
        /// </summary>
        private int GetPhoneMatchScore(string dbPhone, string searchDigits)
        {
            string cleanDbPhone = InputValidator.FilterToPhone(dbPhone);

            if (cleanDbPhone == searchDigits)
                return 100;

            if (cleanDbPhone.StartsWith(searchDigits))
                return 90;

            if (cleanDbPhone.EndsWith(searchDigits))
                return 80;

            if (cleanDbPhone.Contains(searchDigits))
                return 70;

            return 0;
        }

        /// <summary>
        /// Обработчик изменения выбранного клиента
        /// </summary>
        private void CmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedItem != null)
            {
                ClientItem selectedClient = cmbClient.SelectedItem as ClientItem;
                if (selectedClient != null)
                {
                    _selectedClientId = selectedClient.ID;
                    _isUpdatingFromComboBox = true;
                    string cleanPhone = InputValidator.GetCleanPhoneNumber(selectedClient.Phone);
                    _isUpdatingFromComboBox = false;
                }
            }
            else
            {
                _selectedClientId = 0;
            }
        }

        /// <summary>
        /// Обработчик изменения выбранной услуги
        /// </summary>
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

        /// <summary>
        /// Обработчик изменения выбранного статуса
        /// </summary>
        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedValue != null)
            {
                _selectedStatusId = Convert.ToInt32(cmbStatus.SelectedValue);
            }
        }

        #endregion

        #region Расчет стоимости

        /// <summary>
        /// Обновление отображения цены с учетом скидки
        /// </summary>
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

        #region Добавление клиента

        /// <summary>
        /// Открытие формы добавления нового клиента
        /// </summary>
        private void btnAddClient_Click(object sender, EventArgs e)
        {
            AddClientForm show = new AddClientForm();

            if (show.ShowDialog() == DialogResult.OK)
            {
                int newClientId = show.AddedClientId;

                if (newClientId > 0)
                {
                    MessageBox.Show("Клиент успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadInitialClients();
                    SelectClientById(newClientId);
                }
            }
        }

        /// <summary>
        /// Выбор клиента по ID после добавления
        /// </summary>
        private void SelectClientById(int clientId)
        {
            if (cmbClient.Items.Count == 0) return;

            foreach (var item in cmbClient.Items)
            {
                if (item is ClientItem client && client.ID == clientId)
                {
                    cmbClient.SelectedItem = item;
                    _selectedClientId = clientId;

                    _isUpdatingFromComboBox = true;
                    string cleanPhone = InputValidator.GetCleanPhoneNumber(client.Phone);
                    _isUpdatingFromComboBox = false;

                    break;
                }
            }
        }

        #endregion

        #region Сохранение записи

        /// <summary>
        /// Сохранение записи в базу данных
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        INSERT INTO Record 
                        (Client, Master, Date, Service, Status, User, discount) 
                        VALUES 
                        (@Client, @Master, @Date, @Service, @Status, @User, @discount)";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Client", _selectedClientId);
                    cmd.Parameters.AddWithValue("@Master", _selectedMasterId);
                    cmd.Parameters.AddWithValue("@Date", _selectedDateTime);
                    cmd.Parameters.AddWithValue("@Service", _selectedServiceId);
                    cmd.Parameters.AddWithValue("@Status", _selectedStatusId);
                    cmd.Parameters.AddWithValue("@User", GetCurrentUserId());
                    cmd.Parameters.AddWithValue("@discount", _discount > 0);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись успешно создана!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        DialogResult result = MessageBox.Show("Создать чек?", "Чек",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            GenerateReceipt();
                        }

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

        /// <summary>
        /// Валидация формы перед сохранением
        /// </summary>
        private bool ValidateForm()
        {
            if (_selectedClientId == 0)
            {
                MessageBox.Show("Выберите клиента из списка!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Выберите мастера!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var selectedMaster = cmbMaster.SelectedItem as MasterItem;
            if (selectedMaster != null)
            {
                _selectedMasterId = selectedMaster.ID;
            }

            if (_selectedServiceId == 0)
            {
                MessageBox.Show("Выберите услугу!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_selectedDateTime == null)
            {
                MessageBox.Show("Выберите дату и время!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsTimeSlotAvailable())
            {
                MessageBox.Show("Это время уже занято! Выберите другое время.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка доступности временного слота
        /// </summary>
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
        /// Получение ID текущего пользователя (заглушка)
        /// </summary>
        private int GetCurrentUserId()
        {
            return 1;
        }

        #endregion

        #region Создание чека

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

                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2f);
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(3f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2f);

                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Word.Paragraph para;

                // Заголовок
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "ЧЕК";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 24;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // Название салона
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "Салон красоты NailService";
                para.Range.Font.Size = 16;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 20;
                para.Range.InsertParagraphAfter();

                // Разделитель
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "═══════════════════════════════════════";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // Дата и время
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Дата: {_selectedDateTime:dd.MM.yyyy}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Время: {_selectedDateTime:HH:mm}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // Информация о клиенте
                if (cmbClient.SelectedItem != null)
                {
                    ClientItem client = cmbClient.SelectedItem as ClientItem;
                    string fullName = NameFormatter.FormatToFullName(
                        client.LastName, client.FirstName, client.MiddleName);

                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Клиент: {fullName}";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 5;
                    para.Range.InsertParagraphAfter();

                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Телефон: {client.Phone}";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 10;
                    para.Range.InsertParagraphAfter();
                }

                // Услуга
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Услуга: {cmbService.Text}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                // Мастер
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Мастер: {cmbMaster.Text}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                // Статус
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Статус: {cmbStatus.Text}";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 15;
                para.Range.InsertParagraphAfter();

                // Разделитель
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = "───────────────────────────────────────";
                para.Range.Font.Size = 12;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.ParagraphFormat.SpaceAfter = 10;
                para.Range.InsertParagraphAfter();

                // Цены
                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"Стоимость: {_selectedServicePrice:N0} руб.";
                para.Range.Font.Size = 14;
                para.Range.Font.Name = "Times New Roman";
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 5;
                para.Range.InsertParagraphAfter();

                if (_discount > 0)
                {
                    para = doc.Content.Paragraphs.Add(missing);
                    para.Range.Text = $"Скидка: {_discount}%";
                    para.Range.Font.Size = 14;
                    para.Range.Font.Name = "Times New Roman";
                    para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorRed;
                    para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    para.Range.ParagraphFormat.SpaceAfter = 5;
                    para.Range.InsertParagraphAfter();
                }

                para = doc.Content.Paragraphs.Add(missing);
                para.Range.Text = $"ИТОГО К ОПЛАТЕ: {_totalPrice:N0} руб.";
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 16;
                para.Range.Font.Name = "Times New Roman";
                para.Range.Font.Color = Microsoft.Office.Interop.Word.WdColor.wdColorDarkGreen;
                para.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                para.Range.ParagraphFormat.SpaceAfter = 20;
                para.Range.InsertParagraphAfter();

                // Подпись
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

                // Сохранение документа
                string fileName = $"Чек_{_selectedDateTime:yyyyMMdd_HHmm}.docx";
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

        #region Навигация и поиск

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }

        /// <summary>
        /// Открытие формы расширенного поиска клиента
        /// </summary>
        private void BtnSearchClient_Click(object sender, EventArgs e)
        {
            using (SearchClient searchForm = new SearchClient())
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    ClientItem selectedClient = searchForm.GetSelectedClient();
                    if (selectedClient != null)
                    {
                        SetSelectedClient(selectedClient);
                    }
                }
            }
        }

        /// <summary>
        /// Установка выбранного клиента из поиска
        /// </summary>
        private void SetSelectedClient(ClientItem client)
        {
            if (client == null) return;

            _selectedClientId = client.ID;

            List<ClientItem> singleClientList = new List<ClientItem> { client };
            cmbClient.DataSource = singleClientList;
            cmbClient.DisplayMember = "FullName";
            cmbClient.ValueMember = "ID";
            cmbClient.SelectedItem = client;

            _isUpdatingFromComboBox = true;
            string cleanPhone = InputValidator.GetCleanPhoneNumber(client.Phone);
            _isUpdatingFromComboBox = false;
        }

        #endregion

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    /// <summary>
    /// Модель данных для мастера
    /// </summary>
    public class MasterItem
    {
        public int ID { get; set; }
        public string FullName { get; set; }
    }

    /// <summary>
    /// Модель данных для клиента
    /// </summary>
    public class ClientItem
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
    }

    /// <summary>
    /// Вспомогательный класс для форматирования ФИО
    /// </summary>
    public static class NameFormatter
    {
        /// <summary>
        /// Форматирование в короткий формат (Фамилия И.О.)
        /// </summary>
        public static string FormatToShortName(string lastName, string firstName, string middleName)
        {
            string result = lastName;
            if (!string.IsNullOrEmpty(firstName))
                result += " " + firstName[0] + ".";
            if (!string.IsNullOrEmpty(middleName))
                result += " " + middleName[0] + ".";
            return result;
        }

        /// <summary>
        /// Форматирование в полный формат (Фамилия Имя Отчество)
        /// </summary>
        public static string FormatToFullName(string lastName, string firstName, string middleName)
        {
            return $"{lastName} {firstName} {middleName}".Trim();
        }
    }
}