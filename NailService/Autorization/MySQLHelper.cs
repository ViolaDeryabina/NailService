using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NailService
{
    public class MySQLHelper
    {
        private static string _connection = Connection.ConnectionString;
        static public string GetHash(string str)
        {
            var sha2 = SHA256.Create();
            var hbyte = sha2.ComputeHash(Encoding.UTF8.GetBytes(str));

            return BitConverter.ToString(hbyte).Replace("-", "").ToLower();
        }
        static public string GetRoleName(string login, string password)
        {
            string roleName = null;

            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                con.Open();
                string query = @"SELECT 
                            CASE Role 
                                WHEN 1 THEN 'Директор'
                                WHEN 2 THEN 'Админ' 
                                WHEN 3 THEN 'Мастер'
                                WHEN 4 THEN 'Менеджер'
                                ELSE 'Неизвестно'
                            END as RoleName
                         FROM users 
                         WHERE Login = @login AND Password = @password;";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    roleName = result.ToString();
                }

                con.Close();
            }

            return roleName;
        }
        public static int GetRoleId(string login, string passwordHash)
        {
            using (MySqlConnection con = new MySqlConnection(Connection.ConnectionString))
            {
                con.Open();
                string query = @"SELECT u.Role 
                        FROM users u 
                        WHERE u.Login = @Login 
                        AND u.Password = @Password 
                        AND u.IsActive = 1";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@Password", passwordHash);

                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }



        // Получение фамилии и инициалов
        static public string GetLastNameWithInitials(string login, string password)
        {
            using (MySqlConnection con = new MySqlConnection(_connection))
            {
                con.Open();
                string query = @"SELECT LastName, FirstName, MiddleName 
                           FROM users 
                           WHERE Login = @login AND Password = @password;";

                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string lastName = reader["LastName"]?.ToString() ?? "";
                        string firstName = reader["FirstName"]?.ToString() ?? "";
                        string middleName = reader["MiddleName"]?.ToString() ?? "";

                        return FormatLastNameWithInitials(lastName, firstName, middleName);
                    }
                }
            }

            return null;
        }

        // Форматирование фамилии с инициалами
        static private string FormatLastNameWithInitials(string lastName, string firstName, string middleName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return "Не указано";

            var initials = new List<string>();

            if (!string.IsNullOrWhiteSpace(firstName) && firstName.Length > 0)
                initials.Add(firstName[0] + ".");

            if (!string.IsNullOrWhiteSpace(middleName) && middleName.Length > 0)
                initials.Add(middleName[0] + ".");

            return lastName + (initials.Count > 0 ? " " + string.Join(" ", initials) : "");
        }
    }
}

