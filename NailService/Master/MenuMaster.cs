using MySql.Data.MySqlClient;
using NailServiceApp.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public partial class MenuMaster : Form
    {
        private string _fio;
        private int _userId;
        private int _masterId;
        private DateTime currentWeekStart;

        public MenuMaster(string FIO, int userId)
        {
            InitializeComponent();
            _fio = FIO;
            _userId = userId;
            _masterId = userId;
            FIOlabel.Text = $"Мастер: {_fio}";

            // Получаем ID мастера по ID пользователя
            GetMasterId();

            currentWeekStart = GetMonday(DateTime.Today);
            ApplyCustomStyles();
            FillScheduleWithData();
        }

        private void GetMasterId()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT IDMasters FROM Masters WHERE User = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserId", _userId);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        _masterId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения ID мастера: {ex.Message}");
            }
        }

        private void ApplyCustomStyles()
        {
            // Цвета как в Schedule
            Color selectionColor = Color.FromArgb(255, 203, 219); // Цвет выделения
            Color accentColor = Color.HotPink; // Акцентный цвет

            // Применяем стили
            dataGridViewSchedule.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            dataGridViewSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);

            // Цвет выделения ВСЕЙ строки
            dataGridViewSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewSchedule.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Настройки таблицы
            dataGridViewSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSchedule.ReadOnly = true;

            // Настройки для расписания
            dataGridViewSchedule.RowHeadersVisible = true;
            dataGridViewSchedule.RowHeadersWidth = 70;
            dataGridViewSchedule.RowTemplate.Height = 80;

            // Подсветка выходных
            dataGridViewSchedule.CellFormatting += DataGridViewSchedule_CellFormatting;
        }

        private DateTime GetMonday(DateTime date)
        {
            int delta = DayOfWeek.Monday - date.DayOfWeek;
            if (delta > 0) delta -= 7;
            return date.AddDays(delta);
        }

        private void ChangeWeek(int weeks)
        {
            currentWeekStart = currentWeekStart.AddDays(weeks * 7);
            FillScheduleWithData();
        }

        private void FillScheduleWithData()
        {
            try
            {
                UpdateWeekLabel();

                var weekDates = new DateTime[5];
                for (int i = 0; i < 5; i++)
                {
                    weekDates[i] = currentWeekStart.AddDays(i);
                }

                dataGridViewSchedule.Rows.Clear();
                dataGridViewSchedule.Columns.Clear();

                CreateColumns(weekDates);
                AddTimeRows();
                LoadScheduleData(weekDates);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateColumns(DateTime[] weekDates)

        {

            Color accentColor = Color.HotPink;
            // Колонка для времени
            DataGridViewTextBoxColumn timeColumn = new DataGridViewTextBoxColumn
            {


                Name = "Time",
                HeaderText = "Время",
                ReadOnly = true,
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = accentColor,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dataGridViewSchedule.Columns.Add(timeColumn);

            // Колонки для дней недели
            for (int i = 0; i < 5; i++)
            {
                string dayName = weekDates[i].ToString("dd.MM");
                string dayOfWeek = GetRussianDayOfWeekFull(weekDates[i].DayOfWeek);
                string headerText = $"{dayOfWeek}\n{dayName}";

                Color headerBackColor = accentColor;
                if (weekDates[i].Date == DateTime.Today)
                {
                    headerBackColor = Color.FromArgb(255, 100, 150); // Чуть ярче для сегодня
                }

                DataGridViewTextBoxColumn dayColumn = new DataGridViewTextBoxColumn
                {
                    Name = headerText,
                    HeaderText = headerText,
                    ReadOnly = true,
                    Width = 150,
                    HeaderCell = new DataGridViewColumnHeaderCell
                    {
                        Style = new DataGridViewCellStyle
                        {
                            BackColor = headerBackColor,
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            Alignment = DataGridViewContentAlignment.MiddleCenter
                        }
                    },
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.TopCenter,
                        WrapMode = DataGridViewTriState.True,
                        Font = new Font("Segoe UI", 9),
                        BackColor = Color.White,
                        SelectionBackColor = Color.FromArgb(220, 235, 252),
                        SelectionForeColor = Color.Black
                    }
                };
                dataGridViewSchedule.Columns.Add(dayColumn);
            }
        }

        private void AddTimeRows()
        {
            int[] timeSlots = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

            foreach (int hour in timeSlots)
            {
                int rowIndex = dataGridViewSchedule.Rows.Add();
                dataGridViewSchedule.Rows[rowIndex].Cells["Time"].Value = $"{hour:00}:00";

                for (int col = 1; col <= 5; col++)
                {
                    dataGridViewSchedule.Rows[rowIndex].Cells[col].Style.BackColor = Color.White;
                }
            }
        }

        private void LoadScheduleData(DateTime[] weekDates)
        {
            try
            {
                // Очищаем ячейки
                for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
                {
                    for (int col = 1; col <= 5; col++)
                    {
                        dataGridViewSchedule.Rows[row].Cells[col].Value = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Tag = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.White;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.Font = new Font("Segoe UI", 9);
                        dataGridViewSchedule.Rows[row].Cells[col].Style.ForeColor = Color.Black;
                    }
                }

                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    // Загружаем только записи текущего мастера
                    string query = @"
                SELECT 
                    r.IDRecord,
                    r.Date,
                    c.LastName as ClientLastName,
                    c.FirstName as ClientFirstName,
                    c.MiddleName as ClientMiddleName,
                    s.ServiceName,
                    s.Price,
                    stat.StatusName,
                    stat.IDStatus
                FROM Record r
                INNER JOIN Client c ON r.Client = c.IDClient
                INNER JOIN Services s ON r.Service = s.IDServices
                INNER JOIN Status stat ON r.Status = stat.IDStatus
                WHERE r.Master = @MasterId 
                AND r.Date BETWEEN @startDate AND @endDate 
                AND r.Status IN (1, 2, 3)"; // Все активные статусы

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MasterId", _masterId);
                    cmd.Parameters.AddWithValue("@startDate", weekDates[0].Date);
                    cmd.Parameters.AddWithValue("@endDate", weekDates[4].Date.AddDays(1).AddSeconds(-1));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime recordDate = Convert.ToDateTime(reader["Date"]);

                            string clientName = NameFormatter.FormatToShortName(
                                reader["ClientLastName"].ToString(),
                                reader["ClientFirstName"].ToString(),
                                reader["ClientMiddleName"].ToString()
                            );

                            string service = reader["ServiceName"].ToString();
                            int statusId = Convert.ToInt32(reader["IDStatus"]);
                            string statusName = reader["StatusName"].ToString();
                            decimal price = Convert.ToDecimal(reader["Price"]);

                            // Находим день
                            int dayIndex = -1;
                            for (int i = 0; i < weekDates.Length; i++)
                            {
                                if (weekDates[i].Date == recordDate.Date)
                                {
                                    dayIndex = i;
                                    break;
                                }
                            }

                            // Находим время
                            int timeIndex = -1;
                            for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
                            {
                                string timeValue = dataGridViewSchedule.Rows[row].Cells["Time"].Value?.ToString();
                                if (timeValue == $"{recordDate.Hour:00}:00")
                                {
                                    timeIndex = row;
                                    break;
                                }
                            }

                            if (dayIndex >= 0 && timeIndex >= 0)
                            {
                                // СОЗДАЕМ ОБЪЕКТ RecordInfo И СОХРАНЯЕМ В TAG
                                RecordInfo recordInfo = new RecordInfo
                                {
                                    RecordId = Convert.ToInt32(reader["IDRecord"]),
                                    ClientName = clientName,
                                    Service = service,
                                    Date = recordDate,
                                    Status = statusName,
                                    Price = price
                                };

                                string cellValue = $"{clientName}\n{service}";
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Value = cellValue;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Tag = recordInfo; // ТЕПЕРЬ ТУТ ОБЪЕКТ!

                                // Используем цвет статуса из StyleManager
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.BackColor =
                                    StyleManager.GetStatusColor(statusId);
                            }
                        }
                    }
                }

                HighlightCurrentTimeSlot();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                if (dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    // Показываем подробную информацию о записи
                    ShowDetailedRecordInfo(e.RowIndex, e.ColumnIndex);
                }
            }
        }



        private void ShowDetailedRecordInfo(int rowIndex, int columnIndex)
        {
            if (dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Tag is RecordInfo recordInfo)
            {
                string message = $"📋 ИНФОРМАЦИЯ О ЗАПИСИ\n\n" +
                                $"👤 Клиент: {recordInfo.ClientName}\n" +
                                $"💇 Услуга: {recordInfo.Service}\n" +
                                $"💰 Стоимость: {recordInfo.Price:N0} руб.\n" +
                                $"📅 Дата: {recordInfo.Date:dd.MM.yyyy}\n" +
                                $"⏰ Время: {recordInfo.Date:HH:mm}\n" +
                                $"📊 Статус: {recordInfo.Status}";

                MessageBox.Show(message, "Детали записи",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Если Tag не содержит RecordInfo, показываем простую информацию
                string cellValue = dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                MessageBox.Show($"Запись:\n\n{cellValue}",
                    "Информация о записи",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }



        private void DataGridViewSchedule_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Подсветка выходных
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                DateTime[] weekDates = GetCurrentWeekDates();
                int dayIndex = e.ColumnIndex - 1;

                if (dayIndex >= 0 && dayIndex < weekDates.Length)
                {
                    DayOfWeek day = weekDates[dayIndex].DayOfWeek;
                    if ((day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) && e.Value == null)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(250, 240, 240);
                    }
                }
            }
        }

        private void HighlightCurrentTimeSlot()
        {
            int currentHour = DateTime.Now.Hour;

            for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
            {
                string timeValue = dataGridViewSchedule.Rows[row].Cells["Time"].Value?.ToString();
                if (!string.IsNullOrEmpty(timeValue))
                {
                    int rowHour = int.Parse(timeValue.Split(':')[0]);
                    if (rowHour == currentHour)
                    {
                        for (int col = 1; col <= 5; col++)
                        {
                            if (dataGridViewSchedule.Rows[row].Cells[col].Value == null)
                            {
                                //dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(255, 255, 200);
                            }
                        }
                    }
                }
            }
        }

        private DateTime[] GetCurrentWeekDates()
        {
            DateTime[] weekDates = new DateTime[5];
            for (int i = 0; i < 5; i++)
            {
                weekDates[i] = currentWeekStart.AddDays(i);
            }
            return weekDates;
        }

        private string GetRussianDayOfWeekFull(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return "Понедельник";
                case DayOfWeek.Tuesday: return "Вторник";
                case DayOfWeek.Wednesday: return "Среда";
                case DayOfWeek.Thursday: return "Четверг";
                case DayOfWeek.Friday: return "Пятница";
                case DayOfWeek.Saturday: return "Суббота";
                case DayOfWeek.Sunday: return "Воскресенье";
                default: return "";
            }
        }

        private void UpdateWeekLabel()
        {
            DateTime weekEnd = currentWeekStart.AddDays(4);
            string[] months = { "января", "февраля", "марта", "апреля", "мая", "июня",
                               "июля", "августа", "сентября", "октября", "ноября", "декабря" };

            string startStr = $"{currentWeekStart.Day} {months[currentWeekStart.Month - 1]}";
            string endStr = $"{weekEnd.Day} {months[weekEnd.Month - 1]} {weekEnd.Year}";

            lblWeek.Text = $"📅 {startStr} — {endStr}";
        }

        // Навигация по неделям
        private void btnPrevWeek_Click(object sender, EventArgs e)
        {
            ChangeWeek(-1);
        }

        private void btnNextWeek_Click(object sender, EventArgs e)
        {
            ChangeWeek(1);
        }

        private void btnCurrentWeek_Click(object sender, EventArgs e)
        {
            currentWeekStart = GetMonday(DateTime.Today);
            FillScheduleWithData();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Close();
        }

        // Дополнительные методы, если нужны
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillScheduleWithData();
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void ExportToExcel()
        {
            try
            {
                // Получаем данные за текущую неделю
                DataTable reportData = GetWeeklyReportData();

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет записей за выбранную неделю для формирования отчета!",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Создаем диалог сохранения файла
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет",
                    FileName = $"Отчет_мастера_{_fio}_{currentWeekStart:dd.MM.yyyy}-{currentWeekStart.AddDays(4):dd.MM.yyyy}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                // Создаем Excel приложение
                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.ActiveSheet;

                // Настройка страницы
                worksheet.PageSetup.Orientation = Microsoft.Office.Interop.Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
                worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);

                // ЗАГОЛОВОК
                worksheet.Cells[1, 1] = $"ОТЧЕТ МАСТЕРА {_fio.ToUpper()}";
                worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge();
                worksheet.Cells[1, 1].Font.Size = 16;
                worksheet.Cells[1, 1].Font.Bold = true;
                worksheet.Cells[1, 1].Font.Name = "Arial";
                worksheet.Cells[1, 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                worksheet.Cells[1, 1].Interior.Color = Color.FromArgb(255, 203, 219);

                // Период
                worksheet.Cells[2, 1] = $"За период: {currentWeekStart:dd.MM.yyyy} - {currentWeekStart.AddDays(4):dd.MM.yyyy}";
                worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[2, 8]].Merge();
                worksheet.Cells[2, 1].Font.Size = 12;
                worksheet.Cells[2, 1].Font.Bold = true;
                worksheet.Cells[2, 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                // Дата формирования
                worksheet.Cells[3, 1] = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, 8]].Merge();
                worksheet.Cells[3, 1].Font.Size = 10;
                worksheet.Cells[3, 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                // Пустая строка
                worksheet.Rows[4].RowHeight = 15;

                // ЗАГОЛОВКИ ТАБЛИЦЫ
                string[] headers = { "Дата", "Время", "Клиент", "Услуга", "Цена", "Скидка", "Итог", "Статус" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[5, i + 1] = headers[i];
                    worksheet.Cells[5, i + 1].Font.Bold = true;
                    worksheet.Cells[5, i + 1].Font.Name = "Arial";
                    worksheet.Cells[5, i + 1].Interior.Color = Color.HotPink;
                    worksheet.Cells[5, i + 1].Font.Color = Color.White;
                    worksheet.Cells[5, i + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    worksheet.Cells[5, i + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                }

                // ДАННЫЕ
                int row = 6;
                decimal totalSum = 0;

                foreach (DataRow dataRow in reportData.Rows)
                {
                    for (int col = 0; col < reportData.Columns.Count; col++)
                    {
                        object cellValue = dataRow[col];
                        worksheet.Cells[row, col + 1] = cellValue;
                        worksheet.Cells[row, col + 1].Font.Name = "Arial";
                        worksheet.Cells[row, col + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                        // Выравнивание
                        if (col == 0 || col == 1) // Дата и время по центру
                        {
                            worksheet.Cells[row, col + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        }
                        else if (col == 4 || col == 6) // Цены по правому краю
                        {
                            worksheet.Cells[row, col + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;

                            // Пытаемся преобразовать в число для форматирования
                            if (cellValue != null && !string.IsNullOrEmpty(cellValue.ToString()))
                            {
                                if (decimal.TryParse(cellValue.ToString(), out decimal price))
                                {
                                    worksheet.Cells[row, col + 1] = price;

                                    // Устанавливаем числовой формат ТОЛЬКО если это число
                                    try
                                    {
                                        worksheet.Cells[row, col + 1].NumberFormat = "#,##0.00";
                                    }
                                    catch { /* Игнорируем ошибки форматирования */ }

                                    if (col == 6) // Итоговая цена
                                    {
                                        totalSum += price;
                                    }
                                }
                            }
                        }
                        else // Остальное по левому краю
                        {
                            worksheet.Cells[row, col + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                        }
                    }
                    row++;
                }

                // ИТОГОВАЯ СТРОКА
                worksheet.Cells[row, 1] = "ИТОГО:";
                worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 6]].Merge();
                worksheet.Cells[row, 1].Font.Bold = true;
                worksheet.Cells[row, 1].Font.Size = 12;
                worksheet.Cells[row, 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[row, 1].Interior.Color = Color.FromArgb(255, 203, 219);
                worksheet.Cells[row, 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                worksheet.Cells[row, 7] = totalSum;
                worksheet.Cells[row, 7].Font.Bold = true;
                worksheet.Cells[row, 7].Font.Size = 12;

                // Устанавливаем числовой формат для итога
                try
                {
                    worksheet.Cells[row, 7].NumberFormat = "#,##0.00";
                }
                catch { /* Игнорируем ошибки форматирования */ }

                worksheet.Cells[row, 7].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[row, 7].Interior.Color = Color.FromArgb(255, 203, 219);
                worksheet.Cells[row, 7].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                worksheet.Cells[row, 8] = "";
                worksheet.Cells[row, 8].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                // АВТОПОДБОР ШИРИНЫ
                worksheet.Columns.AutoFit();

                // Сохраняем файл
                workbook.SaveAs(saveDialog.FileName);
                workbook.Close();
                excelApp.Quit();

                // Освобождаем ресурсы
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);

                MessageBox.Show($"Отчет успешно сохранен!\n\n{saveDialog.FileName}",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Спрашиваем, открыть ли файл
                if (MessageBox.Show("Открыть отчет?", "Вопрос",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetWeeklyReportData()
        {
            DataTable dt = new DataTable();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    DateTime weekStart = currentWeekStart.Date;
                    DateTime weekEnd = currentWeekStart.AddDays(4).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    string query = @"
                SELECT 
                    DATE_FORMAT(r.Date, '%d.%m.%Y') as 'Дата',
                    DATE_FORMAT(r.Date, '%H:%i') as 'Время',
                    CONCAT(c.LastName, ' ', LEFT(c.FirstName, 1), '.', LEFT(c.MiddleName, 1), '.') as 'Клиент',
                    s.ServiceName as 'Услуга',
                    s.Price as 'Цена',
                    CASE 
                        WHEN r.discount = 1 THEN '5%' 
                        ELSE '-' 
                    END as 'Скидка',
                    s.Price as 'Итоговая цена',
                    stat.StatusName as 'Статус'
                FROM Record r
                INNER JOIN Client c ON r.Client = c.IDClient
                INNER JOIN Services s ON r.Service = s.IDServices
                INNER JOIN Status stat ON r.Status = stat.IDStatus
                WHERE r.Master = @MasterId 
                AND r.Date BETWEEN @startDate AND @endDate 
                AND r.Status != 4  -- Исключаем отмененные записи (статус 4)
                ORDER BY r.Date";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MasterId", _masterId);
                    cmd.Parameters.AddWithValue("@startDate", weekStart);
                    cmd.Parameters.AddWithValue("@endDate", weekEnd);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных для отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        private void ShowWeekStatistics()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    DateTime weekStart = currentWeekStart.Date;
                    DateTime weekEnd = currentWeekStart.AddDays(4).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    string query = @"
                SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN r.Status = 1 THEN 1 ELSE 0 END) as planned,
                    SUM(CASE WHEN r.Status = 2 THEN 1 ELSE 0 END) as confirmed,
                    SUM(CASE WHEN r.Status = 3 THEN 1 ELSE 0 END) as completed,
                    SUM(CASE WHEN r.Status = 4 THEN 1 ELSE 0 END) as cancelled,
                    SUM(CASE WHEN r.Status = 3 THEN s.Price ELSE 0 END) as revenue_completed,
                    SUM(CASE WHEN r.Status IN (1,2) THEN s.Price ELSE 0 END) as revenue_potential
                FROM Record r
                INNER JOIN Services s ON r.Service = s.IDServices
                WHERE r.Master = @MasterId 
                AND r.Date BETWEEN @startDate AND @endDate";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MasterId", _masterId);
                    cmd.Parameters.AddWithValue("@startDate", weekStart);
                    cmd.Parameters.AddWithValue("@endDate", weekEnd);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int total = reader.GetInt32(0);
                            int planned = reader.GetInt32(1);
                            int confirmed = reader.GetInt32(2);
                            int completed = reader.GetInt32(3);
                            int cancelled = reader.GetInt32(4);
                            decimal revenueCompleted = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            decimal revenuePotential = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);

                            string stats = $"📊 СТАТИСТИКА ЗА НЕДЕЛЮ\n\n" +
                                           $"📅 Период: {weekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}\n" +
                                           $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                                           $"✅ Выполнено: {completed}\n" +
                                           $"💰 Выручка: {revenueCompleted:N0} руб.\n" +
                                           $"📌 Подтверждено: {confirmed}\n" +
                                           $"⏳ Запланировано: {planned}\n" +
                                           $"💵 Потенц. выручка: {revenuePotential:N0} руб.\n" +
                                           $"❌ Отменено: {cancelled}\n" +
                                           $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                                           $"📊 Всего записей: {total}";

                            MessageBox.Show(stats, "Статистика",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения статистики: {ex.Message}");
            }
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            ShowWeekStatistics();

        }
    }
}

    
    public class RecordInfo
    {
        public int RecordId { get; set; }
        public string ClientName { get; set; }
        public string Service { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
    }
