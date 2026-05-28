using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.TMDb.Account;

public class TMDbAccountImport : TMDbImportBase<TMDbAccountSettings>
{
    public TMDbAccountImport(ISonarrCloudRequestBuilder requestBuilder,
                          IHttpClient httpClient,
                          IImportListStatusService importListStatusService,
                          IConfigService configService,
                          IParsingService parsingService,
                          ILocalizationService localizationService,
                          Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    {
    }

    public override int PageSize => 20;
    public override string Name => "TMDb Account";

    public override IParseImportListResponse GetParser()
    {
        return new TMDbAccountParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TMDbAccountRequestGenerator(Settings);
    }
}
