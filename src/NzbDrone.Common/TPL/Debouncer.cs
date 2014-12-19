using System;

namespace NzbDrone.Common.TPL
{
    public class Debouncer
    {
        private readonly Action _action;
        private readonly System.Timers.Timer _timer;

        public Debouncer(Action action, TimeSpan debounceDuration)
        {
            _action = action;
            _timer = new System.Timers.Timer(debounceDuration.TotalMilliseconds);
            _timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            _action();
        }

        public void Execute()
        {
            _timer.Start();
        }
    }
}