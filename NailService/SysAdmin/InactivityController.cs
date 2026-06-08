using System;
using System.Configuration;
using System.Windows.Forms;

namespace NailService
{
    public class InactivityController : IMessageFilter
    {
        private Timer _timer;
        private Action _onLock;
        private int _elapsedSeconds = 0;
        private int _timeoutSeconds; // Теперь не константа

        public InactivityController(Action onLockAction, int timeoutSeconds = 30)
        {
            _onLock = onLockAction;
            _timeoutSeconds = timeoutSeconds;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedSeconds++;

            if (_elapsedSeconds >= _timeoutSeconds)
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

        // Добавьте метод для обновления таймаута
        public void UpdateTimeout(int newTimeoutSeconds)
        {
            _timeoutSeconds = newTimeoutSeconds;
            ResetTimer();
        }

        public bool PreFilterMessage(ref Message m)
        {
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
