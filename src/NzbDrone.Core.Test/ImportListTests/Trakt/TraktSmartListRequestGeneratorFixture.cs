using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Trakt.SmartList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Trakt;

[TestFixture]
public class TraktSmartListRequestGeneratorFixture : CoreTest
{
    private const int PAGE_SIZE = 250;
    private const int MAX_NUM_RESULTS = 1000;
    private const string SLUG = "drama-picks-1a2b3c4d5e6f7a8b";

    private static TraktSmartListSettings GivenSettings(string smartListId = SLUG, int limit = 100)
    {
        return new TraktSmartListSettings
        {
            SmartListId = smartListId,
            AccessToken = "token",
            Limit = limit,
        };
    }

    [TestCase("https://app.trakt.tv/lists/smart/view/" + SLUG)]
    [TestCase("not a slug")]
    [TestCase("Drama-Picks")]
    [TestCase("drama_picks")]
    [TestCase("dráma-picks")]
    [TestCase("")]
    public void should_reject_values_that_are_not_smart_list_slugs(string smartListId)
    {
        new TraktSmartListSettingsValidator().Validate(GivenSettings(smartListId))
            .Errors.Should().Contain(e => e.PropertyName == nameof(TraktSmartListSettings.SmartListId));
    }

    [TestCase(SLUG)]
    [TestCase("12345")]
    public void should_accept_smart_list_slugs_and_ids(string smartListId)
    {
        new TraktSmartListSettingsValidator().Validate(GivenSettings(smartListId))
            .Errors.Should().NotContain(e => e.PropertyName == nameof(TraktSmartListSettings.SmartListId));
    }

    [Test]
    public void should_build_url_with_smart_list_slug()
    {
        var generator = new TraktSmartListRequestGenerator(GivenSettings(), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(1);
        requests[0].Url.Path.Should().Be($"/smart-lists/{SLUG}/items/shows");
        requests[0].Url.FullUri.Should().Contain("limit=100&page=1");
    }

    [Test]
    public void should_request_multiple_pages_when_limit_exceeds_page_size()
    {
        var generator = new TraktSmartListRequestGenerator(GivenSettings(limit: 435), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var requests = generator.GetListItems().GetAllTiers().First().ToList();

        requests.Should().HaveCount(2);
        requests[0].Url.FullUri.Should().Contain("limit=250&page=1");
        requests[1].Url.FullUri.Should().Contain("limit=250&page=2");
    }

    [Test]
    public void should_send_rating_and_genres_to_narrow_the_smart_list()
    {
        var settings = GivenSettings();
        settings.Rating = "70-100";
        settings.Genres = "drama";

        var generator = new TraktSmartListRequestGenerator(settings, "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var request = generator.GetListItems().GetAllTiers().First().First();

        request.Url.FullUri.Should().Contain("ratings=70-100");
        request.Url.FullUri.Should().Contain("genres=drama");
    }

    [Test]
    public void should_send_the_access_token()
    {
        var generator = new TraktSmartListRequestGenerator(GivenSettings(), "12345", PAGE_SIZE, MAX_NUM_RESULTS);

        var request = generator.GetListItems().GetAllTiers().First().First();

        request.HttpRequest.Headers.GetSingleValue("Authorization").Should().Be("Bearer token");
    }
}
