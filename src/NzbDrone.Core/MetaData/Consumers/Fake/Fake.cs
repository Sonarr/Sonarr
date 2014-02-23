using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Consumers.Fake
{
    public class FakeMetadata : MetadataBase<FakeMetadataSettings>
    {
        public FakeMetadata(IDiskProvider diskProvider, IHttpProvider httpProvider, Logger logger)
            : base(diskProvider, httpProvider, logger)
        {
        }

        public override void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
        {
            throw new NotImplementedException();
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            throw new NotImplementedException();
        }

        public override void AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
        {
            throw new NotImplementedException();
        }

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            return null;
        }
    }
}
