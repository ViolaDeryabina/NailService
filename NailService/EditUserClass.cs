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
    }
}
