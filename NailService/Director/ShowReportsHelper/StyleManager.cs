using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    public static class StyleManager
    {
        public static Color GetStatusColor(int statusID)
        {
            switch (statusID)
            {
                case 1: return Color.FromArgb(255, 245, 157);    // Запланирован - янтарный
                case 2: return Color.FromArgb(197, 225, 165);    // Подтвержден - салатовый
                case 3: return Color.FromArgb(225, 225, 225);    // Выполнен - светлый серый
                case 4: return Color.FromArgb(255, 171, 145);    // Отменен - коралловый
                default: return Color.White;
            }
        }

        public static void ApplyGridStyles(DataGridView grid)
        {
            grid.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);


            grid.EnableHeadersVisualStyles = false;

            // Цвет заголовков - мягкий серо-голубой
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
    
            // НАСТРОЙКИ ВЫДЕЛЕНИЯ СТРОК
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false; // Запрещаем множественное выделение
    
            // Цвет выделения ВСЕЙ строки
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
    
            // Отключаем выделение ячеек по отдельности
            grid.RowHeadersVisible = false;
    
            // Настройки таблицы
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;
    
            // Чередование цветов строк для лучшей читаемости
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
    
            // Границы
            grid.GridColor = Color.FromArgb(230, 230, 230);
            grid.BorderStyle = BorderStyle.None;
    
            // ДОПОЛНИТЕЛЬНО: Отключаем выделение заголовков столбцов
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = grid.ColumnHeadersDefaultCellStyle.ForeColor;
        }

        // Новый метод для настройки выравнивания колонок
        public static void ApplyColumnAlignments(DataGridView grid)
        {
            // Выравнивание цены по правому краю
            if (grid.Columns.Contains("Price"))
            {
                grid.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns["Price"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Выравнивание даты по центру (опционально)
            if (grid.Columns.Contains("Date"))
            {
                grid.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grid.Columns["Date"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Выравнивание статуса по центру (опционально)
            if (grid.Columns.Contains("Status"))
            {
                grid.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grid.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

       
        
    }
}