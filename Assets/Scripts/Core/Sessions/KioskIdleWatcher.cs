using System;

namespace Eco.Core.Sessions
{
    public class KioskIdleWatcher
    {
        private readonly double _timeoutSeconds;
        private double _elapsedSeconds;

        public event Action OnIdleTimeout;

        public KioskIdleWatcher(double timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public void Tick(double deltaSeconds)
        {
            _elapsedSeconds += deltaSeconds;
            if (_elapsedSeconds < _timeoutSeconds) return;

            _elapsedSeconds = 0;
            OnIdleTimeout?.Invoke();
        }

        public void Reset() => _elapsedSeconds = 0;
    }
}
