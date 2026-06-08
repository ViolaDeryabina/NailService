using System;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Управляет изменением размера формы
    /// </summary>
    public class FormSizeManager
    {
        private readonly Form _form;
        private readonly Size _smallSize;
        private readonly Size _largeSize;

        public FormSizeManager(Form form, Size smallSize, Size largeSize)
        {
            _form = form;
            _smallSize = smallSize;
            _largeSize = largeSize;
        }

        /// <summary>
        /// Устанавливает маленький размер формы
        /// </summary>
        public void SetSmallSize()
        {
            _form.Size = _smallSize;
            CenterForm();
        }

        /// <summary>
        /// Устанавливает большой размер формы
        /// </summary>
        public void SetLargeSize()
        {
            _form.Size = _largeSize;
            CenterForm();
        }

        private void CenterForm()
        {
            // Получаем рабочий экран (где находится форма)
            Screen currentScreen = Screen.FromControl(_form);

            // Вычисляем позицию для центрирования
            int x = currentScreen.WorkingArea.Left + (currentScreen.WorkingArea.Width - _form.Width) / 2;
            int y = currentScreen.WorkingArea.Top + (currentScreen.WorkingArea.Height - _form.Height) / 2;

            // Устанавливаем позицию
            _form.Location = new Point(x, y);
        }

        /// <summary>
        /// Центрирует форму относительно родительской формы (если есть)
        /// </summary>
        public void CenterRelativeToParent(Form parentForm)
        {
            if (parentForm != null)
            {
                int x = parentForm.Location.X + (parentForm.Width - _form.Width) / 2;
                int y = parentForm.Location.Y + (parentForm.Height - _form.Height) / 2;
                _form.Location = new Point(x, y);
            }
            else
            {
                CenterForm();
            }
        }

        /// <summary>
        /// Сбрасывает размер до маленького
        /// </summary>
        public void Reset()
        {
            SetSmallSize();
        }

        /// <summary>
        /// Устанавливает размер без центрирования
        /// </summary>
        public void SetSizeWithoutCentering(Size newSize)
        {
            _form.Size = newSize;
        }
    }
}