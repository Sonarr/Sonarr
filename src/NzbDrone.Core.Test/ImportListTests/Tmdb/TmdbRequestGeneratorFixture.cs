using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Tmdb;
using NzbDrone.Core.ImportLists.Tmdb.Account;
using NzbDrone.Core.ImportLists.Tmdb.Discover;
using NzbDrone.Core.ImportLists.Tmdb.List;
using NzbDrone.Core.ImportLists.Tmdb.Person;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Tmdb;

[TestFixture]
public class TmdbRequestGeneratorFixture : CoreTest
{
    [Test]
    public void should_request_discover_tv_with_bearer_token_and_json_accept_for_discover()
    {
        var requests = Requests(new TmdbDiscoverRequestGenerator(DiscoverSettings(), 1));

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/3/discover/tv");
        requests[0].HttpRequest.Headers["Authorization"].Should().Be("Bearer token");
        requests[0].HttpRequest.Headers.Accept.Should().Contain("application/json");
    }

    [Test]
    public void should_request_one_request_per_page_for_discover()
    {
        var requests = Requests(new TmdbDiscoverRequestGenerator(DiscoverSettings(), 3));

        requests.Should().HaveCount(3);
        requests.Select((r, i) => Query(r).Contains($"page={i + 1}", StringComparison.Ordinal)).Should().AllBeEquivalentTo(true);
    }

    [Test]
    public void should_send_lowercase_boolean_for_include_null_first_air_dates_for_discover()
    {
        var settings = DiscoverSettings();
        settings.IncludeNullFirstAirDates = true;

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain("include_null_first_air_dates=true");

        settings.IncludeNullFirstAirDates = false;

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain("include_null_first_air_dates=false");
    }

    [Test]
    public void should_join_genres_with_pipe_for_or_for_discover()
    {
        var settings = DiscoverSettings();
        settings.WithGenreTypes = [(int)TmdbDiscoverGenreType.Comedy, (int)TmdbDiscoverGenreType.Drama];

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain("with_genres=35%7C18");
    }

    [Test]
    public void should_send_delimited_networks_for_discover()
    {
        var settings = DiscoverSettings();
        settings.WithNetworks = "213|49";

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain("with_networks=213%7C49");
    }

    [Test]
    public void should_not_send_genres_or_networks_when_empty_for_discover()
    {
        var query = Query(Requests(new TmdbDiscoverRequestGenerator(DiscoverSettings(), 1))[0]);

        query.Should().NotContain("with_genres");
        query.Should().NotContain("with_networks");
        query.Should().NotContain("with_original_language");
        query.Should().NotContain("air_date.");
    }

    [Test]
    public void should_send_lowercase_original_language_for_discover()
    {
        var settings = DiscoverSettings();
        settings.OriginalLanguage = (int)TmdbLanguage.Ja;

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain("with_original_language=ja");
    }

