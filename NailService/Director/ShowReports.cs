using System;
using System.Windows.Forms;
using NailServiceApp.Data;
using NailServiceApp.Styles;
using System.IO;
using System.Data;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;

namespace NailService
{
    public partial class ShowReports : Form
    {
        private string _fio;
        private FilterManager _filterManager;
        private DateTime _minDate;
        private DateTime _maxDate;

        public ShowReports(string FIO)
        {
            InitializeComponent();
            _fio = FIO;

            _filterManager = new FilterManager(Connection.ConnectionString);
            _filterManager.PopulateMastersComboBox(cmbMasterFilter);
            _filterManager.PopulateStatusComboBox(cmbStatusFilter);

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
            SetupDataGridView();
            LoadData();

            FIOlabel.Text = $"Директор: {_fio}";
        }

        private void SetupDateTimePickers()
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
            dtpFromDate.ShowUpDown = false; // ВКЛЮЧАЕМ КАЛЕНДАРЬ
            dtpFromDate.ShowCheckBox = false;

            dtpToDate.Format = DateTimePickerFormat.Custom;
            dtpToDate.CustomFormat = "dd.MM.yyyy";
            dtpToDate.ShowUpDown = false; // ВКЛЮЧАЕМ КАЛЕНДАРЬ
            dtpToDate.ShowCheckBox = false;
        }

        private void SetupDataGridView()
        {
            // Полностью очищаем и настраиваем DataGridView
            dataGridViewRecords.Columns.Clear();
            dataGridViewRecords.Rows.Clear();
            dataGridViewRecords.DataSource = null;

            // Создаем колонки
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "MasterName", HeaderText = "Мастер" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClientName", HeaderText = "Клиент" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Дата и время" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Статус" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Услуга" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Цена" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "Менеджер" });

            StyleManager.ApplyGridStyles(dataGridViewRecords);
            StyleManager.ApplyColumnAlignments(dataGridViewRecords);
        }

        private void LoadData()
        {
            try
            {
                if (dataGridViewRecords.Columns.Count == 0)
                {
                    SetupDataGridView();
                }

                string searchText = txtSearch.Text;
                string masterFilter = cmbMasterFilter.SelectedItem?.ToString() ?? "Все мастера";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                // Проверяем корректность дат
                if (fromDate > toDate)
                {
                    // СВАПИВАЕМ ДАТЫ МЕСТАМИ
                    MessageBox.Show("Дата 'С' не может быть больше даты 'По'. Даты будут автоматически изменены местами.",
                                  "Коррекция дат",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);

                    // Меняем даты местами
                    DateTime temp = fromDate;
                    fromDate = toDate;
                    toDate = temp;

                    // Обновляем значения в контролах
                    dtpFromDate.Value = fromDate;
                    dtpToDate.Value = toDate;

                    // Не выходим из метода - продолжаем загрузку с исправленными датами
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

                // ПЕРЕДАЕМ statusFilter В МЕТОД
                var records = _filterManager.GetFilteredRecords(searchText, masterFilter, statusFilter, fromDate, toDate, sortBy, ascending);

                dataGridViewRecords.Rows.Clear();
                foreach (var record in records)
                {
                    int rowIndex = dataGridViewRecords.Rows.Add(
                        record.MasterName,
                        record.ClientName,
                        record.Date.ToString("dd.MM.yyyy HH:mm"),
                        record.Status,
                        record.Service,
                        record.Price.ToString("C0"),
                        record.UserName
                    );

                    if (rowIndex >= 0 && rowIndex < dataGridViewRecords.Rows.Count)
                    {
                        dataGridViewRecords.Rows[rowIndex].Cells[3].Style.BackColor =
                            StyleManager.GetStatusColor(record.StatusID);
                    }
                }

                lblRecordCount.Text = $"Найдено записей: {records.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчики событий для дат - проверяем корректность при каждом изменении
        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            // Проверяем, чтобы дата "С" не была больше даты "По"
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                // Автоматически корректируем дату "По"
                dtpToDate.Value = dtpFromDate.Value;
            }
            LoadData();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            // Проверяем, чтобы дата "По" не была меньше даты "С"
            if (dtpToDate.Value < dtpFromDate.Value)
            {
                // Автоматически корректируем дату "С"
                dtpFromDate.Value = dtpToDate.Value;
            }
            LoadData();
        }

        // Остальные обработчики событий
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

            // Устанавливаем даты за последний месяц
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
            if (!string.IsNullOrEmpty(_fio))
                new MenuDirector(_fio).Show();
            else
                new Schedule().Show();
            this.Hide();
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