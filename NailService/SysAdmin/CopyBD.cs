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
    public partial class CopyBD : Form
    {
        private DatabaseRestore _databaseRestore;
        private string backupFolderPath;
        private string selectedSqlFile = null; // Хранит выбранный SQL файл

        public CopyBD()
        {
            InitializeComponent();

            // Инициализация пути к папке Backups
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            backupFolderPath = Path.Combine(appDirectory, "Backups");

            // Инициализация класса восстановления (замените на свою строку подключения)
            string connectionString =Connection.ConnectionString;
            _databaseRestore = new DatabaseRestore();


            // Настройка button1
            button1.Text = "Выбрать SQL файл";
            button1.BackColor = System.Drawing.Color.LightBlue;
        }

        // Кнопка создания резервной копии
        private async void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите создать резервную копию базы данных?",
                "Подтверждение создания резервной копии",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    button3.Enabled = false;
                    button3.Text = "Создание резервной копии...";

                    await Task.Run(() => DatabaseBackup.CreateBackup());

                    button3.Enabled = true;
                    button3.Text = "Создать резервную копию";

                    DialogResult openFolder = MessageBox.Show(
                        "Резервная копия успешно создана!\n\nОткрыть папку с резервными копиями?",
                        "Успех",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (openFolder == DialogResult.Yes)
                    {
                        OpenBackupFolder();
                    }
                }
                catch (Exception ex)
                {
                    button3.Enabled = true;
                    button3.Text = "Создать резервную копию";

                    MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // Кнопка выбора SQL файла
        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите SQL файл с резервной копией базы данных";
                openFileDialog.Filter = "SQL файлы (*.sql)|*.sql|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false; // Только один файл

                // Устанавливаем начальную папку - папку с бэкапами
                if (Directory.Exists(backupFolderPath))
                {
                    openFileDialog.InitialDirectory = backupFolderPath;
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedSqlFile = openFileDialog.FileName;

                    // Показываем информацию о выбранном файле
                    FileInfo fileInfo = new FileInfo(selectedSqlFile);
                    string message = $"✅ Выбран файл: {Path.GetFileName(selectedSqlFile)}\n" +
                                   $"📁 Размер: {fileInfo.Length / 1024.0:F2} KB\n" +
                                   $"📅 Изменен: {fileInfo.LastWriteTime:dd.MM.yyyy HH:mm:ss}\n\n" +
                                   $"Нажмите кнопку 'Восстановить БД' для начала восстановления.";

                    MessageBox.Show(message, "Файл выбран",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Изменяем цвет кнопки восстановления, чтобы показать, что файл выбран
                    button2.BackColor = System.Drawing.Color.LightGreen;
                    button2.Text = "Восстановить БД (файл выбран)";
                }
            }
        }

        // Кнопка восстановления базы данных
        private async void button2_Click(object sender, EventArgs e)
        {
            // Проверяем, выбран ли файл
            if (string.IsNullOrEmpty(selectedSqlFile))
            {
                DialogResult chooseFile = MessageBox.Show(
                    "Файл для восстановления не выбран.\n\nХотите выбрать файл сейчас?",
                    "Файл не выбран",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (chooseFile == DialogResult.Yes)
                {
                    button1_Click(sender, e); // Вызываем выбор файла
                    return;
                }
                return;
            }

            // Проверяем, существует ли файл
            if (!File.Exists(selectedSqlFile))
            {
                MessageBox.Show("Выбранный файл не найден!\nПожалуйста, выберите файл заново.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                selectedSqlFile = null;
                button2.BackColor = System.Drawing.SystemColors.Control;
                button2.Text = "Восстановить БД";
                return;
            }

            // Запрашиваем подтверждение перед восстановлением
            DialogResult confirmResult = MessageBox.Show(
                "ВНИМАНИЕ! Восстановление базы данных из резервной копии приведет к ПОТЕРЕ текущих данных.\n\n" +
                $"Файл для восстановления: {Path.GetFileName(selectedSqlFile)}\n\n" +
                "Перед восстановлением рекомендуется создать резервную копию.\n\n" +
                "Вы уверены, что хотите продолжить?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.No)
                return;

            // Финальное предупреждение
            DialogResult finalWarning = MessageBox.Show(
                $"Восстановление из файла: {Path.GetFileName(selectedSqlFile)}\n\n" +
                "Это действие ЗАМЕНИТ все текущие данные.\n" +
                "Операцию нельзя отменить!\n\n" +
                "Продолжить восстановление?",
                "Финальное предупреждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (finalWarning == DialogResult.Yes)
            {
                await RestoreDatabase(selectedSqlFile);
            }
        }

        // Метод восстановления базы данных с отображением прогресса
        private async Task RestoreDatabase(string filePath)
        {
            // Создаем форму прогресса
            var progressForm = new Form();
            progressForm.Text = "Восстановление базы данных";
            progressForm.Size = new System.Drawing.Size(550, 180);
            progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            progressForm.StartPosition = FormStartPosition.CenterParent;
            progressForm.ControlBox = false;

            var progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 30;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            var lblStatus = new Label();
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 40;
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblStatus.Text = "Подготовка к восстановлению...";
            lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10);

            var lblDetail = new Label();
            lblDetail.Dock = DockStyle.Fill;
            lblDetail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblDetail.Text = "Начало восстановления...";
            lblDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);

            var lblFile = new Label();
            lblFile.Dock = DockStyle.Bottom;
            lblFile.Height = 30;
            lblFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblFile.Text = $"Файл: {Path.GetFileName(filePath)}";
            lblFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 7);
            lblFile.Padding = new Padding(5, 0, 0, 0);

            progressForm.Controls.Add(lblDetail);
            progressForm.Controls.Add(lblStatus);
            progressForm.Controls.Add(progressBar);
            progressForm.Controls.Add(lblFile);

            // Локальные обработчики событий
            void OnProgressChanged(int percent, string message)
            {
                if (progressForm.InvokeRequired)
                {
                    progressForm.Invoke(new Action(() => OnProgressChanged(percent, message)));
                    return;
                }
                progressBar.Value = percent;
                lblStatus.Text = message;
            }

            void OnStatusChanged(string status)
            {
                if (progressForm.InvokeRequired)
                {
                    progressForm.Invoke(new Action(() => OnStatusChanged(status)));
                    return;
                }
                lblDetail.Text = status;
            }

            // Подписываемся на события
            _databaseRestore.ProgressChanged += OnProgressChanged;
            _databaseRestore.StatusChanged += OnStatusChanged;

            progressForm.Show();
            Application.DoEvents();

            try
            {
                // Выполняем восстановление
                var result = await _databaseRestore.RestoreFromFileAsync(filePath);

                progressForm.Close();

                // Показываем результат
                MessageBox.Show(result.GetSummaryMessage(),
                    result.Success ? "Успех" : "Результат восстановления",
                    MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                // Если восстановление прошло успешно, предлагаем перезагрузить приложение
                if (result.Success)
                {
                    DialogResult restart = MessageBox.Show(
                        "База данных успешно восстановлена!\n\n" +
                        "Для применения изменений рекомендуется перезапустить приложение.\n\n" +
                        "Перезапустить сейчас?",
                        "Перезапуск приложения",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (restart == DialogResult.Yes)
                    {
                        Application.Restart();
                    }
                }
            }
            catch (Exception ex)
            {
                progressForm.Close();
                MessageBox.Show($"Критическая ошибка при восстановлении:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Отписываемся от событий
                _databaseRestore.ProgressChanged -= OnProgressChanged;
                _databaseRestore.StatusChanged -= OnStatusChanged;
            }
        }

        // Открытие папки с бэкапами
        private void OpenBackupFolder()
        {
            try
            {
                if (Directory.Exists(backupFolderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", backupFolderPath);
                }
                else
                {
                    MessageBox.Show($"Папка с резервными копиями ещё не создана.\nОна будет создана автоматически при первом создании бэкапа.\n\nПуть: {backupFolderPath}",
                        "Папка не найдена",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SysAdmin settingForm = new SysAdmin();
            settingForm.Show();
            this.Hide();
        }
    }
}