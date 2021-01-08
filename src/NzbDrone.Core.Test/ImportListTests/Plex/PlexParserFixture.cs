using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Plex;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportList.Plex
{
    public class PlexTest : CoreTest<PlexParser>
    {
        private ImportListResponse CreateResponse(string url, string content)
        {
            var httpRequest = new HttpRequest(url);
            var httpResponse = new HttpResponse(httpRequest, new HttpHeader(), Encoding.UTF8.GetBytes(content));

            return new ImportListResponse(new ImportListRequest(httpRequest), httpResponse);
        }

        [Test]
        public void should_parse_plex_watchlist()
        {
            var json = ReadAllText("Files/plex_watchlist.json");

            var result = Subject.ParseResponse(CreateResponse("https://metadata.provider.plex.tv/library/sections/watchlist/all", json));

            result.First().Title.Should().Be("30 Rock");
            result.First().Year.Should().Be(2006);
            result.First().TvdbId.Should().Be(79488);

            result[1].TmdbId.Should().Be(4533);
            result[1].ImdbId.Should().Be("tt0475900");
        }
    }
}
