using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public class DatabaseRestore
    {
        private string _connectionString;
        private string _databaseName;
        private string _serverConnectionString; // Строка подключения без указания базы данных

        // События для отслеживания прогресса
        public event Action<int, string> ProgressChanged;
        public event Action<string> StatusChanged;

        public DatabaseRestore()
        {
            _connectionString = Connection.ConnectionString;

            // Извлекаем имя базы данных из строки подключения
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            _databaseName = builder.Database;

            // Создаем строку подключения без указания базы данных (только сервер)
            _serverConnectionString = $"server={builder.Server};user id={builder.UserID};password={builder.Password};";
            if (builder.Port != 3306)
                _serverConnectionString += $"port={builder.Port};";
        }

        /// <summary>
        /// Восстановление базы данных из SQL файла
        /// </summary>
        public async Task<RestoreResult> RestoreFromFileAsync(string filePath)
        {
            var result = new RestoreResult();

            try
            {
                OnStatusChanged("Проверка файла...");

                if (!File.Exists(filePath))
                {
                    result.Success = false;
                    result.ErrorMessage = "Файл не найден!";
                    return result;
                }

                OnStatusChanged("Проверка и создание базы данных...");

                // Проверяем существование базы данных и создаем если нужно
                if (!await DatabaseExists())
                {
                    OnStatusChanged($"База данных '{_databaseName}' не найдена. Создание...");
                    await CreateDatabase();
                    OnStatusChanged($"База данных '{_databaseName}' успешно создана!");
                }
                else
                {
                    OnStatusChanged($"База данных '{_databaseName}' существует.");
                }

                OnStatusChanged("Чтение SQL файла...");
                string sqlContent = File.ReadAllText(filePath, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(sqlContent))
                {
                    result.Success = false;
                    result.ErrorMessage = "Файл пуст!";
                    return result;
                }

                OnStatusChanged("Подключение к базе данных...");

                using (MySqlConnection con = new MySqlConnection(_connectionString))
                {
                    await con.OpenAsync();

                    // Сначала удаляем существующие таблицы (если нужно)
                    OnStatusChanged("Очистка существующих таблиц...");
                    await DropAllTables(con);

                    // Разбираем SQL на отдельные запросы
                    OnStatusChanged("Разбор SQL запросов...");
                    var queries = ParseSqlQueriesAdvanced(sqlContent);

                    // Фильтруем нужные запросы
                    var filteredQueries = FilterQueries(queries);

                    result.TotalQueries = filteredQueries.Count;

                    OnProgressChanged(0, $"Найдено {filteredQueries.Count} SQL команд...");

                    // Отключаем проверку внешних ключей
                    using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0", con))
                        await cmd.ExecuteNonQueryAsync();

                    try
                    {
                        int processed = 0;
                        int lastPercent = 0;

                        foreach (var query in filteredQueries)
                        {
                            processed++;
                            int percent = (processed * 100 / result.TotalQueries);

                            if (percent != lastPercent)
                            {
                                OnProgressChanged(percent, $"Выполнение: {processed} из {result.TotalQueries}");
                                lastPercent = percent;
                            }

                            if (string.IsNullOrWhiteSpace(query))
                                continue;

                            try
                            {
                                // Заменяем имя базы данных если нужно
                                string processedQuery = query
                                    .Replace("`nailservicedb`.", $"`{_databaseName}`.")
                                    .Replace("`db86`.", $"`{_databaseName}`.");

                                using (var cmd = new MySqlCommand(processedQuery, con))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                    result.SuccessCount++;
                                }
                            }
                            catch (Exception ex)
                            {
                                result.ErrorCount++;
                                string shortQuery = query.Length > 100 ? query.Substring(0, 100) + "..." : query;
                                result.Errors.Add($"Ошибка: {ex.Message}\nЗапрос: {shortQuery}");

                                // Логируем ошибку для отладки
                                Debug.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
                                Debug.WriteLine($"Запрос: {query}");
                            }
                        }

                        result.Success = result.ErrorCount == 0;
                    }
                    finally
                    {
                        // Включаем проверку внешних ключей обратно
                        using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1", con))
                            await cmd.ExecuteNonQueryAsync();
                    }
                }

                OnProgressChanged(100, "Восстановление завершено!");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Проверка существования базы данных
        /// </summary>
        private async Task<bool> DatabaseExists()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_serverConnectionString))
                {
                    await con.OpenAsync();
                    using (var cmd = new MySqlCommand(
                        $"SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '{_databaseName}'",
                        con))
                    {
                        long count = (long)await cmd.ExecuteScalarAsync();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки существования БД: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Создание базы данных
        /// </summary>
        private async Task CreateDatabase()
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_serverConnectionString))
                {
                    await con.OpenAsync();

                    // Создаем базу данных с кодировкой UTF8
                    string createDbQuery = $"CREATE DATABASE IF NOT EXISTS `{_databaseName}` " +
                                          $"CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";

                    using (var cmd = new MySqlCommand(createDbQuery, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Предоставляем права (опционально)
                    string grantQuery = $"GRANT ALL PRIVILEGES ON `{_databaseName}`.* TO '{GetCurrentUser()}'@'%'";
                    try
                    {
                        using (var cmd = new MySqlCommand(grantQuery, con))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка выдачи прав: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось создать базу данных '{_databaseName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получение текущего пользователя из строки подключения
        /// </summary>
        private string GetCurrentUser()
        {
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            return builder.UserID;
        }

        /// <summary>
        /// Удаление всех таблиц
        /// </summary>
        private async Task DropAllTables(MySqlConnection connection)
        {
            // Получаем список всех таблиц
            var tables = new List<string>();
            using (var cmd = new MySqlCommand(
                $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{_databaseName}'",
                connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
            }

            if (tables.Count == 0)
                return;

            // Отключаем проверку внешних ключей
            using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0", connection))
                await cmd.ExecuteNonQueryAsync();

            // Удаляем таблицы
            foreach (var table in tables)
            {
                try
                {
                    using (var cmd = new MySqlCommand($"DROP TABLE IF EXISTS `{table}`", connection))
                        await cmd.ExecuteNonQueryAsync();
                    Debug.WriteLine($"Удалена таблица: {table}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка удаления таблицы {table}: {ex.Message}");
                }
            }

            // Включаем проверку внешних ключей обратно
            using (var cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1", connection))
                await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Фильтрация SQL запросов - оставляем только CREATE TABLE, INSERT INTO, DROP TABLE
        /// </summary>
        private List<string> FilterQueries(List<string> queries)
        {
            var filtered = new List<string>();

            foreach (var query in queries)
            {
                string upperQuery = query.ToUpperInvariant().TrimStart();

                // Пропускаем комментарии и пустые строки
                if (string.IsNullOrWhiteSpace(query))
                    continue;

                // Оставляем только нужные типы запросов
                if (upperQuery.StartsWith("CREATE TABLE") ||
                    upperQuery.StartsWith("INSERT INTO") ||
                    upperQuery.StartsWith("DROP TABLE") ||
                    upperQuery.StartsWith("LOCK TABLES") ||
                    upperQuery.StartsWith("UNLOCK TABLES"))
                {
                    filtered.Add(query);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Продвинутый парсинг SQL файла
        /// </summary>
        private List<string> ParseSqlQueriesAdvanced(string sqlContent)
        {
            var queries = new List<string>();
            var currentQuery = new StringBuilder();
            bool inString = false;
            bool inComment = false;
            bool inMultilineComment = false;
            char stringChar = '\0';

            for (int i = 0; i < sqlContent.Length; i++)
            {
                char c = sqlContent[i];

                // Обработка однострочных комментариев --
                if (!inString && !inMultilineComment && i + 1 < sqlContent.Length && c == '-' && sqlContent[i + 1] == '-')
                {
                    inComment = true;
                    i++;
                    continue;
                }

                // Обработка многострочных комментариев /* */
                if (!inString && !inComment && i + 1 < sqlContent.Length && c == '/' && sqlContent[i + 1] == '*')
                {
                    inMultilineComment = true;
                    i++;
                    continue;
                }

                // Конец однострочного комментария
                if (inComment && c == '\n')
                {
                    inComment = false;
                    continue;
                }

                // Конец многострочного комментария
                if (inMultilineComment && i + 1 < sqlContent.Length && c == '*' && sqlContent[i + 1] == '/')
                {
                    inMultilineComment = false;
                    i++;
                    continue;
                }

                // Пропускаем содержимое комментариев
                if (inComment || inMultilineComment)
                    continue;

                // Обработка строк
                if ((c == '\'' || c == '"') && (i == 0 || sqlContent[i - 1] != '\\'))
                {
                    if (!inString)
                    {
                        inString = true;
                        stringChar = c;
                    }
                    else if (c == stringChar)
                    {
                        inString = false;
                    }
                }

                currentQuery.Append(c);

                // Проверка на конец запроса (точка с запятой вне строки)
                if (c == ';' && !inString)
                {
                    string query = currentQuery.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        queries.Add(query);
                    }
                    currentQuery.Clear();
                }
            }

            // Добавляем последний запрос, если есть
            if (currentQuery.Length > 0)
            {
                string query = currentQuery.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queries.Add(query);
                }
            }

            return queries;
        }

        private void OnProgressChanged(int percent, string message)
        {
            ProgressChanged?.Invoke(percent, message);
        }

        private void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(status);
        }
    }

    /// <summary>
    /// Результат восстановления базы данных
    /// </summary>
    public class RestoreResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalQueries { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public string GetSummaryMessage()
        {
            if (Success)
            {
                return $"✅ Восстановление успешно завершено!\n\n" +
                       $"Выполнено запросов: {SuccessCount} из {TotalQueries}";
            }
            else if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return $"❌ Ошибка восстановления:\n{ErrorMessage}";
            }
            else
            {
                string message = $"⚠️ Восстановление завершено с ошибками!\n\n" +
                                 $"✅ Успешно: {SuccessCount} запросов\n" +
                                 $"❌ Ошибок: {ErrorCount} из {TotalQueries}\n\n";

                if (Errors.Count > 0 && Errors.Count <= 5)
                {
                    message += $"Ошибки:\n{string.Join("\n", Errors)}";
                }
                else if (Errors.Count > 5)
                {
                    message += $"Первые 5 ошибок:\n{string.Join("\n", Errors.Take(5))}";
                }

                return message;
            }
        }
    }
}