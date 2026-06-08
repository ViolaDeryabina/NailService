using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Статический класс для управления стилями и цветовым оформлением во всем приложении
    /// Содержит единые цветовые схемы для статусов и методы настройки DataGridView
    /// </summary>
    public static class StyleManager
    {
        /// <summary>
        /// Возвращает цвет фона для ячейки в зависимости от статуса записи
        /// </summary>
        /// <param name="statusID">ID статуса (1-Запланирован, 2-Подтвержден, 3-Выполнен, 4-Отменен)</param>
        /// <returns>Цвет для соответствующего статуса</returns>
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

        /// <summary>
        /// Применяет единый стиль оформления к DataGridView во всем приложении
        /// Настраивает шрифты, цвета, выделение строк, чередование фона
        /// </summary>
        /// <param name="grid">DataGridView для применения стилей</param>
        public static void ApplyGridStyles(DataGridView grid)
        {
            grid.DefaultCellStyle.Font = new Font("MS Reference Sans Serif", 10);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("MS Reference Sans Serif", 11, FontStyle.Bold);

            grid.EnableHeadersVisualStyles = false;

            // Цвет заголовков
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            // Настройки выделения строк
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;

            // Цвет выделения всей строки
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Отключение ненужных элементов
            grid.RowHeadersVisible = false;

            // Основные настройки таблицы
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;

            // Чередование цветов строк для лучшей читаемости
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            // Настройка границ
            grid.GridColor = Color.FromArgb(230, 230, 230);
            grid.BorderStyle = BorderStyle.None;

            // Отключение выделения заголовков столбцов
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = grid.ColumnHeadersDefaultCellStyle.ForeColor;
        }

        /// <summary>
        /// Настройка выравнивания содержимого в特定ных колонках
        /// </summary>
        /// <param name="grid">DataGridView для настройки</param>
        public static void ApplyColumnAlignments(DataGridView grid)
        {
            // Цена - выравнивание по правому краю
            if (grid.Columns.Contains("Price"))
            {
                grid.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns["Price"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Дата - выравнивание по центру
            if (grid.Columns.Contains("Date"))
            {
                grid.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grid.Columns["Date"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Статус - выравнивание по центру
            if (grid.Columns.Contains("Status"))
            {
                grid.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                grid.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
    }
}