using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using NailServiceApp.Utilities;

namespace NailServiceApp.Data
{
    public class RecordData
    {
        public string MasterName { get; set; }
        public string ClientName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
        public string UserName { get; set; }
        public int StatusID { get; set; }
    }

    public class DataManager
    {
        private string _connectionString;

        public DataManager(string connectionString)
        {
            _connectionString = connectionString;
        }

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
                        INNER JOIN Masters m ON r.Master = m.IDMasters
                        INNER JOIN Users u_m ON m.User = u_m.IDUser
                        INNER JOIN Client c ON r.Client = c.IDClient
                        INNER JOIN Status s ON r.Status = s.IDStatus
                        INNER JOIN Services sv ON r.Service = sv.IDServices
                        INNER JOIN Users u_u ON r.User = u_u.IDUser
                        ORDER BY r.Date DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Форматируем ФИО мастеров
                            string masterShortName = NameFormatter.FormatToShortName(
                                reader["MasterLastName"].ToString(),
                                reader["MasterFirstName"].ToString(),
                                reader["MasterMiddleName"].ToString()
                            );

                            // Форматируем ФИО клиентов
                            string clientShortName = NameFormatter.FormatToShortName(
                                reader["ClientLastName"].ToString(),
                                reader["ClientFirstName"].ToString(),
                                reader["ClientMiddleName"].ToString()
                            );

                            // Форматируем ФИО пользователей (менеджеров)
                            string userShortName = NameFormatter.FormatToShortName(
                                reader["UserLastName"].ToString(),
                                reader["UserFirstName"].ToString(),
                                reader["UserMiddleName"].ToString()
                            );

                            records.Add(new RecordData
                            {
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

        public List<string> GetStatusList()
        {
            var statuses = new List<string> { "Все статусы" };

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT StatusName FROM Status";

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
    }
}