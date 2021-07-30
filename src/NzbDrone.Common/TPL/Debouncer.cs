using System;

namespace NzbDrone.Common.TPL
{
    public class Debouncer
    {
        private readonly Action _action;
        private readonly System.Timers.Timer _timer;

        private volatile int _paused;
        private volatile bool _triggered;

        public Debouncer(Action action, TimeSpan debounceDuration)
        {
            _action = action;
            _timer = new System.Timers.Timer(debounceDuration.TotalMilliseconds);
            _timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_paused == 0)
            {
                _triggered = false;
                _timer.Stop();
                _action();
            }
        }

        public void Execute()
        {
            lock (_timer)
            {
                _triggered = true;
                if (_paused == 0)
                {
                    _timer.Start();
                }
            }
        }

        public void Pause()
        {
            lock (_timer)
            {
                _paused++;
                _timer.Stop();
            }
        }

        public void Resume()
        {
            lock (_timer)
            {
                _paused--;
                if (_paused == 0 && _triggered)
                {
                    _timer.Start();
                }
            }
        }
    }
}
