using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    public class PasswordVisibilityToggle
    {
        private PictureBox _toggleButton;
        private TextBox _passwordTextBox;
        private bool _isPasswordVisible = false;

        private Image _showPasswordIcon = null; // Иконка "показать пароль"
        private Image _hidePasswordIcon = null; // Иконка "скрыть пароль"
        public PasswordVisibilityToggle(PictureBox toggleButton, TextBox passwordTextBox)
        {
            _toggleButton = toggleButton;
            _passwordTextBox = passwordTextBox;

            Initialize();
        }

        public PasswordVisibilityToggle(PictureBox toggleButton, TextBox passwordTextBox,
                                      Image showIcon, Image hideIcon)
        {
            _toggleButton = toggleButton;
            _passwordTextBox = passwordTextBox;
            _showPasswordIcon = showIcon;
            _hidePasswordIcon = hideIcon;

            Initialize();
        }

        private void Initialize()
        {
            // Настройка внешнего вида
            _toggleButton.Cursor = Cursors.Hand;
            _toggleButton.SizeMode = PictureBoxSizeMode.CenterImage;

            // Установка начального состояния
            UpdatePasswordVisibility();

            // Подписка на событие клика
            _toggleButton.Click += ToggleButton_Click;
        }

        

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ToggleVisibility();
        }

        public void ToggleVisibility()
        {
            _isPasswordVisible = !_isPasswordVisible;
            UpdatePasswordVisibility();
        }
        private void UpdatePasswordVisibility()
        {
            // Изменяем свойство UseSystemPasswordChar
            _passwordTextBox.UseSystemPasswordChar = !_isPasswordVisible;

            // Обновляем иконку
            _toggleButton.Image = _isPasswordVisible ? _hidePasswordIcon : _showPasswordIcon;

            // ToolTip для подсказки
            _toggleButton.Parent?.Controls.OfType<ToolTip>().FirstOrDefault()?
                .SetToolTip(_toggleButton, _isPasswordVisible ? "Скрыть пароль" : "Показать пароль");
        }

        // Метод для установки кастомных иконок
        public void SetIcons(Image showIcon, Image hideIcon)
        {
            _showPasswordIcon = showIcon;
            _hidePasswordIcon = hideIcon;
            UpdatePasswordVisibility();
        }

        // Очистка ресурсов
        public void Dispose()
        {
            if (_toggleButton != null)
            {
                _toggleButton.Click -= ToggleButton_Click;
            }
        }
    }
}
