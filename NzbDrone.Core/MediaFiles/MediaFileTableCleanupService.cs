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
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService, IDiskProvider diskProvider, IEpisodeService episodeService, Logger logger)
        {
            _mediaFileService = mediaFileService;
            _diskProvider = diskProvider;
            _episodeService = episodeService;
            _logger = logger;
        }

        public void Execute(CleanMediaFileDb message)
        {
            var seriesFile = _mediaFileService.GetFilesBySeries(message.SeriesId);

            foreach (var episodeFile in seriesFile)
            {
                try
                {
                    if (!_diskProvider.FileExists(episodeFile.Path))
                    {
                        _logger.Trace("File [{0}] no longer exists on disk. removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                    }

                    if (!_episodeService.GetEpisodesByFileId(episodeFile.Id).Any())
                    {
                        _logger.Trace("File [{0}] is not assigned to any episodes. removing from db", episodeFile.Path);
                        _mediaFileService.Delete(episodeFile);
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = String.Format("Unable to cleanup EpisodeFile in DB: {0}", episodeFile.Id);
                    _logger.ErrorException(errorMessage, ex);
                }
            }
        }
    }
}