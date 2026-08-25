using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.TorrentClientBaseTests
{
    public class TestTorrentIndexerSettings : ITorrentIndexerSettings
    {
        public NzbDroneValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public string BaseUrl { get; set; }

        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }

        public int MinimumSeeders { get; set; }
        public SeedCriteriaSettings SeedCriteria { get; set; }
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }
        public bool RejectTorrentFilesWithBlockedExtensionsWhileGrabbing { get; set; }
    }
}
