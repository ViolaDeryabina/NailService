using System;
using System.Drawing;
using System.Windows.Forms;

namespace NailService
{
    /// <summary>
    /// Управляет отображением и проверкой CAPTCHA
    /// </summary>
    public class CaptchaManager
    {
        private readonly PictureBox _pictureBox;
        private readonly TextBox _textBox;
        private readonly Button _refreshButton;
        private readonly Label _label;
        private readonly GroupBox _groupBox;

        private readonly CaptchaGenerator _generator;
        private string _currentCode;

        private Timer _blockTimer;
        private bool _isBlocked;
        private int _remainingSeconds;
        private int _failedAttempts;

        public event Action OnBlockStarted;
        public event Action OnBlockEnded;

        public int FailedAttempts => _failedAttempts;
        public bool IsBlocked => _isBlocked;
        public int RemainingSeconds => _remainingSeconds;

        public CaptchaManager(PictureBox pictureBox, TextBox textBox, Button refreshButton,
                              Label label, GroupBox groupBox)
        {
            _pictureBox = pictureBox;
            _textBox = textBox;
            _refreshButton = refreshButton;
            _label = label;
            _groupBox = groupBox;

            _generator = new CaptchaGenerator();

            InitializeBlockTimer();
        }

        private void InitializeBlockTimer()
        {
            _blockTimer = new Timer();
            _blockTimer.Interval = 1000;
            _blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds > 0)
            {
                _label.Text = $"Доступ заблокирован!\nОсталось: {_remainingSeconds} сек.";
            }
            else
            {
                _blockTimer.Stop();
                Unblock();
            }
        }

        /// <summary>
        /// Показывает CAPTCHA
        /// </summary>
        public void Show()
        {
            _groupBox.Visible = true;
            _groupBox.Enabled = true;

            _label.Visible = true;
            _pictureBox.Visible = true;
            _textBox.Visible = true;
            _refreshButton.Visible = true;
            _label.Text = "Введите код с картинки:";

            Refresh();
        }

        /// <summary>
        /// Скрывает CAPTCHA
        /// </summary>
        public void Hide()
        {
            _groupBox.Visible = false;
            _groupBox.Enabled = false;

            _label.Visible = false;
            _pictureBox.Visible = false;
            _textBox.Visible = false;
            _refreshButton.Visible = false;

            _textBox.Clear();
            _label.Text = "";
        }

        /// <summary>
        /// Обновляет CAPTCHA изображение
        /// </summary>
        public void Refresh()
        {
            if (!_isBlocked)
            {
                _currentCode = _generator.GenerateCode();
                _pictureBox.Image = _generator.CreateImage(_currentCode);
                _textBox.Clear();
            }
        }

        /// <summary>
        /// Проверяет введенный код CAPTCHA
        /// </summary>
        public bool Validate(string inputCode)
        {
            if (string.IsNullOrWhiteSpace(inputCode))
                return false;

            return inputCode == _currentCode;
        }

        /// <summary>
        /// Увеличивает счетчик неудачных попыток
        /// </summary>
        public void IncrementFailedAttempts()
        {
            _failedAttempts++;
        }

        /// <summary>
        /// Сбрасывает счетчик неудачных попыток
        /// </summary>
        public void ResetFailedAttempts()
        {
            _failedAttempts = 0;
        }

        /// <summary>
        /// Блокирует CAPTCHA на указанное количество секунд
        /// </summary>
        public void Block(int seconds = 10)
        {
            _isBlocked = true;
            _remainingSeconds = seconds;

            _label.Visible = true;
            _label.Text = $"Доступ заблокирован!\nОсталось: {_remainingSeconds} сек.";
            _label.ForeColor = Color.Red;

            if (!_groupBox.Visible)
            {
                _groupBox.Visible = true;
            }

            _pictureBox.Visible = true;
            _textBox.Visible = true;
            _refreshButton.Visible = true;

            EnableControls(false);
            _blockTimer.Start();

            OnBlockStarted?.Invoke();
        }

        private void Unblock()
        {
            _isBlocked = false;

            _label.Text = "Введите код с картинки:";
            _label.ForeColor = SystemColors.ControlText;

            EnableControls(true);
            Refresh();

            OnBlockEnded?.Invoke();
        }

        private void EnableControls(bool enabled)
        {
            _refreshButton.Enabled = enabled;
            _textBox.Enabled = enabled;
        }

        /// <summary>
        /// Очищает поле ввода CAPTCHA
        /// </summary>
        public void ClearInput()
        {
            _textBox.Clear();
        }
    }
}