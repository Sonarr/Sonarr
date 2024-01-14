using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListImport : TraktImportBase<TraktListSettings>
    {
        public TraktListImport(IImportListRepository netImportRepository,
                               IHttpClient httpClient,
                               IImportListStatusService netImportStatusService,
                               IConfigService configService,
                               IParsingService parsingService,
                               ILocalizationService localizationService,
                               Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override string Name => _localizationService.GetLocalizedString("TypeOfList", new Dictionary<string, object> { { "typeOfList", "Trakt" } });

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktListRequestGenerator()
            {
                Settings = Settings,
                ClientId = ClientId
            };
        }
    }
}
