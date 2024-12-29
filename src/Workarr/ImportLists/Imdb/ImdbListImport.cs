using NLog;
using Workarr.Configuration;
using Workarr.Http;
using Workarr.Localization;
using Workarr.Parser;
using Workarr.ThingiProvider;

namespace Workarr.ImportLists.Imdb
{
    public class ImdbListImport : HttpImportListBase<ImdbListSettings>
    {
        public override string Name => _localizationService.GetLocalizedString("TypeOfList", new Dictionary<string, object> { { "typeOfList", "IMDb" } });

        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        public ImdbListImport(IHttpClient httpClient,
                              IImportListStatusService importListStatusService,
                              IConfigService configService,
                              IParsingService parsingService,
                              ILocalizationService localizationService,
                              Logger logger)
        : base(httpClient, importListStatusService, configService, parsingService, localizationService, logger)
        {
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                foreach (var def in base.DefaultDefinitions)
                {
                    yield return def;
                }
            }
        }

        public override IImportListRequestGenerator GetRequestGenerator()
        {
            return new ImdbListRequestGenerator()
            {
                Settings = Settings
            };
        }

        public override IParseImportListResponse GetParser()
        {
            return new ImdbListParser();
        }
    }
}
