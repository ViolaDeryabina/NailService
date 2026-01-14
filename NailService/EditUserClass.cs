using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public class EditUserClass
    {
        private string _connection = Connection.ConnectionString;

        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        //ПОЛЬЗОВАТЕЛИ
        public UserModel LoadUserById(int userId)
        {
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                    u.IDUser,
                    u.LastName,
                    u.FirstName,
                    u.MiddleName,
                    u.Login,
                    u.Password,
                    u.Role as RoleID,
                    r.RoleName
                FROM Users u
                INNER JOIN Role r ON u.Role = r.IDRole
                WHERE u.IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                UserId = reader.GetInt32("IDUser"),
                                LastName = reader["LastName"]?.ToString() ?? "",
                                FirstName = reader["FirstName"]?.ToString() ?? "",
                                MiddleName = reader["MiddleName"]?.ToString() ?? "",
                                Login = reader["Login"]?.ToString() ?? "",
                                Password = reader["Password"]?.ToString() ?? "",
                                RoleId = reader.GetInt32("RoleID"),
                                RoleName = reader["RoleName"]?.ToString() ?? ""
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateUserInDatabase(UserModel user)
        {
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE Users 
                        SET LastName = @LastName, 
                            FirstName = @FirstName, 
                            MiddleName = @MiddleName, 
                            Login = @Login, 
                            Password = @Password, 
                            Role = @Role 
                        WHERE IDUser = @UserId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName);
                    cmd.Parameters.AddWithValue("@Login", user.Login);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Role", user.RoleId);
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                }
            }
        }

        //УСЛУГИ
        public ServiceModel LoadServiceById(int serviceId)
        {
            using (var connection = GetNewConnection()) // Используем новое соединение
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        s.IDServices,
                        s.ServiceName,
                        s.Description,
                        s.Price,
                        s.Category as CategoryID,
                        c.CategoryName
                    FROM Services s
                    INNER JOIN Category c ON s.Category = c.IDCategory
                    WHERE s.IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ServiceModel
                            {
                                IDServices = reader.GetInt32("IDServices"),
                                ServiceName = reader["ServiceName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                Price = reader.GetInt32("Price"),
                                Category = reader.GetInt32("CategoryID"),
                                CategoryName = reader["CategoryName"]?.ToString() ?? ""
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки услуги: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateServiceInDatabase(ServiceModel service)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE Services 
                SET ServiceName = @ServiceName, 
                    Description = @Description, 
                    Price = @Price, 
                    Category = @CategoryId 
                WHERE IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", service.Description);
                    cmd.Parameters.AddWithValue("@Price", service.Price);
                    cmd.Parameters.AddWithValue("@CategoryId", service.Category);  
                    cmd.Parameters.AddWithValue("@ServiceId", service.IDServices);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления услуги: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        //КЛИЕНТЫ
        public ClientModel LoadClientById(int clientId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                IDClient,
                LastName,
                FirstName,
                MiddleName,
                Phone
            FROM Client
            WHERE IDClient = @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClientModel
                            {
                                IDClient = reader.GetInt32("IDClient"),
                                LastName = reader["LastName"]?.ToString() ?? "",
                                FirstName = reader["FirstName"]?.ToString() ?? "",
                                MiddleName = reader["MiddleName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? ""
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки клиента: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateClientInDatabase(ClientModel client)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE Client 
                SET LastName = @LastName, 
                    FirstName = @FirstName, 
                    MiddleName = @MiddleName, 
                    Phone = @Phone 
                WHERE IDClient = @ClientId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LastName", client.LastName);
                    cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
                    cmd.Parameters.AddWithValue("@MiddleName", client.MiddleName);
                    cmd.Parameters.AddWithValue("@Phone", client.Phone);
                    cmd.Parameters.AddWithValue("@ClientId", client.IDClient);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления клиента: {ex.Message}");
                }
            }
        }


        //МАСТЕРА   

        public MasterModel LoadMasterById(int masterId)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    m.IDMasters,
                    m.User,
                    m.Description,
                    m.Phone,
                    u.LastName,
                    u.FirstName,
                    u.MiddleName
                FROM Masters m
                INNER JOIN Users u ON m.User = u.IDUser
                WHERE m.IDMasters = @MasterId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@MasterId", masterId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new MasterModel
                            {
                                IDMasters = reader.GetInt32("IDMasters"),
                                UserId = reader.GetInt32("User"),
                                Description = reader["Description"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                LastName = reader["LastName"]?.ToString() ?? "",
                                FirstName = reader["FirstName"]?.ToString() ?? "",
                                MiddleName = reader["MiddleName"]?.ToString() ?? ""
                            };
                        }
                    }
                    connection.Close();
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки мастера: {ex.Message}");
                    return null;
                }
            }
        }

        public void UpdateMasterInDatabase(MasterModel master)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"
                UPDATE Masters 
                SET Description = @Description, 
                    Phone = @Phone 
                WHERE IDMasters = @MasterId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Description", master.Description);
                    cmd.Parameters.AddWithValue("@Phone", master.Phone);
                    cmd.Parameters.AddWithValue("@MasterId", master.IDMasters);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления мастера: {ex.Message}");
                }
            }
        }

        // СТАТУСЫ
        private void UpdateStatusInDatabase(int statusId, string newStatusName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    // Проверяем, существует ли уже такой статус
                    string checkQuery = "SELECT COUNT(*) FROM Status WHERE StatusName = @StatusName AND IDStatus != @StatusId";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@StatusName", newStatusName);
                    checkCmd.Parameters.AddWithValue("@StatusId", statusId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Статус с таким названием уже существует",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                        return;
                    }

                    // Обновляем статус
                    string query = "UPDATE Status SET StatusName = @StatusName WHERE IDStatus = @StatusId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@StatusName", newStatusName);
                    cmd.Parameters.AddWithValue("@StatusId", statusId);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        ShowInfo("Статус успешно обновлен");
                    }
                    else
                    {
                        ShowInfo("Статус не найден");
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления статуса: {ex.Message}");
                }
            }
        }
        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
