using System;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    
    public class NzbxFixture : CoreTest
    {
        [Test]
        public void should_get_size_when_parsing_recent_feed()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString("https://nzbx.co/api/recent?category=tv", It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "SizeParsing", "nzbx_recent.json"));

            
            var parseResults = Mocker.Resolve<Nzbx>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(890190951);
        }

        [Test]
        public void should_get_size_when_parsing_search_results()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString("https://nzbx.co/api/search?q=30+Rock+S01E01", It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "SizeParsing", "nzbx_search.json"));

            
            var parseResults = Mocker.Resolve<Nzbx>().FetchEpisode("30 Rock", 1, 1);

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(418067671);
        }

        [Test]
        public void should_be_able_parse_results_from_recent_feed()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "nzbx_recent.json"));

            var parseResults = Mocker.Resolve<Nzbx>().FetchRss();

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == "nzbx");
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));
            parseResults.Should().OnlyContain(s => s.Age >= 0);
        }

        [Test]
        public void should_be_able_to_parse_results_from_search_results()
        {
            Mocker.GetMock<HttpProvider>()
                  .Setup(h => h.DownloadString(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                  .Returns(ReadAllText("Files", "Rss", "nzbx_search.json"));

            var parseResults = Mocker.Resolve<Nzbx>().FetchEpisode("30 Rock", 1, 1);

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == "nzbx");
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));
            parseResults.Should().OnlyContain(s => s.Age >= 0);
        }

        [Test]
        public void should_get_postedDate_when_parsing_recent_feed()
        {
            var expectedAge = DateTime.Today.Subtract(new DateTime(2012, 12, 21)).Days;

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString("https://nzbx.co/api/recent?category=tv", It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "SizeParsing", "nzbx_recent.json"));

            
            var parseResults = Mocker.Resolve<Nzbx>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Age.Should().Be(expectedAge);
        }

        [Test]
        public void should_get_postedDate_when_parsing_search_results()
        {
            var expectedAge = DateTime.Today.Subtract(new DateTime(2012, 2, 11)).Days;

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString("https://nzbx.co/api/search?q=30+Rock+S01E01", It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "SizeParsing", "nzbx_search.json"));

            
            var parseResults = Mocker.Resolve<Nzbx>().FetchEpisode("30 Rock", 1, 1);

            parseResults.Should().HaveCount(1);
            parseResults[0].Age.Should().Be(expectedAge);
        }

        [Test]
        public void should_name_nzb_properly()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadString("https://nzbx.co/api/recent?category=tv", It.IsAny<NetworkCredential>()))
                          .Returns(ReadAllText("Files", "Rss", "SizeParsing", "nzbx_recent.json"));

            
            var parseResults = Mocker.Resolve<Nzbx>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].NzbUrl.Should().EndWith(parseResults[0].OriginalString);
        }
    }
}