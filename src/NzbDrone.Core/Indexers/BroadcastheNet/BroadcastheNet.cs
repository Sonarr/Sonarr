using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNet : HttpIndexerBase<BroadcastheNetSettings>
    {
        public override string Name => "BroadcasTheNet";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsRss => true;
        public override bool SupportsSearch => true;
        public override int PageSize => 100;

        public BroadcastheNet(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            var requestGenerator = new BroadcastheNetRequestGenerator() { Settings = Settings, PageSize = PageSize };

            var releaseInfo = _indexerStatusService.GetLastRssSyncReleaseInfo(Definition.Id);
            if (releaseInfo != null)
            {
                int torrentID;
                if (int.TryParse(releaseInfo.Guid.Replace("BTN-", string.Empty), out torrentID))
                {
                    requestGenerator.LastRecentTorrentID = torrentID;
                }
            }

            return requestGenerator;
        }

        public override IParseIndexerResponse GetParser()
        {
            return new BroadcastheNetParser();
        }
    }
}
