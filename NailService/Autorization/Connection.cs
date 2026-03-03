using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace NailService
{
    /// <summary>
    /// Класс для управления подключением к базе данных MySQL
    /// Настройки подключения загружаются из файла конфигурации приложения
    /// </summary>
    public class Connection
    {
        /// <summary>Адрес сервера базы данных</summary>
        public static string Host { get; set; } = Properties.Settings.Default.host;

        /// <summary>Название базы данных</summary>
        public static string Database { get; set; } = Properties.Settings.Default.database;

        /// <summary>Имя пользователя для подключения к БД</summary>
        public static string UserId { get; set; } = Properties.Settings.Default.uid;

        /// <summary>Пароль пользователя для подключения к БД</summary>
        public static string Password { get; set; } = Properties.Settings.Default.pwd;

        /// <summary>
        /// Строка подключения к базе данных, формируемая из текущих настроек
        /// </summary>
        public static string ConnectionString => $"Server={Host};Database={Database};Uid={UserId};Pwd={Password};";

        /// <summary>
        /// Создает и возвращает новое подключение к базе данных
        /// </summary>
        /// <returns>Объект MySqlConnection с настроенной строкой подключения</returns>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>
        /// Проверяет возможность подключения к базе данных с текущими настройками
        /// </summary>
        /// <returns>true если подключение успешно, false в противном случае</returns>
        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}