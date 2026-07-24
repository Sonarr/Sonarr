using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Trakt.List;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt;

[TestFixture]
public class TraktListRequestGeneratorFixture : CoreTest
{
    private const int PAGE_SIZE = 250;
    private const int MAX_NUM_RESULTS = 1000;

    private static TraktListSettings GivenSettings(int limit = 100)
    {
        return new TraktListSettings
        {
            Username = "testuser",
            Listname = "my list",
            AccessToken = "token",
            Limit = limit,
        };
    }

    [Test]
    public void should_build_url_with_username_and_listname()
    {
        var generator = new TraktListRequestGenerator(GivenSettings(), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be("/users/testuser/lists/my-list/items/show,season,episode");
    }

    [Test]
    public void should_request_single_page_when_limit_is_less_than_page_size()
    {
        var generator = new TraktListRequestGenerator(GivenSettings(limit: 100), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.FullUri.Should().Contain("limit=100&page=1");
    }

    [Test]
    public void should_request_single_page_when_limit_is_same_as_page_size()
    {
        var generator = new TraktListRequestGenerator(GivenSettings(limit: 250), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.FullUri.Should().Contain("limit=250&page=1");
    }

    [Test]
    public void should_request_multiple_pages_when_limit_exceeds_page_size()
    {
        var generator = new TraktListRequestGenerator(GivenSettings(limit: 435), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(2);
        requests[0].Url.FullUri.Should().Contain("limit=250&page=1");
        requests[1].Url.FullUri.Should().Contain("limit=250&page=2");
    }

    [Test]
    public void should_request_maximum_pages_when_limit_exceeds_allowed_value()
    {
        var generator = new TraktListRequestGenerator(GivenSettings(limit: 2000), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(4);
    }
}
