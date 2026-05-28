using System.Globalization;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.TMDb.Discover;

public class TMDbDiscoverRequestGenerator : TMDbRequestGeneratorBase<TMDbDiscoverSettings>
{
    public TMDbDiscoverRequestGenerator(TMDbDiscoverSettings settings)
        : base(settings)
    {
    }

    protected override void SetupSeriesRequestsBuilder(HttpRequestBuilder builder)
    {
        var sortString = ((TMDbDiscoverSort)Settings.Sort).ToString().ToLower(CultureInfo.InvariantCulture);
        var sortOrderString = Settings.SortOrder == (int)TMDbDiscoverSortOrder.Ascending ? "asc" : "desc";

        builder.Resource("3/discover/tv")
            .AddQueryParam("include_adult", Settings.IncludeAdult)
            .AddQueryParam("include_null_first_air_dates", Settings.IncludeNullFirstAirDates)
            .AddQueryParam("sort_by", $"{sortString}.{sortOrderString}");

        if (Settings.WithOriginalLanguageCode != 0)
        {
            builder.AddQueryParam("with_original_language",
                TMDbLanguageOptionsConverter.UnpackLanguage(Settings.WithOriginalLanguageCode));
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
