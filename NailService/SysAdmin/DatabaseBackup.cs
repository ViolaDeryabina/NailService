using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NailService
{
    public class DatabaseBackup
    {
        private static string GetBackupFolder()
        {
            string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            return backupFolder;
        }

        /// <summary>
        /// Создание резервной копии с поддержкой BLOB (картинки) и правильной кодировкой
        /// </summary>
        public static string CreateBackup()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                string backupPath = Path.Combine(backupFolder, $"{Connection.Database}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

                string mysqldumpPath = FindMySqlDumpPath();

                if (string.IsNullOrEmpty(mysqldumpPath))
                {
                    MessageBox.Show("mysqldump.exe не найден! Убедитесь, что MySQL установлен.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Добавляем параметры для правильной кодировки
                string arguments = $"-u {Connection.UserId} -p{Connection.Password} " +
                    $"--default-character-set=utf8 " +           // Явно указываем UTF-8
                    $"--hex-blob " +                              // Для BLOB данных (картинки)
                    $"--single-transaction " +                    // Для целостности данных
                    $"--quick " +                                 // Для больших таблиц
                    $"--max_allowed_packet=512M " +               // Для больших BLOB
                    $"--extended-insert " +                       // Компактные INSERT
                    $"--complete-insert " +                       // Полные INSERT
                    $"--set-charset " +                           // Устанавливает SET NAMES
                    $"--skip-comments " +                         // Убираем лишние комментарии
                    $"{Connection.Database}";

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = mysqldumpPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                // В .NET Framework нельзя установить StandardInputEncoding, убираем эту строку

                process.Start();

                // Читаем вывод с правильной кодировкой
                string output = ReadProcessOutput(process.StandardOutput.BaseStream, Encoding.UTF8);
                string error = ReadProcessOutput(process.StandardError.BaseStream, Encoding.UTF8);
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // Сохраняем с UTF-8 без BOM
                    var utf8WithoutBom = new UTF8Encoding(false);
                    File.WriteAllText(backupPath, output, utf8WithoutBom);
                    CleanOldBackups(backupFolder, 10);

                    MessageBox.Show($"Резервная копия создана!\n{backupPath}\nРазмер: {new FileInfo(backupPath).Length / 1024:N0} КБ",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return backupPath;
                }
                else
                {
                    MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Чтение вывода процесса с правильной кодировкой
        /// </summary>
        private static string ReadProcessOutput(Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Поиск пути к mysqldump.exe
        /// </summary>
        private static string FindMySqlDumpPath()
        {
            string[] possiblePaths = {
                @"C:\Program Files\MySQL\MySQL Server 9.4\bin\mysqldump.exe",
                @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
                @"C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe",
                @"C:\Program Files\MySQL\MySQL Server 5.6\bin\mysqldump.exe",
                @"C:\xampp\mysql\bin\mysqldump.exe",
                @"C:\wamp64\bin\mysql\mysql5.7\bin\mysqldump.exe",
                @"C:\wamp\bin\mysql\mysql5.7\bin\mysqldump.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Если не нашли, ищем в PATH
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                foreach (string path in pathEnv.Split(';'))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string fullPath = Path.Combine(path, "mysqldump.exe");
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Восстановление из резервной копии
        /// </summary>
        public static bool RestoreBackup(string backupPath)
        {
            try
            {
                string mysqlPath = FindMySqlPath();

                if (string.IsNullOrEmpty(mysqlPath))
                {
                    MessageBox.Show("mysql.exe не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Используем параметры для правильной кодировки
                string arguments = $"-u {Connection.UserId} -p{Connection.Password} " +
                    $"--default-character-set=utf8 " +
                    $"-f " + // Force continue on errors
                    $"{Connection.Database}";

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = mysqlPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                // Читаем SQL файл и отправляем в mysql
                string sqlContent = File.ReadAllText(backupPath, Encoding.UTF8);

                // Пишем в стандартный ввод процесса
                using (var writer = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8))
                {
                    writer.Write(sqlContent);
                    writer.Flush();
                }
                process.StandardInput.Close();

                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0 || string.IsNullOrEmpty(error))
                {
                    MessageBox.Show("База данных успешно восстановлена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show($"Ошибка восстановления: {error}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Поиск пути к mysql.exe
        /// </summary>
        private static string FindMySqlPath()
        {
            string[] possiblePaths = {
                @"C:\Program Files\MySQL\MySQL Server 9.4\bin\mysql.exe",
                @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
                @"C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe",
                @"C:\Program Files\MySQL\MySQL Server 5.6\bin\mysql.exe",
                @"C:\xampp\mysql\bin\mysql.exe",
                @"C:\wamp64\bin\mysql\mysql5.7\bin\mysql.exe",
                @"C:\wamp\bin\mysql\mysql5.7\bin\mysql.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Если не нашли, ищем в PATH
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                foreach (string path in pathEnv.Split(';'))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string fullPath = Path.Combine(path, "mysql.exe");
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            return null;
        }

        private static void CleanOldBackups(string backupFolder, int maxBackupCount)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupFolder, "*.sql")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(maxBackupCount)
                    .ToList();

                foreach (var file in backupFiles)
                    file.Delete();
            }
            catch { }
        }

        /// <summary>
        /// Получение списка резервных копий
        /// </summary>
        public static System.Collections.Generic.List<FileInfo> GetBackups()
        {
            string backupFolder = GetBackupFolder();
            return Directory.GetFiles(backupFolder, "*.sql")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();
        }
    }
}