
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public class FilterManager
    {
        public class DateRange
        {
            public DateTime MinDate { get; set; }
            public DateTime MaxDate { get; set; }
        }

        private DataManager _dataManager;
        private List<RecordData> _allRecords;

        public FilterManager(string connectionString)
        {
            _dataManager = new DataManager(connectionString);
            RefreshData();
        }

        public DateRange GetDateRange()
        {
            if (_allRecords == null || !_allRecords.Any())
            {
                var today = DateTime.Today;
                return new DateRange { MinDate = today, MaxDate = today };
            }

            var minDate = _allRecords.Min(r => r.Date).Date;
            var maxDate = _allRecords.Max(r => r.Date).Date;

            return new DateRange { MinDate = minDate, MaxDate = maxDate };
        }

        public List<RecordData> GetFilteredRecords(string searchText = "", string masterFilter = "Все", string statusFilter = "Все",
                                          DateTime? fromDate = null, DateTime? toDate = null,
                                          string sortBy = "Date", bool ascending = false)
        {
            var filtered = _allRecords.AsEnumerable();

            // Поиск
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(r =>
                    (r.MasterName?.ToLower() ?? "").Contains(searchText.ToLower()) ||
                    (r.ClientName?.ToLower() ?? "").Contains(searchText.ToLower()) ||
                    (r.Service?.ToLower() ?? "").Contains(searchText.ToLower()));
            }

            // Фильтрация по мастеру
            if (masterFilter != "Все" && masterFilter != "Все мастера")
            {
                filtered = filtered.Where(r => r.MasterName == masterFilter);
            }

            // ФИЛЬТРАЦИЯ ПО СТАТУСУ
            if (statusFilter != "Все" && statusFilter != "Все статусы")
            {
                filtered = filtered.Where(r => r.Status == statusFilter);
            }

            // Фильтр по дате
            if (fromDate.HasValue)
            {
                filtered = filtered.Where(r => r.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                filtered = filtered.Where(r => r.Date <= toDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            // Сортировка
            switch (sortBy)
            {
                case "Price":
                    filtered = ascending ? filtered.OrderBy(r => r.Price) : filtered.OrderByDescending(r => r.Price);
                    break;
                case "Master":
                    filtered = ascending ? filtered.OrderBy(r => r.MasterName) : filtered.OrderByDescending(r => r.MasterName);
                    break;
                case "Client":
                    filtered = ascending ? filtered.OrderBy(r => r.ClientName) : filtered.OrderByDescending(r => r.ClientName);
                    break;
                case "Service":
                    filtered = ascending ? filtered.OrderBy(r => r.Service) : filtered.OrderByDescending(r => r.Service);
                    break;
                case "Status":
                    filtered = ascending ? filtered.OrderBy(r => r.Status) : filtered.OrderByDescending(r => r.Status);
                    break;
                default: // Date
                    filtered = ascending ? filtered.OrderBy(r => r.Date) : filtered.OrderByDescending(r => r.Date);
                    break;
            }

            return filtered.ToList();
        }

        // Фильтрация по мастерам
        public void PopulateMastersComboBox(ComboBox comboBox)
        {
            var masters = new List<string> { "Все мастера" };

            if (_allRecords != null && _allRecords.Any())
            {
                var uniqueMasters = _allRecords
                    .Select(r => r.MasterName)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .OrderBy(m => m);

                masters.AddRange(uniqueMasters);
            }

            comboBox.DataSource = masters;
            comboBox.SelectedIndex = 0;
        }

        public void PopulateStatusComboBox(ComboBox comboBox)
        {
            var statuses = _dataManager.GetStatusList();
            comboBox.DataSource = statuses;
            comboBox.SelectedIndex = 0;
        }

        // НОВЫЙ МЕТОД: Получение статусов для ComboBox в DataGridView
        public List<StatusItem> GetStatusItems()
        {
            return _dataManager.GetStatusItems();
        }

        // НОВЫЙ МЕТОД: Обновление статуса
        public bool UpdateRecordStatus(int recordId, int newStatusId)
        {
            bool result = _dataManager.UpdateRecordStatus(recordId, newStatusId);

            if (result)
            {
                // Обновляем данные в локальном кэше
                var record = _allRecords.FirstOrDefault(r => r.RecordID == recordId);
                if (record != null)
                {
                    record.StatusID = newStatusId;
                    record.Status = _dataManager.GetStatusNameById(newStatusId);
                }
            }

            return result;
        }

        public void RefreshData()
        {
            _allRecords = _dataManager.GetAllRecords();
        }
    }
}