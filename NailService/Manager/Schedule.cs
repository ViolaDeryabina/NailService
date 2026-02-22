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
    public partial class Schedule : Form
    {
        private DateTime currentWeekStart;
        private int _roleID;
        private string _userFIO;
        private int _userId;

        public Schedule(string userFIO, int roleID, int userId)
        {
            InitializeComponent();
            _userFIO = userFIO;
            _roleID = roleID;
            _userId = userId;
            currentWeekStart = GetMonday(DateTime.Today);

            // СНАЧАЛА применяем стили
            ApplyCustomStyles();

            // ПОТОМ загружаем данные
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
        private void ApplyCustomStyles()
        {
            // Цвета согласно вашим требованиям
            Color selectionColor = Color.FromArgb(255, 203, 219); // Цвет выделения
            Color accentColor = Color.HotPink; // Акцентный цвет

            // Применяем базовые стили из StyleManager
            dataGridViewSchedule.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            dataGridViewSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);


            // Цвет выделения ВСЕЙ строки
            dataGridViewSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewSchedule.DefaultCellStyle.SelectionForeColor = Color.Black;


            // Настройки таблицы
            dataGridViewSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSchedule.ReadOnly = true;


            dataGridViewSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSchedule.ReadOnly = true;

            // Настройки для расписания
            dataGridViewSchedule.RowHeadersVisible = true;
            dataGridViewSchedule.RowHeadersWidth = 70;
            dataGridViewSchedule.RowTemplate.Height = 80;


            // Подсветка выходных
            dataGridViewSchedule.CellFormatting += DataGridViewSchedule_CellFormatting;
        }

        private void DataGridViewSchedule_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Подсвечиваем выходные дни очень светлым розовым
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                DateTime[] weekDates = GetCurrentWeekDates();
                int dayIndex = e.ColumnIndex - 1;

                if (dayIndex >= 0 && dayIndex < weekDates.Length)
                {
                    DayOfWeek day = weekDates[dayIndex].DayOfWeek;
                    if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                    {
                        if (e.Value == null)
                        {
                           // e.CellStyle.BackColor = Color.FromArgb(255, 240, 245); // Очень светлый розовый
                        }
                    }
                }
            }
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
                // Обновляем информацию о неделе
                DateTime weekEnd = currentWeekStart.AddDays(4);
                label4.Text = $"Неделя: {currentWeekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";

                var weekDates = new DateTime[5];
                for (int i = 0; i < 5; i++)
                {
                    weekDates[i] = currentWeekStart.AddDays(i);
                }

                // Очищаем существующие данные
                dataGridViewSchedule.Rows.Clear();
                dataGridViewSchedule.Columns.Clear(); // ОЧЕНЬ ВАЖНО: очищаем и колонки тоже

                // Создаем колонки заново
                CreateColumns(weekDates);

                // Добавляем строки с временами
                AddTimeRows();

                // Загружаем реальные данные из базы
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

            // Добавляем колонку для времени
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

            // Добавляем колонки для дней недели
            for (int i = 0; i < 5; i++)
            {
                string dayName = weekDates[i].ToString("dd.MM");
                string dayOfWeek = GetRussianDayOfWeekFull(weekDates[i].DayOfWeek);

                // Отладочный вывод
                Console.WriteLine($"День {i}: {dayOfWeek} {dayName}");

                string headerText = $"{dayOfWeek}\n{dayName}";

                // Определяем цвет заголовка для текущего дня
                Color headerBackColor = accentColor;
                if (weekDates[i].Date == DateTime.Today)
                {
                    headerBackColor = Color.FromArgb(255, 100, 150); // Чуть ярче для сегодня
                }

                DataGridViewTextBoxColumn dayColumn = new DataGridViewTextBoxColumn
                {
                    Name = headerText,
                    HeaderText = headerText,  // Это то, что отображается
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

            // Принудительно обновляем заголовки
            dataGridViewSchedule.Refresh();
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



        private void AddTimeRows()
        {
            int[] timeSlots = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

            foreach (int hour in timeSlots)
            {
                int rowIndex = dataGridViewSchedule.Rows.Add();
                dataGridViewSchedule.Rows[rowIndex].Cells["Time"].Value = $"{hour:00}:00";

                // БЕЛЫЙ ФОН для всех строк
                for (int col = 1; col <= 5; col++)
                {
                    dataGridViewSchedule.Rows[rowIndex].Cells[col].Style.BackColor = Color.White;
                }
            }
        }

        private string GetShortRussianDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return "ПН";
                case DayOfWeek.Tuesday: return "ВТ";
                case DayOfWeek.Wednesday: return "СР";
                case DayOfWeek.Thursday: return "ЧТ";
                case DayOfWeek.Friday: return "ПТ";
                case DayOfWeek.Saturday: return "СБ";
                case DayOfWeek.Sunday: return "ВС";
                default: return "";
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

                    // Подсвечиваем текущий часовой слот очень светлым розовым
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

        private void LoadScheduleData(DateTime[] weekDates)
        {
            try
            {
                // Очищаем все ячейки (кроме колонки времени) - ставим БЕЛЫЙ ФОН
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
                        AND r.Status IN (1, 2)";

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

                            // Находим соответствующий день
                            int dayIndex = -1;
                            for (int i = 0; i < weekDates.Length; i++)
                            {
                                if (weekDates[i].Date == recordDate.Date)
                                {
                                    dayIndex = i;
                                    break;
                                }
                            }

                            // Находим соответствующий час
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

                                // Используем StyleManager для цвета статуса
                                Color statusColor = StyleManager.GetStatusColor(statusId);
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.BackColor = statusColor;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.ForeColor = Color.Black;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.Font =
                                    new Font("MS Reference Sans Serif", 8.5f, FontStyle.Bold);
                            }
                        }
                    }
                }

                // Подсвечиваем текущее время
                HighlightCurrentTimeSlot();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateWeekLabel()
        {
            DateTime weekEnd = currentWeekStart.AddDays(4);
            string[] months = { "января", "февраля", "марта", "апреля", "мая", "июня",
                               "июля", "августа", "сентября", "октября", "ноября", "декабря" };

            string startStr = $"{currentWeekStart.Day} {months[currentWeekStart.Month - 1]}";
            string endStr = $"{weekEnd.Day} {months[weekEnd.Month - 1]} {weekEnd.Year}";

            label4.Text = $"📅 {startStr} — {endStr}";
            label4.ForeColor = Color.HotPink;
            label4.Font = new Font("MS Reference Sans Serif", 10, FontStyle.Bold);
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

        private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                if (dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    ShowExistingRecordInfo(e.RowIndex, e.ColumnIndex);
                }
                else
                {
                    CreateNewRecord(e.RowIndex, e.ColumnIndex);
                }
            }
        }

        private void ShowExistingRecordInfo(int rowIndex, int columnIndex)
        {
            string cellValue = dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Value.ToString();
            int recordId = Convert.ToInt32(dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Tag);

            DialogResult result = MessageBox.Show(
                $"Запись:\n{cellValue}\n\nХотите отменить эту запись?",
                "Информация о записи",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CancelRecord(recordId, rowIndex, columnIndex);
            }
        }

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

        // Переход к другим формам
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
            if (_roleID == 1) // Директор
            {
                MenuDirector menuDirector = new MenuDirector(_userFIO);
                menuDirector.Show();
            }
            else if (_roleID == 2) // Админ
            {
                MenuAdmin menuAdmin = new MenuAdmin(_userFIO);
                menuAdmin.Show();
            }
            else // Менеджер
            {
                MenuManager menuManager = new MenuManager(_userFIO);
                menuManager.Show();
            }
            this.Hide();
        }
    }
}