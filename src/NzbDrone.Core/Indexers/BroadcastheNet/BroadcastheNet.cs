using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.BroadcastheNet
{
    public class BroadcastheNet : HttpIndexerBase<BroadcastheNetSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Torrent; } }
        public override bool SupportsRss { get { return true; } }
        public override bool SupportsSearch { get { return true; } }
        public override int PageSize { get { return 100; } }

        public BroadcastheNet(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new BroadcastheNetRequestGenerator() { Settings = Settings, PageSize = PageSize };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new BroadcastheNetParser();
        }
    }
}
