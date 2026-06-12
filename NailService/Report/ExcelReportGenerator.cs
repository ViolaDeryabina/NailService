using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace NailService
{
    /// <summary>
    /// Класс для генерации Excel отчетов
    /// </summary>
    public class ExcelReportGenerator
    {
        private readonly DateTimePicker _dtpFromDate;
        private readonly DateTimePicker _dtpToDate;
        private readonly ComboBox _cmbMasterFilter;
        private readonly ComboBox _cmbStatusFilter;
        private readonly ComboBox _cmbSort;
        private readonly TextBox _txtSearch;
        private readonly int _totalRecords;
        private readonly List<RecordData> _records;
        private readonly List<StatusItem> _statusItems;

        // Цвета приложения (нежно-розовая гамма)
        private readonly Color AppAccentColor = Color.FromArgb(255, 105, 180); // HotPink
        private readonly Color AppLightColor = Color.FromArgb(255, 218, 224);   // Нежно-розовый
        private readonly Color AppHeaderColor = Color.FromArgb(255, 182, 193);  // Светло-розовый

        public ExcelReportGenerator(
            DateTimePicker dtpFromDate,
            DateTimePicker dtpToDate,
            ComboBox cmbMasterFilter,
            ComboBox cmbStatusFilter,
            ComboBox cmbSort,
            TextBox txtSearch,
            int totalRecords,
            List<RecordData> records,
            List<StatusItem> statusItems)
        {
            _dtpFromDate = dtpFromDate;
            _dtpToDate = dtpToDate;
            _cmbMasterFilter = cmbMasterFilter;
            _cmbStatusFilter = cmbStatusFilter;
            _cmbSort = cmbSort;
            _txtSearch = txtSearch;
            _totalRecords = totalRecords;
            _records = records;
            _statusItems = statusItems;
        }

        /// <summary>
        /// Генерация Excel отчета
        /// </summary>
        public void Generate(string filePath)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet as Excel.Worksheet;

                if (worksheet == null)
                    throw new Exception("Не удалось создать рабочий лист Excel");

                SetupPageLayout(worksheet, excelApp);

                int currentRow = FillHeader(worksheet, excelApp);
                decimal totalSum = FillData(worksheet, currentRow);
                FillTotal(worksheet, currentRow + _records.Count, totalSum, excelApp);

                workbook.SaveAs(filePath);
            }
            finally
            {
                Cleanup(excelApp, workbook, worksheet);
            }
        }

        /// <summary>
        /// Генерация отчета со статистикой по услугам (диаграммы)
        /// </summary>
        public void GenerateServiceStatistics(string filePath, List<ServiceStatistic> serviceStats)
        {
            if (serviceStats == null || serviceStats.Count == 0)
                throw new Exception("Нет данных для формирования статистики");

            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();

                // Лист 1: Гистограмма и таблица на одном листе
                Excel.Worksheet mainSheet = workbook.Worksheets[1];
                mainSheet.Name = "Статистика услуг";

                // Заполняем таблицу и гистограмму на одном листе
                FillMainSheet(mainSheet, serviceStats, excelApp);

                // Лист 2: Круговая диаграмма
                Excel.Worksheet pieChartSheet = workbook.Worksheets.Add();
                pieChartSheet.Name = "Доля услуг";
                CreatePieChartOnSheet(pieChartSheet, serviceStats, excelApp);

                workbook.SaveAs(filePath);
            }
            finally
            {
                Cleanup(excelApp, workbook, null);
            }
        }

        /// <summary>
        /// Заполнение основного листа с таблицей и гистограммой
        /// </summary>
        private void FillMainSheet(Excel.Worksheet worksheet, List<ServiceStatistic> stats, Excel.Application excelApp)
        {
            // Увеличиваем ширину колонок
            worksheet.Columns.ColumnWidth = 25;
            worksheet.Columns[1].ColumnWidth = 8;  // №
            worksheet.Columns[2].ColumnWidth = 45; // Услуга (увеличена)
            worksheet.Columns[3].ColumnWidth = 20; // Количество
            worksheet.Columns[4].ColumnWidth = 25; // Выручка
            worksheet.Columns[5].ColumnWidth = 15; // Доля

            // Заголовок
            Excel.Range titleRange = worksheet.Range["A1", "E1"];
            titleRange.Merge();
            titleRange.Value = "СТАТИСТИКА ПОПУЛЯРНОСТИ УСЛУГ";
            titleRange.Font.Size = 20;
            titleRange.Font.Bold = true;
            titleRange.Font.Name = "Arial";
            titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            titleRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
            titleRange.RowHeight = 40;
            ReleaseComObject(titleRange);

            // Подзаголовок с датой
            worksheet.Rows[3].RowHeight = 25;
            worksheet.Cells[3, 1] = "Дата формирования:";
            worksheet.Cells[3, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            FormatInfoCell(worksheet, 3, 1, true);
            FormatInfoCell(worksheet, 3, 2, false);

            // Заголовки таблицы
            int headerRow = 5;
            worksheet.Rows[headerRow].RowHeight = 30;
            string[] headers = { "№", "Услуга", "Количество записей", "Выручка (руб.)", "Доля (%)" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[headerRow, i + 1] = headers[i];
                Excel.Range headerRange = worksheet.Cells[headerRow, i + 1];
                headerRange.Font.Bold = true;
                headerRange.Font.Name = "Arial";
                headerRange.Font.Size = 12;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ReleaseComObject(headerRange);
            }

            // Данные таблицы
            int dataStartRow = headerRow + 1;
            int totalCount = stats.Sum(s => s.Count);
            decimal totalRevenue = 0;

            for (int i = 0; i < stats.Count; i++)
            {
                var stat = stats[i];
                double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;
                totalRevenue += stat.Revenue;

                worksheet.Cells[dataStartRow + i, 1] = i + 1;
                worksheet.Cells[dataStartRow + i, 2] = stat.ServiceName;
                worksheet.Cells[dataStartRow + i, 3] = stat.Count;
                worksheet.Cells[dataStartRow + i, 4] = stat.Revenue;
                worksheet.Cells[dataStartRow + i, 5] = Math.Round(percentage, 2);

                FormatDataRow(worksheet, dataStartRow + i, headers.Length, (dataStartRow + i) % 2 == 1);
                worksheet.Rows[dataStartRow + i].RowHeight = 25;
            }

            // Итоговая строка
            int totalRow = dataStartRow + stats.Count;
            worksheet.Rows[totalRow].RowHeight = 30;
            worksheet.Cells[totalRow, 2] = "ИТОГО:";
            worksheet.Cells[totalRow, 3] = totalCount;
            worksheet.Cells[totalRow, 4] = totalRevenue;

            Excel.Range totalRange = worksheet.Range[worksheet.Cells[totalRow, 2], worksheet.Cells[totalRow, 4]];
            totalRange.Font.Bold = true;
            totalRange.Font.Size = 12;
            totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            ReleaseComObject(totalRange);

            // ВСТАВЛЯЕМ ПУСТЫЕ СТРОКИ ПЕРЕД ДИАГРАММОЙ
            int chartStartRow = totalRow + 5;

            // Заголовок гистограммы
            worksheet.Rows[chartStartRow].RowHeight = 30;
            worksheet.Cells[chartStartRow, 2] = "ГИСТОГРАММА ПОПУЛЯРНОСТИ УСЛУГ";
            Excel.Range histTitle = worksheet.Range[worksheet.Cells[chartStartRow, 2], worksheet.Cells[chartStartRow, 4]];
            histTitle.Merge();
            histTitle.Font.Size = 16;
            histTitle.Font.Bold = true;
            histTitle.Font.Name = "Arial";
            histTitle.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            histTitle.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            ReleaseComObject(histTitle);

            // Данные для диаграммы (размещаем далеко справа, чтобы не мешали)
            int dataStart = chartStartRow + 2;
            for (int i = 0; i < stats.Count; i++)
            {
                worksheet.Cells[dataStart + i, 8] = stats[i].ServiceName;  // Колонка H
                worksheet.Cells[dataStart + i, 9] = stats[i].Count;        // Колонка I
            }

            try
            {
                var chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                // Размещаем диаграмму начиная с chartStartRow, колонка B
                var chartObject = chartObjects.Add(50, (chartStartRow - 1) * 20, 650, 350);
                var chart = chartObject.Chart;

                var dataRange = worksheet.Range[worksheet.Cells[dataStart, 8], worksheet.Cells[dataStart + stats.Count - 1, 9]];
                chart.SetSourceData(dataRange);

                chart.ChartType = Excel.XlChartType.xl3DColumnClustered;

                chart.HasTitle = true;
                chart.ChartTitle.Text = "Популярность услуг";
                chart.ChartTitle.Font.Size = 14;
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);

                chart.ChartArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                chart.PlotArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 250, 250));

                try
                {
                    var categoryAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary);
                    categoryAxis.HasTitle = true;
                    categoryAxis.AxisTitle.Text = "Услуги";
                    categoryAxis.AxisTitle.Font.Size = 10;
                }
                catch { }

                try
                {
                    var valueAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
                    valueAxis.HasTitle = true;
                    valueAxis.AxisTitle.Text = "Количество записей";
                    valueAxis.AxisTitle.Font.Size = 10;
                    valueAxis.MinimumScale = 0;
                }
                catch { }

                try
                {
                    if (chart.SeriesCollection() != null && chart.SeriesCollection().Count > 0)
                    {
                        var series = chart.SeriesCollection(1);
                        if (series != null)
                        {
                            series.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 182, 193));
                            series.Border.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);
                            series.Border.Weight = Excel.XlBorderWeight.xlThin;
                            series.GapWidth = 50;

                            series.HasDataLabels = true;
                            var dataLabels = series.DataLabels();
                            if (dataLabels != null)
                            {
                                dataLabels.ShowValue = true;
                                dataLabels.Font.Size = 9;
                                dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionAbove;
                            }
                        }
                    }
                }
                catch { }

                try { chart.HasLegend = false; } catch { }

                // Скрываем колонки с данными для диаграммы
                Excel.Range dataColumns = worksheet.Range["H:I"];
                dataColumns.Hidden = true;
                ReleaseComObject(dataColumns);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания гистограммы: {ex.Message}");
                worksheet.Cells[chartStartRow + 5, 2] = "Примечание: не удалось создать диаграмму";
            }
        }

        /// <summary>
        /// Создание гистограммы под таблицей
        /// </summary>
        private void CreateHistogramBelowTable(Excel.Worksheet worksheet, List<ServiceStatistic> stats, int startRow, Excel.Application excelApp)
        {
            if (stats == null || stats.Count == 0) return;

            // Заголовок гистограммы
            worksheet.Cells[startRow, 2] = "ГИСТОГРАММА ПОПУЛЯРНОСТИ УСЛУГ";
            Excel.Range histTitle = worksheet.Range[worksheet.Cells[startRow, 2], worksheet.Cells[startRow, 4]];
            histTitle.Merge();
            histTitle.Font.Size = 16;
            histTitle.Font.Bold = true;
            histTitle.Font.Name = "Arial";
            histTitle.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            histTitle.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            histTitle.RowHeight = 35;
            ReleaseComObject(histTitle);

            // Данные для диаграммы
            int dataStart = startRow + 2;
            for (int i = 0; i < stats.Count; i++)
            {
                worksheet.Cells[dataStart + i, 2] = stats[i].ServiceName;
                worksheet.Cells[dataStart + i, 3] = stats[i].Count;
            }

            try
            {
                var chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                // Размещаем диаграмму справа от данных
                var chartObject = chartObjects.Add(250, (dataStart - 1) * 20, 550, 350);
                var chart = chartObject.Chart;

                var dataRange = worksheet.Range[worksheet.Cells[dataStart, 2], worksheet.Cells[dataStart + stats.Count - 1, 3]];
                chart.SetSourceData(dataRange);

                // 3D гистограмма
                chart.ChartType = Excel.XlChartType.xl3DColumnClustered;

                // Стилизация диаграммы в цветах приложения
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Популярность услуг";
                chart.ChartTitle.Font.Size = 14;
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 105, 180));

                // Цвет области диаграммы
                chart.ChartArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                chart.PlotArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 250, 250));

                // Настройка осей
                try
                {
                    var categoryAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlCategory, Excel.XlAxisGroup.xlPrimary);
                    categoryAxis.HasTitle = true;
                    categoryAxis.AxisTitle.Text = "Услуги";
                    categoryAxis.AxisTitle.Font.Size = 11;
                    categoryAxis.TickLabelSpacing = 1;
                }
                catch { }

                try
                {
                    var valueAxis = (Excel.Axis)chart.Axes(Excel.XlAxisType.xlValue, Excel.XlAxisGroup.xlPrimary);
                    valueAxis.HasTitle = true;
                    valueAxis.AxisTitle.Text = "Количество записей";
                    valueAxis.AxisTitle.Font.Size = 11;
                    valueAxis.MinimumScale = 0;
                    valueAxis.HasMajorGridlines = true;
                    valueAxis.MajorGridlines.Border.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);
                }
                catch { }

                // Настройка рядов данных
                try
                {
                    if (chart.SeriesCollection() != null && chart.SeriesCollection().Count > 0)
                    {
                        var series = chart.SeriesCollection(1);
                        if (series != null)
                        {
                            // Цвет столбцов - нежно-розовый
                            series.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 182, 193));
                            series.Border.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 105, 180));
                            series.Border.Weight = Excel.XlBorderWeight.xlThin;
                            series.GapWidth = 50;

                            series.HasDataLabels = true;
                            var dataLabels = series.DataLabels();
                            if (dataLabels != null)
                            {
                                dataLabels.ShowValue = true;
                                dataLabels.Font.Size = 10;
                                dataLabels.Font.Bold = true;
                                dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionAbove;
                            }
                        }
                    }
                }
                catch { }

                // Убираем легенду
                try { chart.HasLegend = false; } catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания гистограммы: {ex.Message}");
                worksheet.Cells[startRow + 5, 2] = "Примечание: не удалось создать диаграмму";
                worksheet.Cells[startRow + 5, 2].Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Red);
            }
        }

        private void CreatePieChartOnSheet(Excel.Worksheet worksheet, List<ServiceStatistic> stats, Excel.Application excelApp)
        {
            if (stats.Count == 0) return;

            // Увеличиваем ширину колонок
            worksheet.Columns[1].ColumnWidth = 35;
            worksheet.Columns[2].ColumnWidth = 20;

            // Заголовок
            Excel.Range titleRange = worksheet.Range["A1", "D1"];
            titleRange.Merge();
            titleRange.Value = "ДОЛЯ УСЛУГ В ПРОЦЕНТАХ";
            titleRange.Font.Size = 20;
            titleRange.Font.Bold = true;
            titleRange.Font.Name = "Arial";
            titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            titleRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
            titleRange.RowHeight = 40;
            ReleaseComObject(titleRange);

            // Данные для диаграммы
            for (int i = 0; i < stats.Count; i++)
            {
                worksheet.Cells[3 + i, 1] = stats[i].ServiceName;
                worksheet.Cells[3 + i, 2] = stats[i].Count;
            }

            try
            {
                var chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                var chartObject = chartObjects.Add(50, 60, 550, 400);
                var chart = chartObject.Chart;

                var dataRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3 + stats.Count - 1, 2]];
                chart.SetSourceData(dataRange);
                chart.ChartType = Excel.XlChartType.xlPie;

                chart.HasTitle = true;
                chart.ChartTitle.Text = "Распределение услуг";
                chart.ChartTitle.Font.Size = 16;
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);

                // Цвета для круговой диаграммы
                chart.ChartArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.White);

                // Добавляем подписи с процентами
                try
                {
                    if (chart.SeriesCollection() != null && chart.SeriesCollection().Count > 0)
                    {
                        var series = chart.SeriesCollection(1);
                        if (series != null)
                        {
                            series.HasDataLabels = true;
                            var dataLabels = series.DataLabels();
                            if (dataLabels != null)
                            {
                                dataLabels.ShowPercentage = true;
                                dataLabels.ShowCategoryName = true;
                                dataLabels.ShowValue = false;
                                dataLabels.Font.Size = 11;
                                dataLabels.Font.Bold = true;
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionRight;
                    chart.Legend.Font.Size = 10;
                }
                catch { }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания круговой диаграммы: {ex.Message}");
            }
        }

        private void FormatDataRow(Excel.Worksheet worksheet, int row, int colCount, bool alternate)
        {
            Excel.Range borderRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, colCount]];
            borderRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            borderRange.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);
            if (alternate)
                borderRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 248, 248));
            borderRange.Font.Size = 11;
            ReleaseComObject(borderRange);

            // Центрирование номеров
            Excel.Range numRange = worksheet.Cells[row, 1];
            numRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ReleaseComObject(numRange);

            // Центрирование количества
            Excel.Range countRange = worksheet.Cells[row, 3];
            countRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ReleaseComObject(countRange);

            // Центрирование доли
            Excel.Range percentRange = worksheet.Cells[row, 5];
            percentRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ReleaseComObject(percentRange);

            // Форматирование цены
            Excel.Range priceRange = worksheet.Cells[row, 4];
            priceRange.NumberFormat = "#,##0.00 ₽";
            priceRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            ReleaseComObject(priceRange);
        }

        private void SetupPageLayout(Excel.Worksheet worksheet, Excel.Application excelApp)
        {
            worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
            worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
            worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
            worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);
        }

        private int FillHeader(Excel.Worksheet worksheet, Excel.Application excelApp)
        {
            Excel.Range range = worksheet.Range["A1", "G1"];
            range.Merge();
            range.Value = "ОТЧЕТ О ЗАПИСЯХ";
            range.Font.Size = 18;
            range.Font.Bold = true;
            range.Font.Name = "Arial";
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
            ReleaseComObject(range);

            int currentRow = 3;

            AddInfoRow(worksheet, currentRow++, "Период:", $"{_dtpFromDate.Value:dd.MM.yyyy} - {_dtpToDate.Value:dd.MM.yyyy}");

            if (_cmbMasterFilter.SelectedIndex > 0)
                AddInfoRow(worksheet, currentRow++, "Мастер:", _cmbMasterFilter.SelectedItem?.ToString());

            if (_cmbStatusFilter.SelectedIndex > 0)
                AddInfoRow(worksheet, currentRow++, "Статус:", _cmbStatusFilter.SelectedItem?.ToString());

            if (!string.IsNullOrEmpty(_txtSearch.Text))
                AddInfoRow(worksheet, currentRow++, "Поиск:", _txtSearch.Text);

            AddInfoRow(worksheet, currentRow++, "Сортировка:", _cmbSort.SelectedItem?.ToString());
            AddInfoRow(worksheet, currentRow++, "Дата формирования:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

            worksheet.Cells[currentRow, 1] = "Количество записей:";
            worksheet.Cells[currentRow, 2] = _totalRecords.ToString();
            FormatInfoCell(worksheet, currentRow, 1, true);

            Excel.Range rangeTotal = worksheet.Cells[currentRow, 2];
            rangeTotal.Font.Bold = true;
            rangeTotal.Font.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);
            ReleaseComObject(rangeTotal);

            currentRow += 2;

            string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена", "Менеджер" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1] = headers[i];
                Excel.Range headerRange = worksheet.Cells[currentRow, i + 1];
                headerRange.Font.Bold = true;
                headerRange.Font.Name = "Arial";
                headerRange.Font.Size = 11;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                headerRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                ReleaseComObject(headerRange);
            }

            return currentRow + 1;
        }

        private decimal FillData(Excel.Worksheet worksheet, int startRow)
        {
            int currentRow = startRow;
            decimal totalSum = 0;

            foreach (var record in _records)
            {
                string statusName = _statusItems.FirstOrDefault(x => x.ID == record.StatusID)?.Name ?? record.StatusID.ToString();

                worksheet.Cells[currentRow, 1] = record.MasterName;
                worksheet.Cells[currentRow, 2] = record.ClientName;
                worksheet.Cells[currentRow, 3] = record.Date.ToString("dd.MM.yyyy HH:mm");
                worksheet.Cells[currentRow, 4] = statusName;
                worksheet.Cells[currentRow, 5] = record.Service;
                worksheet.Cells[currentRow, 6] = record.Price;
                worksheet.Cells[currentRow, 7] = record.UserName;

                FormatDataCell(worksheet, currentRow, record, statusName);

                if (record.StatusID != 4)
                    totalSum += record.Price;

                currentRow++;
            }

            return totalSum;
        }

        private void FormatDataCell(Excel.Worksheet worksheet, int row, RecordData record, string statusName)
        {
            Excel.Range dateRange = worksheet.Cells[row, 3];
            dateRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ReleaseComObject(dateRange);

            Excel.Range statusRange = worksheet.Cells[row, 4];
            statusRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            if (statusName.Contains("Запланирован"))
                statusRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 245, 157));
            else if (statusName.Contains("Подтвержден"))
                statusRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(197, 225, 165));
            else if (statusName.Contains("Выполнен"))
                statusRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(225, 225, 225));
            else if (statusName.Contains("Отменен"))
                statusRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 171, 145));
            ReleaseComObject(statusRange);

            Excel.Range priceRange = worksheet.Cells[row, 6];
            priceRange.NumberFormat = "#,##0.00 ₽";
            priceRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            ReleaseComObject(priceRange);

            Excel.Range borderRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 7]];
            borderRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            borderRange.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);
            if (row % 2 == 1)
                borderRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 250, 250));
            ReleaseComObject(borderRange);
        }

        private void FillTotal(Excel.Worksheet worksheet, int currentRow, decimal totalSum, Excel.Application excelApp)
        {
            Excel.Range totalRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
            totalRange.Merge();
            totalRange.Value = "ИТОГО:";
            totalRange.Font.Bold = true;
            totalRange.Font.Size = 12;
            totalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            totalRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ReleaseComObject(totalRange);

            Excel.Range sumRange = worksheet.Cells[currentRow, 6];
            sumRange.Value = totalSum;
            sumRange.Font.Bold = true;
            sumRange.NumberFormat = "#,##0.00 ₽";
            sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            sumRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            sumRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ReleaseComObject(sumRange);

            Excel.Range emptyRange = worksheet.Cells[currentRow, 7];
            emptyRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            emptyRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(AppLightColor);
            ReleaseComObject(emptyRange);

            worksheet.Columns.AutoFit();
            worksheet.Columns[2].ColumnWidth = 35; // Увеличиваем колонку с клиентом
        }

        private void AddInfoRow(Excel.Worksheet worksheet, int row, string label, string value)
        {
            worksheet.Cells[row, 1] = label;
            worksheet.Cells[row, 2] = value;
            FormatInfoCell(worksheet, row, 1, true);
            FormatInfoCell(worksheet, row, 2, false);
        }

        private void FormatInfoCell(Excel.Worksheet worksheet, int row, int col, bool isLabel)
        {
            Excel.Range range = worksheet.Cells[row, col];
            if (isLabel)
            {
                range.Font.Bold = true;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(AppAccentColor);
            }
            range.Font.Name = "Arial";
            range.Font.Size = 10;
            ReleaseComObject(range);
        }

        private void ReleaseComObject(object obj)
        {
            if (obj != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            }
        }

        private void Cleanup(Excel.Application excelApp, Excel.Workbook workbook, Excel.Worksheet worksheet)
        {
            try
            {
                if (worksheet != null)
                    ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    ReleaseComObject(excelApp);
                }
            }
            catch { }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

    /// <summary>
    /// Класс для хранения статистики по услуге
    /// </summary>
    public class ServiceStatistic
    {
        public string ServiceName { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }
}