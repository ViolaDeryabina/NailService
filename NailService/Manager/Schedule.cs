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
    /// Форма расписания записей на неделю
    /// Отображает записи по дням и временным слотам с цветовой индикацией статусов
    /// </summary>
    public partial class Schedule : Form
    {
        private DateTime currentWeekStart;
        private int _roleID;
        private string _userFIO;
        private int _userId;

        /// <summary>
        /// Конструктор формы расписания
        /// </summary>
        /// <param name="userFIO">ФИО текущего пользователя</param>
        /// <param name="roleID">ID роли пользователя</param>
        /// <param name="userId">ID пользователя</param>
        public Schedule(string userFIO, int roleID, int userId)
        {
            InitializeComponent();
            Rectangle screenBounds = Screen.PrimaryScreen.WorkingArea;

            // Если форма больше экрана, масштабируем
            if (this.Size.Height > screenBounds.Height || this.Size.Width > screenBounds.Width)
            {
                // Вариант А: Масштабируем форму
                float scaleX = (float)screenBounds.Width / this.Size.Width;
                float scaleY = (float)screenBounds.Height / this.Size.Height;
                float scale = Math.Min(scaleX, scaleY);

                if (scale < 1)
                {
                    this.Scale(new SizeF(scale, scale));
                    this.Size = new Size((int)(this.Size.Width * scale), (int)(this.Size.Height * scale));
                }

                // Вариант Б: Включаем прокрутку
                // this.AutoScroll = true;
                // this.Size = new Size(Math.Min(this.Size.Width, screenBounds.Width), 
                //                       Math.Min(this.Size.Height, screenBounds.Height));
            }

            // Центрируем форму на экране
            this.StartPosition = FormStartPosition.CenterScreen;
            _userFIO = userFIO;
            _roleID = roleID;
            _userId = userId;
            currentWeekStart = GetMonday(DateTime.Today);

            ApplyCustomStyles();
            FillScheduleWithData();

            FIOlabel.Text = $"Менеджер: {_userFIO}";
            LoadUserData();

        }

        private void LoadUserData()
        {
            if (_userId == 0)
            {
                _roleID = 4; // Менеджер по умолчанию
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

        #region Загрузка и отображение данных

        /// <summary>
        /// Заполнение расписания данными
        /// </summary>
        private void FillScheduleWithData()
        {
            try
            {
                DateTime weekEnd = currentWeekStart.AddDays(4);
                label4.Text = $"Неделя: {currentWeekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";
                label4.ForeColor = Color.Black;
                label4.Font = new Font("MS Reference Sans Serif", 12, FontStyle.Bold);

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
        /// Загрузка данных записей из базы данных
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
                            stat.IDStatus,
                            u_m.LastName as MasterLastName,
                            u_m.FirstName as MasterFirstName
                        FROM Record r
                        INNER JOIN Client c ON r.Client = c.IDClient
                        INNER JOIN Services s ON r.Service = s.IDServices
                        INNER JOIN Status stat ON r.Status = stat.IDStatus
                        INNER JOIN Masters m ON r.Master = m.IDMasters
                        INNER JOIN Users u_m ON m.User = u_m.IDUser
                        WHERE r.Date BETWEEN @startDate AND @endDate 
                        AND r.Status IN (1, 2, 3, 4)";

                    MySqlCommand cmd = new MySqlCommand(query, con);
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
                            string masterName = NameFormatter.FormatToShortName(
                                reader["MasterLastName"].ToString(),
                                reader["MasterFirstName"].ToString(),
                                ""
                            );
                            int statusId = Convert.ToInt32(reader["IDStatus"]);

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
                                string cellValue = $"{clientName}\n{service}\nмастер: {masterName}";
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Value = cellValue;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Tag = reader["IDRecord"];

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

        #endregion

        #region Обработка событий DataGridView

        /// <summary>
        /// Обработка двойного клика по ячейке
        /// </summary>
        private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                if (dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    int recordId = Convert.ToInt32(dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag);
                    RecordsInfo recordsInfo = new RecordsInfo(recordId, _userFIO, _roleID);

                    if (recordsInfo.ShowDialog() == DialogResult.OK)
                    {
                        FillScheduleWithData();
                    }
                }
                else
                {
                    CreateNewRecord(e.RowIndex, e.ColumnIndex);
                }
            }
        }

        /// <summary>
        /// Создание новой записи
        /// </summary>
        private void CreateNewRecord(int rowIndex, int columnIndex)
        {
            DateTime[] weekDates = GetCurrentWeekDates();
            DateTime selectedDate = weekDates[columnIndex - 1];

            string timeStr = dataGridViewSchedule.Rows[rowIndex].Cells["Time"].Value?.ToString();
            if (string.IsNullOrEmpty(timeStr)) return;

            int hour = int.Parse(timeStr.Split(':')[0]);
            DateTime selectedDateTime = selectedDate.Date.AddHours(hour);

            if (selectedDateTime < DateTime.Now)
            {
                MessageBox.Show("Нельзя создать запись на прошедшее время!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RecordingClients recordingForm = new RecordingClients(_userFIO);
            recordingForm.SetSelectedDateTime(selectedDateTime);

            if (recordingForm.ShowDialog() == DialogResult.OK)
            {
                FillScheduleWithData();
            }
        }

        /// <summary>
        /// Отмена записи
        /// </summary>
        private void CancelRecord(int recordId, int rowIndex, int columnIndex)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "UPDATE Record SET Status = 4 WHERE IDRecord = @IDRecord";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IDRecord", recordId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Value = null;
                        dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Tag = null;
                        dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.White;

                        MessageBox.Show("Запись успешно отменена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        #region Переход к другим формам

        private void ListButton_Click(object sender, EventArgs e)
        {
            Show showForm = new Show(_userFIO, _roleID);
            showForm.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, EventArgs e)
        {
            ShowReports showReports = new ShowReports(_userFIO, _roleID);
            showReports.Show();
            this.Hide();
        }

        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2)
            {
                MenuAdmin menuAdmin = new MenuAdmin(_userFIO);
                menuAdmin.Show();
            }
            else
            {
                MenuManager menuManager = new MenuManager(_userFIO);
                menuManager.Show();
            }
            this.Hide();
        }

        #endregion

        #region Генерация отчетов

        private void btnMonthlyReport_Click(object sender, EventArgs e)
        {
            GenerateMonthlyReport();
        }

        /// <summary>
        /// Получение начала месяца
        /// </summary>
        private DateTime GetMonthStart(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Получение конца месяца
        /// </summary>
        private DateTime GetMonthEnd(DateTime date)
        {
            return GetMonthStart(date).AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        /// <summary>
        /// Получение данных для месячного отчета
        /// </summary>
        private DataTable GetMonthlyReportData(DateTime monthDate)
        {
            DataTable dt = new DataTable();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    DateTime monthStart = GetMonthStart(monthDate);
                    DateTime monthEnd = GetMonthEnd(monthDate);

                    string query = @"
                        SELECT 
                            DATE_FORMAT(r.Date, '%d.%m.%Y') as 'Дата',
                            DATE_FORMAT(r.Date, '%H:%i') as 'Время',
                            CONCAT(c.LastName, ' ', LEFT(c.FirstName, 1), '.', LEFT(c.MiddleName, 1), '.') as 'Клиент',
                            CONCAT(u_m.LastName, ' ', LEFT(u_m.FirstName, 1), '.') as 'Мастер',
                            s.ServiceName as 'Услуга',
                            s.Price as 'Цена',
                            CASE 
                                WHEN r.discount = 1 THEN '5%' 
                                ELSE '-' 
                            END as 'Скидка',
                            CASE 
                                WHEN r.Status = 1 THEN 'Запланирован'
                                WHEN r.Status = 2 THEN 'Подтвержден'
                                WHEN r.Status = 3 THEN 'Выполнен'
                                WHEN r.Status = 4 THEN 'Отменен'
                                ELSE 'Неизвестно'
                            END as 'Статус',
                            u_u.LastName as 'Менеджер'
                        FROM Record r
                        INNER JOIN Client c ON r.Client = c.IDClient
                        INNER JOIN Services s ON r.Service = s.IDServices
                        INNER JOIN Masters m ON r.Master = m.IDMasters
                        INNER JOIN Users u_m ON m.User = u_m.IDUser
                        INNER JOIN Users u_u ON r.User = u_u.IDUser
                        WHERE r.Date BETWEEN @startDate AND @endDate 
                        ORDER BY r.Date";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@startDate", monthStart);
                    cmd.Parameters.AddWithValue("@endDate", monthEnd);

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
        /// Получение статистики за месяц
        /// </summary>
        private Dictionary<string, decimal> GetMonthlyStatistics(DateTime monthDate)
        {
            Dictionary<string, decimal> stats = new Dictionary<string, decimal>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    DateTime monthStart = GetMonthStart(monthDate);
                    DateTime monthEnd = GetMonthEnd(monthDate);

                    string query = @"
                        SELECT 
                            COUNT(*) as TotalRecords,
                            SUM(CASE WHEN r.Status = 3 THEN 1 ELSE 0 END) as Completed,
                            SUM(CASE WHEN r.Status = 4 THEN 1 ELSE 0 END) as Cancelled,
                            SUM(CASE WHEN r.Status = 1 THEN 1 ELSE 0 END) as Planned,
                            SUM(CASE WHEN r.Status = 2 THEN 1 ELSE 0 END) as Confirmed,
                            SUM(CASE WHEN r.Status = 3 THEN s.Price ELSE 0 END) as Revenue,
                            COUNT(DISTINCT r.Master) as ActiveMasters,
                            COUNT(DISTINCT r.Client) as UniqueClients
                        FROM Record r
                        INNER JOIN Services s ON r.Service = s.IDServices
                        WHERE r.Date BETWEEN @startDate AND @endDate";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@startDate", monthStart);
                    cmd.Parameters.AddWithValue("@endDate", monthEnd);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats["TotalRecords"] = Convert.ToDecimal(reader["TotalRecords"]);
                            stats["Completed"] = Convert.ToDecimal(reader["Completed"]);
                            stats["Cancelled"] = Convert.ToDecimal(reader["Cancelled"]);
                            stats["Planned"] = Convert.ToDecimal(reader["Planned"]);
                            stats["Confirmed"] = Convert.ToDecimal(reader["Confirmed"]);
                            stats["Revenue"] = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            stats["ActiveMasters"] = Convert.ToDecimal(reader["ActiveMasters"]);
                            stats["UniqueClients"] = Convert.ToDecimal(reader["UniqueClients"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения статистики: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// Получение названия месяца
        /// </summary>
        private string GetMonthName(int month)
        {
            string[] months = { "январь", "февраль", "март", "апрель", "май", "июнь",
                               "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь" };
            return months[month - 1];
        }

        /// <summary>
        /// Генерация Excel-отчета за месяц
        /// </summary>
        private void GenerateMonthlyReport()
        {
            
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
            Microsoft.Office.Interop.Excel.Range range = null;

            try
            {
                DateTime selectedMonth = currentWeekStart;

                DataTable reportData = GetMonthlyReportData(selectedMonth);
                Dictionary<string, decimal> statistics = GetMonthlyStatistics(selectedMonth);

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show($"Нет записей за {GetMonthName(selectedMonth.Month)} {selectedMonth.Year} для формирования отчета!",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    Title = "Сохранить отчет",
                    FileName = $"Отчет_за_{GetMonthName(selectedMonth.Month)}_{selectedMonth.Year}.xlsx",
                    DefaultExt = "xlsx"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filePath = saveDialog.FileName;

                excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.ActiveSheet;

                worksheet.PageSetup.Orientation = Microsoft.Office.Interop.Excel.XlPageOrientation.xlLandscape;
                worksheet.PageSetup.LeftMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.RightMargin = excelApp.CentimetersToPoints(1);
                worksheet.PageSetup.TopMargin = excelApp.CentimetersToPoints(1.5);
                worksheet.PageSetup.BottomMargin = excelApp.CentimetersToPoints(1);

                Color accentColor = Color.HotPink;
                Color lightPink = Color.FromArgb(255, 203, 219);

                // Заголовок
                range = worksheet.Range["A1", "I1"];
                range.Merge();
                range.Value = $"МЕСЯЧНЫЙ ОТЧЕТ";
                range.Font.Size = 20;
                range.Font.Bold = true;
                range.Font.Name = "Arial";
                range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                range.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Период
                range = worksheet.Range["A2", "I2"];
                range.Merge();
                DateTime monthStart = GetMonthStart(selectedMonth);
                DateTime monthEnd = GetMonthEnd(selectedMonth);
                range.Value = $"{GetMonthName(selectedMonth.Month).ToUpper()} {selectedMonth.Year}";
                range.Font.Size = 14;
                range.Font.Bold = true;
                range.Font.Name = "Arial";
                range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Дата формирования
                range = worksheet.Range["A3", "I3"];
                range.Merge();
                range.Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                range.Font.Size = 10;
                range.Font.Name = "Arial";
                range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                // Статистика
                int currentRow = 5;

                worksheet.Cells[currentRow, 1] = "📊 СТАТИСТИКА ЗА МЕСЯЦ";
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 3]];
                range.Merge();
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.Font.Name = "Arial";
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                currentRow += 2;

                // Левая колонка статистики
                worksheet.Cells[currentRow, 1] = "Всего записей:";
                worksheet.Cells[currentRow, 2] = statistics["TotalRecords"];
                FormatStatCell(worksheet, currentRow, 1, true);
                FormatStatCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "✅ Выполнено:";
                worksheet.Cells[currentRow, 2] = statistics["Completed"];
                FormatStatCell(worksheet, currentRow, 1, true);
                FormatStatCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "📌 Подтверждено:";
                worksheet.Cells[currentRow, 2] = statistics["Confirmed"];
                FormatStatCell(worksheet, currentRow, 1, true);
                FormatStatCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "⏳ Запланировано:";
                worksheet.Cells[currentRow, 2] = statistics["Planned"];
                FormatStatCell(worksheet, currentRow, 1, true);
                FormatStatCell(worksheet, currentRow, 2, false);
                currentRow++;

                worksheet.Cells[currentRow, 1] = "❌ Отменено:";
                worksheet.Cells[currentRow, 2] = statistics["Cancelled"];
                FormatStatCell(worksheet, currentRow, 1, true);
                FormatStatCell(worksheet, currentRow, 2, false);
                currentRow += 2;

                int statRow = currentRow - 7;

                worksheet.Cells[statRow, 4] = "Активных мастеров:";
                worksheet.Cells[statRow, 5] = statistics["ActiveMasters"];
                FormatStatCell(worksheet, statRow, 4, true);
                FormatStatCell(worksheet, statRow, 5, false);
                statRow++;

                worksheet.Cells[statRow, 4] = "Уникальных клиентов:";
                worksheet.Cells[statRow, 5] = statistics["UniqueClients"];
                FormatStatCell(worksheet, statRow, 4, true);
                FormatStatCell(worksheet, statRow, 5, false);
                statRow += 2;

                // Выручка
                worksheet.Cells[statRow, 4] = "💰 ОБЩАЯ ВЫРУЧКА:";
                worksheet.Cells[statRow, 5] = statistics["Revenue"];
                range = worksheet.Cells[statRow, 4];
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                range = worksheet.Cells[statRow, 5];
                range.Value = statistics["Revenue"];
                range.Font.Bold = true;
                range.Font.Size = 14;
                range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.Green);
                range.NumberFormat = "#,##0.00";
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                currentRow = statRow + 3;

                // Заголовки таблицы
                string[] headers = { "Дата", "Время", "Клиент", "Мастер", "Услуга", "Цена", "Скидка", "Статус", "Менеджер" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[currentRow, i + 1] = headers[i];
                    range = worksheet.Cells[currentRow, i + 1];
                    range.Font.Bold = true;
                    range.Font.Name = "Arial";
                    range.Font.Size = 11;
                    range.Interior.Color = System.Drawing.ColorTranslator.ToOle(accentColor);
                    range.Font.Color = System.Drawing.ColorTranslator.ToOle(Color.White);
                    range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                }

                currentRow++;

                // Данные
                for (int i = 0; i < reportData.Rows.Count; i++)
                {
                    DataRow row = reportData.Rows[i];

                    for (int j = 0; j < reportData.Columns.Count; j++)
                    {
                        worksheet.Cells[currentRow, j + 1] = row[j].ToString();
                        range = worksheet.Cells[currentRow, j + 1];
                        range.Font.Name = "Arial";
                        range.Font.Size = 10;
                        range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                        if (j == 0 || j == 1)
                        {
                            range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        }
                        else if (j == 5)
                        {
                            range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                        }
                        else
                        {
                            range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                        }

                        if (j == 7)
                        {
                            string status_val = row[j].ToString();
                            if (status_val.Contains("Запланирован") || status_val.Contains("Подтвержден"))
                                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 245, 157));
                            else if (status_val.Contains("Выполнен"))
                                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(197, 225, 165));
                            else if (status_val.Contains("Отменен"))
                                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(255, 171, 145));
                        }

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    }

                    if (i % 2 == 1)
                    {
                        range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, headers.Length]];
                        range.Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 250, 250));
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                    }

                    currentRow++;
                }

                // Итог
                currentRow += 1;
                range = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 4]];
                range.Merge();
                range.Value = "ИТОГО ЗА МЕСЯЦ:";
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                range = worksheet.Cells[currentRow, 5];
                range.Value = statistics["Revenue"];
                range.Font.Bold = true;
                range.Font.Size = 12;
                range.NumberFormat = "#,##0.00";
                range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(lightPink);
                range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(range);

                worksheet.Columns.AutoFit();

                workbook.SaveAs(filePath);
                workbook.Close();

                MessageBox.Show($"Отчет за {GetMonthName(selectedMonth.Month)} {selectedMonth.Year} успешно сохранен!\n\n{filePath}",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            finally
            {
                if (range != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
                if (worksheet != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Форматирование ячейки статистики в Excel
        /// </summary>
        private void FormatStatCell(Microsoft.Office.Interop.Excel.Worksheet worksheet, int row, int col, bool isLabel)
        {
            Microsoft.Office.Interop.Excel.Range range = worksheet.Cells[row, col];

            if (isLabel)
            {
                range.Font.Bold = true;
            }

            range.Font.Name = "Arial";
            range.Font.Size = 11;
            range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(range);
        }

        #endregion
    }
}