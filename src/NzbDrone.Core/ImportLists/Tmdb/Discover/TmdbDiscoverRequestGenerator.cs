using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Tmdb.Discover;

public class TmdbDiscoverRequestGenerator : TmdbRequestGeneratorBase<TmdbDiscoverSettings>
{
    public TmdbDiscoverRequestGenerator(TmdbDiscoverSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
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

        builder.Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", Settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", $"{sortString}.{sortOrderString}");

        if (originalLanguage != TmdbLanguage.Any)
        {
            builder.AddQueryParam("with_original_language", originalLanguage.ToString().ToLowerInvariant());
        }

        AddOrSkipQueryParam(builder, "vote_average.gte", Settings.MinimumVoteAverage);
        AddOrSkipQueryParam(builder, "vote_count.gte", Settings.MinimumVoteCount);
        AddOrSkipQueryParam(builder, "with_companies", Settings.WithCompanies);
        AddOrSkipQueryParam(builder, "with_keywords", Settings.WithKeywords);
    }

    private static void AddOrSkipQueryParam(HttpRequestBuilder builder, string name, string value)
    {
        if (value.IsNotNullOrWhiteSpace())
        {
            builder.AddQueryParam(name, value);
        }
    }
}
