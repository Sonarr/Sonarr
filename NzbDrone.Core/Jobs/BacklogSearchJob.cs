using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class BacklogSearchJob : IJob
    {
        private readonly EpisodeProvider _episodeProvider;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly SeasonSearchJob _seasonSearchJob;
        private readonly ConfigProvider _configProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BacklogSearchJob(EpisodeProvider episodeProvider, EpisodeSearchJob episodeSearchJob,
                                    SeasonSearchJob seasonSearchJob, ConfigProvider configProvider)
        {
            _episodeProvider = episodeProvider;
            _episodeSearchJob = episodeSearchJob;
            _seasonSearchJob = seasonSearchJob;
            _configProvider = configProvider;
        }

        public string Name
        {
            get { return "Backlog Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(30); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            var missingEpisodes = GetMissingForEnabledSeries().GroupBy(e => new { e.SeriesId, e.SeasonNumber });
          
            var individualEpisodes = new List<Episode>();

            Logger.Trace("Processing missing episodes list");
            foreach (var group in missingEpisodes)
            {
                var count = group.Count();

                if (count == 1)
                    individualEpisodes.Add(group.First());

                else
                {
                    //Get count and compare to the actual number of episodes for this season
                    //If numbers don't match then add to individual episodes, else process as full season...
                    var seriesId = group.Key.SeriesId;
                    var seasonNumber = group.Key.SeasonNumber;

                    var countInDb = _episodeProvider.GetEpisodeNumbersBySeason(seriesId, seasonNumber).Count;

                    //Todo: Download a full season if more than n% is missing?

                    if (count != countInDb)
                    {
                        //Add the episodes to be processed manually
                        individualEpisodes.AddRange(group);
                    }

                    else
                    {
                        //Process as a full season
                        Logger.Debug("Processing Full Season: {0} Season {1}", seriesId, seasonNumber);
                        _seasonSearchJob.Start(notification, seriesId, seasonNumber);
                    }
                }
            }

            Logger.Debug("Processing standalone episodes");
            //Process the list of remaining episodes, 1 by 1
            foreach (var episode in individualEpisodes)
            {
                _episodeSearchJob.Start(notification, episode.EpisodeId, 0);
            }
        }

        public List<Episode> GetMissingForEnabledSeries()
        {
            if (!_configProvider.EnableBacklogSearching)
            {
                Logger.Trace("Backlog searching is not enabled, only running for explicitly enabled series.");
                return _episodeProvider.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.Series.BacklogSetting == BacklogSettingType.Enable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }

            else
            {
                Logger.Trace("Backlog searching is enabled, skipping explicity disabled series.");
                return _episodeProvider.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.Series.BacklogSetting != BacklogSettingType.Disable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }
        }
    }
}