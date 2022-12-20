using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Simkl.User
{
    public class SimklUserImport : SimklImportBase<SimklUserSettings>
    {
        public SimklUserImport(IImportListRepository netImportRepository,
                               IHttpClient httpClient,
                               IImportListStatusService netImportStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Simkl User Watchlist";

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new SimklUserRequestGenerator()
            {
                Settings = Settings,
                ClientId = ClientId
            };
        }
    }
}
