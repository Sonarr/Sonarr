using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(Series series);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            IDiskProvider diskProvider,
                                            IEpisodeService episodeService,
                                            ISeriesService seriesService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Clean(Series series)
        {
            var seriesFile = _mediaFileService.GetFilesBySeries(series.Id);
            var episodes = _episodeService.GetEpisodeBySeries(series.Id);

            foreach (var episodeFile in seriesFile)
            {
                var episodeFilePath = Path.Combine(series.Path, episodeFile.RelativePath);

                try
                {
                    if (!_diskProvider.FileExists(episodeFilePath))
                    {
                        _logger.Debug("File [{0}] no longer exists on disk, removing from db", episodeFilePath);
                        _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.MissingFromDisk);
                        continue;
                    }

                    if (!episodes.Any(e => e.EpisodeFileId == episodeFile.Id))
                    {
                        _logger.Debug("File [{0}] is not assigned to any episodes, removing from db", episodeFilePath);
                        _mediaFileService.Delete(episodeFile, DeleteMediaFileReason.NoLinkedEpisodes);
                        continue;
                    }

//                    var localEpsiode = _parsingService.GetLocalEpisode(episodeFile.Path, series);
//
//                    if (localEpsiode == null || episodes.Count != localEpsiode.Episodes.Count)
//                    {
//                        _logger.Debug("File [{0}] parsed episodes has changed, removing from db", episodeFile.Path);
//                        _mediaFileService.Delete(episodeFile);
//                        continue;
//                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = String.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.Id);
                    _logger.ErrorException(errorMessage, ex);
                }
            }

            foreach (var episode in episodes)
            {
                if (episode.EpisodeFileId > 0 && !seriesFile.Any(f => f.Id == episode.EpisodeFileId))
                {
                    episode.EpisodeFileId = 0;
                    _episodeService.UpdateEpisode(episode);
                }
            }
        }
    }
}