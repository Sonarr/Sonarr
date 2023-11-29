using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.IPTorrents
{
    public class IPTorrents : HttpIndexerBase<IPTorrentsSettings>
    {
        public override string Name => "IP Torrents";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsSearch => false;
        public override int PageSize => 0;

        public IPTorrents(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new IPTorrentsRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorrentRssParser() { ParseSizeInDescription = true };
        }
    }
}
