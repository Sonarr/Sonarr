using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverRequestGenerator : TmdbRequestGeneratorBase<TmdbDiscoverSettings>
{
    public TmdbDiscoverRequestGenerator(TmdbDiscoverSettings settings, int maxPages)
        : base(settings, maxPages)
    {
    }

    protected override HttpRequestBuilder CreateSeriesRequestsBuilder()
    {
        var originalLanguage = (TmdbLanguage)Settings.OriginalLanguage;
        var sortOrderString = Settings.SortOrderType == (int)TmdbDiscoverSortOrderType.Ascending ? "asc" : "desc";

        var sortType = (TmdbDiscoverSortType)Settings.SortType;
        var sortString = sortType switch
        {
            TmdbDiscoverSortType.FirstAirDate => "first_air_date",
            TmdbDiscoverSortType.OriginalName => "original_name",
            TmdbDiscoverSortType.VoteAverage => "vote_average",
            TmdbDiscoverSortType.VoteCount => "vote_count",
            _ => sortType.ToString().ToLowerInvariant()
        };

        var builder = new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", Settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", $"{sortString}.{sortOrderString}");

        if (originalLanguage != TmdbLanguage.Any)
        {
            builder.AddQueryParam("with_original_language", originalLanguage.ToString().ToLowerInvariant());
        }

        AddOrSkipQueryParam(builder, "air_date.gte", Settings.AirDateMinimum);
        AddOrSkipQueryParam(builder, "air_date.lte", Settings.AirDateMaximum);
        AddOrSkipQueryParam(builder, "vote_average.gte", Settings.VoteAverageMinimum);
        AddOrSkipQueryParam(builder, "vote_count.gte", Settings.VoteCountMinimum);
        AddOrSkipQueryParam(builder, "with_companies", Settings.WithCompanies);
        AddOrSkipQueryParam(builder, "with_keywords", Settings.WithKeywords);
        AddOrSkipQueryParam(builder, "with_networks", Settings.WithNetworks);

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
