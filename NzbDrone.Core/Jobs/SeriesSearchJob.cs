using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class SeriesSearchJob : IJob
    {
        private readonly SeasonSearchJob _seasonSearchJob;
        private readonly SeasonProvider _seasonProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeriesSearchJob(SeasonSearchJob seasonSearchJob,
                                SeasonProvider seasonProvider)
        {
            _seasonSearchJob = seasonSearchJob;
            _seasonProvider = seasonProvider;
        }

        public string Name
        {
            get { return "Series Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            Logger.Debug("Getting seasons from database for series: {0}", targetId);
            var seasons = _seasonProvider.GetSeasons(targetId).Where(s => s > 0);

            foreach (var season in seasons)
            {
                //Skip ignored seasons
                if (_seasonProvider.IsIgnored(targetId, season))
                    continue;

                _seasonSearchJob.Start(notification, targetId, season);
            }
        }
    }
}