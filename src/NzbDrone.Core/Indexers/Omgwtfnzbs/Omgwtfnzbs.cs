using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class Omgwtfnzbs : HttpIndexerBase<OmgwtfnzbsSettings>
    {
        public override DownloadProtocol Protocol { get { return DownloadProtocol.Usenet; } }

        public Omgwtfnzbs(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {

        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new OmgwtfnzbsRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new OmgwtfnzbsRssParser();
        }
    }
}
