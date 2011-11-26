using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class SeriesSearchJob : IJob
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly SeasonSearchJob _seasonSearchJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeriesSearchJob(EpisodeProvider episodeProvider, SeasonSearchJob seasonSearchJob)
        {
            _episodeProvider = episodeProvider;
            _seasonSearchJob = seasonSearchJob;
        }

        public string Name
        {
            get { return "Series Search"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (targetId <= 0)
                throw new ArgumentOutOfRangeException("targetId");

            Logger.Debug("Getting seasons from database for series: {0}", targetId);
            var seasons = _episodeProvider.GetSeasons(targetId).Where(s => s > 0);

            foreach (var season in seasons)
            {
                //Skip ignored seasons
                if (_episodeProvider.IsIgnored(targetId, season))
                    continue;

                _seasonSearchJob.Start(notification, targetId, season);
            }
        }
    }
}