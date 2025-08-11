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
            var airDateUtc = episodes.First().AirDateUtc;

            if (!airDateUtc.HasValue)
            {
                return false;
            }

            return _configService.FileDate switch
            {
                FileDateType.LocalAirDate =>
                    ChangeFileDateToLocalDate(episodeFilePath, airDateUtc.Value.ToLocalTime()),

                // Intentionally pass UTC as local per user preference
                FileDateType.UtcAirDate =>
                    ChangeFileDateToLocalDate(
                        episodeFilePath,
                        DateTime.SpecifyKind(airDateUtc.Value, DateTimeKind.Local)),

                _ => false,
            };
        }

        private bool ChangeFileDateToLocalDate(string filePath, DateTime localDate)
        {
            // FileGetLastWrite returns UTC; convert to local to compare
            var oldLastWrite = _diskProvider.FileGetLastWrite(filePath).ToLocalTime();

            if (OsInfo.IsNotWindows && localDate.ToUniversalTime() < DateTimeExtensions.EpochTime)
            {
                _logger.Debug("Setting date of file to 1970-01-01 as actual airdate is before that time and will not be set properly");
                localDate = DateTimeExtensions.EpochTime.ToLocalTime();
            }

            if (!DateTime.Equals(localDate.WithoutTicks(), oldLastWrite.WithoutTicks()))
            {
                try
                {
                    // Preserve prior mtime subseconds per https://github.com/Sonarr/Sonarr/issues/7228
                    var mtime = localDate.WithTicksFrom(oldLastWrite);

                    _diskProvider.FileSetLastWriteTime(filePath, mtime);
                    _logger.Debug("Date of file [{0}] changed from '{1}' to '{2}'", filePath, oldLastWrite, mtime);

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
