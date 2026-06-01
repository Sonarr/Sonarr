using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Tmdb.Account;

public class TmdbAccountImport : TmdbImportBase<TmdbAccountSettings>
{
    public TmdbAccountImport(ISonarrCloudRequestBuilder requestBuilder,
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
        return new TmdbAccountParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TmdbAccountRequestGenerator(Settings, 10);
    }
}
