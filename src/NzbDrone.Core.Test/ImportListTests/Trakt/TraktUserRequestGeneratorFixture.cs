using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Trakt.User;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt;

[TestFixture]
public class TraktUserRequestGeneratorFixture : CoreTest
{
    private const int PAGE_SIZE = 250;
    private const int MAX_NUM_RESULTS = 1000;

    private static TraktUserSettings GivenSettings(int limit = 100, TraktUserListType? listType = null, TraktUserWatchedListType? watchedListType = null, TraktUserWatchSorting? watchSorting = null)
    {
        var settings = new TraktUserSettings
        {
            Username = "testuser",
            AccessToken = "token",
            Limit = limit,
        };

        if (listType.HasValue)
        {
            settings.TraktListType = (int)listType.Value;
        }

        if (watchedListType.HasValue)
        {
            settings.TraktWatchedListType = (int)watchedListType.Value;
        }

        if (watchSorting.HasValue)
        {
            settings.TraktWatchSorting = (int)watchSorting.Value;
        }

        return settings;
    }

    [Test]
    public void should_build_url_with_username_and_defaults()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/users/testuser/watchlist/shows/rank");
        requests[0].Url.FullUri.Should().NotContain("extended=full");
    }

    [Test]
    public void should_build_url_with_username_and_list_type_watch()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(listType: TraktUserListType.UserWatchList, watchSorting: TraktUserWatchSorting.Added), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/users/testuser/watchlist/shows/added");
        requests[0].Url.FullUri.Should().NotContain("extended=full");
    }

    [Test]
    public void should_build_url_with_username_and_list_type_watched()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(listType: TraktUserListType.UserWatchedList), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/users/testuser/watched/shows");
        requests[0].Url.FullUri.Should().Contain("extended=full");
    }

    [Test]
    public void should_build_url_with_username_and_list_type_collection()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(listType: TraktUserListType.UserCollectionList), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/users/testuser/collection/shows");
        requests[0].Url.FullUri.Should().NotContain("extended=full");
    }

    [Test]
    public void should_request_single_page_when_limit_is_less_than_page_size()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(limit: 100), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.FullUri.Should().Contain("limit=100&page=1");
    }

    [Test]
    public void should_request_single_page_when_limit_is_same_as_page_size()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(limit: 250), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.FullUri.Should().Contain("limit=250&page=1");
    }

    [Test]
    public void should_request_multiple_pages_when_limit_exceeds_page_size()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(limit: 435), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(2);
        requests[0].Url.FullUri.Should().Contain("limit=250&page=1");
        requests[1].Url.FullUri.Should().Contain("limit=250&page=2");
    }

    [Test]
    public void should_request_maximum_pages_when_limit_exceeds_allowed_value()
    {
        var generator = new TraktUserRequestGenerator(GivenSettings(limit: 2000), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(4);
    }
}
