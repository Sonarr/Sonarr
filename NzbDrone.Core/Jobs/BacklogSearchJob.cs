using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Jobs.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs
{
    public class BacklogSearchJob : IJob
    {
        private readonly IEpisodeService _episodeService;
        private readonly EpisodeSearchJob _episodeSearchJob;
        private readonly SeasonSearchJob _seasonSearchJob;
        private readonly IConfigService _configService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BacklogSearchJob(IEpisodeService episodeService, EpisodeSearchJob episodeSearchJob,
                                    SeasonSearchJob seasonSearchJob, IConfigService configService)
        {
            _episodeService = episodeService;
            _episodeSearchJob = episodeSearchJob;
            _seasonSearchJob = seasonSearchJob;
            _configService = configService;
        }

        public string Name
        {
            get { return "Backlog Search"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(30); }
        }

        public void Start(ProgressNotification notification, dynamic options)
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

                    var countInDb = _episodeService.GetEpisodeNumbersBySeason(seriesId, seasonNumber).Count;

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
                        _seasonSearchJob.Start(notification, new { SeriesId = seriesId, SeasonNumber = seasonNumber });
                    }
                }
            }

            Logger.Debug("Processing standalone episodes");
            //Process the list of remaining episodes, 1 by 1
            foreach (var episode in individualEpisodes)
            {
                _episodeSearchJob.Start(notification, new { EpisodeId = episode.Id});
            }
        }

        public List<Episode> GetMissingForEnabledSeries()
        {
            if (!_configService.EnableBacklogSearching)
            {
                Logger.Trace("Backlog searching is not enabled, only running for explicitly enabled series.");
                return _episodeService.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.Series.BacklogSetting == BacklogSettingType.Enable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }

            else
            {
                Logger.Trace("Backlog searching is enabled, skipping explicity disabled series.");
                return _episodeService.EpisodesWithoutFiles(true).Where(e =>
                                                                                e.Series.BacklogSetting != BacklogSettingType.Disable &&
                                                                                e.Series.Monitored
                                                                            ).ToList();
            }
        }
    }
}