using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverRequestGenerator(TmdbDiscoverSettings settings, int maxPages) : IImportListRequestGenerator
{
    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private static void AddAirDateQueryParam(HttpRequestBuilder builder, string name, int? daysFromToday)
    {
        if (!daysFromToday.HasValue)
        {
            return;
        }

        var date = DateTime.UtcNow.Date.AddDays(daysFromToday.Value);
        builder.AddQueryParam(name, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    private static void AddOrSkipQueryParam(HttpRequestBuilder builder, string name, string value)
    {
        if (value.IsNotNullOrWhiteSpace())
        {
            builder.AddQueryParam(name, value.Trim());
        }
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = CreateSeriesRequestsBuilder();

        for (var i = 1; i <= maxPages; i++)
        {
            builder.AddQueryParam("page", i, true);
            yield return new ImportListRequest(builder.Build());
        }
    }

    private HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        var originalLanguage = (TmdbLanguage)settings.OriginalLanguage;

        var sortByType = (TmdbDiscoverSortByType)settings.SortByType;
        var sortByString = sortByType switch
        {
            TmdbDiscoverSortByType.FirstAirDateAsc => "first_air_date.asc",
            TmdbDiscoverSortByType.FirstAirDateDesc => "first_air_date.desc",

            TmdbDiscoverSortByType.NameAsc => "name.asc",
            TmdbDiscoverSortByType.NameDesc => "name.desc",

            TmdbDiscoverSortByType.OriginalNameAsc => "original_name.asc",
            TmdbDiscoverSortByType.OriginalNameDesc => "original_name.desc",

            TmdbDiscoverSortByType.PopularityAsc => "popularity.asc",
            TmdbDiscoverSortByType.PopularityDesc => "popularity.desc",

            TmdbDiscoverSortByType.VoteAverageAsc => "vote_average.asc",
            TmdbDiscoverSortByType.VoteAverageDesc => "vote_average.desc",

            TmdbDiscoverSortByType.VoteCountAsc => "vote_count.asc",
            TmdbDiscoverSortByType.VoteCountDesc => "vote_count.desc",

            _ => "popularity.desc"
        };

        var builder = new HttpRequestBuilder(settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {settings.AuthToken}")
            .Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", settings.IncludeNullFirstAirDates ? "true" : "false")
            .AddQueryParam("sort_by", sortByString);

        if (originalLanguage != TmdbLanguage.Any)
        {
            // TMDb expects lowercase ISO 639-1 codes
            builder.AddQueryParam("with_original_language", originalLanguage.ToString().ToLowerInvariant());
        }

        if (settings.WithGenreTypes?.Any() == true)
        {
            builder.AddQueryParam("with_genres", string.Join('|', settings.WithGenreTypes));
        }

        AddAirDateQueryParam(builder, "air_date.gte", settings.AirDateMinimumDays);
        AddAirDateQueryParam(builder, "air_date.lte", settings.AirDateMaximumDays);
        AddOrSkipQueryParam(builder, "vote_average.gte", settings.VoteAverageMinimum);
        AddOrSkipQueryParam(builder, "vote_count.gte", settings.VoteCountMinimum);
        AddOrSkipQueryParam(builder, "with_networks", settings.WithNetworks);
        AddOrSkipQueryParam(builder, "with_companies", settings.WithCompanies);
        AddOrSkipQueryParam(builder, "with_keywords", settings.WithKeywords);

        return builder;
    }
}
