using System.Globalization;
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
        var sortString = ((TmdbDiscoverSort)Settings.Sort).ToString().ToLower(CultureInfo.InvariantCulture);
        var sortOrderString = Settings.SortOrder == (int)TmdbDiscoverSortOrder.Ascending ? "asc" : "desc";

        builder.Resource("3/discover/tv")
            .AddQueryParam("include_null_first_air_dates", Settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", $"{sortString}.{sortOrderString}");

        if (Settings.WithOriginalLanguageCode != 0)
        {
            builder.AddQueryParam("with_original_language",
                TmdbLanguageOptionsConverter.UnpackLanguage(Settings.WithOriginalLanguageCode));
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
