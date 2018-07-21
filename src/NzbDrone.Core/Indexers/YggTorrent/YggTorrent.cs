using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NLog;

namespace NzbDrone.Core.Indexers.YggTorrent
{
    public class YggTorrent : HttpIndexerBase<YggTorrentSettings>
    {
        public override string Name => "YggTorrent";
        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override bool SupportsSearch => true;
        public override int PageSize => 0;

        public YggTorrent(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new YggTorrentRequestGenerator() { Settings = Settings, HttpClient = _httpClient };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new YggTorrentHtmlParser(Settings);
        }
    }
}
