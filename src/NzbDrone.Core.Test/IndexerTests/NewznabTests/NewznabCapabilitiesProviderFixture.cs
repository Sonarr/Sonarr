using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    [TestFixture]
    public class NewznabCapabilitiesProviderFixture : CoreTest<NewznabCapabilitiesProvider>
    {
        private NewznabSettings _settings;
        private string _caps;

        [SetUp]
        public void SetUp()
        {
            _settings = new NewznabSettings()
            {
                Url = "http://indxer.local"
            };

            _caps = ReadAllText("Files", "Indexers", "Newznab", "newznab_caps.xml");
        }

        private void GivenCapsResponse(string caps)
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Get(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), caps));
        }

        [Test]
        public void should_not_request_same_caps_twice()
        {
            GivenCapsResponse(_caps);

            Subject.GetCapabilities(_settings);
            Subject.GetCapabilities(_settings);

            Mocker.GetMock<IHttpClient>()
                .Verify(o => o.Get(It.IsAny<HttpRequest>()), Times.Once());
        }

        [Test]
        public void should_report_pagesize()
        {
            GivenCapsResponse(_caps);

            var caps = Subject.GetCapabilities(_settings);

            caps.DefaultPageSize.Should().Be(25);
            caps.MaxPageSize.Should().Be(60);
        }

        [Test]
        public void should_use_default_pagesize_if_missing()
        {
            GivenCapsResponse(_caps.Replace("<limits", "<abclimits"));

            var caps = Subject.GetCapabilities(_settings);

            caps.DefaultPageSize.Should().Be(100);
            caps.MaxPageSize.Should().Be(100);
        }
    }
}
