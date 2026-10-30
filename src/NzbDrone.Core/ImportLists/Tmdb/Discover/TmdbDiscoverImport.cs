using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverImport(ISonarrCloudRequestBuilder requestBuilder,
                                IHttpClient httpClient,
                                IImportListStatusService importListStatusService,
                                IConfigService configService,
                                IParsingService parsingService,
                                ILocalizationService localizationService,
                                Logger logger)
    : TmdbImportBase<TmdbDiscoverSettings>(requestBuilder, httpClient, importListStatusService, configService, parsingService, localizationService, logger)
{
    public override int PageSize => 20;
    public override string Name => "TMDb Discover";

    public override IParseImportListResponse GetParser()
    {
        return new TmdbDiscoverParser();
    }

    public override IImportListRequestGenerator GetRequestGenerator()
    {
        return new TmdbDiscoverRequestGenerator(Settings, 10);
    }

    protected override IEnumerable<KeyValuePair<string, TmdbDiscoverSettings>> GetPresetDefinitionPairs()
    {
        yield return new KeyValuePair<string, TmdbDiscoverSettings>("Airing Today",
            new TmdbDiscoverSettings
            {
                AirDateMinimumDays = 0,
                AirDateMaximumDays = 0,
                SortByType = (int)TmdbDiscoverSortByType.PopularityDesc
            });

        yield return new KeyValuePair<string, TmdbDiscoverSettings>("On The Air",
            new TmdbDiscoverSettings
            {
                AirDateMinimumDays = 0,
                AirDateMaximumDays = 7,
                SortByType = (int)TmdbDiscoverSortByType.PopularityDesc
            });

        yield return new KeyValuePair<string, TmdbDiscoverSettings>("Top Rated",
            new TmdbDiscoverSettings
            {
                VoteCountMinimum = "200",
                SortByType =  (int)TmdbDiscoverSortByType.VoteAverageDesc
            });
    }
}
