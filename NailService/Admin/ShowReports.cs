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
        private bool _isCentered = false;

        // Переменные для пагинации
        private int currentPage = 1;
        private int pageSize = 20;
        private int totalRecords = 0;
        private int totalPages = 0;
        private List<RecordData> _allFilteredRecords;

        public ShowReports(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;

            _filterManager = new FilterManager(Connection.ConnectionString);

            SetupDataGridView();
            _filterManager.PopulateMastersComboBox(cmbMasterFilter);
            _filterManager.PopulateStatusComboBox(cmbStatusFilter);

            _statusItems = _filterManager.GetStatusItems();

            cmbSort.Items.Clear();
            cmbSort.Items.AddRange(new string[] {
                "Цена (по возрастанию)",
                "Цена (по убыванию)"
            });
            cmbSort.SelectedIndex = 0;

            SetupDateTimePickers();
            InitializePagination();

            LoadData();
            FIOlabel.Text = RoleID == 2 ? $"Админ: {_fio}" : "";
        }

        private void InitializePagination()
        {
            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;
            btnFirst.Click += BtnFirst_Click;
            btnLast.Click += BtnLast_Click;
        }

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

                dtpToDate.Format = DateTimePickerFormat.Custom;
                dtpToDate.CustomFormat = "dd.MM.yyyy";
                dtpToDate.ShowUpDown = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке дат: {ex.Message}");
            }
        }

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
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                });

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Date",
                    HeaderText = "Дата и время",
                    ReadOnly = true
                });

                _statusItems = _filterManager.GetStatusItems();

                dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Статус",
                    ReadOnly = true
                });

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

        private void DataGridViewRecords_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                try
                {
                    var cell = dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (cell.Tag != null)
                    {
                        int statusId = Convert.ToInt32(cell.Tag);
                        e.CellStyle.BackColor = StyleManager.GetStatusColor(statusId);
                    }
                    else if (cell.Value != null)
                    {
                        // Пытаемся найти статус по названию
                        string statusName = cell.Value.ToString();
                        var statusItem = _statusItems?.FirstOrDefault(s => s.Name == statusName);
                        if (statusItem != null)
                        {
                            e.CellStyle.BackColor = StyleManager.GetStatusColor(statusItem.ID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка форматирования: {ex.Message}");
                }
            }
        }

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
                    }
                }

                _allFilteredRecords = _filterManager.GetFilteredRecords(searchText, masterFilter, statusFilter,
                    fromDate, toDate, sortBy, ascending);

                totalRecords = _allFilteredRecords.Count;
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (totalPages == 0) totalPages = 1;

                if (currentPage > totalPages) currentPage = totalPages;
                if (currentPage < 1) currentPage = 1;

                var pagedRecords = _allFilteredRecords
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

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

                foreach (var record in pagedRecords)
                {
                    int rowIndex = dataGridViewRecords.Rows.Add(                                  
                        record.MasterName,
                        record.ClientName,
                        record.Date.ToString("dd.MM.yyyy HH:mm"),
                        record.Status,
                        record.Service,
                        record.Price.ToString("C0"),
                        record.UserName,
                        record.RecordID
                    );

                    if (rowIndex >= 0)
                    {
                        dataGridViewRecords.Rows[rowIndex].Cells["Status"].Tag = record.StatusID;
                    }
                }

                decimal totalRevenue = CalculateTotalRevenue(_allFilteredRecords);
                lblTotalRevenue.Text = $"Общая выручка: {totalRevenue:N0} руб.";

                UpdatePaginationInfo();
                UpdateNavigationButtons();
                CreatePageButtons();
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

        private void UpdatePaginationInfo()
        {
            int startRecord = ((currentPage - 1) * pageSize) + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);

            if (totalRecords == 0)
            {
                startRecord = 0;
                endRecord = 0;
            }

            lblPaginationInfo.Text = $"Показано: {startRecord}-{endRecord} из {totalRecords} записей | Страница: {currentPage} из {totalPages}";
        }

        private void UpdateNavigationButtons()
        {
            btnFirst.Enabled = currentPage > 1;
            btnPrev.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < totalPages;
            btnLast.Enabled = currentPage < totalPages;
        }

        private void CreatePageButtons()
        {
            // flowLayoutPanelPages.Controls.Clear();

            if (totalPages <= 1) return;

            int startPage = Math.Max(1, currentPage - 4);
            int endPage = Math.Min(totalPages, startPage + 9);

            if (endPage - startPage < 9)
            {
                startPage = Math.Max(1, endPage - 9);
            }

            for (int i = startPage; i <= endPage; i++)
            {
                Button btnPage = new Button
                {
                    Text = i.ToString(),
                    Width = 35,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = (i == currentPage) ? Color.HotPink : Color.LightGray,
                    ForeColor = (i == currentPage) ? Color.White : Color.Black,
                    Font = new Font("Arial", 10, (i == currentPage) ? FontStyle.Bold : FontStyle.Regular),
                    Tag = i
                };
                btnPage.Click += BtnPage_Click;
                // flowLayoutPanelPages.Controls.Add(btnPage);
            }
        }

        private void BtnPage_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int page = (int)btn.Tag;
                if (page != currentPage)
                {
                    currentPage = page;
                    LoadData();
                }
            }
        }

        private void BtnFirst_Click(object sender, EventArgs e) { if (currentPage > 1) { currentPage = 1; LoadData(); } }
        private void BtnPrev_Click(object sender, EventArgs e) { if (currentPage > 1) { currentPage--; LoadData(); } }
        private void BtnNext_Click(object sender, EventArgs e) { if (currentPage < totalPages) { currentPage++; LoadData(); } }
        private void BtnLast_Click(object sender, EventArgs e) { if (currentPage < totalPages) { currentPage = totalPages; LoadData(); } }

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

        private void DataGridViewRecords_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name != "Status") return;

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

                if (oldStatusId == newStatusId) return;

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

                        var record = _allFilteredRecords.FirstOrDefault(r => r.RecordID == recordId);
                        if (record != null)
                        {
                            record.StatusID = newStatusId;
                        }

                        decimal totalRevenue = CalculateTotalRevenue(_allFilteredRecords);
                        lblTotalRevenue.Text = $"Общая выручка: {totalRevenue:N0} руб.";
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
        private void dtpFromDate_ValueChanged(object sender, EventArgs e) { if (dtpFromDate.Value > dtpToDate.Value) dtpToDate.Value = dtpFromDate.Value; currentPage = 1; LoadData(); }
        private void dtpToDate_ValueChanged(object sender, EventArgs e) { if (dtpToDate.Value < dtpFromDate.Value) dtpFromDate.Value = dtpToDate.Value; currentPage = 1; LoadData(); }
        private void txtSearch_TextChanged(object sender, EventArgs e) { currentPage = 1; LoadData(); }
        private void cmbMasterFilter_SelectedIndexChanged(object sender, EventArgs e) { currentPage = 1; LoadData(); }
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) { currentPage = 1; LoadData(); }
        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) { currentPage = 1; LoadData(); }
        #endregion

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbMasterFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;

            var dateRange = _filterManager.GetDateRange();
            DateTime defaultFrom = dateRange.MaxDate.AddMonths(-1);
            if (defaultFrom < dateRange.MinDate) defaultFrom = dateRange.MinDate;

            dtpFromDate.Value = defaultFrom;
            dtpToDate.Value = dateRange.MaxDate;

            currentPage = 1;
            LoadData();
        }

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
        }

        private void ReportsButton_Click(object sender, EventArgs e)
        {
            if (dataGridViewRecords.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования отчета!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string format = ReportHelper.ShowFormatDialog();
            if (string.IsNullOrEmpty(format)) return;

            string filePath = ReportHelper.ShowSaveFileDialog(
                format == "Excel" ? "Excel Files|*.xlsx" : "PDF Files|*.pdf",
                $"Сохранить отчет {format}",
                format == "Excel" ? "xlsx" : "pdf");

            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                if (format == "Excel")
                {
                    var excelGenerator = new ExcelReportGenerator(
                        dtpFromDate, dtpToDate, cmbMasterFilter, cmbStatusFilter, cmbSort, txtSearch,
                        totalRecords, _allFilteredRecords, _statusItems);
                    excelGenerator.Generate(filePath);
                }
                else if (format == "PDF")
                {
                    decimal totalRevenue = CalculateTotalRevenue(_allFilteredRecords);

                    var pdfGenerator = new PdfReportGenerator(
                        dtpFromDate, dtpToDate, cmbMasterFilter, cmbStatusFilter, cmbSort, txtSearch,
                        totalRecords, _allFilteredRecords, _statusItems, totalRevenue);
                    pdfGenerator.Generate(filePath);
                }

                ReportHelper.OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета:\n{ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Расчет общей выручки
        /// </summary>
        private decimal CalculateTotalRevenue(List<RecordData> records)
        {
            decimal total = 0;
            foreach (var record in records)
            {
                if (record.StatusID != 3) // 3 - Отменено
                {
                    total += record.Price;
                }
            }
            return total;
        }
        private void ShowReports_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Normal && !_isCentered)
            {
                this.CenterToScreen();
                _isCentered = true;
            }

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            groupBox1.Width = w - 24;
            groupBox2.Width = w - 24;
            dataGridViewRecords.Width = w - 24;
            dataGridViewRecords.Height = h - 340;

            label1.Location = new Point((w - 342) / 2, 10);
            FIOlabel.Location = new Point(w - 200, 12);

            InMenu.Location = new Point(12, h - 60);
            button1.Location = new Point(w - 575, h - 60);
            ReportsButton.Location = new Point(w - 275, h - 60);
            lblTotalRevenue.Location = new Point(12, h - 105);

            int paginationY = h - 130;
            lblPaginationInfo.Location = new Point(12, paginationY);

            int btnY = h - 120;
            btnLast.Location = new Point(w - 60, btnY);
            btnNext.Location = new Point(w - 115, btnY);
            btnPrev.Location = new Point(w - 220, btnY);
            btnFirst.Location = new Point(w - 275, btnY);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.CenterToScreen();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем статистику по услугам
                var serviceStats = GetServiceStatistics();

                if (serviceStats == null || serviceStats.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования статистики!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string formatResult = ReportHelper.ShowFormatDialog();
                if (string.IsNullOrEmpty(formatResult)) return;

                // Настройка SaveFileDialog напрямую
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = $"Сохранить отчет со статистикой ({formatResult})";

                // Правильная строка фильтра
                if (formatResult == "Excel")
                {
                    saveDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx";
                    saveDialog.DefaultExt = "xlsx";
                    saveDialog.FileName = $"Статистика_услуг_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
                else // PDF
                {
                    saveDialog.Filter = "PDF файлы (*.pdf)|*.pdf";
                    saveDialog.DefaultExt = "pdf";
                    saveDialog.FileName = $"Статистика_услуг_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                }

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveDialog.FileName;

                    if (formatResult == "Excel")
                    {
                        // Убедимся, что расширение файла .xlsx
                        if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                            filePath += ".xlsx";

                        var generator = new ExcelReportGenerator(null, null, null, null, null, null, 0, null, null);
                        generator.GenerateServiceStatistics(filePath, serviceStats);
                    }
                    else // PDF
                    {
                        // Убедимся, что расширение файла .pdf
                        if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                            filePath += ".pdf";

                        var pdfGenerator = new PdfReportGenerator(
                            null, null, null, null, null, null, 0, null, null, 0, serviceStats);
                        pdfGenerator.GenerateServiceStatistics(filePath, serviceStats);
                    }

                    MessageBox.Show($"Отчет успешно сохранен в формате {formatResult}!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (MessageBox.Show("Открыть сохраненный файл?", "Открыть файл",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<ServiceStatistic> GetServiceStatistics()
        {
            var statistics = new List<ServiceStatistic>();

            using (var connection = new MySqlConnection(Connection.ConnectionString))
            {
                connection.Open();
                string query = @"
                SELECT 
                s.ServiceName,
                COUNT(r.IDRecord) as Count,
                SUM(s.Price) as Revenue
                FROM Record r
                INNER JOIN Services s ON r.Service = s.IDServices
                WHERE r.Status != 3
                GROUP BY s.ServiceName, s.IDServices
                ORDER BY COUNT(r.IDRecord) DESC;";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statistics.Add(new ServiceStatistic
                        {
                            ServiceName = reader["ServiceName"].ToString(),
                            Count = Convert.ToInt32(reader["Count"]),
                            Revenue = Convert.ToDecimal(reader["Revenue"])
                        });
                    }
                }
            }

            return statistics;
        }
    }
}