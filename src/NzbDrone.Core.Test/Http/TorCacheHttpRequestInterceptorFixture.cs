using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Http
{
    [TestFixture]
    public class TorCacheHttpRequestInterceptorFixture : TestBase<TorCacheHttpRequestInterceptor>
    {
        [Test]
        public void should_remove_query_params_from_torcache_request()
        {
            var request = new HttpRequest("http://torcache.net/download/123.torrent?title=something");

            var newRequest = Subject.PreRequest(request);

            newRequest.Url.FullUri.Should().Be("http://torcache.net/download/123.torrent");
        }

        [Test]
        public void should_add_referrer_torcache_request()
        {
            var request = new HttpRequest("http://torcache.net/download/123.torrent?title=something");

            var newRequest = Subject.PreRequest(request);

            newRequest.Headers.Should().Contain("Referer", "http://torcache.net/");
        }

        [TestCase("http://site.com/download?url=torcache.net&blaat=1")]
        [TestCase("http://torcache.net.com/download?url=123")]
        public void should_not_remove_query_params_from_other_requests(string url)
        {
            var request = new HttpRequest(url);

            var newRequest = Subject.PreRequest(request);

            newRequest.Url.FullUri.Should().Be(url);
        }
    }
}
