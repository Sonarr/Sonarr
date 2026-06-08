using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverImport : TmdbImportBase<TmdbDiscoverSettings>
{
    public TmdbDiscoverImport(ISonarrCloudRequestBuilder requestBuilder,
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
        var now = DateTime.UtcNow;
        var todaysDate = now.ToString("yyyy-MM-dd");
        var sevenDaysFromNowDate = now.AddDays(7).ToString("yyyy-MM-dd");

        yield return new KeyValuePair<string, TmdbDiscoverSettings>($"Airing Today ({todaysDate})",
            new TmdbDiscoverSettings
            {
                AirDateMinimum =  todaysDate,
                AirDateMaximum =  todaysDate,
                SortType = (int)TmdbDiscoverSortType.Popularity,
                SortOrderType = (int)TmdbDiscoverSortOrderType.Descending
            });

        yield return new KeyValuePair<string, TmdbDiscoverSettings>($"On The Air ({todaysDate} - {sevenDaysFromNowDate})",
            new TmdbDiscoverSettings
            {
                AirDateMinimum =  todaysDate,
                AirDateMaximum =  sevenDaysFromNowDate,
                SortType = (int)TmdbDiscoverSortType.Popularity,
                SortOrderType = (int)TmdbDiscoverSortOrderType.Descending
            });

        yield return new KeyValuePair<string, TmdbDiscoverSettings>("Top Rated",
            new TmdbDiscoverSettings
            {
                VoteCountMinimum = "200",
                SortType = (int)TmdbDiscoverSortType.VoteAverage,
                SortOrderType = (int)TmdbDiscoverSortOrderType.Descending
            });
    }
}
