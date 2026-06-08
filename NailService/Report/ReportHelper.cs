using System;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Вспомогательный класс для работы с отчетами
    /// </summary>
    public static class ReportHelper
    {
        /// <summary>
        /// Показать диалог выбора формата отчета
        /// </summary>
        public static string ShowFormatDialog()
        {
            using (Form formatDialog = new Form())
            {
                formatDialog.Text = "Выберите формат отчета";
                formatDialog.Size = new System.Drawing.Size(350, 180);
                formatDialog.StartPosition = FormStartPosition.CenterParent;
                formatDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                formatDialog.MaximizeBox = false;
                formatDialog.MinimizeBox = false;
                formatDialog.BackColor = System.Drawing.SystemColors.ControlLightLight;
                formatDialog.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular);

                Button btnExcel = new Button()
                {
                    Text = "Microsoft Excel (.xlsx)",
                    Location = new System.Drawing.Point(15, 30),
                    Size = new System.Drawing.Size(145, 50),
                    BackColor = System.Drawing.Color.HotPink,
                    FlatStyle = FlatStyle.Popup,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular)
                };

                Button btnPdf = new Button()
                {
                    Text = "PDF (.pdf)",
                    Location = new System.Drawing.Point(175, 30),
                    Size = new System.Drawing.Size(145, 50),
                    BackColor = System.Drawing.Color.HotPink,
                    FlatStyle = FlatStyle.Popup,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular)
                };

                Button btnCancel = new Button()
                {
                    Text = "Отмена",
                    Location = new System.Drawing.Point(15, 95),
                    Size = new System.Drawing.Size(305, 40),
                    BackColor = System.Drawing.Color.LightGray,
                    FlatStyle = FlatStyle.Popup,
                    Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular)
                };

                string result = null;

                btnExcel.Click += (s, arg) => { result = "Excel"; formatDialog.Close(); };
                btnPdf.Click += (s, arg) => { result = "PDF"; formatDialog.Close(); };
                btnCancel.Click += (s, arg) => { formatDialog.Close(); };

                formatDialog.Controls.Add(btnExcel);
                formatDialog.Controls.Add(btnPdf);
                formatDialog.Controls.Add(btnCancel);

                formatDialog.ShowDialog();
                return result;
            }
        }

        /// <summary>
        /// Показать диалог сохранения файла
        /// </summary>
        public static string ShowSaveFileDialog(string filter, string title, string defaultExt)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                FileName = $"Отчет_записей_{DateTime.Now:yyyyMMdd_HHmmss}.{defaultExt}",
                DefaultExt = defaultExt
            };

            return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.FileName : null;
        }

        /// <summary>
        /// Открыть созданный файл
        /// </summary>
        public static void OpenFile(string filePath)
        {
            DialogResult result = MessageBox.Show($"Отчет успешно сохранен в файл:\n{filePath}\n\nХотите открыть файл?",
                                                 "Отчет создан",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}