    [Test]
    public void should_resolve_air_dates_relative_to_today_at_request_time_for_discover()
    {
        var settings = DiscoverSettings();
        settings.AirDateMinimumDays = -3;
        settings.AirDateMaximumDays = 7;

        var query = Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0]);

        var today = DateTime.UtcNow.Date;
        query.Should().Contain($"air_date.gte={today.AddDays(-3):yyyy-MM-dd}");
        query.Should().Contain($"air_date.lte={today.AddDays(7):yyyy-MM-dd}");
    }

    [Test]
    public void should_send_zero_day_offsets_as_today_for_discover()
    {
        var settings = DiscoverSettings();
        settings.AirDateMinimumDays = 0;
        settings.AirDateMaximumDays = 0;

        var query = Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0]);

        query.Should().Contain($"air_date.gte={DateTime.UtcNow.Date:yyyy-MM-dd}");
        query.Should().Contain($"air_date.lte={DateTime.UtcNow.Date:yyyy-MM-dd}");
    }

    [Test]
    public void should_send_optional_filters_when_set_for_discover()
    {
        var settings = DiscoverSettings();
        settings.VoteAverageMinimum = "7.5";
        settings.VoteCountMinimum = "200";
        settings.WithCompanies = "1|2";
        settings.WithKeywords = "3,4";

        var query = Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0]);

        query.Should().Contain("vote_average.gte=7.5");
        query.Should().Contain("vote_count.gte=200");
        query.Should().Contain("with_companies=1%7C2");
        query.Should().Contain("with_keywords=3%2C4");
    }

    [TestCase(TmdbDiscoverSortByType.FirstAirDateAsc, "first_air_date.asc")]
    [TestCase(TmdbDiscoverSortByType.FirstAirDateDesc, "first_air_date.desc")]
    [TestCase(TmdbDiscoverSortByType.NameAsc, "name.asc")]
    [TestCase(TmdbDiscoverSortByType.NameDesc, "name.desc")]
    [TestCase(TmdbDiscoverSortByType.OriginalNameAsc, "original_name.asc")]
    [TestCase(TmdbDiscoverSortByType.OriginalNameDesc, "original_name.desc")]
    [TestCase(TmdbDiscoverSortByType.PopularityAsc, "popularity.asc")]
    [TestCase(TmdbDiscoverSortByType.PopularityDesc, "popularity.desc")]
    [TestCase(TmdbDiscoverSortByType.VoteAverageAsc, "vote_average.asc")]
    [TestCase(TmdbDiscoverSortByType.VoteAverageDesc, "vote_average.desc")]
    [TestCase(TmdbDiscoverSortByType.VoteCountAsc, "vote_count.asc")]
    [TestCase(TmdbDiscoverSortByType.VoteCountDesc, "vote_count.desc")]
    public void should_map_sort_by_type_for_discover(TmdbDiscoverSortByType sortBy, string expected)
    {
        var settings = DiscoverSettings();
        settings.SortByType = (int)sortBy;

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0])
            .Should().Contain($"sort_by={expected}");
    }

    [TestCase(TmdbAccountListType.Favorite, "/4/account/acc123/tv/favorites")]
    [TestCase(TmdbAccountListType.Rated, "/4/account/acc123/tv/rated")]
    [TestCase(TmdbAccountListType.Recommended, "/4/account/acc123/tv/recommendations")]
    [TestCase(TmdbAccountListType.Watchlist, "/4/account/acc123/tv/watchlist")]
    public void should_use_endpoint_for_list_type_for_account(TmdbAccountListType type, string path)
    {
        var settings = new TmdbAccountSettings { AuthToken = "token", AccountId = "acc123", AccountListType = (int)type };

        var requests = Requests(new TmdbAccountRequestGenerator(settings, 2));

        requests.Should().HaveCount(2);
        requests.Should().OnlyContain(r => r.Url.Path == path);
        requests[0].HttpRequest.Headers["Authorization"].Should().Be("Bearer token");
    }

    [Test]
    public void should_use_list_id_when_provided_for_list()
    {
        var settings = new TmdbListSettings { AuthToken = "token", ListId = 42 };

        Requests(new TmdbListRequestGenerator(settings, 1))[0].Url.Path.Should().Be("/4/list/42");
    }

    [Test]
    public void should_use_account_list_id_when_selected_for_list()
    {
        var settings = new TmdbListSettings { AuthToken = "token", AccountListId = 77 };

        Requests(new TmdbListRequestGenerator(settings, 1))[0].Url.Path.Should().Be("/4/list/77");
    }

    [Test]
    public void should_use_list_id_when_account_list_is_none_for_list()
    {
        var settings = new TmdbListSettings { AuthToken = "token", ListId = 42, AccountListId = 0 };

        Requests(new TmdbListRequestGenerator(settings, 1))[0].Url.Path.Should().Be("/4/list/42");
    }

    [Test]
    public void should_request_tv_credits_once_for_person()
    {
        var settings = new TmdbPersonSettings { AuthToken = "token", PersonId = 9 };

        var requests = Requests(new TmdbPersonRequestGenerator(settings));

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/3/person/9/tv_credits");
        requests[0].HttpRequest.Headers["Authorization"].Should().Be("Bearer token");
    }

    [Test]
    public void should_not_throw_when_genres_and_networks_are_null_for_discover()
    {
        var settings = DiscoverSettings();
        settings.WithGenreTypes = null;
        settings.WithNetworks = null;

        var query = Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0]);

        query.Should().NotContain("with_genres");
        query.Should().NotContain("with_networks");
    }

    [Test]
    public void should_trim_optional_string_filters_for_discover()
    {
        var settings = DiscoverSettings();
        settings.VoteAverageMinimum = " 7.5 ";

        Query(Requests(new TmdbDiscoverRequestGenerator(settings, 1))[0]).Should().Contain("vote_average.gte=7.5");
    }

    [Test]
    public void should_escape_account_id_in_path_for_account()
    {
        var settings = new TmdbAccountSettings { AuthToken = "token", AccountId = "a/b c" };

        Requests(new TmdbAccountRequestGenerator(settings, 1))[0].Url.FullUri.Should().Contain("/4/account/a%2Fb%20c/tv/");
    }

    private static ImportListRequest[] Requests(IImportListRequestGenerator generator)
    {
        return [.. generator.GetListItems().GetAllTiers().SelectMany(t => t)];
    }

    private static string Query(ImportListRequest request) => request.Url.FullUri;

    private static TmdbDiscoverSettings DiscoverSettings() => new() { AuthToken = "token" };
}
