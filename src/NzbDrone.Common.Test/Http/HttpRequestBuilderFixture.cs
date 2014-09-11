using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpRequestBuilderFixture : TestBase
    {
        [Test]
        public void should_remove_duplicated_slashes()
        {
            var builder = new HttpRequestBuilder("http://domain/");

            var request = builder.Build("/v1/");

            request.Url.ToString().Should().Be("http://domain/v1/");

        }
    }
}