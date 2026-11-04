using System.Net;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    [TestFixture]
    public class MDBListParserFixture : CoreTest<MDBListParser>
    {
        private ImportListResponse CreateResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var httpRequest = new HttpRequest("https://api.mdblist.com/lists/123/items?mediatype=show&limit=1000");
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content), statusCode);

            return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
        }

        [Test]
        public void should_parse_shows_and_skip_items_without_ids()
        {
            var json = ReadAllText("Files/mdblist_list_items.json");

            var result = Subject.ParseResponse(CreateResponse(json));

            result.Should().HaveCount(2);

            result[0].Title.Should().Be("Breaking Bad");
            result[0].Year.Should().Be(2008);
            result[0].TvdbId.Should().Be(81189);
            result[0].TmdbId.Should().Be(1396);
            result[0].ImdbId.Should().Be("tt0903747");

            result[1].TvdbId.Should().Be(0);
            result[1].TmdbId.Should().Be(99999);
            result[1].ImdbId.Should().Be("tt1234567");

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_return_empty_when_list_has_no_shows()
        {
            var result = Subject.ParseResponse(CreateResponse("{\"movies\": [], \"shows\": []}"));

            result.Should().BeEmpty();
        }

        [TestCase(HttpStatusCode.Unauthorized, "authenticate with MDBList again")]
        [TestCase(HttpStatusCode.NotFound, "list not found")]
        [TestCase(HttpStatusCode.InternalServerError, "unexpected StatusCode")]
        public void should_explain_error_status_codes(HttpStatusCode statusCode, string message)
        {
            var ex = Assert.Throws<ImportListException>(() => Subject.ParseResponse(CreateResponse("{\"error\": \"x\"}", statusCode)));

            ex.Message.Should().Contain(message);
        }

        [Test]
        public void should_include_api_error_when_forbidden()
        {
            var ex = Assert.Throws<ImportListException>(() => Subject.ParseResponse(CreateResponse("{\"error\": \"Supporter account required\"}", HttpStatusCode.Forbidden)));

            ex.Message.Should().Contain("Supporter account required");
        }
    }
}
