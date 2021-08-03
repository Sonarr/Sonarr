using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.TorrentRssIndexerTests
{
    [TestFixture]
    public class TorrentRssParserFactoryFixture : CoreTest<TorrentRssParserFactory>
    {
        private TorrentRssIndexerSettings _indexerSettings1;
        private TorrentRssIndexerSettings _indexerSettings2;
        private TorrentRssIndexerSettings _indexerSettings3;

        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());

            _indexerSettings1 = new TorrentRssIndexerSettings
            {
                BaseUrl = "http://my.indexer.com/"
            };

            _indexerSettings2 = new TorrentRssIndexerSettings
            {
                BaseUrl = "http://my.other.indexer.com/"
            };

            _indexerSettings3 = new TorrentRssIndexerSettings
            {
                BaseUrl = "http://my.indexer.com/",
                AllowZeroSize = true
            };
        }

        private void GivenSuccessful(TorrentRssIndexerParserSettings parserSettings = null)
        {
            if (parserSettings == null)
            {
                parserSettings = new TorrentRssIndexerParserSettings
                {
                    UseEnclosureLength = true,
                    ParseSizeInDescription = false
                };
            }

            Mocker.GetMock<ITorrentRssSettingsDetector>()
                .Setup(v => v.Detect(It.IsAny<TorrentRssIndexerSettings>()))
                .Returns(parserSettings);
        }

        private void GivenFailed()
        {
            Mocker.GetMock<ITorrentRssSettingsDetector>()
                .Setup(v => v.Detect(It.IsAny<TorrentRssIndexerSettings>()))
                .Returns((TorrentRssIndexerParserSettings)null);
        }

        private void VerifyDetectionCount(int count)
        {
            Mocker.GetMock<ITorrentRssSettingsDetector>()
                .Verify(v => v.Detect(It.IsAny<TorrentRssIndexerSettings>()), Times.Exactly(count));
        }

        [Test]
        public void should_return_ezrssparser()
        {
            GivenSuccessful(new TorrentRssIndexerParserSettings
            {
                UseEZTVFormat = true
            });

            var parser = Subject.GetParser(_indexerSettings1);

            parser.Should().BeOfType<EzrssTorrentRssParser>();
        }

        [Test]
        public void should_return_generic_torrentrssparser()
        {
            GivenSuccessful(new TorrentRssIndexerParserSettings
            {
                ParseSeedersInDescription = true,
                ParseSizeInDescription = true,
                SizeElementName = "Hello"
            });

            var parser = Subject.GetParser(_indexerSettings1);

            parser.Should().BeOfType<TorrentRssParser>();

            var rssParser = parser as TorrentRssParser;

            rssParser.ParseSeedersInDescription.Should().BeTrue();
            rssParser.ParseSizeInDescription.Should().BeTrue();
            rssParser.SizeElementName.Should().Be("Hello");
        }

        [Test]
        public void should_throw_on_failure()
        {
            GivenFailed();

            Assert.Throws<UnsupportedFeedException>(() => Subject.GetParser(_indexerSettings1));
        }

        [Test]
        public void should_cache_settings_for_same_baseurl()
        {
            GivenSuccessful();

            var detection1 = Subject.GetParser(_indexerSettings1);

            var detection2 = Subject.GetParser(_indexerSettings1);

            detection1.Should().BeEquivalentTo(detection2);

            VerifyDetectionCount(1);
        }

        [Test]
        public void should_not_cache_failure()
        {
            GivenFailed();

            Assert.Throws<UnsupportedFeedException>(() => Subject.GetParser(_indexerSettings1));

            GivenSuccessful();

            Subject.GetParser(_indexerSettings1);

            VerifyDetectionCount(2);
        }

        [Test]
        public void should_not_cache_settings_for_different_baseurl()
        {
            GivenSuccessful();

            var detection1 = Subject.GetParser(_indexerSettings1);

            var detection2 = Subject.GetParser(_indexerSettings2);

            VerifyDetectionCount(2);
        }

        [Test]
        public void should_not_cache_settings_for_different_settings()
        {
            GivenSuccessful();

            var detection1 = Subject.GetParser(_indexerSettings1);

            var detection2 = Subject.GetParser(_indexerSettings3);

            VerifyDetectionCount(2);
        }
    }
}
