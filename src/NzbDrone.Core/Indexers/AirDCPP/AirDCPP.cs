using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.AirDCPP
{
    public class AirDCPP : HttpIndexerBase<AirDCPPSettings>
    {
        public AirDCPP(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "AirDC++";
        public override bool SupportsRss => false;

        public override DownloadProtocol Protocol => DownloadProtocol.DirectConnect;        

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new AirDCPPRequestGenerator(_httpClient, _logger) { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new AirDCPPParser();
        }
    }
}