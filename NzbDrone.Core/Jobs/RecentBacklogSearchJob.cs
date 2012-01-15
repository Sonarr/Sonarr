using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Jobs
{
    public class RecentBacklogSearchJob : IJob
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly ConfigProvider _configProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RecentBacklogSearchJob(EpisodeProvider episodeProvider, EpisodeSearchJob episodeSearchJob,
                                            ConfigProvider configProvider)
        {
            _episodeProvider = episodeProvider;
            _episodeSearchJob = episodeSearchJob;
            _configProvider = configProvider;
        }

        public string Name
        {
            get { return "Recent Backlog Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(1); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            if (!_configProvider.EnableBacklogSearching)
            {
                Logger.Trace("Backlog searching is not enabled, aborting job.");
                return;
            }

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