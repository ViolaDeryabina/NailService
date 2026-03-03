using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Статический класс для настройки внешнего вида и поведения DataGridView
    /// Содержит методы конфигурации для различных типов таблиц в приложении
    /// </summary>
    public static class DataGridViewConfigurator
    {
        /// <summary>
        /// Настройка DataGridView для отображения списка услуг с изображениями
        /// </summary>
        /// <param name="dataGridViewServices">DataGridView для настройки</param>
        /// <param name="thumbnailWidth">Ширина колонки с миниатюрами (по умолчанию 80)</param>
        public static void ConfigureServicesDataGridView(DataGridView dataGridViewServices, int thumbnailWidth = 80)
        {
            // Скрытие служебных колонок
            if (dataGridViewServices.Columns.Contains("ID"))
                dataGridViewServices.Columns["ID"].Visible = false;

            if (dataGridViewServices.Columns.Contains("CategoryID"))
                dataGridViewServices.Columns["CategoryID"].Visible = false;

            if (dataGridViewServices.Columns.Contains("Имя файла"))
                dataGridViewServices.Columns["Имя файла"].Visible = false;

            // Настройка колонки с изображением услуги
            if (dataGridViewServices.Columns.Contains("Миниатюра"))
            {
                dataGridViewServices.Columns["Миниатюра"].Width = thumbnailWidth;
                dataGridViewServices.Columns["Миниатюра"].HeaderText = "Фото";
                dataGridViewServices.Columns["Миниатюра"].Resizable = DataGridViewTriState.False;

                if (dataGridViewServices.Columns["Миниатюра"] is DataGridViewImageColumn imageColumn)
                {
                    imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                }
            }

            // Настройка колонки с описанием (многострочный текст)
            if (dataGridViewServices.Columns.Contains("Описание"))
            {
                dataGridViewServices.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewServices.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            }

            // Настройка колонки с ценой (формат валюты и выравнивание)
            if (dataGridViewServices.Columns.Contains("Цена"))
            {
                dataGridViewServices.Columns["Цена"].DefaultCellStyle.Format = "C2";
                dataGridViewServices.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}