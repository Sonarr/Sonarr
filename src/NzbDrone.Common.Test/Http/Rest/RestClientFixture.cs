using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http.Rest
{
    [TestFixture]
    public class RestClientFixture : TestBase<HttpClient>
    {
        [Test]
        public void should_execute_simple_get()
        {
            var request = new HttpRequest("http://httpbin.org/get");

            var response = Subject.Exetcute(request);

            response.Content.Should().NotBeNullOrWhiteSpace();
        }
    }
}