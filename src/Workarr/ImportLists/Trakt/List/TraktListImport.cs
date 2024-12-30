using NLog;
using Workarr.Configuration;
using Workarr.Http;
using Workarr.Localization;
using Workarr.Parser;

namespace Workarr.ImportLists.Trakt.List
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
