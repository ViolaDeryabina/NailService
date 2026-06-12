using MySql.Data.MySqlClient;
using NailServiceApp.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
   

    public partial class Schedule : Form
    {
        private DateTime currentWeekStart;
        private int _roleID;
        private string _userFIO;
        private int _userId;
        private bool _isCentered = false;

        public Schedule(string userFIO, int roleID, int userId)
        {
            InitializeComponent();
            _userFIO = userFIO;
            _roleID = roleID;
            _userId = userId;

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1000, 700);
            currentWeekStart = GetMonday(DateTime.Today);
            this.StartPosition = FormStartPosition.CenterScreen;

            ApplyCustomStyles();
            FillScheduleWithData();

            FIOlabel.Text = $"Менеджер: {_userFIO}";
            LoadUserData();
            RecalculateLayout();
        }

        private void LoadUserData()
        {
            if (_userId == 0) _roleID = 4;
        }

        #region Стили и оформление
        private void ApplyCustomStyles()
        {
            dataGridViewSchedule.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            dataGridViewSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);
            dataGridViewSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewSchedule.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridViewSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridViewSchedule.ReadOnly = true;
            dataGridViewSchedule.RowHeadersVisible = true;
            dataGridViewSchedule.RowHeadersWidth = 70;
            dataGridViewSchedule.RowTemplate.Height = 80;
            dataGridViewSchedule.CellFormatting += DataGridViewSchedule_CellFormatting;
        }

        private void DataGridViewSchedule_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) { }

        private Color GetStatusColor(int statusId)
        {
            switch (statusId)
            {
                case 1: return Color.FromArgb(255, 245, 157); // Занято
                case 2: return Color.FromArgb(197, 225, 165); // Выполнено
                case 3: return Color.FromArgb(255, 171, 145); // Отменено
                default: return Color.White;
            }
        }
        #endregion

        #region Навигация по неделям
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

        private DateTime[] GetCurrentWeekDates()
        {
            DateTime[] weekDates = new DateTime[5];
            for (int i = 0; i < 5; i++) weekDates[i] = currentWeekStart.AddDays(i);
            return weekDates;
        }
        #endregion

        #region Загрузка и отображение данных
        private void FillScheduleWithData()
        {
            try
            {
                DateTime weekEnd = currentWeekStart.AddDays(4);
                label4.Text = $"Неделя: {currentWeekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";
                label4.ForeColor = Color.Black;
                label4.Font = new Font("MS Reference Sans Serif", 12, FontStyle.Bold);

                var weekDates = GetCurrentWeekDates();
                dataGridViewSchedule.Rows.Clear();
                dataGridViewSchedule.Columns.Clear();

                CreateColumns(weekDates);
                AddTimeRows();
                LoadScheduleData(weekDates);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateColumns(DateTime[] weekDates)
        {
            Color accentColor = Color.HotPink;
            dataGridViewSchedule.Columns.Clear();

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

            int dayColumnWidth = (dataGridViewSchedule.Width - 100) / 5;

            for (int i = 0; i < 5; i++)
            {
                string dayName = weekDates[i].ToString("dd.MM");
                string dayOfWeek = GetRussianDayOfWeekFull(weekDates[i].DayOfWeek);
                string headerText = $"{dayOfWeek}\n{dayName}";

                Color headerBackColor = weekDates[i].Date == DateTime.Today ? Color.FromArgb(255, 100, 150) : accentColor;

                DataGridViewTextBoxColumn dayColumn = new DataGridViewTextBoxColumn
                {
                    Name = headerText,
                    HeaderText = headerText,
                    ReadOnly = true,
                    Width = dayColumnWidth,
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

        private string GetRussianDayOfWeekFull(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return "Понедельник";
                case DayOfWeek.Tuesday: return "Вторник";
                case DayOfWeek.Wednesday: return "Среда";
                case DayOfWeek.Thursday: return "Четверг";
                case DayOfWeek.Friday: return "Пятница";
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
                for (int col = 1; col <= 5; col++)
                    dataGridViewSchedule.Rows[rowIndex].Cells[col].Style.BackColor = Color.White;
            }
        }

        private void HighlightCurrentTimeSlot()
        {
            int currentHour = DateTime.Now.Hour;
            for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
            {
                string timeValue = dataGridViewSchedule.Rows[row].Cells["Time"].Value?.ToString();
                if (!string.IsNullOrEmpty(timeValue) && int.Parse(timeValue.Split(':')[0]) == currentHour)
                {
                    for (int col = 1; col <= 5; col++)
                        if (dataGridViewSchedule.Rows[row].Cells[col].Value == null)
                            dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(255, 230, 240);
                }
            }
        }

        private void LoadScheduleData(DateTime[] weekDates)
        {
            try
            {
                for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
                    for (int col = 1; col <= 5; col++)
                    {
                        dataGridViewSchedule.Rows[row].Cells[col].Value = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Tag = null;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.BackColor = Color.White;
                        dataGridViewSchedule.Rows[row].Cells[col].Style.Font = new Font("MS Reference Sans Serif", 8.5f);
                        dataGridViewSchedule.Rows[row].Cells[col].Style.ForeColor = Color.Black;
                    }

                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            r.IDRecord,
                            r.Date,
                            r.ClientName,
                            r.ClientPhone,
                            s.ServiceName,
                            s.Price,
                            stat.StatusName,
                            stat.IDStatus,
                            u_m.LastName as MasterLastName,
                            u_m.FirstName as MasterFirstName
                        FROM Record r
                        INNER JOIN Services s ON r.Service = s.IDServices
                        INNER JOIN Status stat ON r.Status = stat.IDStatus
                        INNER JOIN Masters m ON r.Master = m.IDMasters
                        INNER JOIN Users u_m ON m.User = u_m.IDUser
                        WHERE r.Date BETWEEN @startDate AND @endDate";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@startDate", weekDates[0].Date);
                    cmd.Parameters.AddWithValue("@endDate", weekDates[4].Date.AddDays(1).AddSeconds(-1));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime recordDate = Convert.ToDateTime(reader["Date"]);
                            string clientName = reader["ClientName"].ToString();
                            string service = reader["ServiceName"].ToString();

                            string masterLastName = reader["MasterLastName"]?.ToString() ?? "";
                            string masterFirstName = reader["MasterFirstName"]?.ToString() ?? "";
                            string masterInitial = string.IsNullOrEmpty(masterFirstName) ? "" : masterFirstName[0].ToString();
                            string masterName = $"{masterLastName} {masterInitial}.";

                            int statusId = Convert.ToInt32(reader["IDStatus"]);

                            int dayIndex = -1;
                            for (int i = 0; i < weekDates.Length; i++)
                                if (weekDates[i].Date == recordDate.Date) { dayIndex = i; break; }

                            int timeIndex = -1;
                            for (int row = 0; row < dataGridViewSchedule.Rows.Count; row++)
                            {
                                string timeValue = dataGridViewSchedule.Rows[row].Cells["Time"].Value?.ToString();
                                if (timeValue == $"{recordDate.Hour:00}:00") { timeIndex = row; break; }
                            }

                            if (dayIndex >= 0 && timeIndex >= 0)
                            {
                                string cellValue = $"{clientName}\n{service}\nмастер: {masterName}";
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Value = cellValue;
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Tag = new RecordCellInfo
                                {
                                    RecordId = Convert.ToInt32(reader["IDRecord"]),
                                    StatusId = statusId,
                                    RecordDate = recordDate
                                };
                                Color statusColor = GetStatusColor(statusId);
                                dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.BackColor = statusColor;
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
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Обработка событий DataGridView
       private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1 && e.ColumnIndex <= 5)
            {
                object tag = dataGridViewSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag;
                if (tag != null && tag is RecordCellInfo recordInfo)
                {
                    // 1) Запрет на редактирование выполненных записей (статус 2)
                    if (recordInfo.StatusId == 2)
                    {
                        MessageBox.Show("Нельзя изменить выполненную запись.", "Доступ запрещён",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2) Запрет на редактирование записей за прошедшую дату
                    if (recordInfo.RecordDate.Date < DateTime.Today)
                    {
                        MessageBox.Show("Нельзя изменить запись за прошедшую дату.", "Доступ запрещён",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3) Отменённая запись (статус 3) – только перезапись
                    if (recordInfo.StatusId == 3)
                    {
                        DialogResult result = MessageBox.Show(
                            "Эта запись была отменена. Хотите перезаписать (удалить старую и создать новую)?",
                            "Перезапись", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            DeleteRecord(recordInfo.RecordId);
                            CreateNewRecord(e.RowIndex, e.ColumnIndex);
                        }
                        return;
                    }
                    else
                    {
                        // Обычное редактирование (статус 1 – Занято)
                        RecordsInfo recordsInfo = new RecordsInfo(recordInfo.RecordId, _userFIO, _roleID);
                        if (recordsInfo.ShowDialog() == DialogResult.OK)
                            FillScheduleWithData();
                    }
                }
                else if (tag == null)
                {
                    CreateNewRecord(e.RowIndex, e.ColumnIndex);
                }
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
                MessageBox.Show("Нельзя создать запись на прошедшее время!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RecordingClients recordingForm = new RecordingClients(_userId);
            recordingForm.SetSelectedDateTime(selectedDateTime);
            if (recordingForm.ShowDialog() == DialogResult.OK) FillScheduleWithData();
        }

        private void DeleteRecord(int recordId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Record WHERE IDRecord = @recordId";
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@recordId", recordId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Кнопки навигации
        private void btnPrevWeek_Click(object sender, EventArgs e) => ChangeWeek(-1);
        private void btnNextWeek_Click(object sender, EventArgs e) => ChangeWeek(1);
        private void btnCurrentWeek_Click(object sender, EventArgs e)
        {
            currentWeekStart = GetMonday(DateTime.Today);
            FillScheduleWithData();
        }
        #endregion

        #region Переход в меню
        private void InMenu_Click(object sender, EventArgs e)
        {
            if (_roleID == 2) new MenuAdmin(_userFIO).Show();
            else new MenuManager(_userFIO, _userId).Show();
            this.Hide();
        }
        #endregion

        #region Отчёты (заглушка)
        private void btnMonthlyReport_Click(object sender, EventArgs e) => GenerateMonthlyReport();

        private DateTime GetMonthStart(DateTime date) => new DateTime(date.Year, date.Month, 1);
        private DateTime GetMonthEnd(DateTime date) => GetMonthStart(date).AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

       

        private void GenerateMonthlyReport()
        {
            MessageBox.Show("Функция генерации Excel-отчета требует адаптации. Обратитесь к разработчику.", "Информация");
        }
        #endregion

        #region Layout
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Normal && !_isCentered) { CenterToScreen(); _isCentered = true; }
            else if (WindowState == FormWindowState.Maximized) _isCentered = false;
            RecalculateLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            groupBox1.Width = w - 40;
            groupBox1.Location = new Point(20, 0);
            label1.Location = new Point((groupBox1.Width - label1.Width) / 2, 20);
            FIOlabel.Location = new Point(groupBox1.Width - FIOlabel.Width - 20, 20);
            pictureBox1.Location = new Point(20, 10);

            dataGridViewSchedule.Width = w - 40;
            dataGridViewSchedule.Height = h - 250;
            dataGridViewSchedule.Location = new Point(20, 140);

            label4.Location = new Point(w - 330, 110);

            int legendX = w - 600, legendY = h - 100;
            panel1.Location = new Point(legendX, legendY);
            label2.Location = new Point(legendX + 35, legendY - 3);
            panel2.Location = new Point(legendX, legendY + 25);
            label3.Location = new Point(legendX + 35, legendY + 22);
            panel3.Location = new Point(legendX, legendY + 50);
            label5.Location = new Point(legendX + 35, legendY + 47);

            InMenu.Location = new Point(20, h - 65);
            button4.Location = new Point(w - 200, h - 65);
            button3.Location = new Point(w - 350, h - 65);
            button2.Location = new Point(w - 300, h - 65);

            if (dataGridViewSchedule.Columns.Count > 0)
            {
                int availableWidth = dataGridViewSchedule.Width - dataGridViewSchedule.RowHeadersWidth - 20;
                int dayColumnWidth = (availableWidth - 80) / 5;
                dataGridViewSchedule.Columns[0].Width = 80;
                for (int i = 1; i <= 5 && i < dataGridViewSchedule.Columns.Count; i++)
                    dataGridViewSchedule.Columns[i].Width = dayColumnWidth;
            }
        }
        #endregion
    }
    public class RecordCellInfo
    {
        public int RecordId { get; set; }
        public int StatusId { get; set; }
        public DateTime RecordDate { get; set; }
    }
}