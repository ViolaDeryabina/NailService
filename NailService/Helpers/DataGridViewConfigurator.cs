using System;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    public static class DataGridViewConfigurator
    {
        /// <summary>
        /// Настройка DataGridView для отображения услуг
        /// </summary>
        public static void ConfigureServicesDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                // Настройка внешнего вида
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dataGridView.AllowUserToResizeRows = false;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ReadOnly = true;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = false;

                // Настройка высоты строк
                dataGridView.RowTemplate.Height = 80;
                dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                // Настройка колонок, если они существуют
                if (dataGridView.Columns.Contains("ID"))
                {
                    dataGridView.Columns["ID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("CategoryID"))
                {
                    dataGridView.Columns["CategoryID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("Миниатюра") && dataGridView.Columns["Миниатюра"] != null)
                {
                    dataGridView.Columns["Миниатюра"].Width = 100;
                    dataGridView.Columns["Миниатюра"].MinimumWidth = 80;
                    dataGridView.Columns["Миниатюра"].HeaderText = "Фото";
                    dataGridView.Columns["Миниатюра"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (dataGridView.Columns.Contains("Название услуги") && dataGridView.Columns["Название услуги"] != null)
                {
                    dataGridView.Columns["Название услуги"].Width = 180;
                    dataGridView.Columns["Название услуги"].MinimumWidth = 120;
                }

                if (dataGridView.Columns.Contains("Описание") && dataGridView.Columns["Описание"] != null)
                {
                    dataGridView.Columns["Описание"].Width = 250;
                    dataGridView.Columns["Описание"].MinimumWidth = 150;
                    dataGridView.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                if (dataGridView.Columns.Contains("Цена") && dataGridView.Columns["Цена"] != null)
                {
                    dataGridView.Columns["Цена"].Width = 100;
                    dataGridView.Columns["Цена"].MinimumWidth = 80;
                    dataGridView.Columns["Цена"].DefaultCellStyle.Format = "N0";
                    dataGridView.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                if (dataGridView.Columns.Contains("Категория") && dataGridView.Columns["Категория"] != null)
                {
                    dataGridView.Columns["Категория"].Width = 120;
                    dataGridView.Columns["Категория"].MinimumWidth = 100;
                }

                // Включаем AutoSizeMode для оставшихся колонок
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    if (column.Visible &&
                        column.Name != "Миниатюра" &&
                        column.Name != "Название услуги" &&
                        column.Name != "Описание" &&
                        column.Name != "Цена" &&
                        column.Name != "Категория")
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки DataGridView: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DataGridView для отображения пользователей
        /// </summary>
        public static void ConfigureUsersDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ReadOnly = true;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = false;

                if (dataGridView.Columns.Contains("ID"))
                {
                    dataGridView.Columns["ID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("RoleID"))
                {
                    dataGridView.Columns["RoleID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("Пароль"))
                {
                    dataGridView.Columns["Пароль"].Visible = false;
                }

                if (dataGridView.Columns.Contains("ФИО") && dataGridView.Columns["ФИО"] != null)
                {
                    dataGridView.Columns["ФИО"].FillWeight = 35;
                }

                if (dataGridView.Columns.Contains("Логин") && dataGridView.Columns["Логин"] != null)
                {
                    dataGridView.Columns["Логин"].FillWeight = 30;
                }

                if (dataGridView.Columns.Contains("Роль") && dataGridView.Columns["Роль"] != null)
                {
                    dataGridView.Columns["Роль"].FillWeight = 35;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки DataGridView пользователей: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DataGridView для отображения мастеров
        /// </summary>
        public static void ConfigureMastersDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ReadOnly = true;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = false;

                if (dataGridView.Columns.Contains("ID"))
                {
                    dataGridView.Columns["ID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("ФИО") && dataGridView.Columns["ФИО"] != null)
                {
                    dataGridView.Columns["ФИО"].FillWeight = 30;
                }

                if (dataGridView.Columns.Contains("Описание") && dataGridView.Columns["Описание"] != null)
                {
                    dataGridView.Columns["Описание"].FillWeight = 50;
                    dataGridView.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }

                if (dataGridView.Columns.Contains("Телефон") && dataGridView.Columns["Телефон"] != null)
                {
                    dataGridView.Columns["Телефон"].FillWeight = 20;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки DataGridView мастеров: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DataGridView для отображения клиентов
        /// </summary>
        public static void ConfigureClientsDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ReadOnly = true;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = false;

                if (dataGridView.Columns.Contains("ID"))
                {
                    dataGridView.Columns["ID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("ФИО") && dataGridView.Columns["ФИО"] != null)
                {
                    dataGridView.Columns["ФИО"].FillWeight = 60;
                }

                if (dataGridView.Columns.Contains("Телефон") && dataGridView.Columns["Телефон"] != null)
                {
                    dataGridView.Columns["Телефон"].FillWeight = 40;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки DataGridView клиентов: {ex.Message}");
            }
        }

        /// <summary>
        /// Настройка DataGridView для отображения статусов
        /// </summary>
        public static void ConfigureStatusesDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ReadOnly = true;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = false;

                if (dataGridView.Columns.Contains("ID"))
                {
                    dataGridView.Columns["ID"].Visible = false;
                }

                if (dataGridView.Columns.Contains("Название статуса") && dataGridView.Columns["Название статуса"] != null)
                {
                    dataGridView.Columns["Название статуса"].FillWeight = 100;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка настройки DataGridView статусов: {ex.Message}");
            }
        }
    }
}