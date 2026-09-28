using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.TorrentClientBaseTests
{
    public class TestTorrentClient : TorrentClientBase<TestTorrentClientSettings>
    {
        public TestTorrentClient(ITorrentFileInfoReader torrentFileInfoReader,
            IHttpClient httpClient,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            ILocalizationService localizationService,
            IBlocklistService blocklistService,
            Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
        }

        public override string Name => "Test Torrent Client";

        // Overridable per-test so the shared fixture can exercise both the torrent-file
        // and magnet-fallback code paths without depending on a concrete client.
        private bool _preferTorrentFile;
        public override bool PreferTorrentFile => _preferTorrentFile;
        public void SetPreferTorrentFile(bool value) => _preferTorrentFile = value;

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink) => hash;
        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent) => hash;

        public override IEnumerable<DownloadClientItem> GetItems() => Enumerable.Empty<DownloadClientItem>();
        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
        }

        public override DownloadClientInfo GetStatus() => new() { IsLocalhost = true };

        protected override void Test(List<ValidationFailure> failures)
        {
        }
    }
}
