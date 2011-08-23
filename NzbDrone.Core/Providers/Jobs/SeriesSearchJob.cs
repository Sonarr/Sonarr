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
        private readonly EpisodeSearchJob _episodeSearchJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeriesSearchJob(EpisodeProvider episodeProvider, EpisodeSearchJob episodeSearchJob)
        {
            _episodeProvider = episodeProvider;
            _episodeSearchJob = episodeSearchJob;
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

            Logger.Debug("Getting episodes from database for series: {0}.", targetId);
            var episodes = _episodeProvider.GetEpisodeBySeries(targetId);

            if (episodes == null)
            {
                Logger.Warn("No episodes in database found for series: {0}.", targetId);
                return;
            }

            //Todo: Search for a full season NZB before individual episodes

            foreach (var episode in episodes.Where(e => !e.Ignored))
            {
                _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            }
        }
    }
}