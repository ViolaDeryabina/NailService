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

                // Настройка страницы
                SetupPageLayout(worksheet, excelApp);

                // Заполнение отчета
                int currentRow = FillHeader(worksheet, excelApp);
                decimal totalSum = FillData(worksheet, currentRow);
                FillTotal(worksheet, currentRow + _records.Count, totalSum, excelApp);

                // Сохранение
                workbook.SaveAs(filePath);
            }
            finally
            {
                Cleanup(excelApp, workbook, worksheet);
            }
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
            Color accentColor = Color.HotPink;
            Color lightPink = Color.FromArgb(255, 203, 219);

            // Заголовок
            Excel.Range range = worksheet.Range["A1", "G1"];
            range.Merge();
            range.Value = "ОТЧЕТ О ЗАПИСЯХ";
            range.Font.Size = 18;
            range.Font.Bold = true;
            range.Font.Name = "Arial";
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
            ReleaseComObject(range);

            int currentRow = 3;

            // Информация о фильтрах
            AddInfoRow(worksheet, currentRow++, "Период:", $"{_dtpFromDate.Value:dd.MM.yyyy} - {_dtpToDate.Value:dd.MM.yyyy}");

            if (_cmbMasterFilter.SelectedIndex > 0)
                AddInfoRow(worksheet, currentRow++, "Мастер:", _cmbMasterFilter.SelectedItem?.ToString());

            if (_cmbStatusFilter.SelectedIndex > 0)
                AddInfoRow(worksheet, currentRow++, "Статус:", _cmbStatusFilter.SelectedItem?.ToString());

            if (!string.IsNullOrEmpty(_txtSearch.Text))
                AddInfoRow(worksheet, currentRow++, "Поиск:", _txtSearch.Text);

            AddInfoRow(worksheet, currentRow++, "⬆Сортировка:", _cmbSort.SelectedItem?.ToString());
            AddInfoRow(worksheet, currentRow++, "Дата формирования:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

            worksheet.Cells[currentRow, 1] = "Количество записей:";
            worksheet.Cells[currentRow, 2] = _totalRecords.ToString();
            FormatInfoCell(worksheet, currentRow, 1, true);

            Excel.Range rangeTotal = worksheet.Cells[currentRow, 2];
            rangeTotal.Font.Bold = true;
            rangeTotal.Font.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
            ReleaseComObject(rangeTotal);

            currentRow += 2;

            // Заголовки таблицы
            string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена", "Менеджер" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[currentRow, i + 1] = headers[i];
                Excel.Range headerRange = worksheet.Cells[currentRow, i + 1];
                headerRange.Font.Bold = true;
                headerRange.Font.Name = "Arial";
                headerRange.Font.Size = 11;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
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
            string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена", "Менеджер" };

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

                // Форматирование даты
                Excel.Range dateRange = worksheet.Cells[currentRow, 3];
                dateRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ReleaseComObject(dateRange);

                // Форматирование статуса с цветом
                Excel.Range statusRange = worksheet.Cells[currentRow, 4];
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

                // Форматирование цены
                Excel.Range priceRange = worksheet.Cells[currentRow, 6];
                priceRange.NumberFormat = "#,##0.00";
                priceRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                ReleaseComObject(priceRange);

                if (record.StatusID != 4)
                    totalSum += record.Price;

                // Границы
                Excel.Range borderRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, headers.Length]];
                borderRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                borderRange.Borders.Color = System.Drawing.ColorTranslator.ToOle(Color.LightGray);
                if (currentRow % 2 == 1)
                    borderRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 250, 250));
                ReleaseComObject(borderRange);

                currentRow++;
            }

            return totalSum;
        }

        private void FillTotal(Excel.Worksheet worksheet, int currentRow, decimal totalSum, Excel.Application excelApp)
        {
            Color lightPink = Color.FromArgb(255, 203, 219);

            Excel.Range totalRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
            totalRange.Merge();
            totalRange.Value = "ИТОГО:";
            totalRange.Font.Bold = true;
            totalRange.Font.Size = 12;
            totalRange.Font.Name = "Arial";
            totalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
            totalRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ReleaseComObject(totalRange);

            Excel.Range sumRange = worksheet.Cells[currentRow, 6];
            sumRange.Value = totalSum;
            sumRange.Font.Bold = true;
            sumRange.Font.Size = 12;
            sumRange.NumberFormat = "#,##0.00";
            sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            sumRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
            sumRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            ReleaseComObject(sumRange);

            Excel.Range emptyRange = worksheet.Cells[currentRow, 7];
            emptyRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            emptyRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
            ReleaseComObject(emptyRange);

            // Автоподбор ширины
            Excel.Range usedRange = worksheet.UsedRange;
            usedRange.Columns.AutoFit();
            ReleaseComObject(usedRange);
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
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.HotPink);
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
}