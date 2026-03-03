using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Класс для управления видимостью пароля в текстовом поле
    /// Позволяет переключать отображение символов пароля и скрытого режима
    /// </summary>
    public class PasswordVisibilityToggle
    {
        private PictureBox _toggleButton;
        private TextBox _passwordTextBox;
        private bool _isPasswordVisible = false;

        private Image _showPasswordIcon; // Иконка "показать пароль" (глаз открыт)
        private Image _hidePasswordIcon; // Иконка "скрыть пароль" (глаз закрыт)

        /// <summary>
        /// Конструктор с автоматической загрузкой иконок из ресурсов
        /// </summary>
        /// <param name="toggleButton">PictureBox для переключения видимости</param>
        /// <param name="passwordTextBox">Текстовое поле с паролем</param>
        public PasswordVisibilityToggle(PictureBox toggleButton, TextBox passwordTextBox)
        {
            _toggleButton = toggleButton;
            _passwordTextBox = passwordTextBox;

            Initialize();
        }

        /// <summary>
        /// Конструктор с пользовательскими иконками
        /// </summary>
        /// <param name="toggleButton">PictureBox для переключения видимости</param>
        /// <param name="passwordTextBox">Текстовое поле с паролем</param>
        /// <param name="showIcon">Иконка для режима "показать пароль"</param>
        /// <param name="hideIcon">Иконка для режима "скрыть пароль"</param>
        public PasswordVisibilityToggle(PictureBox toggleButton, TextBox passwordTextBox,
                                      Image showIcon, Image hideIcon)
        {
            _toggleButton = toggleButton;
            _passwordTextBox = passwordTextBox;
            _showPasswordIcon = showIcon;
            _hidePasswordIcon = hideIcon;

            Initialize();
        }

        /// <summary>
        /// Инициализация компонента: настройка внешнего вида и подписка на события
        /// </summary>
        private void Initialize()
        {
            _toggleButton.Cursor = Cursors.Hand;
            _toggleButton.SizeMode = PictureBoxSizeMode.CenterImage;

            UpdatePasswordVisibility();

            _toggleButton.Click += ToggleButton_Click;
        }

        /// <summary>
        /// Обработчик клика по кнопке - переключение видимости пароля
        /// </summary>
        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ToggleVisibility();
        }

        /// <summary>
        /// Переключение видимости пароля
        /// </summary>
        public void ToggleVisibility()
        {
            _isPasswordVisible = !_isPasswordVisible;
            UpdatePasswordVisibility();
        }

        /// <summary>
        /// Обновление состояния полей: режим отображения пароля и иконка
        /// </summary>
        private void UpdatePasswordVisibility()
        {
            _passwordTextBox.UseSystemPasswordChar = !_isPasswordVisible;

            _toggleButton.Image = _isPasswordVisible ? _hidePasswordIcon : _showPasswordIcon;

            _toggleButton.Parent?.Controls.OfType<ToolTip>().FirstOrDefault()?
                .SetToolTip(_toggleButton, _isPasswordVisible ? "Скрыть пароль" : "Показать пароль");
        }

        /// <summary>
        /// Установка пользовательских иконок для переключения
        /// </summary>
        /// <param name="showIcon">Иконка для режима "показать пароль"</param>
        /// <param name="hideIcon">Иконка для режима "скрыть пароль"</param>
        public void SetIcons(Image showIcon, Image hideIcon)
        {
            _showPasswordIcon = showIcon;
            _hidePasswordIcon = hideIcon;
            UpdatePasswordVisibility();
        }

        /// <summary>
        /// Освобождение ресурсов (отписка от событий)
        /// </summary>
        public void Dispose()
        {
            if (_toggleButton != null)
            {
                _toggleButton.Click -= ToggleButton_Click;
            }
        }
    }
}