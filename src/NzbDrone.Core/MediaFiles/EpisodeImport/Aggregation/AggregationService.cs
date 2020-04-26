using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation
{
    public interface IAggregationService
    {
        LocalEpisode Augment(LocalEpisode localEpisode, DownloadClientItem downloadClientItem, bool otherFiles);
    }

    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IAggregateLocalEpisode> _augmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AggregationService(IEnumerable<IAggregateLocalEpisode> augmenters,
                                 IDiskProvider diskProvider,
                                 IVideoFileInfoReader videoFileInfoReader,
                                 IConfigService configService,
                                 Logger logger)
        {
            _augmenters = augmenters;
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public LocalEpisode Augment(LocalEpisode localEpisode, DownloadClientItem downloadClientItem, bool otherFiles)
        {
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localEpisode.Path));

            if (localEpisode.DownloadClientEpisodeInfo == null &&
                localEpisode.FolderEpisodeInfo == null &&
                localEpisode.FileEpisodeInfo == null)
            {
                if (isMediaFile)
                {
                    throw new AugmentingFailedException("Unable to parse episode info from path: {0}", localEpisode.Path);
                }
            }

            localEpisode.Size = _diskProvider.GetFileSize(localEpisode.Path);

            if (isMediaFile && (!localEpisode.ExistingFile || _configService.EnableMediaInfo))
            {
                localEpisode.MediaInfo = _videoFileInfoReader.GetMediaInfo(localEpisode.Path);
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(localEpisode, downloadClientItem, otherFiles);
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
