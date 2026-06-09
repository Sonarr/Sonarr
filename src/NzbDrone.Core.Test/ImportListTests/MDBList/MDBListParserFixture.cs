using System.Linq;
using System.Net;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    public class MDBListParserFixture : CoreTest<MDBListParser>
    {
        private static ImportListResponse CreateResponse(string content)
        {
            var httpRequest = new HttpRequest("https://api.mdblist.com/lists/fuzi0n/tv-shows/items", HttpAccept.Json);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content), HttpStatusCode.OK);

            return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
        }

        [Test]
        public void should_parse_api_show_items()
        {
            var json = ReadAllText("Files/mdblist_list_items.json");

            var result = Subject.ParseResponse(CreateResponse(json));

            result.Should().HaveCount(2);

            result.First().Title.Should().Be("Scam 1992: The Harshad Mehta Story");
            result.First().TvdbId.Should().Be(389680);
            result.First().TmdbId.Should().Be(90823);
            result.First().ImdbId.Should().Be("tt12392504");
            result.First().Year.Should().Be(2020);
        }

        [Test]
        public void should_parse_direct_list_array_items()
        {
            var json = ReadAllText("Files/mdblist_direct_list.json");

            var result = Subject.ParseResponse(CreateResponse(json));

            result.Should().HaveCount(1);

            result.First().Title.Should().Be("Panchayat");
            result.First().TvdbId.Should().Be(379446);
            result.First().ImdbId.Should().Be("tt12004706");
        }
    }
}
