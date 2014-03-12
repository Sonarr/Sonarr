using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpdateEpisodeFileService
    {
        void ChangeFileDateToAirdate(EpisodeFile episodeFile, Series series);
    }

    public class UpdateEpisodeFileService : IUpdateEpisodeFileService,
                                            IExecute<AirDateSeriesCommand>,
                                            IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public UpdateEpisodeFileService(IDiskProvider diskProvider,
                                        IConfigService configService,
                                        ISeriesService seriesService,
                                        IEpisodeService episodeService,
                                        IEventAggregator eventAggregator,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _seriesService = seriesService;
            _episodeService = episodeService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void ChangeFileDateToAirdate(EpisodeFile episodeFile, Series series)
        {
            var episode = new Episode();
            episode.AirDate = episodeFile.Episodes.Value.First().AirDate;
            episode.EpisodeFile = episodeFile;
            episode.EpisodeFileId = 1;

            var episodes = new List<Episode>();
            episodes.Add(episode);

            ChangeFileDateToAirdate(episodes, series);
        }

        private void ChangeFileDateToAirdate(List<Episode> episodes, Series series)
        {
            if (!episodes.Any())
            {
                _logger.ProgressDebug("{0} has no media files available to update with air dates", series.Title);
            }

            else
            {
                var done = new List<Episode>();

                _logger.ProgressDebug("{0} ... checking {1} media file dates match air date", series.Title, episodes.Count);

                foreach (var episode in episodes)
                {
                    if (episode.HasFile
                        && episode.EpisodeFile.IsLoaded
                        && ChangeFileDate(episode.EpisodeFile.Value.Path, episode.AirDate, series.AirTime))
                    {
                        done.Add(episode);
                    }
                }

                if (done.Any())
                {
                    _eventAggregator.PublishEvent(new SeriesAirDatedEvent(series));
                    _logger.ProgressDebug("{0} had {1} of {2} media file dates changed to the date and time the episode aired", series.Title, done.Count, episodes.Count);
                }

                else
                {
                    _logger.ProgressDebug("{0} has all its media file dates matching the date each aired", series.Title);
                }
            }
        }

        public void Execute(AirDateSeriesCommand message)
        {
            var seriesToAirDate = _seriesService.GetSeries(message.SeriesIds);

            foreach (var series in seriesToAirDate)
            {
                var episodes = _episodeService.EpisodesWithFiles(series.Id);

                ChangeFileDateToAirdate(episodes, series);
            }
        }

        public void Handle(SeriesScannedEvent message)
        {
             if (_configService.FileDateAiredDate)
             {
                 var episodes = _episodeService.EpisodesWithFiles(message.Series.Id);

                 ChangeFileDateToAirdate(episodes, message.Series);
             }
        }

        private bool ChangeFileDate(String filePath, String fileDate, String fileTime)
        {
            DateTime dateTime, oldDateTime;
            bool result = false;

            if (DateTime.TryParse(fileDate + ' ' + fileTime, out dateTime))
            {
                // avoiding false +ve checks and set date skewing by not using UTC (Windows)
                oldDateTime = _diskProvider.GetLastFileWrite(filePath);

                if (!DateTime.Equals(dateTime, oldDateTime))
                {
                    try
                    {
                        _diskProvider.FileSetLastWriteTime(filePath, dateTime);
                        _diskProvider.FileSetLastAccessTime(filePath, dateTime);
                        _logger.Info("Date of file [{0}] changed from \"{1}\" to \"{2}\"", filePath, oldDateTime, dateTime);
                        result = true;
                    }

                    catch (Exception ex)
                    {
                        _logger.WarnException("Unable to set date of file [" + filePath + "]", ex);
                    }
                }
            }

            else
            {
                _logger.Warn("Could not create valid date to set [{0}]", filePath);
            }

            return result;
        }
    }
}
