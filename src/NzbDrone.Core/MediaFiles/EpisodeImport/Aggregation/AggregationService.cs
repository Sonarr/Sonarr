using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation
{
    public interface IAugmentingService
    {
        LocalEpisode Augment(LocalEpisode localEpisode, bool otherFiles);
    }

    public class AugmentingService : IAugmentingService
    {
        private readonly IEnumerable<IAggregateLocalEpisode> _augmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly Logger _logger;

        public AugmentingService(IEnumerable<IAggregateLocalEpisode> augmenters,
                                 IDiskProvider diskProvider,
                                 IVideoFileInfoReader videoFileInfoReader,
                                 Logger logger)
        {
            _augmenters = augmenters;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _logger = logger;
        }

        public LocalEpisode Augment(LocalEpisode localEpisode, bool otherFiles)
        {
            if (localEpisode.DownloadClientEpisodeInfo == null &&
                localEpisode.FolderEpisodeInfo == null &&
                localEpisode.FileEpisodeInfo == null)
            {
                if (MediaFileExtensions.Extensions.Contains(Path.GetExtension(localEpisode.Path)))
                {
                    throw new AugmentingFailedException("Unable to parse episode info from path: {0}", localEpisode.Path);
                }
            }

            localEpisode.Size = _diskProvider.GetFileSize(localEpisode.Path);
            localEpisode.MediaInfo = _videoFileInfoReader.GetMediaInfo(localEpisode.Path);

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(localEpisode, otherFiles);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return localEpisode;
        }
    }
}
