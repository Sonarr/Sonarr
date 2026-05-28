using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Tmdb.Person;

public class TmdbPersonImport : TmdbImportBase<TmdbPersonSettings>
{
    public TmdbPersonImport(ISonarrCloudRequestBuilder requestBuilder,
                            IHttpClient httpClient,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
        : base(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
    {
    }

    public override string Name => "TMDb Person";

    public override IParseImportListResponse GetParser()
    {
        return new TmdbPersonParser(Settings);
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TmdbPersonRequestGenerator(Settings);
    }
}
