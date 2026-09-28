using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverRequestGenerator : IImportListRequestGenerator
{
    private readonly TmdbDiscoverSettings _settings;
    private readonly int _maxPages;

    public TmdbDiscoverRequestGenerator(TmdbDiscoverSettings settings, int maxPages)
    {
        _settings = settings;
        _maxPages = maxPages;
    }

    public ImportListPageableRequestChain GetListItems()
    {
        var pageableRequests = new ImportListPageableRequestChain();
        pageableRequests.Add(GetSeriesRequests());
        return pageableRequests;
    }

    private IEnumerable<ImportListRequest> GetSeriesRequests()
    {
        var builder = CreateSeriesRequestsBuilder();

        for (var i = 1; i <= _maxPages; i++)
        {
            builder.AddQueryParam("page", i, true);
            yield return new ImportListRequest(builder.Build());
        }
    }

    private HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        var originalLanguage = (TmdbLanguage)_settings.OriginalLanguage;

        var sortByType = (TmdbDiscoverSortByType)_settings.SortByType;
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

        var builder = new HttpRequestBuilder(_settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {_settings.AuthToken}")
            .Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", _settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", sortByString);

        if (originalLanguage != TmdbLanguage.Any)
        {
            builder.AddQueryParam("with_original_language", originalLanguage.ToString().ToLowerInvariant());
        }

        if (_settings.WithGenreTypes.Any())
        {
            builder.AddQueryParam("with_genres", string.Join(',', _settings.WithGenreTypes));
        }

        if (_settings.WithNetworks.Any())
        {
            builder.AddQueryParam("with_networks", string.Join(',', _settings.WithNetworks));
        }

        AddOrSkipQueryParam(builder, "air_date.gte", _settings.AirDateMinimum);
        AddOrSkipQueryParam(builder, "air_date.lte", _settings.AirDateMaximum);
        AddOrSkipQueryParam(builder, "vote_average.gte", _settings.VoteAverageMinimum);
        AddOrSkipQueryParam(builder, "vote_count.gte", _settings.VoteCountMinimum);
        AddOrSkipQueryParam(builder, "with_companies", _settings.WithCompanies);
        AddOrSkipQueryParam(builder, "with_keywords", _settings.WithKeywords);

        return builder;
    }

    private static void AddOrSkipQueryParam(HttpRequestBuilder builder, string name, string value)
    {
        if (value.IsNotNullOrWhiteSpace())
        {
            builder.AddQueryParam(name, value);
        }
    }
}
