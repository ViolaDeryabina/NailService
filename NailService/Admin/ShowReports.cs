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
        private bool _isCentered = false;

        // Переменные для пагинации
        private int currentPage = 1;
        private int pageSize = 20;
        private int totalRecords = 0;
        private int totalPages = 0;
        private List<RecordData> _allFilteredRecords; // Кэш всех отфильтрованных записей

        /// <summary>
        /// Конструктор формы отчетов
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        /// <param name="RoleID">ID роли (2-админ, 4-менеджер)</param>
        public ShowReports(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;

            // Включаем изменение размера окна

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
        "Цена (по убыванию)",
        "Мастер (А-Я)",
        "Мастер (Я-А)",
        "Клиент (А-Я)",
        "Клиент (Я-А)"
    });
            cmbSort.SelectedIndex = 0;

            SetupDateTimePickers();
            InitializePagination();

            LoadData();
            FIOlabel.Text = RoleID == 2 ? $"Админ: {_fio}" : "";
        }

       
        /// <summary>
        /// Инициализация элементов управления пагинацией
        /// </summary>
        private void InitializePagination()
        {
            // Настраиваем кнопки навигации
            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;
            btnFirst.Click += BtnFirst_Click;
            btnLast.Click += BtnLast_Click;
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
        /// Загрузка данных с применением текущих фильтров и пагинацией
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

                // Получаем все отфильтрованные записи
                _allFilteredRecords = _filterManager.GetFilteredRecords(searchText, masterFilter, statusFilter,
                    fromDate, toDate, sortBy, ascending);

                // Обновляем общее количество записей
                totalRecords = _allFilteredRecords.Count;

                // Рассчитываем общее количество страниц
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (totalPages == 0) totalPages = 1;

                // Корректируем текущую страницу
                if (currentPage > totalPages)
                {
                    currentPage = totalPages;
                }
                if (currentPage < 1)
                {
                    currentPage = 1;
                }

                // Получаем записи для текущей страницы
                var pagedRecords = _allFilteredRecords
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Заполняем DataGridView
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

                // Рассчитываем общую выручку по ВСЕМ отфильтрованным записям
                decimal totalRevenue = CalculateTotalRevenue(_allFilteredRecords);
                lblTotalRevenue.Text = $"Общая выручка: {totalRevenue:N0} руб.";

                // Обновляем информацию о пагинации
                UpdatePaginationInfo();

                // Обновляем кнопки навигации
                UpdateNavigationButtons();

                // Создаем кнопки для номеров страниц
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

        /// <summary>
        /// Обновление информации о пагинации (сколько записей показано из общего количества)
        /// </summary>
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

        /// <summary>
        /// Обновление состояния кнопок навигации
        /// </summary>
        private void UpdateNavigationButtons()
        {
            btnFirst.Enabled = currentPage > 1;
            btnPrev.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < totalPages;
            btnLast.Enabled = currentPage < totalPages;
        }

        /// <summary>
        /// Создание кнопок для перехода по страницам
        /// </summary>
        private void CreatePageButtons()
        {
           // flowLayoutPanelPages.Controls.Clear();

            if (totalPages <= 1) return;

            // Определяем диапазон отображаемых страниц (максимум 10 кнопок)
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

        /// <summary>
        /// Обработчик клика по кнопке страницы
        /// </summary>
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

        /// <summary>
        /// Обработчик кнопки "Первая страница"
        /// </summary>
        private void BtnFirst_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage = 1;
                LoadData();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Предыдущая страница"
        /// </summary>
        private void BtnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadData();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Следующая страница"
        /// </summary>
        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadData();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Последняя страница"
        /// </summary>
        private void BtnLast_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage = totalPages;
                LoadData();
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

                        // Обновляем данные в кэше
                        var record = _allFilteredRecords.FirstOrDefault(r => r.RecordID == recordId);
                        if (record != null)
                        {
                            record.StatusID = newStatusId;
                        }

                        // Пересчитываем общую выручку
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

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                dtpToDate.Value = dtpFromDate.Value;
            }
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if (dtpToDate.Value < dtpFromDate.Value)
            {
                dtpFromDate.Value = dtpToDate.Value;
            }
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

        private void cmbMasterFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1; // Сбрасываем на первую страницу при изменении фильтра
            LoadData();
        }

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

            currentPage = 1; // Сбрасываем на первую страницу
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
        }

        /// <summary>
        /// Генерация Excel-отчета с текущими данными
        /// </summary>
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
                // В методе ReportsButton_Click, там где создаёте PDF генератор:
                else if (format == "PDF")
                {
                    decimal totalRevenue = CalculateTotalRevenue(_allFilteredRecords);

                    var pdfGenerator = new PdfReportGenerator(
                        dtpFromDate,
                        dtpToDate,
                        cmbMasterFilter,
                        cmbStatusFilter,
                        cmbSort,
                        txtSearch,
                        totalRecords,
                        _allFilteredRecords,    // список записей
                        _statusItems,           // список статусов
                        totalRevenue);
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


        private void ShowReports_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }

        /// <summary>
        /// Обработчик изменения размера формы - пересчитывает позиции элементов
        /// </summary>

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

            // Растягиваем элементы по ширине
            groupBox1.Width = w - 24;
            groupBox2.Width = w - 24;
            dataGridViewRecords.Width = w - 24;
            dataGridViewRecords.Height = h - 340;

            // Центрируем заголовок
            label1.Location = new Point((w - 342) / 2, 10);
            FIOlabel.Location = new Point(w - 200, 12);

            // Кнопка "В меню" в левом нижнем углу
            InMenu.Location = new Point(12, h - 60);

            // Кнопка "Сформировать отчёт" в правом нижнем углу
            ReportsButton.Location = new Point(w - 275, h - 60);

            // Общая выручка - над кнопкой "В меню"
            lblTotalRevenue.Location = new Point(12, h - 105);

            // ===== ПАГИНАЦИЯ (слева, над lblTotalRevenue) =====
            int paginationY = h - 130;

            // Информация о записях (слева)
            lblPaginationInfo.Location = new Point(12, paginationY);

        
            // Кнопки пагинации (справа)
            int btnY = h - 120;
            btnLast.Location = new Point(w - 60, btnY);
            btnNext.Location = new Point(w - 115, btnY);
            btnPrev.Location = new Point(w - 220, btnY);
            btnFirst.Location = new Point(w - 275, btnY);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Устанавливаем начальную позицию по центру
            this.StartPosition = FormStartPosition.CenterScreen;
            this.CenterToScreen();
        }
    }
}