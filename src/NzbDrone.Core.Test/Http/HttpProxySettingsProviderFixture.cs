using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Core.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Http
{
    [TestFixture]
    public class HttpProxySettingsProviderFixture : TestBase<HttpProxySettingsProvider>
    {
        private HttpProxySettings GetProxySettings()
        {
            return new HttpProxySettings(ProxyType.Socks5, "localhost", 8080, "*.httpbin.org,google.com,172.16.0.0/12", true, null, null);
        }

        [TestCase("http://eu.httpbin.org/get")]
        [TestCase("http://google.com/get")]
        [TestCase("http://localhost:8654/get")]
        [TestCase("http://172.21.0.1:8989/api/v3/indexer/schema")]
        public void should_bypass_proxy(string url)
        {
            var settings = GetProxySettings();

            Subject.ShouldProxyBeBypassed(settings, new HttpUri(url)).Should().BeTrue();
        }

        [TestCase("http://bing.com/get")]
        [TestCase("http://172.3.0.1:8989/api/v3/indexer/schema")]
        public void should_not_bypass_proxy(string url)
        {
            var settings = GetProxySettings();

            Subject.ShouldProxyBeBypassed(settings, new HttpUri(url)).Should().BeFalse();
        }
    }
}
