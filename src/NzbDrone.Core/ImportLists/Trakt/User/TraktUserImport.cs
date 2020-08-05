using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserImport : TraktImportBase<TraktUserSettings>
    {
        public TraktUserImport(IImportListRepository netImportRepository,
                               IHttpClient httpClient,
                               IImportListStatusService netImportStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, logger)
        {
        }

        public override string Name => "Trakt User";

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktUserRequestGenerator()
            {
                Settings = Settings,
                ClientId = ClientId
            };
        }
    }
}
