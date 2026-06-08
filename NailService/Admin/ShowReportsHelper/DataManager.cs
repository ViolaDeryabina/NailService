using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NailServiceApp.Utilities;

namespace NailService
{
    /// <summary>
    /// Модель данных для записи (одной строки в расписании)
    /// </summary>
    public class RecordData
    {
        public int RecordID { get; set; }
        public string MasterName { get; set; }
        public string ClientName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
        public string UserName { get; set; }
        public int StatusID { get; set; }
    }

    /// <summary>
    /// Класс для управления данными приложения
    /// Выполняет операции с базой данных: получение записей, статусов, обновление статусов
    /// </summary>
    public class DataManager
    {
        private string _connectionString;

        /// <summary>
        /// Конструктор менеджера данных
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        public DataManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Получение всех записей из базы данных с полной информацией
        /// </summary>
        /// <returns>Список записей RecordData</returns>
        public List<RecordData> GetAllRecords()
        {
            var records = new List<RecordData>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                        r.IDRecord,
                        u_m.LastName as MasterLastName,
                        u_m.FirstName as MasterFirstName, 
                        u_m.MiddleName as MasterMiddleName,
                        c.LastName as ClientLastName,
                        c.FirstName as ClientFirstName,
                        c.MiddleName as ClientMiddleName,
                        r.Date,
                        s.StatusName as Status,
                        sv.ServiceName as Service,
                        sv.Price,
                        u_u.LastName as UserLastName,
                        u_u.FirstName as UserFirstName,
                        u_u.MiddleName as UserMiddleName,
                        r.Status as StatusID
                    FROM Record r
                    LEFT JOIN Masters m ON r.Master = m.IDMasters
                    LEFT JOIN Users u_m ON m.User = u_m.IDUser
                    LEFT JOIN Client c ON r.Client = c.IDClient
                    LEFT JOIN Status s ON r.Status = s.IDStatus
                    LEFT JOIN Services sv ON r.Service = sv.IDServices
                    LEFT JOIN Users u_u ON r.User = u_u.IDUser;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string masterShortName = NameFormatter.FormatToShortName(
                                reader["MasterLastName"].ToString(),
                                reader["MasterFirstName"].ToString(),
                                reader["MasterMiddleName"].ToString()
                            );

                            string clientShortName = NameFormatter.FormatToShortName(
                                reader["ClientLastName"].ToString(),
                                reader["ClientFirstName"].ToString(),
                                reader["ClientMiddleName"].ToString()
                            );

                            string userShortName = NameFormatter.FormatToShortName(
                                reader["UserLastName"].ToString(),
                                reader["UserFirstName"].ToString(),
                                reader["UserMiddleName"].ToString()
                            );

                            records.Add(new RecordData
                            {
                                RecordID = Convert.ToInt32(reader["IDRecord"]),
                                MasterName = masterShortName,
                                ClientName = clientShortName,
                                Date = Convert.ToDateTime(reader["Date"]),
                                Status = reader["Status"].ToString(),
                                Service = reader["Service"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                UserName = userShortName,
                                StatusID = Convert.ToInt32(reader["StatusID"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }

            return records;
        }

        /// <summary>
        /// Получение списка названий статусов для фильтрации
        /// </summary>
        /// <returns>Список статусов с добавленным элементом "Все статусы"</returns>
        public List<string> GetStatusList()
        {
            var statuses = new List<string> { "Все статусы" };

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusName FROM Status ORDER BY StatusName";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(reader["StatusName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
            }

            return statuses;
        }

        /// <summary>
        /// Получение списка статусов с ID для использования в ComboBox DataGridView
        /// </summary>
        /// <returns>Список объектов StatusItem с ID и Name</returns>
        public List<StatusItem> GetStatusItems()
        {
            var statuses = new List<StatusItem>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT IDStatus, StatusName FROM Status ORDER BY StatusName";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(new StatusItem
                            {
                                ID = Convert.ToInt32(reader["IDStatus"]),
                                Name = reader["StatusName"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}");
            }

            return statuses;
        }

        /// <summary>
        /// Обновление статуса записи в базе данных
        /// </summary>
        /// <param name="recordId">ID записи</param>
        /// <param name="newStatusId">Новый ID статуса</param>
        /// <returns>true если обновление успешно</returns>
        public bool UpdateRecordStatus(int recordId, int newStatusId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"UPDATE Record 
                                    SET Status = @Status 
                                    WHERE IDRecord = @IDRecord";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Status", newStatusId);
                        command.Parameters.AddWithValue("@IDRecord", recordId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении статуса: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение названия статуса по его ID
        /// </summary>
        /// <param name="statusId">ID статуса</param>
        /// <returns>Название статуса или пустая строка</returns>
        public string GetStatusNameById(int statusId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT StatusName FROM Status WHERE IDStatus = @StatusID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusID", statusId);

                        var result = command.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }

    /// <summary>
    /// Модель для статуса с ID и названием
    /// Используется в ComboBox для выбора статуса
    /// </summary>
    public class StatusItem
    {
        public int ID { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Переопределение для корректного отображения в ComboBox
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}