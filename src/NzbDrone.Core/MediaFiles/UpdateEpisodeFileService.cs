using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IUpdateEpisodeFileService
    {
        void ChangeFileDateForFile(EpisodeFile episodeFile, Series series, List<Episode> episodes);
    }

    public class UpdateEpisodeFileService : IUpdateEpisodeFileService,
                                            IHandle<SeriesScannedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UpdateEpisodeFileService(IDiskProvider diskProvider,
                                        IConfigService configService,
                                        IEpisodeService episodeService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void ChangeFileDateForFile(EpisodeFile episodeFile, Series series, List<Episode> episodes)
        {
            ChangeFileDate(episodeFile, series, episodes);
        }

        private bool ChangeFileDate(EpisodeFile episodeFile, Series series, List<Episode> episodes)
        {
            var episodeFilePath = Path.Combine(series.Path, episodeFile.RelativePath);

            switch (_configService.FileDate)
            {
                case FileDateType.LocalAirDate:
                    {
                        var airDate = episodes.First().AirDate;
                        var airTime = series.AirTime;

                        if (airDate.IsNullOrWhiteSpace() || airTime.IsNullOrWhiteSpace())
                        {
                            return false;
                        }

                        return ChangeFileDateToLocalAirDate(episodeFilePath, airDate, airTime);
                    }

                case FileDateType.UtcAirDate:
                    {
                        var airDateUtc = episodes.First().AirDateUtc;

                        if (!airDateUtc.HasValue)
                        {
                            return false;
                        }

                        return ChangeFileDateToUtcAirDate(episodeFilePath, airDateUtc.Value);
                    }
            }

            return false;
        }

        private bool ChangeFileDateToLocalAirDate(string filePath, string fileDate, string fileTime)
        {
            DateTime airDate;

            if (DateTime.TryParse(fileDate + ' ' + fileTime, out airDate))
            {
                // avoiding false +ve checks and set date skewing by not using UTC (Windows)
                DateTime oldDateTime = _diskProvider.FileGetLastWrite(filePath);

                if (OsInfo.IsNotWindows && airDate < EpochTime)
                {
                    _logger.Debug("Setting date of file to 1970-01-01 as actual airdate is before that time and will not be set properly");
                    airDate = EpochTime;
                }

                if (!DateTime.Equals(airDate, oldDateTime))
                {
                    try
                    {
                        _diskProvider.FileSetLastWriteTime(filePath, airDate);
                        _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldDateTime, airDate);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                    }
                }
            }
            else
            {
                _logger.Debug("Could not create valid date to change file [{0}]", filePath);
            }

            return false;
        }

        private bool ChangeFileDateToUtcAirDate(string filePath, DateTime airDateUtc)
        {
            DateTime oldLastWrite = _diskProvider.FileGetLastWrite(filePath);

            if (OsInfo.IsNotWindows && airDateUtc < EpochTime)
            {
                _logger.Debug("Setting date of file to 1970-01-01 as actual airdate is before that time and will not be set properly");
                airDateUtc = EpochTime;
            }

            if (!DateTime.Equals(airDateUtc, oldLastWrite))
            {
                try
                {
                    _diskProvider.FileSetLastWriteTime(filePath, airDateUtc);
                    _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldLastWrite, airDateUtc);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to set date of file [" + filePath + "]");
                }
            }

            return false;
        }

        public void Handle(SeriesScannedEvent message)
        {
            if (_configService.FileDate == FileDateType.None)
            {
                return;
            }

            var episodes = _episodeService.EpisodesWithFiles(message.Series.Id);

            var episodeFiles = new List<EpisodeFile>();
            var updated = new List<EpisodeFile>();

            foreach (var group in episodes.GroupBy(e => e.EpisodeFileId))
            {
                var episodesInFile = group.Select(e => e).ToList();
                var episodeFile = episodesInFile.First().EpisodeFile;

                episodeFiles.Add(episodeFile);

                if (ChangeFileDate(episodeFile, message.Series, episodesInFile))
                {
                    updated.Add(episodeFile);
                }
            }

            if (updated.Any())
            {
                _logger.ProgressDebug("Changed file date for {0} files of {1} in {2}", updated.Count, episodeFiles.Count, message.Series.Title);
            }
            else
            {
                _logger.ProgressDebug("No file dates changed for {0}", message.Series.Title);
            }
        }
    }
}
