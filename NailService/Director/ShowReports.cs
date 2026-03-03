using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace NailService
{
    /// <summary>
    /// Форма для просмотра и фильтрации отчетов по записям
    /// Поддерживает фильтрацию по дате, мастеру, статусу, поиск и сортировку
    /// Для администраторов доступно редактирование статусов записей
    /// </summary>
    public partial class ShowReports : Form
    {
        private string _fio;
        private int _roleID;
        private FilterManager _filterManager;
        private DateTime _minDate;
        private DateTime _maxDate;
        private int _editingRowIndex = -1;
        private int _editingColumnIndex = -1;
        private List<StatusItem> _statusItems;
        private bool _isErrorMessageShown = false;

        /// <summary>
        /// Конструктор формы отчетов
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        /// <param name="RoleID">ID роли (1-директор, 2-админ, 4-менеджер)</param>
        public ShowReports(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;

            _filterManager = new FilterManager(Connection.ConnectionString);

            SetupDataGridView();
            _filterManager.PopulateMastersComboBox(cmbMasterFilter);
            _filterManager.PopulateStatusComboBox(cmbStatusFilter);

            _statusItems = _filterManager.GetStatusItems();

            cmbSort.Items.AddRange(new string[] {
                "Цена (по возрастанию)",
                "Цена (по убыванию)",
                "Мастер (А-Я)",
                "Мастер (Я-А)",
                "Клиент (А-Я)",
                "Клиент (Я-А)"
            });
            cmbSort.SelectedIndex = 0;

            SetupDateTimePickers();
            LoadData();

            FIOlabel.Text = RoleID == 1 ? $"Директор: {_fio}" :
                           RoleID == 2 ? $"Администратор: {_fio}" : "";
        }

        /// <summary>
        /// Настройка DateTimePicker с ограничениями по датам из БД
        /// </summary>
        private void SetupDateTimePickers()
        {
            try
            {
                var dateRange = _filterManager.GetDateRange();
                _minDate = dateRange.MinDate;
                _maxDate = dateRange.MaxDate;

                dtpFromDate.MinDate = _minDate;
                dtpFromDate.MaxDate = _maxDate;
                dtpToDate.MinDate = _minDate;
                dtpToDate.MaxDate = _maxDate;

                DateTime defaultFrom = _maxDate.AddMonths(-1);
                if (defaultFrom < _minDate)
                {
                    defaultFrom = _minDate;
                }

                dtpFromDate.Value = defaultFrom;
                dtpToDate.Value = _maxDate;

                dtpFromDate.Format = DateTimePickerFormat.Custom;
                dtpFromDate.CustomFormat = "dd.MM.yyyy";
                dtpFromDate.ShowUpDown = false;
                dtpFromDate.ShowCheckBox = false;

                dtpToDate.Format = DateTimePickerFormat.Custom;
                dtpToDate.CustomFormat = "dd.MM.yyyy";
                dtpToDate.ShowUpDown = false;
                dtpToDate.ShowCheckBox = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке дат: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DataGridView: создание колонок, установка стилей, подписка на события
        /// </summary>
        private void SetupDataGridView()
        {
            try
            {
                dataGridViewRecords.Columns.Clear();

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "MasterName",
                    HeaderText = "Мастер",
                    ReadOnly = true
                });

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "ClientName",
                    HeaderText = "Клиент",
                    ReadOnly = true
                });

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Date",
                    HeaderText = "Дата и время",
                    ReadOnly = true
                });

                _statusItems = _filterManager.GetStatusItems();

                DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Статус",
                    DataSource = new BindingSource(_statusItems, null),
                    DisplayMember = "Name",
                    ValueMember = "ID",
                    FlatStyle = FlatStyle.Flat,
                    ReadOnly = (_roleID != 2),
                    AutoComplete = true
                };
                dataGridViewRecords.Columns.Add(statusColumn);

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Service",
                    HeaderText = "Услуга",
                    ReadOnly = true
                });

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Price",
                    HeaderText = "Цена",
                    ReadOnly = true
                });

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "UserName",
                    HeaderText = "Менеджер",
                    ReadOnly = true
                });

                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "RecordID",
                    HeaderText = "ID",
                    Visible = false
                };
                dataGridViewRecords.Columns.Add(idColumn);

                StyleManager.ApplyGridStyles(dataGridViewRecords);
                StyleManager.ApplyColumnAlignments(dataGridViewRecords);
                dataGridViewRecords.ReadOnly = false;

                dataGridViewRecords.CellValueChanged += DataGridViewRecords_CellValueChanged;
                dataGridViewRecords.DataError += DataGridViewRecords_DataError;
                dataGridViewRecords.CellBeginEdit += DataGridViewRecords_CellBeginEdit;
                dataGridViewRecords.CellEndEdit += DataGridViewRecords_CellEndEdit;
                dataGridViewRecords.EditingControlShowing += DataGridViewRecords_EditingControlShowing;
                dataGridViewRecords.CellFormatting += DataGridViewRecords_CellFormatting;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке DataGridView: {ex.Message}");
            }
        }

        /// <summary>
        /// Форматирование ячеек - установка цвета фона для статуса
        /// </summary>
        private void DataGridViewRecords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                if (dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    int statusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    e.CellStyle.BackColor = StyleManager.GetStatusColor(statusId);
                }
            }
        }

        /// <summary>
        /// Загрузка данных с применением текущих фильтров
        /// </summary>
        private void LoadData()
        {
            try
            {
                if (dataGridViewRecords.Columns.Count == 0)
                {
                    MessageBox.Show("DataGridView не настроен. Сначала выполните настройку.");
                    return;
                }

                dataGridViewRecords.CellValueChanged -= DataGridViewRecords_CellValueChanged;
                dataGridViewRecords.Rows.Clear();

                string searchText = txtSearch.Text;
                string masterFilter = cmbMasterFilter.SelectedItem?.ToString() ?? "Все мастера";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                if (fromDate > toDate)
                {
                    DateTime temp = fromDate;
                    fromDate = toDate;
                    toDate = temp;
                    dtpFromDate.Value = fromDate;
                    dtpToDate.Value = toDate;
                }

                string sortBy = "Date";
                bool ascending = false;

                if (cmbSort.SelectedItem != null)
                {
                    string sortText = cmbSort.SelectedItem.ToString();
                    switch (sortText)
                    {
                        case "Цена (по возрастанию)": sortBy = "Price"; ascending = true; break;
                        case "Цена (по убыванию)": sortBy = "Price"; ascending = false; break;
                        case "Мастер (А-Я)": sortBy = "Master"; ascending = true; break;
                        case "Мастер (Я-А)": sortBy = "Master"; ascending = false; break;
                        case "Клиент (А-Я)": sortBy = "Client"; ascending = true; break;
                        case "Клиент (Я-А)": sortBy = "Client"; ascending = false; break;
                    }
                }

                var records = _filterManager.GetFilteredRecords(searchText, masterFilter, statusFilter,
                    fromDate, toDate, sortBy, ascending);

                decimal totalRevenue = CalculateTotalRevenue(records);
                lblTotalRevenue.Text = $"Общая выручка: {totalRevenue:N0} руб.";

                int statusColumnIndex = -1;
                int recordIdColumnIndex = -1;

                for (int i = 0; i < dataGridViewRecords.Columns.Count; i++)
                {
                    if (dataGridViewRecords.Columns[i].Name == "Status")
                        statusColumnIndex = i;
                    if (dataGridViewRecords.Columns[i].Name == "RecordID")
                        recordIdColumnIndex = i;
                }

                if (statusColumnIndex == -1 || recordIdColumnIndex == -1)
                {
                    MessageBox.Show("Не найдены необходимые колонки в DataGridView");
                    return;
                }

                foreach (var record in records)
                {
                    int rowIndex = dataGridViewRecords.Rows.Add(
                        record.MasterName,
                        record.ClientName,
                        record.Date.ToString("dd.MM.yyyy HH:mm"),
                        record.StatusID,
                        record.Service,
                        record.Price.ToString("C0"),
                        record.UserName,
                        record.RecordID
                    );

                    if (rowIndex >= 0)
                    {
                        dataGridViewRecords.Rows[rowIndex].Cells[statusColumnIndex].Tag = record.StatusID;
                    }
                }

                lblRecordCount.Text = $"Найдено записей: {records.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                dataGridViewRecords.CellValueChanged += DataGridViewRecords_CellValueChanged;
            }
        }

        /// <summary>
        /// Проверка прав на редактирование при начале редактирования ячейки
        /// </summary>
        private void DataGridViewRecords_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name != "Status")
            {
                e.Cancel = true;
                return;
            }

            if (_roleID != 2)
            {
                e.Cancel = true;

                if (!_isErrorMessageShown)
                {
                    _isErrorMessageShown = true;
                    MessageBox.Show("Только администратор может изменять статусы записей.",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                    timer.Interval = 100;
                    timer.Tick += (s, args) => {
                        _isErrorMessageShown = false;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
            }
        }

        private void DataGridViewRecords_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            _editingRowIndex = -1;
            _editingColumnIndex = -1;
        }

        private void DataGridViewRecords_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewRecords.CurrentCell != null &&
                dataGridViewRecords.Columns[dataGridViewRecords.CurrentCell.ColumnIndex].Name == "Status" &&
                e.Control is ComboBox comboBox)
            {
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && _editingRowIndex >= 0 && _editingColumnIndex >= 0)
            {
                dataGridViewRecords.EndEdit();
            }
        }

        /// <summary>
        /// Обработка изменения значения в ячейке - обновление статуса в БД
        /// </summary>
        private void DataGridViewRecords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dataGridViewRecords.Columns[e.ColumnIndex].Name != "Status")
                return;

            try
            {
                int recordIdColumnIndex = -1;
                for (int i = 0; i < dataGridViewRecords.Columns.Count; i++)
                {
                    if (dataGridViewRecords.Columns[i].Name == "RecordID")
                    {
                        recordIdColumnIndex = i;
                        break;
                    }
                }

                if (recordIdColumnIndex == -1)
                {
                    MessageBox.Show("Не найдена колонка с ID записи");
                    return;
                }

                int recordId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[recordIdColumnIndex].Value);
                int newStatusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                int oldStatusId = 0;
                if (dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag != null)
                {
                    oldStatusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag);
                }

                if (oldStatusId == newStatusId)
                    return;

                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите изменить статус этой записи?",
                    "Подтверждение изменения",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool updated = _filterManager.UpdateRecordStatus(recordId, newStatusId);

                    if (updated)
                    {
                        dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor =
                            StyleManager.GetStatusColor(newStatusId);

                        dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag = newStatusId;

                        MessageBox.Show("Статус успешно обновлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить статус.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoadData();
                    }
                }
                else
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData();
            }
        }

        private void DataGridViewRecords_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        #region Обработчики фильтров

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                dtpToDate.Value = dtpFromDate.Value;
            }
            LoadData();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpToDate.Value < dtpFromDate.Value)
            {
                dtpFromDate.Value = dtpToDate.Value;
            }
            LoadData();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => LoadData();
        private void cmbMasterFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();

        #endregion

        /// <summary>
        /// Сброс всех фильтров к значениям по умолчанию
        /// </summary>
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbMasterFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;

            var dateRange = _filterManager.GetDateRange();
            DateTime defaultFrom = dateRange.MaxDate.AddMonths(-1);
            if (defaultFrom < dateRange.MinDate)
            {
                defaultFrom = dateRange.MinDate;
            }

            dtpFromDate.Value = defaultFrom;
            dtpToDate.Value = dateRange.MaxDate;

            LoadData();
        }

        /// <summary>
        /// Возврат в главное меню в зависимости от роли
        /// </summary>
        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2)
            {
                MenuAdmin menuAdmin = new MenuAdmin(_fio);
                menuAdmin.Show();
                this.Hide();
            }
            else if (_roleID == 4)
            {
                Schedule menuManager = new Schedule(_fio, 4, 0);
                menuManager.Show();
                this.Hide();
            }
            else if (_roleID == 1)
            {
                MenuDirector menuManager = new MenuDirector(_fio);
                menuManager.Show();
                this.Hide();
            }
        }

        /// <summary>
        /// Генерация Excel-отчета с текущими данными
        /// </summary>
        private void ReportsButton_Click(object sender, EventArgs e)
        {
            GenerateExcelReport();
        }

        /// <summary>
        /// Создание Excel-отчета с текущими отфильтрованными данными
        /// </summary>
        private void GenerateExcelReport()
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range range = null;

            try
            {
                if (dataGridViewRecords.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования отчета!", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет Excel",
                    FileName = $"Отчет_записей_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string filePath = saveFileDialog.FileName;

                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet as Excel.Worksheet;

                if (worksheet == null)
                {
                    throw new Exception("Не удалось создать рабочий лист Excel");
                }

                worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
                worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);

                Color accentColor = Color.HotPink;
                Color lightPink = Color.FromArgb(255, 203, 219);
                Color lightGray = Color.FromArgb(240, 240, 240);

                // Заголовок
                range = worksheet.Range["A1", "G1"];
                range.Merge();
                range.Value = "ОТЧЕТ О ЗАПИСЯХ";
                range.Font.Size = 18;
                range.Font.Bold = true;
                range.Font.Name = "Arial";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Информация о фильтрах
                int currentRow = 3;

                worksheet.Cells[currentRow, 1] = "📅 Период:";
                worksheet.Cells[currentRow, 2] = $"{dtpFromDate.Value:dd.MM.yyyy} - {dtpToDate.Value:dd.MM.yyyy}";
                FormatInfoCell(worksheet, currentRow, 1, true);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                if (cmbMasterFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "👤 Мастер:";
                    worksheet.Cells[currentRow, 2] = cmbMasterFilter.SelectedItem?.ToString();
                    FormatInfoCell(worksheet, currentRow, 1, true);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                if (cmbStatusFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "📊 Статус:";
                    worksheet.Cells[currentRow, 2] = cmbStatusFilter.SelectedItem?.ToString();
                    FormatInfoCell(worksheet, currentRow, 1, true);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    worksheet.Cells[currentRow, 1] = "🔍 Поиск:";
                    worksheet.Cells[currentRow, 2] = txtSearch.Text;
                    FormatInfoCell(worksheet, currentRow, 1, true);
                    FormatInfoCell(worksheet, currentRow, 2, false);
                    currentRow++;
                }

                worksheet.Cells[currentRow, 1] = "⬆️ Сортировка:";
                worksheet.Cells[currentRow, 2] = cmbSort.SelectedItem?.ToString();
                FormatInfoCell(worksheet, currentRow, 1, true);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "⏱️ Дата формирования:";
                worksheet.Cells[currentRow, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                FormatInfoCell(worksheet, currentRow, 1, true);
                FormatInfoCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "📝 Количество записей:";
                worksheet.Cells[currentRow, 2] = dataGridViewRecords.Rows.Count.ToString();
                FormatInfoCell(worksheet, currentRow, 1, true);

                range = worksheet.Cells[currentRow, 2];
                range.Font.Bold = true;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                currentRow += 2;

                // Заголовки таблицы
                string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена", "Менеджер" };
                int columnCount = headers.Length;

                for (int i = 0; i < columnCount; i++)
                {
                    worksheet.Cells[currentRow, i + 1] = headers[i];
                    range = worksheet.Cells[currentRow, i + 1];
                    range.Font.Bold = true;
                    range.Font.Name = "Arial";
                    range.Font.Size = 11;
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                    range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                }

                currentRow++;

                // Данные
                decimal totalSum = 0;
                for (int i = 0; i < dataGridViewRecords.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridViewRecords.Rows[i];
                    if (row.IsNewRow) continue;

                    string statusName = "";
                    int statusId = 0;

                    for (int s = 0; s < dataGridViewRecords.Columns.Count; s++)
                    {
                        if (dataGridViewRecords.Columns[s].Name == "Status")
                        {
                            if (row.Cells[s].Value != null)
                            {
                                statusId = Convert.ToInt32(row.Cells[s].Value);
                                var statusItem = _statusItems.FirstOrDefault(x => x.ID == statusId);
                                statusName = statusItem?.Name ?? statusId.ToString();
                            }
                            break;
                        }
                    }

                    for (int j = 0; j < dataGridViewRecords.Columns.Count; j++)
                    {
                        object cellValue = row.Cells[j].Value;
                        string columnName = dataGridViewRecords.Columns[j].Name;

                        if (cellValue != null)
                        {
                            if (columnName == "Status")
                            {
                                worksheet.Cells[currentRow, j + 1] = statusName;

                                range = worksheet.Cells[currentRow, j + 1];
                                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                                if (statusName.Contains("Запланирован"))
                                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 245, 157));
                                else if (statusName.Contains("Подтвержден"))
                                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(197, 225, 165));
                                else if (statusName.Contains("Выполнен"))
                                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(225, 225, 225));
                                else if (statusName.Contains("Отменен"))
                                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 171, 145));

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                            }
                            else if (columnName == "Price" || j == 5)
                            {
                                decimal price = 0;

                                if (cellValue is string priceStr)
                                {
                                    string cleanPrice = priceStr.Replace("₽", "").Replace("$", "").Replace("€", "").Replace(" ", "").Trim();
                                    cleanPrice = cleanPrice.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                    cleanPrice = cleanPrice.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                    decimal.TryParse(cleanPrice, out price);
                                }
                                else if (cellValue is decimal || cellValue is double || cellValue is int)
                                {
                                    price = Convert.ToDecimal(cellValue);
                                }

                                if (price > 0)
                                {
                                    totalSum += price;
                                    range = worksheet.Cells[currentRow, j + 1];
                                    range.Value = price;
                                    range.NumberFormat = "#,##0.00";
                                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                                }
                                else
                                {
                                    worksheet.Cells[currentRow, j + 1] = cellValue.ToString();
                                }
                            }
                            else
                            {
                                worksheet.Cells[currentRow, j + 1] = cellValue.ToString();

                                range = worksheet.Cells[currentRow, j + 1];
                                if (columnName == "Date" || j == 2)
                                {
                                    range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                                }
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                            }
                        }
                    }

                    range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, headers.Length]];
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    range.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);

                    if (i % 2 == 1)
                    {
                        range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 250, 250));
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    currentRow++;
                }

                range = worksheet.Columns[8];
                range.ClearContents();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Итог
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
                range.Merge();
                range.Value = "ИТОГО:";
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.Font.Name = "Arial";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                range = worksheet.Cells[currentRow, 6];
                range.Value = totalSum;
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.Font.Name = "Arial";
                range.NumberFormat = "#,##0.00";
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                range = worksheet.Cells[currentRow, 7];
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Автоподбор ширины
                range = worksheet.UsedRange;
                range.Columns.AutoFit();

                if (worksheet.Columns[1].ColumnWidth < 12) worksheet.Columns[1].ColumnWidth = 12;
                if (worksheet.Columns[2].ColumnWidth < 15) worksheet.Columns[2].ColumnWidth = 15;
                if (worksheet.Columns[4].ColumnWidth < 20) worksheet.Columns[4].ColumnWidth = 20;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Сохранение
                workbook.SaveAs(filePath);
                workbook.Close(false);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;

                excelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                excelApp = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                DialogResult result = MessageBox.Show($"Отчет успешно сохранен в файл:\n{filePath}\n\nХотите открыть файл?",
                                                     "Отчет создан",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Excel отчета:\n{ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    if (range != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    if (workbook != null)
                    {
                        workbook.Close(false);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    }
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                    }
                }
                catch { }
                finally
                {
                    range = null;
                    worksheet = null;
                    workbook = null;
                    excelApp = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        /// <summary>
        /// Расчет общей выручки (исключая отмененные записи)
        /// </summary>
        private decimal CalculateTotalRevenue(List<RecordData> records)
        {
            decimal total = 0;
            foreach (var record in records)
            {
                if (record.StatusID != 4)
                {
                    total += record.Price;
                }
            }
            return total;
        }

        /// <summary>
        /// Форматирование информационных ячеек в Excel
        /// </summary>
        private void FormatInfoCell(Excel.Worksheet worksheet, int row, int col, bool isLabel)
        {
            Excel.Range range = worksheet.Cells[row, col];

            if (isLabel)
            {
                range.Font.Bold = true;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.HotPink);
            }

            range.Font.Name = "Arial";
            range.Font.Size = 10;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
        }

        private void ReleaseExcelObjects(object worksheet, object workbook, object excelApp)
        {
            try
            {
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                if (excelApp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
            catch { }
            finally
            {
                worksheet = null;
                workbook = null;
                excelApp = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}