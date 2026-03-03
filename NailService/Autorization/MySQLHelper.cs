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
    /// <summary>
    /// Вспомогательный класс для работы с MySQL и криптографией
    /// Содержит методы для хеширования паролей, получения информации о пользователях
    /// </summary>
    public class MySQLHelper
    {
        private static string _connection = Connection.ConnectionString;

        /// <summary>
        /// Вычисляет SHA-256 хеш от переданной строки
        /// </summary>
        /// <param name="str">Исходная строка (пароль)</param>
        /// <returns>Хеш строки в нижнем регистре без дефисов</returns>
        static public string GetHash(string str)
        {
            var sha2 = SHA256.Create();
            var hbyte = sha2.ComputeHash(Encoding.UTF8.GetBytes(str));

            return BitConverter.ToString(hbyte).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Получает название роли пользователя по логину и паролю
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя (в открытом виде)</param>
        /// <returns>Название роли или null, если пользователь не найден</returns>
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

        /// <summary>
        /// Получает ID роли активного пользователя по логину и хешу пароля
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <returns>ID роли или 0, если пользователь не найден</returns>
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

        /// <summary>
        /// Получает фамилию с инициалами пользователя по логину и паролю
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Фамилия с инициалами (например: "Иванов И.И.") или null, если пользователь не найден</returns>
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

        /// <summary>
        /// Форматирует фамилию с инициалами из отдельных компонентов
        /// </summary>
        /// <param name="lastName">Фамилия</param>
        /// <param name="firstName">Имя</param>
        /// <param name="middleName">Отчество</param>
        /// <returns>Форматированная строка вида "Фамилия И.О."</returns>
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