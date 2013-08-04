using System;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{

    public class MediaFileTableCleanupService : IExecute<CleanMediaFileDb>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesService _seriesService;
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
            _seriesService = seriesService;
            _logger = logger;
        }

        public void Execute(CleanMediaFileDb message)
        {
            var seriesFile = _mediaFileService.GetFilesBySeries(message.SeriesId);
            var series = _seriesService.GetSeries(message.SeriesId);
            var episodes = _episodeService.GetEpisodeBySeries(message.SeriesId);

            foreach (var episodeFile in seriesFile)
            {
                try
                {
                    if (!_diskProvider.FileExists(episodeFile.Path))
                    {
                        _logger.Trace("File [{0}] no longer exists on disk, removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                        continue;
                    }

                    if (!_diskProvider.IsParent(series.Path, episodeFile.Path))
                    {
                        _logger.Trace("File [{0}] does not belong to this series, removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                        continue; 
                    }

                    if (!episodes.Any(e => e.EpisodeFileId == episodeFile.Id))
                    {
                        _logger.Trace("File [{0}] is not assigned to any episodes, removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                        continue;
                    }

//                    var localEpsiode = _parsingService.GetEpisodes(episodeFile.Path, series);
//
//                    if (localEpsiode == null || episodes.Count != localEpsiode.Episodes.Count)
//                    {
//                        _logger.Trace("File [{0}] parsed episodes has changed, removing from db", episodeFile.Path);
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