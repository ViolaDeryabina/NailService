using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Класс для управления фильтрацией и сортировкой записей
    /// </summary>
    public class FilterManager
    {
        /// <summary>
        /// Вспомогательный класс для хранения диапазона дат
        /// </summary>
        public class DateRange
        {
            public DateTime MinDate { get; set; }
            public DateTime MaxDate { get; set; }
        }

        private DataManager _dataManager;
        private List<RecordData> _allRecords;

        /// <summary>
        /// Конструктор менеджера фильтрации
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public FilterManager(string connectionString)
        {
            _dataManager = new DataManager(connectionString);
            RefreshData();
        }

        /// <summary>
        /// Получение минимальной и максимальной даты среди всех записей
        /// </summary>
        public DateRange GetDateRange()
        {
            if (_allRecords == null || _allRecords.Count == 0)
            {
                var today = DateTime.Today;
                return new DateRange { MinDate = today, MaxDate = today };
            }

            var minDate = _allRecords.Min(r => r.Date).Date;
            var maxDate = _allRecords.Max(r => r.Date).Date;

            return new DateRange { MinDate = minDate, MaxDate = maxDate };
        }

        /// <summary>
        /// Получение отфильтрованных и отсортированных записей
        /// </summary>
        public List<RecordData> GetFilteredRecords(string searchText = "", string masterFilter = "Все мастера", string statusFilter = "Все статусы",
                                          DateTime? fromDate = null, DateTime? toDate = null,
                                          string sortBy = "Date", bool ascending = false)
        {
            if (_allRecords == null)
                return new List<RecordData>();

            var filtered = _allRecords.AsEnumerable();

            // Поиск по тексту (по клиенту, телефону, услуге)
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(r =>
                    (r.ClientName?.ToLower() ?? "").Contains(searchText.ToLower()) ||
                    (r.ClientPhone?.ToLower() ?? "").Contains(searchText.ToLower()) ||
                    (r.Service?.ToLower() ?? "").Contains(searchText.ToLower()));
            }

            // Фильтрация по мастеру
            if (masterFilter != "Все мастера" && !string.IsNullOrEmpty(masterFilter))
            {
                filtered = filtered.Where(r => r.MasterName == masterFilter);
            }

            // Фильтрация по статусу
            if (statusFilter != "Все статусы" && !string.IsNullOrEmpty(statusFilter))
            {
                filtered = filtered.Where(r => r.Status == statusFilter);
            }

            // Фильтрация по дате
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

        /// <summary>
        /// Заполнение ComboBox списком уникальных мастеров
        /// </summary>
        public void PopulateMastersComboBox(ComboBox comboBox)
        {
            var masters = new List<string> { "Все мастера" };

            if (_allRecords != null && _allRecords.Count > 0)
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

        /// <summary>
        /// Заполнение ComboBox списком статусов
        /// </summary>
        public void PopulateStatusComboBox(ComboBox comboBox)
        {
            var statuses = _dataManager.GetStatusList();
            comboBox.DataSource = statuses;
            comboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Получение списка статусов с ID для ComboBox в DataGridView
        /// </summary>
        public List<StatusItem> GetStatusItems()
        {
            return _dataManager.GetStatusItems();
        }

        /// <summary>
        /// Обновление статуса записи и синхронизация локального кэша
        /// </summary>
        public bool UpdateRecordStatus(int recordId, int newStatusId)
        {
            bool result = _dataManager.UpdateRecordStatus(recordId, newStatusId);

            if (result)
            {
                var record = _allRecords.FirstOrDefault(r => r.RecordID == recordId);
                if (record != null)
                {
                    record.StatusID = newStatusId;
                    record.Status = _dataManager.GetStatusNameById(newStatusId);
                }
            }

            return result;
        }

        /// <summary>
        /// Обновление данных из базы (перезагрузка кэша)
        /// </summary>
        public void RefreshData()
        {
            _allRecords = _dataManager.GetAllRecords();
        }
    }
}