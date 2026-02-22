using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace NailService
{
    public class StatusUpdater
    {
        private string _connectionString;

        public StatusUpdater(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool UpdateRecordStatus(int recordId, int newStatusId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"UPDATE record 
                                    SET Status = @Status 
                                    WHERE IDRecord = @IDRecord";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
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

        public DataTable GetStatuses()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT IDStatus, StatusName FROM status WHERE IsActive = 1 ORDER BY StatusName";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке статусов: {ex.Message}");
            }
        }
    }
}