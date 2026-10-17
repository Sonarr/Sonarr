using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public class TraktSmartListImport : TraktImportBase<TraktSmartListSettings>
    {
        public TraktSmartListImport(IImportListRepository netImportRepository,
                                    IHttpClient httpClient,
                                    IImportListStatusService netImportStatusService,
                                    IConfigService configService,
                                    IParsingService parsingService,
                                    ILocalizationService localizationService,
                                    Logger logger)
        : base(netImportRepository, httpClient, netImportStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override string Name => _localizationService.GetLocalizedString("ImportListsTraktSettingsSmartListName");

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new TraktSmartListRequestGenerator(Settings, ClientId, PageSize, MaxNumResultsPerQuery);
        }
    }
}
