using System;
using System.Windows.Forms;
using NailServiceApp.Data;
using NailServiceApp.Styles;

namespace NailService
{
    public partial class ShowReports : Form
    {
        private string _fio;
        private FilterManager _filterManager;

        public ShowReports(string FIO)
        {
            InitializeComponent();
            _fio = FIO;

            _filterManager = new FilterManager(Connection.ConnectionString);
            _filterManager.PopulateMastersComboBox(cmbMasterFilter);
            _filterManager.PopulateStatusComboBox(cmbStatusFilter);

            // Настройка сортировки
            cmbSort.Items.AddRange(new string[] {
                "Цена (по возрастанию)",
                "Цена (по убыванию)",
                "Мастер (А-Я)",
                "Мастер (Я-А)",
                "Клиент (А-Я)",
                "Клиент (Я-А)"
            });
            cmbSort.SelectedIndex = 0;

            SetupDateTimePickers();
            SetupDataGridView();
            LoadData();

            FIOlabel.Text = $"Директор: {_fio}";            
        }

        private void SetupDateTimePickers()
        {
            // Получаем минимальную и максимальную даты из базы данных
            var dateRange = _filterManager.GetDateRange();

            // Устанавливаем ограничения
            dtpFromDate.MinDate = dateRange.MinDate;
            dtpFromDate.MaxDate = dateRange.MaxDate;
            dtpToDate.MinDate = dateRange.MinDate;
            dtpToDate.MaxDate = dateRange.MaxDate;

            // Устанавливаем значения по умолчанию (последний месяц)
            dtpFromDate.Value = dateRange.MaxDate.AddMonths(-1);
            dtpToDate.Value = dateRange.MaxDate;

            // НАСТРОЙКА ДЛЯ КАЛЕНДАРЯ
            dtpFromDate.Format = DateTimePickerFormat.Custom;
            dtpFromDate.CustomFormat = "dd.MM.yyyy";
            dtpFromDate.ShowUpDown = false; // ВКЛЮЧАЕМ КАЛЕНДАРЬ
            dtpFromDate.ShowCheckBox = false;

            dtpToDate.Format = DateTimePickerFormat.Custom;
            dtpToDate.CustomFormat = "dd.MM.yyyy";
            dtpToDate.ShowUpDown = false; // ВКЛЮЧАЕМ КАЛЕНДАРЬ
            dtpToDate.ShowCheckBox = false;

            
        }

        private void SetupDataGridView()
        {
            // Полностью очищаем и настраиваем DataGridView
            dataGridViewRecords.Columns.Clear();
            dataGridViewRecords.Rows.Clear();
            dataGridViewRecords.DataSource = null;

            // Создаем колонки
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "MasterName", HeaderText = "Мастер" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClientName", HeaderText = "Клиент" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Дата и время" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Статус" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Service", HeaderText = "Услуга" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Цена" });
            dataGridViewRecords.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "Менеджер" });

            StyleManager.ApplyGridStyles(dataGridViewRecords);
            StyleManager.ApplyColumnAlignments(dataGridViewRecords);
        }

        private void LoadData()
        {
            try
            {
                if (dataGridViewRecords.Columns.Count == 0)
                {
                    SetupDataGridView();
                }

                string searchText = txtSearch.Text;
                string masterFilter = cmbMasterFilter.SelectedItem?.ToString() ?? "Все мастера";
                string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
                DateTime fromDate = dtpFromDate.Value;
                DateTime toDate = dtpToDate.Value;

                if (fromDate > toDate)
                {
                    MessageBox.Show("Дата 'С' не может быть больше даты 'По'", "Ошибка дат",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string sortBy = "Date";
                bool ascending = false;

                if (cmbSort.SelectedItem != null)
                {
                    string sortText = cmbSort.SelectedItem.ToString();
                    switch (sortText)
                    {
                        case "Цена (по возрастанию)": sortBy = "Price"; ascending = true; break;
                        case "Цена (по убыванию)": sortBy = "Price"; ascending = false; break;
                        case "Мастер (А-Я)": sortBy = "Master"; ascending = true; break;
                        case "Мастер (Я-А)": sortBy = "Master"; ascending = false; break;
                        case "Клиент (А-Я)": sortBy = "Client"; ascending = true; break;
                        case "Клиент (Я-А)": sortBy = "Client"; ascending = false; break;
                    }
                }

                // ПЕРЕДАЕМ statusFilter В МЕТОД
                var records = _filterManager.GetFilteredRecords(searchText, masterFilter, statusFilter, fromDate, toDate, sortBy, ascending);

                dataGridViewRecords.Rows.Clear();
                foreach (var record in records)
                {
                    int rowIndex = dataGridViewRecords.Rows.Add(
                        record.MasterName,
                        record.ClientName,
                        record.Date.ToString("dd.MM.yyyy HH:mm"),
                        record.Status,
                        record.Service,
                        record.Price.ToString("C0"),
                        record.UserName
                    );

                    if (rowIndex >= 0 && rowIndex < dataGridViewRecords.Rows.Count)
                    {
                        dataGridViewRecords.Rows[rowIndex].Cells[3].Style.BackColor =
                            StyleManager.GetStatusColor(record.StatusID);
                    }
                }

                lblRecordCount.Text = $"Найдено записей: {records.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчики событий
        private void txtSearch_TextChanged(object sender, EventArgs e) => LoadData();
        
        private void cmbMasterFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) => LoadData();
        private void dtpFromDate_ValueChanged(object sender, EventArgs e) => LoadData();
        private void dtpToDate_ValueChanged(object sender, EventArgs e) => LoadData();
        private void cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e) => LoadData();

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbMasterFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0; // ДОБАВЛЕНО
            cmbSort.SelectedIndex = 0;

            var dateRange = _filterManager.GetDateRange();
            dtpFromDate.Value = dateRange.MaxDate.AddMonths(-1);
            dtpToDate.Value = dateRange.MaxDate;

            LoadData();
        }

        private void InMenu_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_fio))
                new MenuDirector(_fio).Show();
            else
                new Schedule().Show();
            this.Hide();
        }
    }
}