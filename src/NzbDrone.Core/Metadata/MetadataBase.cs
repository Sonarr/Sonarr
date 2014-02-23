using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata where TSettings : IProviderConfig, new()
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        protected MetadataBase(IDiskProvider diskProvider, IHttpProvider httpProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public Type ConfigContract
        {
            get
            {
                return typeof(TSettings);
            }
        }

        public IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                return new List<ProviderDefinition>();
            }
        }

        public ProviderDefinition Definition { get; set; }

        public abstract void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles);
        public abstract void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload);
        public abstract void AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles);
        public abstract MetadataFile FindMetadataFile(Series series, string path);

        protected TSettings Settings
        {
            get
            {
                return (TSettings)Definition.Settings;
            }
        }

        protected virtual void DownloadImage(Series series, string url, string path)
        {
            try
            {
                if (_diskProvider.FileExists(path))
                {
                    _logger.Trace("Image already exists: {0}, will not download again.", path);
                    return;
                }

                _httpProvider.DownloadFile(url, path);
            }
            catch (WebException e)
            {
                _logger.Warn(string.Format("Couldn't download image {0} for {1}. {2}", url, series, e.Message));
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't download image " + url + " for " + series, e);
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
