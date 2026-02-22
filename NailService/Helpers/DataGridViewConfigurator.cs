
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    public static class DataGridViewConfigurator
    {
        public static void ConfigureServicesDataGridView(DataGridView dataGridViewServices, int thumbnailWidth = 80)
        {
            if (dataGridViewServices.Columns.Contains("ID"))
                dataGridViewServices.Columns["ID"].Visible = false;

            if (dataGridViewServices.Columns.Contains("CategoryID"))
                dataGridViewServices.Columns["CategoryID"].Visible = false;

            if (dataGridViewServices.Columns.Contains("Имя файла"))
                dataGridViewServices.Columns["Имя файла"].Visible = false;

            // Настраиваем колонку с изображением
            if (dataGridViewServices.Columns.Contains("Миниатюра"))
            {
                dataGridViewServices.Columns["Миниатюра"].Width = thumbnailWidth;
                dataGridViewServices.Columns["Миниатюра"].HeaderText = "Фото";
                dataGridViewServices.Columns["Миниатюра"].Resizable = DataGridViewTriState.False;

                // Устанавливаем режим отображения изображения
                if (dataGridViewServices.Columns["Миниатюра"] is DataGridViewImageColumn imageColumn)
                {
                    imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                }
            }

            // Настраиваем колонку с описанием
            if (dataGridViewServices.Columns.Contains("Описание"))
            {
                dataGridViewServices.Columns["Описание"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dataGridViewServices.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            }

            // Настраиваем колонку с ценой
            if (dataGridViewServices.Columns.Contains("Цена"))
            {
                dataGridViewServices.Columns["Цена"].DefaultCellStyle.Format = "C2"; // Формат валюты
                dataGridViewServices.Columns["Цена"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}