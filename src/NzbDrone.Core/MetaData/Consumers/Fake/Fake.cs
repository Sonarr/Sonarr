using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Consumers.Fake
{
    public class FakeMetadata : MetadataBase<FakeMetadataSettings>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public FakeMetadata(IDiskProvider diskProvider, IHttpProvider httpProvider, Logger logger)
            : base(diskProvider, httpProvider, logger)
        {
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public override void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles)
        {
            throw new NotImplementedException();
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            throw new NotImplementedException();
        }

        public override void AfterRename(Series series)
        {
            throw new NotImplementedException();
        }

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            return null;
        }
    }
}
