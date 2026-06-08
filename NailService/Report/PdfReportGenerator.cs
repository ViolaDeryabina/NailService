using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace NailService
{
    /// <summary>
    /// Класс для генерации PDF отчетов
    /// </summary>
    public class PdfReportGenerator
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
        private readonly decimal _totalRevenue;

        public PdfReportGenerator(
            DateTimePicker dtpFromDate,
            DateTimePicker dtpToDate,
            ComboBox cmbMasterFilter,
            ComboBox cmbStatusFilter,
            ComboBox cmbSort,
            TextBox txtSearch,
            int totalRecords,
            List<RecordData> records,
            List<StatusItem> statusItems,
            decimal totalRevenue)
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
            _totalRevenue = totalRevenue;
        }

        /// <summary>
        /// Генерация PDF отчета
        /// </summary>
        public void Generate(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4.Rotate(), 15, 15, 20, 20);
                    PdfWriter.GetInstance(document, fs);
                    document.Open();

                    // ВАЖНО: Используем шрифт Arial, который поддерживает кириллицу
                    // Пытаемся загрузить системный шрифт Arial
                    BaseFont baseFont = null;
                    try
                    {
                        // Способ 1: Использовать системный шрифт Arial
                        baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    }
                    catch
                    {
                        try
                        {
                            // Способ 2: Использовать другой путь к шрифту
                            baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        }
                        catch
                        {
                            // Способ 3: Использовать встроенный шрифт с кириллицей через кодировку
                            baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, "CP1251", BaseFont.NOT_EMBEDDED);
                        }
                    }

                    iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                    iTextSharp.text.Font headerFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font boldFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);

                    // Заголовок
                    Paragraph title = new Paragraph("ОТЧЕТ О ЗАПИСЯХ", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 15;
                    document.Add(title);

                    // Информация о фильтрах
                    document.Add(CreateInfoTable(normalFont, boldFont));
                    document.Add(new Paragraph("\n"));

                    // Таблица данных
                    PdfPTable dataTable = CreateDataTable(headerFont, normalFont);
                    document.Add(dataTable);
                    document.Add(new Paragraph("\n"));

                    // Итог
                    document.Add(CreateTotalTable(boldFont));

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании PDF: {ex.Message}", ex);
            }
        }

        private PdfPTable CreateInfoTable(iTextSharp.text.Font normalFont, iTextSharp.text.Font boldFont)
        {
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 25f, 75f });
            infoTable.SpacingBefore = 5;

            AddInfoRow(infoTable, "Период:", $"{_dtpFromDate.Value:dd.MM.yyyy} - {_dtpToDate.Value:dd.MM.yyyy}", normalFont, boldFont);

            if (_cmbMasterFilter.SelectedIndex > 0)
                AddInfoRow(infoTable, "Мастер:", _cmbMasterFilter.SelectedItem?.ToString(), normalFont, boldFont);

            if (_cmbStatusFilter.SelectedIndex > 0)
                AddInfoRow(infoTable, "Статус:", _cmbStatusFilter.SelectedItem?.ToString(), normalFont, boldFont);

            if (!string.IsNullOrEmpty(_txtSearch.Text))
                AddInfoRow(infoTable, "Поиск:", _txtSearch.Text, normalFont, boldFont);

            AddInfoRow(infoTable, "Сортировка:", _cmbSort.SelectedItem?.ToString(), normalFont, boldFont);
            AddInfoRow(infoTable, "Дата формирования:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"), normalFont, boldFont);
            AddInfoRow(infoTable, "Количество записей:", _totalRecords.ToString(), normalFont, boldFont);

            return infoTable;
        }

        private void AddInfoRow(PdfPTable table, string label, string value, iTextSharp.text.Font normalFont, iTextSharp.text.Font boldFont)
        {
            // Обрабатываем кириллицу - преобразуем строки в Unicode
            string labelUnicode = label;
            string valueUnicode = value ?? "";

            PdfPCell labelCell = new PdfPCell(new Phrase(labelUnicode, boldFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.Padding = 3;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(valueUnicode, normalFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.Padding = 3;
            table.AddCell(valueCell);
        }

        private PdfPTable CreateDataTable(iTextSharp.text.Font headerFont, iTextSharp.text.Font normalFont)
        {
            PdfPTable dataTable = new PdfPTable(7);
            dataTable.WidthPercentage = 100;
            dataTable.SetWidths(new float[] { 13f, 13f, 15f, 10f, 20f, 12f, 17f });
            dataTable.HeaderRows = 1;
            dataTable.SpacingBefore = 10;

            // Заголовки
            string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена (руб)", "Менеджер" };
            foreach (string header in headers)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont));
                headerCell.BackgroundColor = new BaseColor(255, 203, 219);
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerCell.Padding = 5;
                dataTable.AddCell(headerCell);
            }

            // Данные из списка записей
            int rowNum = 0;
            foreach (var record in _records)
            {
                string statusName = _statusItems.FirstOrDefault(x => x.ID == record.StatusID)?.Name ?? record.StatusID.ToString();
                BaseColor rowColor = (rowNum % 2 == 0) ? BaseColor.WHITE : new BaseColor(245, 245, 250);

                // Мастер - проверяем на null и преобразуем
                string masterName = record.MasterName ?? "";
                PdfPCell masterCell = new PdfPCell(new Phrase(masterName, normalFont));
                masterCell.Padding = 4;
                masterCell.BackgroundColor = rowColor;
                dataTable.AddCell(masterCell);

                // Клиент
                string clientName = record.ClientName ?? "";
                PdfPCell clientCell = new PdfPCell(new Phrase(clientName, normalFont));
                clientCell.Padding = 4;
                clientCell.BackgroundColor = rowColor;
                dataTable.AddCell(clientCell);

                // Дата
                string dateStr = record.Date.ToString("dd.MM.yyyy HH:mm");
                PdfPCell dateCell = new PdfPCell(new Phrase(dateStr, normalFont));
                dateCell.HorizontalAlignment = Element.ALIGN_CENTER;
                dateCell.Padding = 4;
                dateCell.BackgroundColor = rowColor;
                dataTable.AddCell(dateCell);

                // Статус с цветом
                PdfPCell statusCell = new PdfPCell(new Phrase(statusName, normalFont));
                statusCell.HorizontalAlignment = Element.ALIGN_CENTER;
                statusCell.Padding = 4;
                if (statusName.Contains("Запланирован"))
                    statusCell.BackgroundColor = new BaseColor(255, 245, 157);
                else if (statusName.Contains("Подтвержден"))
                    statusCell.BackgroundColor = new BaseColor(197, 225, 165);
                else if (statusName.Contains("Выполнен"))
                    statusCell.BackgroundColor = new BaseColor(225, 225, 225);
                else if (statusName.Contains("Отменен"))
                    statusCell.BackgroundColor = new BaseColor(255, 171, 145);
                else
                    statusCell.BackgroundColor = rowColor;
                dataTable.AddCell(statusCell);

                // Услуга
                string service = record.Service ?? "";
                PdfPCell serviceCell = new PdfPCell(new Phrase(service, normalFont));
                serviceCell.Padding = 4;
                serviceCell.BackgroundColor = rowColor;
                dataTable.AddCell(serviceCell);

                // Цена
                string priceText = record.Price.ToString("N0");
                PdfPCell priceCell = new PdfPCell(new Phrase(priceText, normalFont));
                priceCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                priceCell.Padding = 4;
                priceCell.BackgroundColor = rowColor;
                dataTable.AddCell(priceCell);

                // Менеджер
                string userName = record.UserName ?? "";
                PdfPCell managerCell = new PdfPCell(new Phrase(userName, normalFont));
                managerCell.Padding = 4;
                managerCell.BackgroundColor = rowColor;
                dataTable.AddCell(managerCell);

                rowNum++;
            }

            return dataTable;
        }

        private PdfPTable CreateTotalTable(iTextSharp.text.Font boldFont)
        {
            PdfPTable totalTable = new PdfPTable(2);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 85f, 15f });
            totalTable.SpacingBefore = 15;

            PdfPCell totalLabelCell = new PdfPCell(new Phrase("ИТОГО:", boldFont));
            totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabelCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            totalLabelCell.BackgroundColor = new BaseColor(255, 203, 219);
            totalLabelCell.Padding = 6;
            totalTable.AddCell(totalLabelCell);

            PdfPCell totalValueCell = new PdfPCell(new Phrase(_totalRevenue.ToString("N0") + " руб.", boldFont));
            totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalValueCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            totalValueCell.BackgroundColor = new BaseColor(255, 203, 219);
            totalValueCell.Padding = 6;
            totalTable.AddCell(totalValueCell);

            return totalTable;
        }
    }
}