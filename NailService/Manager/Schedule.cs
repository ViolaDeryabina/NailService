using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class Schedule : Form
    {
        private DateTime currentWeekStart;
        public Schedule()
        {
            InitializeComponent();
            currentWeekStart = GetMonday(DateTime.Today);

            FillScheduleWithData();
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
            // Обновляем информацию о неделе
            DateTime weekEnd = currentWeekStart.AddDays(4);
            label4.Text = $"Неделя: {currentWeekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";

            // Остальной код заполнения расписания...
            var weekDates = new DateTime[5];
            for (int i = 0; i < 5; i++)
            {
                weekDates[i] = currentWeekStart.AddDays(i);
            }

            // Очищаем и заполняем DataGridView...
            dataGridViewSchedule.Columns.Clear();
            dataGridViewSchedule.Rows.Clear();

            // Добавляем колонку для времени
            dataGridViewSchedule.Columns.Add("Time", "Время");
            dataGridViewSchedule.Columns["Time"].Width = 80;
            dataGridViewSchedule.Columns["Time"].ReadOnly = true;
            dataGridViewSchedule.Columns["Time"].DefaultCellStyle.BackColor = Color.LightGray;

            // Добавляем колонки для дней недели
            for (int i = 0; i < 5; i++)
            {
                string dayName = weekDates[i].ToString("dd.MM");
                string dayOfWeek = GetRussianDayOfWeek(weekDates[i].DayOfWeek);

                dataGridViewSchedule.Columns.Add($"Day{i}", $"{dayOfWeek}\n{dayName}");
                dataGridViewSchedule.Columns[i + 1].Width = 150;
            }

            // Добавляем строки с временами
            int[] timeSlots = { 8, 10, 12, 14, 16, 18 };
            foreach (int time in timeSlots)
            {
                int rowIndex = dataGridViewSchedule.Rows.Add();
                dataGridViewSchedule.Rows[rowIndex].HeaderCell.Value = $"{time}:00";
                dataGridViewSchedule.Rows[rowIndex].Cells["Time"].Value = $"{time}:00";
                dataGridViewSchedule.Rows[rowIndex].Height = 60; // Увеличиваем высоту для текста
            }

            // Загружаем реальные данные
            LoadRealScheduleData(weekDates, timeSlots);
        }

        private DateTime[] GetCurrentWeekDates()
        {
            DateTime today = DateTime.Today;
            // Находим понедельник текущей недели
            int delta = DayOfWeek.Monday - today.DayOfWeek;
            if (delta > 0) delta -= 7;
            DateTime monday = today.AddDays(delta);

            DateTime[] weekDates = new DateTime[5];
            for (int i = 0; i < 5; i++)
            {
                weekDates[i] = monday.AddDays(i);
            }

            return weekDates;
        }

        private string GetRussianDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return "Пн";
                case DayOfWeek.Tuesday: return "Вт";
                case DayOfWeek.Wednesday: return "Ср";
                case DayOfWeek.Thursday: return "Чт";
                case DayOfWeek.Friday: return "Пт";
                case DayOfWeek.Saturday: return "Сб";
                case DayOfWeek.Sunday: return "Вс";
                default: return "";
            }
        }

        private void DataGridViewSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 1) // Исключаем колонку времени и заголовки
            {
                string time = dataGridViewSchedule.Rows[e.RowIndex].Cells["Time"].Value?.ToString();
                string day = dataGridViewSchedule.Columns[e.ColumnIndex].HeaderText;

                DateTime selectedDate = GetCurrentWeekDates()[e.ColumnIndex - 1];
                DateTime selectedDateTime = selectedDate.AddHours(int.Parse(time.Split(':')[0]));

                // Показываем диалог записи

                RecordingClients recordingClients = new RecordingClients();
                recordingClients.ShowDialog();

                ShowBookingDialog(selectedDateTime, e.RowIndex, e.ColumnIndex);
            }
        }

        private void ShowBookingDialog(DateTime dateTime, int rowIndex, int columnIndex)
        {
            var result = MessageBox.Show($"Записать клиента на {dateTime:dd.MM.yyyy HH:mm}?",
                "Запись клиента", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Здесь можно открыть форму для выбора клиента и услуги
                // Временно просто помечаем ячейку как занятую
                dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Value = "Занято";
                dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.Gold;
                dataGridViewSchedule.Rows[rowIndex].Cells[columnIndex].Style.ForeColor = Color.Black;
            }
        }

        private void LoadRealScheduleData(DateTime[] weekDates, int[] timeSlots)
        {

            // Здесь должен быть код для загрузки записей из базы данных
            // Пример:
            using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
            {
                con.Open();
                string query = @"SELECT r.Date, c.LastName, c.FirstName, s.ServiceName 
                           FROM Record r 
                           INNER JOIN Client c ON r.Client = c.IDClient 
                           INNER JOIN Services s ON r.Service = s.IDServices 
                           WHERE r.Date BETWEEN @startDate AND @endDate 
                           AND r.Status IN (1, 2)"; // Запланированные и подтвержденные

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@startDate", weekDates[0]);
                cmd.Parameters.AddWithValue("@endDate", weekDates[4].AddDays(1));

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime recordDate = (DateTime)reader["Date"];
                        string clientName = $"{reader["LastName"]} {reader["FirstName"]}";
                        string service = reader["ServiceName"].ToString();

                        // Находим соответствующую ячейку в расписании
                        int dayIndex = Array.IndexOf(weekDates, recordDate.Date);
                        int timeIndex = Array.IndexOf(timeSlots, recordDate.Hour);

                        if (dayIndex >= 0 && timeIndex >= 0)
                        {
                            dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Value =
                                $"{clientName}\n{service}";
                            dataGridViewSchedule.Rows[timeIndex].Cells[dayIndex + 1].Style.BackColor = Color.LightYellow;
                        }
                    }
                }
            }
        }

      

        private void ListButton_Click(object sender, EventArgs e)
        {
            Show Show = new Show("");
            Show.Show();
            this.Hide();
        }

        private void Reports_Click(object sender, EventArgs e)
        {
            ShowReports showReports = new ShowReports("");
            showReports.Show();
            this.Hide();
        }

        private void InMenu_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
