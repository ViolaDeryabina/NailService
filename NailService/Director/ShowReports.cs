using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace NailService
{
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

        public ShowReports(string FIO, int RoleID)
        {
            InitializeComponent();
            _fio = FIO;
            _roleID = RoleID;

            _filterManager = new FilterManager(Connection.ConnectionString);

            // Сначала настраиваем DataGridView
            SetupDataGridView();

            // Затем загружаем данные для комбобоксов
            _filterManager.PopulateMastersComboBox(cmbMasterFilter);
            _filterManager.PopulateStatusComboBox(cmbStatusFilter);

            // Загружаем статусы для ComboBox в DataGridView
            _statusItems = _filterManager.GetStatusItems();

            // Настройка сортировки
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

            // Загружаем данные только после полной настройки
            LoadData();
            if (RoleID == 1)
            {
                FIOlabel.Text = $"Директор: {_fio}";
            }else if (RoleID == 2)
            {
                FIOlabel.Text = $"Администратор: {_fio}";
            }
           
        }

        private void SetupDateTimePickers()
        {
            try
            {
                // Получаем минимальную и максимальную даты из базы данных
                var dateRange = _filterManager.GetDateRange();
                _minDate = dateRange.MinDate;
                _maxDate = dateRange.MaxDate;

                // Устанавливаем ограничения
                dtpFromDate.MinDate = _minDate;
                dtpFromDate.MaxDate = _maxDate;
                dtpToDate.MinDate = _minDate;
                dtpToDate.MaxDate = _maxDate;

                // Устанавливаем значения по умолчанию (последний месяц)
                DateTime defaultFrom = _maxDate.AddMonths(-1);
                if (defaultFrom < _minDate)
                {
                    defaultFrom = _minDate;
                }

                dtpFromDate.Value = defaultFrom;
                dtpToDate.Value = _maxDate;

                // НАСТРОЙКА ДЛЯ КАЛЕНДАРЯ
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

        private void SetupDataGridView()
        {
            try
            {
                // Очищаем колонки
                dataGridViewRecords.Columns.Clear();

                // Добавляем колонки
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

                // Загружаем статусы
                _statusItems = _filterManager.GetStatusItems();

                // Колонка статуса - сразу ComboBox
                DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Статус",
                    DataSource = new BindingSource(_statusItems, null),
                    DisplayMember = "Name",     // Отображаем название
                    ValueMember = "ID",          // Храним ID
                    FlatStyle = FlatStyle.Flat,
                    ReadOnly = (_roleID != 2),   // Только админ может редактировать
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

                // Скрытая колонка для ID записи
                DataGridViewTextBoxColumn idColumn = new DataGridViewTextBoxColumn
                {
                    Name = "RecordID",
                    HeaderText = "ID",
                    Visible = false
                };
                dataGridViewRecords.Columns.Add(idColumn);

                // Применяем стили
                StyleManager.ApplyGridStyles(dataGridViewRecords);
                StyleManager.ApplyColumnAlignments(dataGridViewRecords);
                dataGridViewRecords.ReadOnly = false;

                // Подписываемся на события
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
            // Для колонки статуса устанавливаем цвет фона
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name == "Status" && e.RowIndex >= 0)
            {
                if (dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    int statusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    e.CellStyle.BackColor = StyleManager.GetStatusColor(statusId);
                }
            }
        }

        private void LoadData()
        {
            try
            {
                // Проверяем, есть ли колонки
                if (dataGridViewRecords.Columns.Count == 0)
                {
                    MessageBox.Show("DataGridView не настроен. Сначала выполните настройку.");
                    return;
                }

                // Отключаем обработчик событий на время загрузки
                dataGridViewRecords.CellValueChanged -= DataGridViewRecords_CellValueChanged;

                dataGridViewRecords.Rows.Clear();

                string searchText = txtSearch.Text;
                string masterFilter = cmbMasterFilter.SelectedItem?.ToString() ?? "Все мастера";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                // Проверка дат...
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

                // Находим индексы колонок
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
                        record.StatusID, // Для ComboBox передаем ID статуса - он сам отобразит название
                        record.Service,
                        record.Price.ToString("C0"),
                        record.UserName,
                        record.RecordID
                    );

                    if (rowIndex >= 0)
                    {
                        // Сохраняем значение в Tag для отслеживания изменений
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
                // Включаем обработчик событий обратно
                dataGridViewRecords.CellValueChanged += DataGridViewRecords_CellValueChanged;
            }
        }


        private void DataGridViewRecords_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Разрешаем редактирование только администраторам (RoleID = 2) и только для колонки статуса
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name != "Status")
            {
                e.Cancel = true;
                return;
            }

            if (_roleID != 2)
            {
                e.Cancel = true;

                // Используем флаг, чтобы показать сообщение только один раз
                if (!_isErrorMessageShown)
                {
                    _isErrorMessageShown = true;
                    MessageBox.Show("Только администратор может изменять статусы записей.",
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Сбрасываем флаг через таймер или после закрытия сообщения
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

            // Проверяем, что это колонка статуса
            if (dataGridViewRecords.Columns[e.ColumnIndex].Name != "Status")
                return;

            try
            {
                // Находим индекс колонки с ID записи
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

                // Получаем ID записи из скрытой колонки
                int recordId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[recordIdColumnIndex].Value);

                // Получаем новый статус (ID)
                int newStatusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                // Получаем старое значение статуса для проверки
                int oldStatusId = 0;
                if (dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag != null)
                {
                    oldStatusId = Convert.ToInt32(dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag);
                }

                // Если статус не изменился, ничего не делаем
                if (oldStatusId == newStatusId)
                    return;

                // Запрашиваем подтверждение
                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите изменить статус этой записи?",
                    "Подтверждение изменения",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Обновляем статус в базе данных
                    bool updated = _filterManager.UpdateRecordStatus(recordId, newStatusId);

                    if (updated)
                    {
                        // Обновляем цвет ячейки
                        dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor =
                            StyleManager.GetStatusColor(newStatusId);

                        // Сохраняем новое значение в Tag
                        dataGridViewRecords.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag = newStatusId;

                        MessageBox.Show("Статус успешно обновлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить статус.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Возвращаем старое значение
                        LoadData();
                    }
                }
                else
                {
                    // Отменяем изменение - возвращаем старое значение
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
            // Игнорируем ошибки DataGridView
            e.Cancel = true;
        }

        // Обработчики событий фильтров
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
                Schedule menuManager = new Schedule(_fio, 4,0);
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

        private void ReportsButton_Click(object sender, EventArgs e)
        {
            GenerateExcelReport();
        }


// Создание отчёта в Excel
private void GenerateExcelReport()
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range range = null;

            try
            {
                // Проверяем, есть ли данные для отчета
                if (dataGridViewRecords.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования отчета!", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Создаем диалог сохранения файла
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

                // Создаем приложение Excel
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                // Создаем новую книгу
                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet as Excel.Worksheet;

                if (worksheet == null)
                {
                    throw new Exception("Не удалось создать рабочий лист Excel");
                }

                // Заголовок отчета (строка 1)
                range = worksheet.Range["A1", "G1"];
                range.Merge();
                range.Value = "ОТЧЕТ О ЗАПИСЯХ";
                range.Font.Size = 16;
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightSkyBlue);

                // Сброс reference
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Информация о фильтрах (начиная со строки 3)
                int currentRow = 3;

                // Период
                worksheet.Cells[currentRow, 1] = "Период:";
                worksheet.Cells[currentRow, 2] = $"{dtpFromDate.Value:dd.MM.yyyy} - {dtpToDate.Value:dd.MM.yyyy}";
                currentRow++;

                // Мастер (если выбран)
                if (cmbMasterFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "Мастер:";
                    worksheet.Cells[currentRow, 2] = cmbMasterFilter.SelectedItem?.ToString();
                    currentRow++;
                }

                // Статус (если выбран)
                if (cmbStatusFilter.SelectedIndex > 0)
                {
                    worksheet.Cells[currentRow, 1] = "Статус:";
                    worksheet.Cells[currentRow, 2] = cmbStatusFilter.SelectedItem?.ToString();
                    currentRow++;
                }

                // Поисковый запрос (если есть)
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    worksheet.Cells[currentRow, 1] = "Поиск:";
                    worksheet.Cells[currentRow, 2] = txtSearch.Text;
                    currentRow++;
                }

                // Сортировка
                worksheet.Cells[currentRow, 1] = "Сортировка:";
                worksheet.Cells[currentRow, 2] = cmbSort.SelectedItem?.ToString();
                currentRow++;

                // Дата формирования
                worksheet.Cells[currentRow, 1] = "Дата формирования:";
                worksheet.Cells[currentRow, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                currentRow++;

                // Количество записей
                worksheet.Cells[currentRow, 1] = "Количество записей:";
                worksheet.Cells[currentRow, 2] = dataGridViewRecords.Rows.Count.ToString();
                currentRow += 2; // Пропускаем строку

                // Заголовки таблицы
                string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена", "Менеджер" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1] = headers[i];

                    range = worksheet.Cells[currentRow, i + 1];
                    range.Font.Bold = true;
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    range = null;
                }

                // Автоматическая ширина столбцов для заголовков
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, headers.Length]];
                range.Columns.AutoFit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                currentRow++;

                // Данные из DataGridView
                decimal totalSum = 0;
                for (int i = 0; i < dataGridViewRecords.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridViewRecords.Rows[i];

                    // Пропускаем последнюю строку, если она пустая (автоматически добавляется в DataGridView)
                    if (row.IsNewRow) continue;

                    for (int j = 0; j < dataGridViewRecords.Columns.Count; j++)
                    {
                        object cellValue = row.Cells[j].Value;
                        if (cellValue != null)
                        {
                            // Для столбца "Цена" вычисляем сумму
                            if (j == 5) // Индекс столбца "Цена" (нумерация с 0)
                            {
                                if (cellValue is string priceStr)
                                {
                                    // Убираем символ валюты и пробелы
                                    string cleanPrice = priceStr.Replace("₽", "").Replace("$", "").Replace("€", "").Replace(" ", "").Trim();
                                    cleanPrice = cleanPrice.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                    cleanPrice = cleanPrice.Replace(",", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                                    if (decimal.TryParse(cleanPrice, out decimal price))
                                    {
                                        totalSum += price;
                                        range = worksheet.Cells[currentRow, j + 1];
                                        range.Value = price;
                                        range.NumberFormat = "#,##0.00";

                                        System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                                        range = null;
                                    }
                                    else
                                    {
                                        worksheet.Cells[currentRow, j + 1] = cellValue.ToString();
                                    }
                                }
                                else if (cellValue is decimal || cellValue is double || cellValue is int)
                                {
                                    decimal price = Convert.ToDecimal(cellValue);
                                    totalSum += price;
                                    range = worksheet.Cells[currentRow, j + 1];
                                    range.Value = price;
                                    range.NumberFormat = "#,##0.00";

                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                                    range = null;
                                }
                                else
                                {
                                    worksheet.Cells[currentRow, j + 1] = cellValue.ToString();
                                }
                            }
                            else
                            {
                                worksheet.Cells[currentRow, j + 1] = cellValue.ToString();
                            }
                        }
                    }

                    // Добавляем границы для строки
                    range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, headers.Length]];
                    range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    range = null;

                    currentRow++;
                }

                // Итоговая строка
                worksheet.Cells[currentRow, 1] = "ИТОГО:";

                // Объединяем ячейки для "ИТОГО" (A-E)
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
                range.Merge();
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Итоговая сумма в столбце F
                range = worksheet.Cells[currentRow, 6];
                range.Value = totalSum;
                range.NumberFormat = "#,##0.00";
                range.Font.Bold = true;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Форматируем итоговую строку
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 6]];
                range.Font.Bold = true;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGreen);
                range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Автоматическая ширина всех столбцов
                range = worksheet.UsedRange;
                range.Columns.AutoFit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                range = null;

                // Добавляем автофильтр
                if (dataGridViewRecords.Rows.Count > 0)
                {
                    int dataStartRow = currentRow - dataGridViewRecords.Rows.Count;
                    int dataEndRow = currentRow - 1;

                    range = worksheet.Range[worksheet.Cells[dataStartRow, 1], worksheet.Cells[dataEndRow, headers.Length]];
                    range.AutoFilter(1, Type.Missing, Excel.XlAutoFilterOperator.xlAnd, Type.Missing, true);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    range = null;
                }

                // Сохраняем файл
                workbook.SaveAs(filePath);
                workbook.Close(false);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;

                excelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                excelApp = null;

                // Принудительный сбор мусора
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Показываем сообщение об успехе
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

                // Освобождаем ресурсы в случае ошибки
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