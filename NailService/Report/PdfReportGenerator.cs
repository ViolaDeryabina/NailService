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
        private readonly List<ServiceStatistic> _serviceStats;

        // Цвета приложения
        private readonly BaseColor AppAccentColor = new BaseColor(255, 105, 180);
        private readonly BaseColor AppLightColor = new BaseColor(255, 203, 219);
        private readonly BaseColor AppChartColor = new BaseColor(255, 182, 193);
        private readonly BaseColor AppBorderColor = new BaseColor(255, 105, 180);

        // Массив цветов для секторов круговой диаграммы
        private readonly BaseColor[] PieColors = new BaseColor[]
        {
            new BaseColor(255, 105, 180), // HotPink
            new BaseColor(255, 140, 170),
            new BaseColor(255, 160, 180),
            new BaseColor(255, 182, 193), // Светло-розовый
            new BaseColor(255, 200, 210),
            new BaseColor(255, 218, 224), // Очень светлый розовый
            new BaseColor(255, 225, 230),
            new BaseColor(255, 235, 238)  // Бледно-розовый
        };

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
            decimal totalRevenue,
            List<ServiceStatistic> serviceStats = null)
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
            _serviceStats = serviceStats ?? GetServiceStatisticsFromRecords(records);
        }

        #region Основные методы генерации
        /// <summary>
        /// Генерация PDF отчета (каждая запись отдельно, как в Excel)
        /// </summary>
        public void Generate(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4.Rotate(), 30, 30, 40, 40);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    var titleFont = CreateFont(16, Font.BOLD);
                    var headerFont = CreateFont(11, Font.BOLD);
                    var normalFont = CreateFont(9, Font.NORMAL);
                    var boldFont = CreateFont(10, Font.BOLD);

                    // Заголовок
                    Paragraph title = new Paragraph("ОТЧЕТ О ЗАПИСЯХ", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20;
                    document.Add(title);

                    // Информация о фильтрах
                    document.Add(CreateInfoTable(normalFont, boldFont));
                    document.Add(new Paragraph("\n"));

                    // Таблица с записями
                    PdfPTable dataTable = CreateDataTable(headerFont, normalFont);
                    document.Add(dataTable);
                    document.Add(new Paragraph("\n"));

                    // Итоговая сумма
                    document.Add(CreateTotalTable(boldFont));

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании PDF: {ex.Message}", ex);
            }
        }

        private PdfPTable CreateDataTable(Font headerFont, Font normalFont)
        {
            PdfPTable dataTable = new PdfPTable(7);
            dataTable.WidthPercentage = 100;
            dataTable.SetWidths(new float[] { 13f, 13f, 15f, 10f, 20f, 12f, 17f });
            dataTable.HeaderRows = 1;

            string[] headers = { "Мастер", "Клиент", "Дата и время", "Статус", "Услуга", "Цена (руб)", "Менеджер" };
            foreach (string header in headers)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont));
                headerCell.BackgroundColor = AppLightColor;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.Padding = 6;
                dataTable.AddCell(headerCell);
            }

            int rowNum = 0;
            foreach (var record in _records)
            {
                string statusName = _statusItems.FirstOrDefault(x => x.ID == record.StatusID)?.Name ?? record.StatusID.ToString();
                BaseColor rowColor = (rowNum % 2 == 0) ? BaseColor.WHITE : new BaseColor(248, 248, 250);

                dataTable.AddCell(CreateCell(record.MasterName ?? "", normalFont, rowColor));
                dataTable.AddCell(CreateCell(record.ClientName ?? "", normalFont, rowColor));
                dataTable.AddCell(CreateCell(record.Date.ToString("dd.MM.yyyy HH:mm"), normalFont, rowColor, Element.ALIGN_CENTER));
                dataTable.AddCell(CreateStatusCell(statusName, normalFont, rowColor));
                dataTable.AddCell(CreateCell(record.Service ?? "", normalFont, rowColor));
                dataTable.AddCell(CreateCell(record.Price.ToString("N0"), normalFont, rowColor, Element.ALIGN_RIGHT));
                dataTable.AddCell(CreateCell(record.UserName ?? "", normalFont, rowColor));
                rowNum++;
            }

            return dataTable;
        }

        private PdfPTable CreateTotalTable(Font boldFont)
        {
            PdfPTable totalTable = new PdfPTable(2);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 85f, 15f });
            totalTable.SpacingBefore = 15;

            PdfPCell totalLabelCell = new PdfPCell(new Phrase("ИТОГО:", boldFont));
            totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabelCell.BackgroundColor = AppLightColor;
            totalLabelCell.Padding = 8;
            totalTable.AddCell(totalLabelCell);

            PdfPCell totalValueCell = new PdfPCell(new Phrase($"{_totalRevenue:N0} руб.", boldFont));
            totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalValueCell.BackgroundColor = AppLightColor;
            totalValueCell.Padding = 8;
            totalTable.AddCell(totalValueCell);

            return totalTable;
        }

        private PdfPTable CreateInfoTable(Font normalFont, Font boldFont)
        {
            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 25f, 75f });
            infoTable.SpacingBefore = 10;

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

        public void GenerateServiceStatistics(string filePath, List<ServiceStatistic> serviceStats)
        {
            if (serviceStats == null || serviceStats.Count == 0)
                throw new Exception("Нет данных для формирования статистики");

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 40, 40, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    var titleFont = CreateFont(18, Font.BOLD);
                    var headerFont = CreateFont(12, Font.BOLD);
                    var normalFont = CreateFont(10, Font.NORMAL);
                    var boldFont = CreateFont(11, Font.BOLD);

                    AddStatisticsTablePage(document, titleFont, headerFont, normalFont, boldFont, serviceStats);
                    document.NewPage();
                    AddHistogramPage(document, writer, titleFont, serviceStats);
                    document.NewPage();
                    AddPieChartPage(document, writer, titleFont, normalFont, serviceStats);

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании PDF: {ex.Message}", ex);
            }
        }

        #endregion

        #region Вспомогательные методы

        private Font CreateFont(int size, int style)
        {
            BaseFont baseFont = null;
            try
            {
                baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            catch
            {
                try
                {
                    baseFont = BaseFont.CreateFont("C:\\Windows\\Fonts\\times.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
                catch
                {
                    baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, "CP1251", BaseFont.NOT_EMBEDDED);
                }
            }
            return new Font(baseFont, size, style);
        }

        private PdfPCell CreateCell(string text, Font font, BaseColor bgColor, int align = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = align;
            return cell;
        }

        private PdfPCell CreateCell(string text, Font font, BaseColor bgColor, int align, int padding)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = padding;
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = align;
            return cell;
        }


        private void AddInfoRow(PdfPTable table, string label, string value, Font normalFont, Font boldFont)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, boldFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.Padding = 4;
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", normalFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.Padding = 4;
            table.AddCell(valueCell);
        }


        private PdfPCell CreateStatusCell(string statusName, Font font, BaseColor rowColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(statusName, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;

            if (statusName.Contains("Запланирован"))
                cell.BackgroundColor = new BaseColor(255, 245, 157);
            else if (statusName.Contains("Подтвержден"))
                cell.BackgroundColor = new BaseColor(197, 225, 165);
            else if (statusName.Contains("Выполнен"))
                cell.BackgroundColor = new BaseColor(225, 225, 225);
            else if (statusName.Contains("Отменен"))
                cell.BackgroundColor = new BaseColor(255, 171, 145);
            else
                cell.BackgroundColor = rowColor;

            return cell;
        }

        #endregion

        #region Методы статистики

        private void AddServiceStatistics(Document document, PdfWriter writer, Font titleFont, Font headerFont, Font normalFont)
        {
            Paragraph statsTitle = new Paragraph("СТАТИСТИКА ПОПУЛЯРНОСТИ УСЛУГ", titleFont);
            statsTitle.Alignment = Element.ALIGN_CENTER;
            statsTitle.SpacingBefore = 20;
            statsTitle.SpacingAfter = 15;
            document.Add(statsTitle);

            document.Add(CreateStatsTable(headerFont, normalFont));
            document.Add(new Paragraph("\n"));
            document.Add(CreateStatsTotalTable(normalFont));
            document.Add(new Paragraph("\n"));

            CreateHistogram(document, writer);
            document.Add(new Paragraph("\n"));
            CreatePieChart(document, writer);
        }

        private PdfPTable CreateStatsTable(Font headerFont, Font normalFont)
        {
            PdfPTable statsTable = new PdfPTable(5);
            statsTable.WidthPercentage = 100;
            statsTable.SetWidths(new float[] { 8f, 42f, 15f, 20f, 15f });
            statsTable.HeaderRows = 1;

            string[] headers = { "№", "Услуга", "Количество", "Выручка (руб)", "Доля (%)" };
            foreach (string header in headers)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont));
                headerCell.BackgroundColor = AppAccentColor;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.Padding = 6;
                statsTable.AddCell(headerCell);
            }

            int totalCount = _serviceStats.Sum(s => s.Count);
            for (int i = 0; i < _serviceStats.Count; i++)
            {
                var stat = _serviceStats[i];
                double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;
                BaseColor rowColor = (i % 2 == 0) ? BaseColor.WHITE : new BaseColor(248, 248, 250);

                statsTable.AddCell(CreateCell((i + 1).ToString(), normalFont, rowColor, Element.ALIGN_CENTER));
                statsTable.AddCell(CreateCell(stat.ServiceName, normalFont, rowColor));
                statsTable.AddCell(CreateCell(stat.Count.ToString(), normalFont, rowColor, Element.ALIGN_CENTER));
                statsTable.AddCell(CreateCell(stat.Revenue.ToString("N0"), normalFont, rowColor, Element.ALIGN_RIGHT));
                statsTable.AddCell(CreateCell($"{Math.Round(percentage, 2)}%", normalFont, rowColor, Element.ALIGN_CENTER));
            }

            return statsTable;
        }

        private PdfPTable CreateStatsTotalTable(Font normalFont)
        {
            int totalCount = _serviceStats.Sum(s => s.Count);
            decimal totalRevenue = _serviceStats.Sum(s => s.Revenue);

            PdfPTable totalTable = new PdfPTable(2);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 85f, 15f });

            PdfPCell totalLabelCell = new PdfPCell(new Phrase("ИТОГО:", CreateFont(11, Font.BOLD)));
            totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabelCell.BackgroundColor = AppLightColor;
            totalLabelCell.Padding = 6;
            totalTable.AddCell(totalLabelCell);

            PdfPCell totalValueCell = new PdfPCell(new Phrase($"{totalRevenue:N0} руб.", CreateFont(11, Font.BOLD)));
            totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalValueCell.BackgroundColor = AppLightColor;
            totalValueCell.Padding = 6;
            totalTable.AddCell(totalValueCell);

            return totalTable;
        }

        private void CreateHistogram(Document document, PdfWriter writer)
        {
            if (_serviceStats == null || _serviceStats.Count == 0) return;

            try
            {
                Paragraph histTitle = new Paragraph("Гистограмма популярности услуг", CreateFont(14, Font.BOLD));
                histTitle.Alignment = Element.ALIGN_CENTER;
                histTitle.SpacingBefore = 15;
                histTitle.SpacingAfter = 15;
                document.Add(histTitle);

                PdfContentByte cb = writer.DirectContent;
                float pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                float chartWidth = pageWidth - 50;
                float chartHeight = 280;
                float chartX = document.LeftMargin + 25;
                float chartY = document.Top - 330;

                cb.Rectangle(chartX, chartY, chartWidth, chartHeight);
                cb.SetColorStroke(AppBorderColor);
                cb.SetLineWidth(1f);
                cb.Stroke();

                cb.MoveTo(chartX + 50, chartY + 15);
                cb.LineTo(chartX + 50, chartY + chartHeight - 15);
                cb.LineTo(chartX + chartWidth - 15, chartY + chartHeight - 15);
                cb.Stroke();

                float drawingWidth = chartWidth - 70;
                float barWidth = drawingWidth / _serviceStats.Count * 0.7f;
                float barSpacing = drawingWidth / _serviceStats.Count * 0.3f;
                float maxCount = _serviceStats.Max(s => s.Count);
                float yScale = (chartHeight - 45) / maxCount;

                var axisFont = CreateFont(8, Font.NORMAL);
                for (int i = 0; i <= 5; i++)
                {
                    float value = maxCount * i / 5;
                    float yPos = chartY + 12 + (chartHeight - 40) * i / 5;
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                        new Phrase(Math.Round(value, 0).ToString(), axisFont),
                        chartX + 45, yPos - 3, 0);
                }

                float currentX = chartX + 58;
                for (int i = 0; i < _serviceStats.Count; i++)
                {
                    var stat = _serviceStats[i];
                    float barHeight = Math.Max(stat.Count * yScale, 2);
                    float barY = chartY + 12;

                    cb.SetColorFill(AppChartColor);
                    cb.Rectangle(currentX, barY, barWidth, barHeight);
                    cb.Fill();

                    cb.SetColorStroke(AppBorderColor);
                    cb.Rectangle(currentX, barY, barWidth, barHeight);
                    cb.Stroke();

                    string shortName = stat.ServiceName.Length > 20 ? stat.ServiceName.Substring(0, 17) + "..." : stat.ServiceName;
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        new Phrase(shortName, axisFont),
                        currentX + barWidth / 2, barY - 8, 0);

                    ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        new Phrase(stat.Count.ToString(), CreateFont(9, Font.BOLD)),
                        currentX + barWidth / 2, barY + barHeight + 5, 0);

                    currentX += barWidth + barSpacing;
                }
            }
            catch (Exception ex)
            {
                document.Add(new Paragraph($"Примечание: не удалось создать гистограмму - {ex.Message}"));
            }
        }

        private void CreatePieChart(Document document, PdfWriter writer)
        {
            if (_serviceStats == null || _serviceStats.Count == 0) return;

            try
            {
                Paragraph pieTitle = new Paragraph("Распределение услуг (круговая диаграмма)", CreateFont(14, Font.BOLD));
                pieTitle.Alignment = Element.ALIGN_CENTER;
                pieTitle.SpacingBefore = 15;
                pieTitle.SpacingAfter = 15;
                document.Add(pieTitle);

                PdfPTable pieDataTable = new PdfPTable(2);
                pieDataTable.WidthPercentage = 70;
                pieDataTable.HorizontalAlignment = Element.ALIGN_CENTER;
                pieDataTable.SetWidths(new float[] { 65f, 35f });

                PdfPCell legendHeader = new PdfPCell(new Phrase("Услуга", CreateFont(11, Font.BOLD)));
                legendHeader.BackgroundColor = AppLightColor;
                legendHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                legendHeader.Padding = 8;
                pieDataTable.AddCell(legendHeader);

                PdfPCell percentHeader = new PdfPCell(new Phrase("Доля (%)", CreateFont(11, Font.BOLD)));
                percentHeader.BackgroundColor = AppLightColor;
                percentHeader.HorizontalAlignment = Element.ALIGN_CENTER;
                percentHeader.Padding = 8;
                pieDataTable.AddCell(percentHeader);

                int totalCount = _serviceStats.Sum(s => s.Count);
                int colorIndex = 0;
                foreach (var stat in _serviceStats.OrderByDescending(s => s.Count))
                {
                    double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;

                    PdfPCell nameCell = new PdfPCell(new Phrase(stat.ServiceName, CreateFont(9, Font.NORMAL)));
                    nameCell.Padding = 5;
                    nameCell.BackgroundColor = PieColors[colorIndex % PieColors.Length];
                    nameCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    pieDataTable.AddCell(nameCell);

                    PdfPCell percentCell = new PdfPCell(new Phrase($"{Math.Round(percentage, 2)}%", CreateFont(9, Font.BOLD)));
                    percentCell.Padding = 5;
                    percentCell.BackgroundColor = PieColors[colorIndex % PieColors.Length];
                    percentCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    pieDataTable.AddCell(percentCell);

                    colorIndex++;
                }

                document.Add(pieDataTable);
            }
            catch (Exception ex)
            {
                document.Add(new Paragraph($"Примечание: не удалось создать круговую диаграмму - {ex.Message}"));
            }
        }

        private void AddStatisticsTablePage(Document document, Font titleFont, Font headerFont, Font normalFont, Font boldFont, List<ServiceStatistic> serviceStats)
        {
            Paragraph title = new Paragraph("СТАТИСТИКА ПОПУЛЯРНОСТИ УСЛУГ", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 25;
            document.Add(title);

            Paragraph datePara = new Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", normalFont);
            datePara.Alignment = Element.ALIGN_RIGHT;
            datePara.SpacingAfter = 20;
            document.Add(datePara);

            PdfPTable statsTable = CreateStatsTable(headerFont, normalFont, serviceStats);
            document.Add(statsTable);

            int totalCount = serviceStats.Sum(s => s.Count);
            decimal totalRevenue = serviceStats.Sum(s => s.Revenue);

            PdfPTable totalTable = new PdfPTable(2);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 85f, 15f });
            totalTable.SpacingBefore = 15;

            totalTable.AddCell(CreateCell("ИТОГО:", boldFont, AppLightColor, Element.ALIGN_RIGHT, 8));
            totalTable.AddCell(CreateCell($"{totalRevenue:N0} руб.", boldFont, AppLightColor, Element.ALIGN_RIGHT, 8));

            document.Add(totalTable);
        }

        private PdfPTable CreateStatsTable(Font headerFont, Font normalFont, List<ServiceStatistic> serviceStats)
        {
            PdfPTable statsTable = new PdfPTable(5);
            statsTable.WidthPercentage = 100;
            statsTable.SetWidths(new float[] { 8f, 47f, 15f, 20f, 10f });
            statsTable.HeaderRows = 1;

            string[] headers = { "№", "Услуга", "Количество", "Выручка (руб)", "Доля (%)" };
            foreach (string header in headers)
            {
                PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont));
                headerCell.BackgroundColor = AppAccentColor;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.Padding = 8;
                statsTable.AddCell(headerCell);
            }

            int totalCount = serviceStats.Sum(s => s.Count);
            for (int i = 0; i < serviceStats.Count; i++)
            {
                var stat = serviceStats[i];
                double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;
                BaseColor rowColor = (i % 2 == 0) ? BaseColor.WHITE : new BaseColor(248, 248, 250);

                statsTable.AddCell(CreateCell((i + 1).ToString(), normalFont, rowColor, Element.ALIGN_CENTER, 5));
                statsTable.AddCell(CreateCell(stat.ServiceName, normalFont, rowColor, Element.ALIGN_LEFT, 5));
                statsTable.AddCell(CreateCell(stat.Count.ToString(), normalFont, rowColor, Element.ALIGN_CENTER, 5));
                statsTable.AddCell(CreateCell(stat.Revenue.ToString("N0"), normalFont, rowColor, Element.ALIGN_RIGHT, 5));
                statsTable.AddCell(CreateCell($"{Math.Round(percentage, 2)}%", normalFont, rowColor, Element.ALIGN_CENTER, 5));
            }

            return statsTable;
        }

        private void AddHistogramPage(Document document, PdfWriter writer, Font titleFont, List<ServiceStatistic> serviceStats)
        {
            Paragraph histTitle = new Paragraph("ГИСТОГРАММА ПОПУЛЯРНОСТИ УСЛУГ", titleFont);
            histTitle.Alignment = Element.ALIGN_CENTER;
            histTitle.SpacingAfter = 40;
            document.Add(histTitle);

            CreateHistogram(document, writer, serviceStats);
        }

        private void CreateHistogram(Document document, PdfWriter writer, List<ServiceStatistic> serviceStats)
        {
            if (serviceStats == null || serviceStats.Count == 0) return;

            try
            {
                PdfContentByte cb = writer.DirectContent;
                float pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                float chartWidth = pageWidth - 50;
                float chartHeight = 320;
                float chartX = document.LeftMargin + 25;
                float chartY = document.Top - 380;

                cb.Rectangle(chartX, chartY, chartWidth, chartHeight);
                cb.SetColorStroke(AppBorderColor);
                cb.SetLineWidth(1.5f);
                cb.Stroke();

                cb.MoveTo(chartX + 55, chartY + 20);
                cb.LineTo(chartX + 55, chartY + chartHeight - 20);
                cb.LineTo(chartX + chartWidth - 20, chartY + chartHeight - 20);
                cb.Stroke();

                float drawingWidth = chartWidth - 80;
                float barWidth = drawingWidth / serviceStats.Count * 0.7f;
                float barSpacing = drawingWidth / serviceStats.Count * 0.3f;
                float maxCount = serviceStats.Max(s => s.Count);
                float yScale = (chartHeight - 50) / maxCount;

                var axisFont = CreateFont(8, Font.NORMAL);
                for (int i = 0; i <= 5; i++)
                {
                    float value = maxCount * i / 5;
                    float yPos = chartY + 15 + (chartHeight - 45) * i / 5;
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT,
                        new Phrase(Math.Round(value, 0).ToString(), axisFont),
                        chartX + 48, yPos - 3, 0);
                }

                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase("Количество записей", CreateFont(9, Font.NORMAL)),
                    chartX + 25, chartY + chartHeight / 2, 90);

                float currentX = chartX + 65;
                for (int i = 0; i < serviceStats.Count; i++)
                {
                    var stat = serviceStats[i];
                    float barHeight = Math.Max(stat.Count * yScale, 2);
                    float barY = chartY + 15;

                    cb.SetColorFill(AppChartColor);
                    cb.Rectangle(currentX, barY, barWidth, barHeight);
                    cb.Fill();

                    cb.SetColorStroke(AppBorderColor);
                    cb.Rectangle(currentX, barY, barWidth, barHeight);
                    cb.Stroke();

                    string shortName = stat.ServiceName.Length > 18 ? stat.ServiceName.Substring(0, 15) + "..." : stat.ServiceName;
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        new Phrase(shortName, axisFont),
                        currentX + barWidth / 2, barY - 10, 0);

                    ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                        new Phrase(stat.Count.ToString(), CreateFont(10, Font.BOLD)),
                        currentX + barWidth / 2, barY + barHeight + 5, 0);

                    currentX += barWidth + barSpacing;
                }

                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase("Услуги", CreateFont(9, Font.NORMAL)),
                    chartX + chartWidth / 2, chartY + 8, 0);
            }
            catch (Exception ex)
            {
                document.Add(new Paragraph($"Примечание: не удалось создать гистограмму - {ex.Message}"));
            }
        }

        private void AddPieChartPage(Document document, PdfWriter writer, Font titleFont, Font normalFont, List<ServiceStatistic> serviceStats)
        {
            Paragraph pieTitle = new Paragraph("РАСПРЕДЕЛЕНИЕ УСЛУГ", titleFont);
            pieTitle.Alignment = Element.ALIGN_CENTER;
            pieTitle.SpacingAfter = 30;
            document.Add(pieTitle);

            CreatePieChart(document, writer, serviceStats);
        }

        private void CreatePieChart(Document document, PdfWriter writer, List<ServiceStatistic> serviceStats)
        {
            if (serviceStats == null || serviceStats.Count == 0) return;

            try
            {
                Paragraph subTitle = new Paragraph("Таблица распределения услуг", CreateFont(14, Font.BOLD));
                subTitle.Alignment = Element.ALIGN_CENTER;
                subTitle.SpacingBefore = 20;
                subTitle.SpacingAfter = 15;
                document.Add(subTitle);

                PdfPTable dataTable = new PdfPTable(3);
                dataTable.WidthPercentage = 80;
                dataTable.HorizontalAlignment = Element.ALIGN_CENTER;
                dataTable.SetWidths(new float[] { 50f, 25f, 25f });

                PdfPCell header1 = new PdfPCell(new Phrase("Услуга", CreateFont(12, Font.BOLD)));
                header1.BackgroundColor = AppAccentColor;
                header1.HorizontalAlignment = Element.ALIGN_CENTER;
                header1.Padding = 8;
                dataTable.AddCell(header1);

                PdfPCell header2 = new PdfPCell(new Phrase("Количество", CreateFont(12, Font.BOLD)));
                header2.BackgroundColor = AppAccentColor;
                header2.HorizontalAlignment = Element.ALIGN_CENTER;
                header2.Padding = 8;
                dataTable.AddCell(header2);

                PdfPCell header3 = new PdfPCell(new Phrase("Доля (%)", CreateFont(12, Font.BOLD)));
                header3.BackgroundColor = AppAccentColor;
                header3.HorizontalAlignment = Element.ALIGN_CENTER;
                header3.Padding = 8;
                dataTable.AddCell(header3);

                int totalCount = serviceStats.Sum(s => s.Count);
                int rowNum = 0;
                int colorIndex = 0;

                foreach (var stat in serviceStats.OrderByDescending(s => s.Count))
                {
                    double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;
                    BaseColor rowColor = PieColors[colorIndex % PieColors.Length];

                    PdfPCell nameCell = new PdfPCell(new Phrase(stat.ServiceName, CreateFont(10, Font.NORMAL)));
                    nameCell.BackgroundColor = rowColor;
                    nameCell.Padding = 6;
                    nameCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    dataTable.AddCell(nameCell);

                    PdfPCell countCell = new PdfPCell(new Phrase(stat.Count.ToString(), CreateFont(10, Font.NORMAL)));
                    countCell.BackgroundColor = rowColor;
                    countCell.Padding = 6;
                    countCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    dataTable.AddCell(countCell);

                    PdfPCell percentCell = new PdfPCell(new Phrase($"{Math.Round(percentage, 2)}%", CreateFont(10, Font.BOLD)));
                    percentCell.BackgroundColor = rowColor;
                    percentCell.Padding = 6;
                    percentCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    dataTable.AddCell(percentCell);

                    rowNum++;
                    colorIndex++;
                }

                document.Add(dataTable);

                // Визуализация полосами
                Paragraph barTitle = new Paragraph("Визуализация распределения", CreateFont(12, Font.BOLD));
                barTitle.Alignment = Element.ALIGN_CENTER;
                barTitle.SpacingBefore = 25;
                barTitle.SpacingAfter = 15;
                document.Add(barTitle);

                PdfPTable barTable = new PdfPTable(2);
                barTable.WidthPercentage = 85;
                barTable.HorizontalAlignment = Element.ALIGN_CENTER;
                barTable.SetWidths(new float[] { 40f, 60f });

                colorIndex = 0;
                foreach (var stat in serviceStats.OrderByDescending(s => s.Count))
                {
                    double percentage = totalCount > 0 ? (stat.Count * 100.0 / totalCount) : 0;
                    BaseColor barColor = PieColors[colorIndex % PieColors.Length];

                    PdfPCell nameCell = new PdfPCell(new Phrase(stat.ServiceName, CreateFont(9, Font.NORMAL)));
                    nameCell.Border = Rectangle.NO_BORDER;
                    nameCell.Padding = 5;
                    barTable.AddCell(nameCell);

                    PdfPCell barCell = new PdfPCell();
                    barCell.Border = Rectangle.NO_BORDER;
                    barCell.Padding = 5;

                    Paragraph barPara = new Paragraph();
                    int barLength = (int)(percentage * 2);
                    if (barLength < 1) barLength = 1;
                    if (barLength > 100) barLength = 100;

                    Chunk barChunk = new Chunk(new string('█', barLength));
                   // barChunk.SetFont(CreateFont(10, Font.NORMAL));
                    barChunk.SetBackground(barColor);
                    barPara.Add(barChunk);
                    barPara.Add(new Chunk($" {Math.Round(percentage, 1)}%", CreateFont(10, Font.NORMAL)));

                    barCell.AddElement(barPara);
                    barTable.AddCell(barCell);

                    colorIndex++;
                }

                document.Add(barTable);
            }
            catch (Exception ex)
            {
                document.Add(new Paragraph($"Примечание: не удалось создать диаграмму - {ex.Message}"));
            }
        }

        private List<ServiceStatistic> GetServiceStatisticsFromRecords(List<RecordData> records)
        {
            return records
                .GroupBy(r => r.Service)
                .Select(g => new ServiceStatistic
                {
                    ServiceName = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(r => r.Price)
                })
                .OrderByDescending(s => s.Count)
                .ToList();
        }

        #endregion
    }
}