using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class HttpRateLimitKeyFactoryFixture
    {
        [TestCase("http://127.0.0.2:9117/jackett/api/v2.0/indexers/viva/results/torznab/api?t=search&cat=5000,5070,100030,100041", "127.0.0.2:9117/jackett/api/v2.0/indexers/viva")]
        public void should_detect_jackett(string url, string expectedKey)
        {
            var request = new HttpRequest(url);

            var key = HttpRateLimitKeyFactory.GetRateLimitKey(request);

            key.Should().Be(expectedKey);
        }

        [TestCase("http://127.0.0.2:9117/jackett", "127.0.0.2")]
        public void should_default_to_host(string url, string expectedKey)
        {
            var request = new HttpRequest(url);

            var key = HttpRateLimitKeyFactory.GetRateLimitKey(request);

            key.Should().Be(expectedKey);
        }
    }
}