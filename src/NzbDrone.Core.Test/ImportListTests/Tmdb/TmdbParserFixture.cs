using System.Linq;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.Tmdb;
using NzbDrone.Core.ImportLists.Tmdb.Account;
using NzbDrone.Core.ImportLists.Tmdb.Discover;
using NzbDrone.Core.ImportLists.Tmdb.List;
using NzbDrone.Core.ImportLists.Tmdb.Person;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.Tmdb;

[TestFixture]
public class TmdbParserFixture : CoreTest
{
    private const string PagedJson = """
        {
          "page": 1,
          "results": [
            { "id": 1, "name": "Show One", "media_type": "tv" },
            { "id": 2, "name": "Show Two" },
            { "id": 3, "title": "A Movie", "media_type": "movie" }
          ],
          "total_pages": 1
        }
        """;

    private const string CreditsJson = """
        {
          "id": 9,
          "cast": [
            { "id": 10, "name": "Cast Show" }
          ],
          "crew": [
            { "id": 20, "name": "Directed Show", "department": "Directing" },
            { "id": 21, "name": "Costume Show", "department": "Costume & Make-Up" },
            { "id": 22, "name": "Vfx Show", "department": "Visual Effects" },
            { "id": 23, "name": "Written Show", "department": "Writing" }
          ]
        }
        """;

    [Test]
    public void should_parse_tv_items_and_skip_movies_for_list()
    {
        var items = new TmdbListParser().ParseResponse(Response(PagedJson)).Where(i => i != null).ToList();

        items.Select(i => i.TmdbId).Should().Equal(1, 2);
        items.Select(i => i.Title).Should().Equal("Show One", "Show Two");
    }

    [Test]
    public void should_parse_results_for_discover()
    {
        var items = new TmdbDiscoverParser().ParseResponse(Response(PagedJson)).Where(i => i != null).ToList();

        items.Select(i => i.TmdbId).Should().Equal(1, 2);
    }

    [Test]
    public void should_parse_results_for_account()
    {
        var items = new TmdbAccountParser().ParseResponse(Response(PagedJson)).Where(i => i != null).ToList();

        items.Select(i => i.TmdbId).Should().Equal(1, 2);
    }

    [Test]
    public void should_return_empty_for_empty_results()
    {
        new TmdbListParser().ParseResponse(Response("""{ "results": [] }""")).Should().BeEmpty();
    }

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.TooManyRequests)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void should_throw_on_unexpected_status_code(HttpStatusCode status)
    {
        var action = () => new TmdbListParser().ParseResponse(Response("{}", status));

        action.Should().Throw<ImportListException>().Which.Message.Should().Contain(status.ToString()).And.NotContain("$");
    }

    [Test]
    public void should_throw_on_html_response()
    {
        var action = () => new TmdbListParser().ParseResponse(Response("<html></html>", contentType: "text/html"));

        action.Should().Throw<ImportListException>().Which.Message.Should().Contain("html").And.NotContain("$");
    }

    [Test]
    public void should_parse_cast_only_for_person()
    {
        var items = PersonParser(true).ParseResponse(Response(CreditsJson));

        items.Select(i => i.TmdbId).Should().Equal(10);
    }

    [Test]
    public void should_parse_selected_crew_departments_only_for_person()
    {
        var items = PersonParser(false, TmdbDepartmentType.Directing, TmdbDepartmentType.Writing).ParseResponse(Response(CreditsJson));

        items.Select(i => i.TmdbId).Should().BeEquivalentTo([20, 23]);
    }

    [Test]
    public void should_combine_cast_and_crew_for_person()
    {
        var items = PersonParser(true, TmdbDepartmentType.Directing).ParseResponse(Response(CreditsJson));

        items.Select(i => i.TmdbId).Should().BeEquivalentTo([10, 20]);
    }

    [Test]
    public void should_match_costume_and_make_up_department_for_person()
    {
        var items = PersonParser(false, TmdbDepartmentType.CostumeMakeup).ParseResponse(Response(CreditsJson));

        items.Select(i => i.TmdbId).Should().Equal(21);
    }

    [Test]
    public void should_match_visual_effects_department_for_person()
    {
        var items = PersonParser(false, TmdbDepartmentType.VisualEffects).ParseResponse(Response(CreditsJson));

        items.Select(i => i.TmdbId).Should().Equal(22);
    }

    [Test]
    public void should_handle_missing_cast_and_crew_for_person()
    {
        PersonParser(true, TmdbDepartmentType.Directing).ParseResponse(Response("""{ "id": 9 }""")).Should().BeEmpty();
    }

    [Test]
    public void should_handle_empty_cast_and_crew_for_person()
    {
        PersonParser(true, TmdbDepartmentType.Directing).ParseResponse(Response("""{ "id": 9, "cast": [], "crew": [] }""")).Should().BeEmpty();
    }

    [Test]
    public void should_return_null_entries_for_non_tv_items_so_paging_counts_full_pages_for_list()
    {
        var items = new TmdbListParser().ParseResponse(Response(PagedJson));

        items.Should().HaveCount(3);
        items[2].Should().BeNull();
    }

    [Test]
    public void should_return_empty_when_results_is_missing_or_null_for_parsers()
    {
        foreach (var json in new[] { "{}", """{ "results": null }""" })
        {
            new TmdbListParser().ParseResponse(Response(json)).Should().BeEmpty();
            new TmdbDiscoverParser().ParseResponse(Response(json)).Should().BeEmpty();
            new TmdbAccountParser().ParseResponse(Response(json)).Should().BeEmpty();
        }
    }

    [Test]
    public void should_handle_null_department_selection_for_person()
    {
        var parser = new TmdbPersonParser(new TmdbPersonSettings { IncludingCastCredits = true, IncludeDepartmentTypes = null });

        parser.ParseResponse(Response(CreditsJson)).Select(i => i.TmdbId).Should().Equal(10);
    }

    private static ImportListResponse Response(string content, HttpStatusCode status = HttpStatusCode.OK, string contentType = "application/json")
    {
        var httpRequest = new HttpRequest("https://api.themoviedb.org/3/test")
        {
            Headers = { Accept = "application/json" }
        };

        var headers = new HttpHeader();
        if (contentType != null)
        {
            headers.ContentType = contentType;
        }

        var httpResponse = new HttpResponse(httpRequest, headers, content, status);

        return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
    }

    private static TmdbPersonParser PersonParser(bool cast, params TmdbDepartmentType[] departments)
    {
        return new TmdbPersonParser(new TmdbPersonSettings
        {
            PersonId = 9,
            IncludingCastCredits = cast,
            IncludeDepartmentTypes = [.. departments.Select(d => (int)d)]
        });
    }
}
