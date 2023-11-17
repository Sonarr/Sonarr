using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Indexers.Fanzub
{
    public class Fanzub : HttpIndexerBase<FanzubSettings>
    {
        public override string Name => "Fanzub";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public Fanzub(IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new FanzubRequestGenerator() { Settings = Settings };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new RssParser() { UseEnclosureUrl = true, UseEnclosureLength = true };
        }
    }
}
