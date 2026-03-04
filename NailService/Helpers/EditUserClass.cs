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
    /// <summary>
    /// Класс для выполнения операций редактирования данных в базе
    /// Содержит методы для загрузки и обновления пользователей, услуг, клиентов, мастеров
    /// </summary>
    public class EditUserClass
    {
        private string _connection = Connection.ConnectionString;

        /// <summary>
        /// Создание нового подключения к базе данных
        /// </summary>
        private MySqlConnection GetNewConnection()
        {
            return new MySqlConnection(_connection);
        }

        /// <summary>
        /// Получение ID мастера по логину и паролю (для входа)
        /// </summary>
        public static int GetMasterId(string login, string passwordHash)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT m.IDMasters 
                        FROM Users u
                        INNER JOIN Masters m ON u.IDUser = m.User
                        WHERE u.Login = @Login 
                        AND u.Password = @Password 
                        AND u.IsActive = 1";

                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Password", passwordHash);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }

                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения ID мастера: {ex.Message}");
                return 0;
            }
        }

        #region Работа с пользователями

        /// <summary>
        /// Загрузка данных пользователя по ID
        /// </summary>
        public UserModel LoadUserById(int userId)
        {
            using (var connection = GetNewConnection())
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

        /// <summary>
        /// Обновление данных пользователя в базе
        /// </summary>
        public void UpdateUserInDatabase(UserModel user)
        {
            using (var connection = GetNewConnection())
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

        #endregion

        #region Работа с услугами

        /// <summary>
        /// Загрузка данных услуги по ID
        /// </summary>
        public ServiceModel LoadServiceById(int serviceId)
        {
            ServiceModel service = null;

            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                        s.IDServices,
                        s.ServiceName,
                        s.Description,
                        s.Price,
                        s.Category,
                        s.Photo,
                        s.IsActive,
                        c.CategoryName
                    FROM services s
                    LEFT JOIN Category c ON s.Category = c.IDCategory
                    WHERE s.IDServices = @ServiceId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            service = new ServiceModel
                            {
                                IDServices = Convert.ToInt32(reader["IDServices"]),
                                ServiceName = reader["ServiceName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                Price = Convert.ToInt32(reader["Price"]),
                                Category = Convert.ToInt32(reader["Category"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                Photo = reader["Photo"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                ServiceImage = null
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки услуги: {ex.Message}");
                }
            }

            return service;
        }

        /// <summary>
        /// Обновление данных услуги в базе
        /// </summary>
        public bool UpdateServiceInDatabase(ServiceModel service)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

                    string query;
                    MySqlCommand cmd;

                    if (service.PhotoBytes != null && service.PhotoBytes.Length > 0)
                    {
                        query = @"UPDATE services 
                         SET ServiceName = @ServiceName,
                             Description = @Description,
                             Price = @Price,
                             Category = @Category,
                             Photo = @Photo
                         WHERE IDServices = @ServiceId";

                        cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@Photo", service.PhotoBytes);
                    }
                    else
                    {
                        query = @"UPDATE services 
                         SET ServiceName = @ServiceName,
                             Description = @Description,
                             Price = @Price,
                             Category = @Category,
                             Photo = NULL
                         WHERE IDServices = @ServiceId";

                        cmd = new MySqlCommand(query, connection);
                    }

                    cmd.Parameters.AddWithValue("@ServiceId", service.IDServices);
                    cmd.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", service.Description);
                    cmd.Parameters.AddWithValue("@Price", service.Price);
                    cmd.Parameters.AddWithValue("@Category", service.Category);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно обновлена", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить услугу", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении услуги: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }


        #endregion

        #region Работа с клиентами

        /// <summary>
        /// Загрузка данных клиента по ID
        /// </summary>
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

        /// <summary>
        /// Обновление данных клиента в базе
        /// </summary>
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

        #endregion

        #region Работа с мастерами

        /// <summary>
        /// Загрузка данных мастера по ID
        /// </summary>
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

        /// <summary>
        /// Обновление данных мастера в базе
        /// </summary>
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

        #endregion

        #region Работа со статусами

        /// <summary>
        /// Обновление названия статуса в базе
        /// </summary>
        private void UpdateStatusInDatabase(int statusId, string newStatusName)
        {
            using (var connection = GetNewConnection())
            {
                try
                {
                    connection.Open();

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

        #endregion

        /// <summary>
        /// Отображение информационного сообщения
        /// </summary>
        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}