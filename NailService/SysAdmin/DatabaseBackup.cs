using MySqlConnector;        // Важно: именно MySqlConnector
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NailService
{
    public class DatabaseBackup
    {
        /// <summary>
        /// Создание резервной копии базы данных.
        /// </summary>
        public static string CreateBackup()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                string backupPath = Path.Combine(backupFolder, $"{Connection.Database}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

                // --- Код бэкапа из официальной документации ---
                using (var conn = new MySqlConnection(Connection.ConnectionString))
                using (var cmd = conn.CreateCommand())
                using (var mb = new MySqlBackup(cmd))
                {
                    conn.Open();
                    mb.ExportToFile(backupPath);   // Экспорт базы в файл
                }
                // --------------------------------------------

                CleanOldBackups(backupFolder, 10);
                FileInfo fileInfo = new FileInfo(backupPath);
                MessageBox.Show($"Резервная копия создана!\n{backupPath}\nРазмер: {fileInfo.Length / 1024:N0} КБ",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return backupPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания бэкапа:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Восстановление базы данных из резервной копии.
        /// </summary>
        public static bool RestoreBackup(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    MessageBox.Show("Файл бэкапа не найден!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // --- Код восстановления из официальной документации ---
                using (var conn = new MySqlConnection(Connection.ConnectionString))
                using (var cmd = conn.CreateCommand())
                using (var mb = new MySqlBackup(cmd))
                {
                    conn.Open();
                    mb.ImportFromFile(backupPath); // Импорт из файла
                }
                // ----------------------------------------------------

                MessageBox.Show("База данных успешно восстановлена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // --- Вспомогательные методы (остаются без изменений) ---
        private static string GetBackupFolder()
        {
            string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);
            return backupFolder;
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

        public static List<FileInfo> GetBackups()
        {
            string backupFolder = GetBackupFolder();
            return Directory.GetFiles(backupFolder, "*.sql")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();
        }
    }
}