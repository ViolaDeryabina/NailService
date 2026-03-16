using System;
using System.Configuration;
using System.Windows.Forms;

namespace NailService
{
    public class InactivityController : IMessageFilter
    {
        // Объявляем константу (в секундах)
        private const int InactivityTimeoutSeconds = 30;

        private Timer _timer;
        private Action _onLock;
        private int _elapsedSeconds = 0;

        public InactivityController(Action onLockAction)
        {
            _onLock = onLockAction;

            _timer = new Timer();
            _timer.Interval = 1000; // Тик каждую секунду
            _timer.Tick += Timer_Tick;

            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;

            // Используем константу для проверки
            if (_elapsedSeconds >= InactivityTimeoutSeconds)
            {
                _timer.Stop();
                _onLock?.Invoke();
            }
        }

        public void ResetTimer()
        {
            _elapsedSeconds = 0;
        }

        public void Restart()
        {
            ResetTimer();
            _timer.Start();
        }

        public bool PreFilterMessage(ref Message m)
        {
            // Коды событий мыши и клавиатуры
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;

            if (m.Msg == WM_MOUSEMOVE || m.Msg == WM_LBUTTONDOWN ||
                m.Msg == WM_RBUTTONDOWN || m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
            {
                ResetTimer();
            }
            return false;
        }
    }

}