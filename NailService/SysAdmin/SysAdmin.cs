using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public partial class SysAdmin : Form
    {
        private string _connection;
        public SysAdmin()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string databaseName = "db86";

            try
            {
                // 1. Строка подключения без указания базы данных
                string serverConnection = $"Server={Connection.Host};Uid={Connection.UserId};Pwd={Connection.Password};";

                // 2. Проверяем и создаем базу данных если её нет
                using (MySqlConnection con = new MySqlConnection(serverConnection))
                {
                    await con.OpenAsync();

                    string checkDbQuery = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";
                    MySqlCommand checkCmd = new MySqlCommand(checkDbQuery, con);
                    object result = await checkCmd.ExecuteScalarAsync();

                    if (result == null)
                    {
                        string createDbQuery = $"CREATE DATABASE `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
                        MySqlCommand createCmd = new MySqlCommand(createDbQuery, con);
                        await createCmd.ExecuteNonQueryAsync();
                        MessageBox.Show($"База данных '{databaseName}' успешно создана!", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    await con.CloseAsync();
                }

                // 3. Обновляем имя базы данных
                Connection.Database = databaseName;

                // 4. Создаем таблицы (сначала удаляем в правильном порядке)
                using (MySqlConnection con = Connection.GetConnection())
                {
                    await con.OpenAsync();

                    // 4.1 Сначала удаляем таблицы, которые имеют внешние ключи (в обратном порядке)
                    string[] dropTablesOrder = new string[]
                    {
                "record",      // зависит от всех
                "services",    // зависит от category
                "masters",     // зависит от users
                "users",       // зависит от role
                "category",    // независимая
                "client",      // независимая
                "status",      // независимая
                "role"         // независимая
                    };

                    foreach (string table in dropTablesOrder)
                    {
                        try
                        {
                            string dropQuery = $"DROP TABLE IF EXISTS `{table}`";
                            using (MySqlCommand cmd = new MySqlCommand(dropQuery, con))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch { }
                    }

                    // 4.2 Создаем таблицы заново (в правильном порядке)
                    string[] createQueries = GetTableStructure().Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sql in createQueries)
                    {
                        if (!string.IsNullOrWhiteSpace(sql))
                        {
                            try
                            {
                                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                                {
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Логируем ошибку но продолжаем
                                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                            }
                        }
                    }

                    MessageBox.Show($"Структура БД '{databaseName}' успешно создана!", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await con.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetTableStructure()
        {
            return @"
-- Table structure for table `category`
DROP TABLE IF EXISTS `category`;
CREATE TABLE `category` (
  `IDCategory` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `client`
DROP TABLE IF EXISTS `client`;
CREATE TABLE `client` (
  `IDClient` int NOT NULL AUTO_INCREMENT,
  `LastName` varchar(50) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Phone` varchar(20) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDClient`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `role`
DROP TABLE IF EXISTS `role`;
CREATE TABLE `role` (
  `IDRole` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) NOT NULL,
  PRIMARY KEY (`IDRole`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `users`
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `IDUser` int NOT NULL AUTO_INCREMENT,
  `LastName` varchar(50) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Login` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDUser`),
  UNIQUE KEY `Login` (`Login`),
  KEY `Role` (`Role`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`Role`) REFERENCES `role` (`IDRole`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `masters`
DROP TABLE IF EXISTS `masters`;
CREATE TABLE `masters` (
  `IDMasters` int NOT NULL AUTO_INCREMENT,
  `User` int NOT NULL,
  `Description` text,
  `Phone` varchar(20) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDMasters`),
  KEY `User` (`User`),
  CONSTRAINT `masters_ibfk_1` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `status`
DROP TABLE IF EXISTS `status`;
CREATE TABLE `status` (
  `IDStatus` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(50) NOT NULL,
  PRIMARY KEY (`IDStatus`),
  UNIQUE KEY `StatusName` (`StatusName`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `services`
DROP TABLE IF EXISTS `services`;
CREATE TABLE `services` (
  `IDServices` int NOT NULL AUTO_INCREMENT,
  `ServiceName` varchar(100) NOT NULL,
  `Description` text,
  `Price` decimal(10,2) NOT NULL,
  `Photo` longblob,
  `Category` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`IDServices`),
  KEY `Category` (`Category`),
  CONSTRAINT `services_ibfk_1` FOREIGN KEY (`Category`) REFERENCES `category` (`IDCategory`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Table structure for table `record`
DROP TABLE IF EXISTS `record`;
CREATE TABLE `record` (
  `IDRecord` int NOT NULL AUTO_INCREMENT,
  `Master` int NOT NULL,
  `Client` int NOT NULL,
  `Date` datetime NOT NULL,
  `Status` int NOT NULL,
  `Service` int NOT NULL,
  `User` int NOT NULL,
  `discount` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`IDRecord`),
  KEY `Master` (`Master`),
  KEY `Client` (`Client`),
  KEY `Status` (`Status`),
  KEY `Service` (`Service`),
  KEY `User` (`User`),
  CONSTRAINT `record_ibfk_1` FOREIGN KEY (`Master`) REFERENCES `masters` (`IDMasters`),
  CONSTRAINT `record_ibfk_2` FOREIGN KEY (`Client`) REFERENCES `client` (`IDClient`),
  CONSTRAINT `record_ibfk_3` FOREIGN KEY (`Status`) REFERENCES `status` (`IDStatus`),
  CONSTRAINT `record_ibfk_4` FOREIGN KEY (`Service`) REFERENCES `services` (`IDServices`),
  CONSTRAINT `record_ibfk_5` FOREIGN KEY (`User`) REFERENCES `users` (`IDUser`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 show = new Form1();
            show.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImportData importForm = new ImportData();
            importForm.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm();
            settingForm.Show();
            this.Hide();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => DatabaseBackup.CreateBackup());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");

                // Создаем папку, если её нет
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                    MessageBox.Show("Папка для резервных копий создана.",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Открываем папку в проводнике
                System.Diagnostics.Process.Start("explorer.exe", backupFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия папки: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
