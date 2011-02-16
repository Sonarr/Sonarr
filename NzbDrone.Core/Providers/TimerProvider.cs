using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers
{
    public class TimerProvider : ITimerProvider
    {
        private IRssSyncProvider _rssSyncProvider;
        private ISeriesProvider _seriesProvider;
        private ISeasonProvider _seasonProvider;
        private IEpisodeProvider _episodeProvider;

        private Timer _rssSyncTimer;
        private Timer _minuteTimer;
        private DateTime _rssSyncNextInterval;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TimerProvider(IRssSyncProvider rssSyncProvider, ISeriesProvider seriesProvider, ISeasonProvider seasonProvider, IEpisodeProvider episodeProvider)
        {
            _rssSyncProvider = rssSyncProvider;
            _seriesProvider = seriesProvider;
            _seasonProvider = seasonProvider;
            _episodeProvider = episodeProvider;

            _rssSyncTimer = new Timer();
            _minuteTimer = new Timer(60000);
        }

        #region ITimerProvider Members

        public void ResetRssSyncTimer()
        {
            double interval = _rssSyncTimer.Interval;
            _rssSyncTimer.Interval = interval;
        }

        public void StartRssSyncTimer()
        {
            if (_rssSyncTimer.Interval < 900000) //If Timer is less than 15 minutes, throw an error! This should also be handled when saving the config, though a user could by-pass it by editing the DB directly... TNO (Trust No One)
            {
                Logger.Error("RSS Sync Frequency is invalid, please set the interval first");
                throw new InvalidOperationException("RSS Sync Frequency Invalid");
            }

            _rssSyncTimer.Elapsed += new ElapsedEventHandler(RunRssSync);
            _rssSyncTimer.Start();
            _rssSyncNextInterval = DateTime.Now.AddMilliseconds(_rssSyncTimer.Interval);
        }

        public void StopRssSyncTimer()
        {
            _rssSyncTimer.Stop();
        }

        public void SetRssSyncTimer(int minutes)
        {
            long ms = minutes * 60 * 1000;
            _rssSyncTimer.Interval = ms;
        }

        public TimeSpan RssSyncTimeLeft()
        {
            return _rssSyncNextInterval.Subtract(DateTime.Now);
        }

        public DateTime NextRssSyncTime()
        {
            return _rssSyncNextInterval;
        }

        public void StartMinuteTimer()
        {
            _minuteTimer.Elapsed += new ElapsedEventHandler(MinuteTimer_Elapsed);
            _minuteTimer.Start();
        }

        public void StopMinuteTimer()
        {
            _minuteTimer.Stop();
        }

        #endregion

        private void RunRssSync(object obj, ElapsedEventArgs args)
        {
            _rssSyncNextInterval = DateTime.Now.AddMilliseconds(_rssSyncTimer.Interval);
            _rssSyncProvider.Begin();
        }

        private void MinuteTimer_Elapsed(object obj, ElapsedEventArgs args)
        {
            //Check to see if anything should be run at this time, if so run it

            var now = DateTime.Now;

            //Daily (Except Sunday) 03:00 - Update the lastest season for all TV Shows
            if (now.Hour == 3 && now.Minute == 0 && now.DayOfWeek != DayOfWeek.Sunday)
            {
                foreach (var series in _seriesProvider.GetAllSeries())
                {
                    var season = _seasonProvider.GetLatestSeason(series.SeriesId);
                    _episodeProvider.RefreshEpisodeInfo(season);
                }
            }

            //Sunday 03:00 - Update all TV Shows
            if (now.Hour == 3 && now.Minute == 0 && now.DayOfWeek == DayOfWeek.Sunday)
            {
                foreach (var series in _seriesProvider.GetAllSeries())
                {
                    _episodeProvider.RefreshEpisodeInfo(series.SeriesId);
                }
            }

            throw new NotImplementedException();
        }
    }
}
