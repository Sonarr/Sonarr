using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RecentBacklogSearchJob : IJob
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly EpisodeSearchJob _episodeSearchJob;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RecentBacklogSearchJob(EpisodeProvider episodeProvider, EpisodeSearchJob episodeSearchJob)
        {
            _episodeProvider = episodeProvider;
            _episodeSearchJob = episodeSearchJob;
        }

        public string Name
        {
            get { return "Recent Backlog Search"; }
        }

        public int DefaultInterval
        {
            get { return 1440; }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            //Get episodes that are considered missing and aired in the last 30 days
            var missingEpisodes = _episodeProvider.EpisodesWithoutFiles(true).Where(e => e.AirDate >= DateTime.Today.AddDays(-30));

            Logger.Debug("Processing missing episodes from the last 30 days");
            //Process the list of remaining episodes, 1 by 1
            foreach (var episode in missingEpisodes)
            {
                _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            }
        }
    }
}