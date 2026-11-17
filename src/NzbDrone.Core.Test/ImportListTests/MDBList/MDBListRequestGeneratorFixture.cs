using System.Linq;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.MDBList;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests.MDBList
{
    [TestFixture]
    public class MDBListRequestGeneratorFixture : CoreTest
    {
        [Test]
        public void should_request_shows_with_bearer_token()
        {
            var generator = new MDBListRequestGenerator
            {
                Settings = new MDBListSettings
                {
                    ListId = "https://mdblist.com/lists/username/list-name",
                    AccessToken = "token"
                }
            };

            var requests = generator.GetListItems().GetAllTiers().First().ToList();

            requests.Should().HaveCount(1);

            var request = requests[0].HttpRequest;
            request.Url.Host.Should().Be("api.mdblist.com");
            request.Url.Path.Should().Be("/lists/username/list-name/items");
            request.Url.Query.Should().Contain("mediatype=show");
            request.Url.Query.Should().Contain("limit=1000");
            request.Headers["Authorization"].Should().Be("Bearer token");
            request.SuppressHttpErrorStatusCodes.Should().BeEquivalentTo(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound });
        }
    }
}
