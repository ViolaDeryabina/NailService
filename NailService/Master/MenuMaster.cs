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
    /// <summary>
    /// Форма главного меню для пользователей с ролью "Мастер"
    /// Отображает расписание записей только для текущего мастера
    /// </summary>
    public partial class MenuMaster : Form
    {
        private string _fio;
        private int _userId;
        private int _masterId;
        private DateTime currentWeekStart;

        /// <summary>
        /// Конструктор формы меню мастера
        /// </summary>
        /// <param name="FIO">ФИО текущего пользователя</param>
        /// <param name="userId">ID пользователя</param>
        public MenuMaster(string FIO, int userId)
        {
            InitializeComponent();
            _fio = FIO;
            _userId = userId;
            _masterId = userId;
            FIOlabel.Text = $"Мастер: {_fio}";

            GetMasterId();
            currentWeekStart = GetMonday(DateTime.Today);
            ApplyCustomStyles();
            FillScheduleWithData();
        }

        /// <summary>
        /// Получение ID мастера по ID пользователя
        /// </summary>
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

        #region Стили и оформление

        /// <summary>
        /// Применение стилей к DataGridView
        /// </summary>
        private void ApplyCustomStyles()
        {
            Color selectionColor = Color.FromArgb(255, 203, 219);
            Color accentColor = Color.HotPink;

            dataGridViewSchedule.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            dataGridViewSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);

            dataGridViewSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewSchedule.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSchedule.ReadOnly = true;

            dataGridViewSchedule.RowHeadersVisible = true;
            dataGridViewSchedule.RowHeadersWidth = 70;
            dataGridViewSchedule.RowTemplate.Height = 80;

            dataGridViewSchedule.CellFormatting += DataGridViewSchedule_CellFormatting;
        }

        /// <summary>
        /// Возвращает цвет для статуса записи
        /// </summary>
        private Color GetStatusColor(int statusId)
        {
            switch (statusId)
            {
                case 3: // Выполнен
                    return Color.FromArgb(197, 225, 165);
                case 1: // Запланирован
                case 2: // Подтвержден
                    return Color.FromArgb(255, 245, 157);
                case 4: // Отменен
                    return Color.FromArgb(255, 171, 145);
                default:
                    return Color.White;
            }
        }

        #endregion

        #region Навигация по неделям

        /// <summary>
        /// Получение понедельника указанной даты
        /// </summary>
        private DateTime GetMonday(DateTime date)
        {
            int delta = DayOfWeek.Monday - date.DayOfWeek;
            if (delta > 0) delta -= 7;
            return date.AddDays(delta);
        }

        /// <summary>
        /// Смена недели
        /// </summary>
        private void ChangeWeek(int weeks)
        {
            currentWeekStart = currentWeekStart.AddDays(weeks * 7);
            FillScheduleWithData();
        }

        /// <summary>
        /// Получение дат текущей недели (пн-пт)
        /// </summary>
        private DateTime[] GetCurrentWeekDates()
        {
            DateTime[] weekDates = new DateTime[5];
            for (int i = 0; i < 5; i++)
            {
                weekDates[i] = currentWeekStart.AddDays(i);
            }
            return weekDates;
        }

        #endregion

        #region Загрузка и отображение расписания

        /// <summary>
        /// Заполнение расписания данными
        /// </summary>
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

        /// <summary>
        /// Создание колонок для дней недели
        /// </summary>
        private void CreateColumns(DateTime[] weekDates)
        {
            Color accentColor = Color.HotPink;

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
                    Font = new Font("MS Reference Sans Serif", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };
            dataGridViewSchedule.Columns.Add(timeColumn);

            for (int i = 0; i < 5; i++)
            {
                string dayName = weekDates[i].ToString("dd.MM");
                string dayOfWeek = GetRussianDayOfWeekFull(weekDates[i].DayOfWeek);
                string headerText = $"{dayOfWeek}\n{dayName}";

                Color headerBackColor = accentColor;
                if (weekDates[i].Date == DateTime.Today)
                {
                    headerBackColor = Color.FromArgb(255, 100, 150);
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
                            Font = new Font("MS Reference Sans Serif", 9, FontStyle.Bold),
                            Alignment = DataGridViewContentAlignment.MiddleCenter,
                            Padding = new Padding(0, 5, 0, 5)
                        }
                    },
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.TopCenter,
                        WrapMode = DataGridViewTriState.True,
                        Font = new Font("MS Reference Sans Serif", 8.5f),
                        BackColor = Color.White,
                        SelectionBackColor = Color.FromArgb(255, 203, 219),
                        SelectionForeColor = Color.Black,
                        ForeColor = Color.Black
                    }
                };
                dataGridViewSchedule.Columns.Add(dayColumn);
            }

            dataGridViewSchedule.Refresh();
        }

        /// <summary>
        /// Получение полного названия дня недели на русском
        /// </summary>
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

        /// <summary>
        /// Добавление строк с временными слотами
        /// </summary>
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

        /// <summary>
        /// Загрузка данных записей текущего мастера из базы данных
        /// </summary>
        private void LoadScheduleData(DateTime[] weekDates)
        {
            try
            {
                for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
                {
                    for (int col = 1; col <= 5; col++)
                    {
                        dataGridViewSchedule.Rows[row].Cells[col].Value = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Tag = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.White;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.Font = new Font("MS Reference Sans Serif", 8.5f);
                        dataGridViewSchedule.Rows[row].Cells[col].Style.ForeColor = Color.Black;
                    }
                }

                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

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
                        AND r.Status IN (1, 2, 3, 4)";

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

                            int dayIndex = -1;
                            for (int i = 0; i < weekDates.Length; i++)
                            {
                                if (weekDates[i].Date == recordDate.Date)
                                {
                                    dayIndex = i;
                                    break;
                                }
                            }

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
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Tag = recordInfo;

                                Color statusColor = GetStatusColor(statusId);
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.BackColor = statusColor;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.ForeColor = Color.Black;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.Font =
                                    new Font("MS Reference Sans Serif", 8.5f, FontStyle.Bold);
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

        /// <summary>
        /// Подсветка текущего временного слота
        /// </summary>
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
                                dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(255, 230, 240);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Форматирование ячеек - подсветка выходных дней
        /// </summary>
        private void DataGridViewSchedule_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                DateTime[] weekDates = GetCurrentWeekDates();
                int dayIndex = e.ColumnIndex - 1;

                if (dayIndex >= 0 && dayIndex < weekDates.Length)
                {
                    DayOfWeek day = weekDates[dayIndex].DayOfWeek;
                    if ((day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) && e.Value == null)
                    {
                        // Можно раскомментировать для подсветки выходных
                        // e.CellStyle.BackColor = Color.FromArgb(255, 240, 245);
                    }
                }
            }
        }

        /// <summary>
        /// Обновление надписи с неделей
        /// </summary>
        private void UpdateWeekLabel()
        {
            DateTime weekEnd = currentWeekStart.AddDays(4);
            string[] months = { "января", "февраля", "марта", "апреля", "мая", "июня",
                               "июля", "августа", "сентября", "октября", "ноября", "декабря" };

            string startStr = $"{currentWeekStart.Day} {months[currentWeekStart.Month - 1]}";
            string endStr = $"{weekEnd.Day} {months[weekEnd.Month - 1]} {weekEnd.Year}";

            lblWeek.Text = $"📅 {startStr} — {endStr}";
            lblWeek.ForeColor = Color.HotPink;
            lblWeek.Font = new Font("MS Reference Sans Serif", 10, FontStyle.Bold);
        }

        #endregion

        #region Обработка событий DataGridView

        /// <summary>
        /// Обработка двойного клика по ячейке - показ информации о записи
        /// </summary>
        private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                if (dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    ShowDetailedRecordInfo(e.RowIndex, e.ColumnIndex);
                }
            }
        }

        /// <summary>
        /// Отображение подробной информации о записи
        /// </summary>
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
                string cellValue = dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                MessageBox.Show($"Запись:\n\n{cellValue}",
                    "Информация о записи",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Навигация по неделям (кнопки)

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

        #endregion

        #region Выход и обновление

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 show = new Form1();
            show.Show();
            this.Hide();
        }

        #endregion

        #region Отчеты и статистика

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            ShowWeekStatistics();
        }

        /// <summary>
        /// Экспорт данных в Excel
        /// </summary>
        private void ExportToExcel()
        {
            try
            {
                DataTable reportData = GetWeeklyReportData();

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет записей за выбранную неделю для формирования отчета!",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет",
                    FileName = $"Отчет_мастера_{_fio}_{currentWeekStart:dd.MM.yyyy}-{currentWeekStart.AddDays(4):dd.MM.yyyy}.xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filePath = saveDialog.FileName;

                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Add();
                Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.ActiveSheet;

                worksheet.PageSetup.Orientation = Microsoft.Office.Interop.Excel.XlPageOrientation.xlLandscape;

                // Заголовок
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

                // Заголовки таблицы
                string[] headers = { "Дата", "Время", "Клиент", "Услуга", "Цена", "Скидка", "Итог", "Статус" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1] = headers[i];
                    worksheet.Cells[4, i + 1].Font.Bold = true;
                    worksheet.Cells[4, i + 1].Font.Name = "Arial";
                    worksheet.Cells[4, i + 1].Interior.Color = Color.HotPink;
                    worksheet.Cells[4, i + 1].Font.Color = Color.White;
                    worksheet.Cells[4, i + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                }

                // Данные
                int row = 5;
                decimal totalSum = 0;

                foreach (DataRow dataRow in reportData.Rows)
                {
                    for (int col = 0; col < reportData.Columns.Count; col++)
                    {
                        worksheet.Cells[row, col + 1] = dataRow[col].ToString();
                        worksheet.Cells[row, col + 1].Font.Name = "Arial";

                        if (col == 4 || col == 6)
                        {
                            if (decimal.TryParse(dataRow[col].ToString(), out decimal price))
                            {
                                worksheet.Cells[row, col + 1] = price;
                                //worksheet.Cells[row, col + 1].NumberFormat = "#,##0.00";
                                worksheet.Cells[row, col + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                                if (col == 6) totalSum += price;
                            }
                        }
                        else if (col == 0 || col == 1)
                        {
                            worksheet.Cells[row, col + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        }

                        // Подкрашивание статуса
                        if (col == 7)
                        {
                            string status = dataRow[col].ToString();
                            if (status.Contains("Запланирован") || status.Contains("Подтвержден"))
                                worksheet.Cells[row, col + 1].Interior.Color = Color.FromArgb(255, 245, 157);
                            else if (status.Contains("Выполнен"))
                                worksheet.Cells[row, col + 1].Interior.Color = Color.FromArgb(197, 225, 165);
                            else if (status.Contains("Отменен"))
                                worksheet.Cells[row, col + 1].Interior.Color = Color.FromArgb(255, 171, 145);
                        }
                    }
                    row++;
                }

                // Итог
                worksheet.Cells[row, 1] = "ИТОГО:";
                worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 6]].Merge();
                worksheet.Cells[row, 1].Font.Bold = true;
                worksheet.Cells[row, 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[row, 1].Interior.Color = Color.FromArgb(255, 203, 219);

                worksheet.Cells[row, 7] = totalSum;
                worksheet.Cells[row, 7].Font.Bold = true;
                //
                //worksheet.Cells[row, 7].NumberFormat = "#,##0.00";
                worksheet.Cells[row, 7].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                worksheet.Cells[row, 7].Interior.Color = Color.FromArgb(255, 203, 219);

                worksheet.Columns.AutoFit();
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                MessageBox.Show($"Отчет успешно сохранен!\n\n{filePath}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information); 
                if (MessageBox.Show("Открыть отчет?", "Вопрос",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Получение данных для недельного отчета
        /// </summary>
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
                        AND r.Status != 4
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

        /// <summary>
        /// Отображение статистики за неделю
        /// </summary>
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
                            SUM(CASE WHEN r.Status = 3 THEN s.Price ELSE 0 END) as revenue_completed
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

                            string stats = $"📊 СТАТИСТИКА ЗА НЕДЕЛЮ\n\n" +
                                           $"📅 Период: {weekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}\n" +
                                           $"━━━━━━━━━━━━━━━━━━━━━━\n" +
                                           $"✅ Выполнено: {completed}\n" +
                                           $"💰 Выручка: {revenueCompleted:N0} руб.\n" +
                                           $"📌 Подтверждено: {confirmed}\n" +
                                           $"⏳ Запланировано: {planned}\n" +
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

        #endregion

        private void MenuMaster_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }
    }

    /// <summary>
    /// Класс для хранения информации о записи в Tag ячейки DataGridView
    /// </summary>
    public class RecordInfo
    {
        public int RecordId { get; set; }
        public string ClientName { get; set; }
        public string Service { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
    }
}