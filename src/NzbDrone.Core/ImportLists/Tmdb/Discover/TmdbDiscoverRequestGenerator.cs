using System.Linq;
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

        var sortByType = (TmdbDiscoverSortByType)Settings.SortByType;
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

        var builder = new HttpRequestBuilder(Settings.BaseUrl)
            .Accept(HttpAccept.Json)
            .SetHeader("Authorization", $"Bearer {Settings.AuthToken}")
            .Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", Settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", sortByString);

        if (originalLanguage != TmdbLanguage.Any)
        {
            builder.AddQueryParam("with_original_language", originalLanguage.ToString().ToLowerInvariant());
        }

        if (Settings.WithGenreTypes.Any())
        {
            builder.AddQueryParam("with_genres", string.Join(',', Settings.WithGenreTypes));
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
