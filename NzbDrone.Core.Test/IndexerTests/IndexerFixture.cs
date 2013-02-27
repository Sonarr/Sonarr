using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Indexers;
using NzbDrone.Core.Test.ProviderTests;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerFixture : CoreTest
    {
        private void WithConfiguredIndexers()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.NzbsOrgHash).Returns("MockedConfigValue");
            Mocker.GetMock<IConfigService>().SetupGet(c => c.NzbsOrgUId).Returns("MockedConfigValue");

            Mocker.GetMock<IConfigService>().SetupGet(c => c.NzbsrusHash).Returns("MockedConfigValue");
            Mocker.GetMock<IConfigService>().SetupGet(c => c.NzbsrusUId).Returns("MockedConfigValue");

            Mocker.GetMock<IConfigService>().SetupGet(c => c.FileSharingTalkUid).Returns("MockedConfigValue");
            Mocker.GetMock<IConfigService>().SetupGet(c => c.FileSharingTalkSecret).Returns("MockedConfigValue");

            Mocker.GetMock<IConfigService>().SetupGet(c => c.OmgwtfnzbsUsername).Returns("MockedConfigValue");
            Mocker.GetMock<IConfigService>().SetupGet(c => c.OmgwtfnzbsApiKey).Returns("MockedConfigValue");
        }

        [TestCase("nzbsrus.xml")]
        [TestCase("newznab.xml")]
        [TestCase("wombles.xml")]
        [TestCase("filesharingtalk.xml")]
        [TestCase("nzbindex.xml")]
        [TestCase("nzbclub.xml")]
        [TestCase("omgwtfnzbs.xml")]
        public void parse_feed_xml(string fileName)
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", fileName));

            var fakeSettings = Builder<Indexer>.CreateNew().Build();
            Mocker.GetMock<IIndexerService>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = Mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == mockIndexer.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));
            parseResults.Should().OnlyContain(s => s.Age >= 0);
        }

        [Test]
        public void custom_parser_partial_success()
        {
            const string title = "Adventure.Inc.S03E19.DVDRip.XviD-OSiTV";
            const int season = 3;
            const int episode = 19;
            var quality = Quality.DVD;

            const string summary = "My fake summary";

            var fakeSettings = Builder<Indexer>.CreateNew().Build();
            Mocker.GetMock<IIndexerService>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .With(c => c.Summary = new TextSyndicationContent(summary))
                .Build();

            var result = Mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNotNull(result);
            Assert.AreEqual(LanguageType.Finnish, result.Language);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumbers[0]);
            Assert.AreEqual(quality, result.Quality.Quality);
        }

        [TestCase("Adventure.Inc.DVDRip.XviD-OSiTV")]
        public void custom_parser_full_parse(string title)
        {
            const string summary = "My fake summary";

            var fakeSettings = Builder<Indexer>.CreateNew().Build();
            Mocker.GetMock<IIndexerService>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .With(c => c.Summary = new TextSyndicationContent(summary))
                .Build();

            var result = Mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNotNull(result);
            Assert.AreEqual(LanguageType.Finnish, result.Language);
        }

        [TestCase("hawaii five-0 (2010)", "hawaii+five+0+2010")]
        [TestCase("this& that", "this+that")]
        [TestCase("this&    that", "this+that")]
        [TestCase("grey's anatomy", "grey+s+anatomy")]
        public void get_query_title(string raw, string clean)
        {
            var mock = new Mock<IndexerBase>();
            mock.CallBase = true;
            var result = mock.Object.GetQueryTitle(raw);
            result.Should().Be(clean);
        }

        [Test]
        public void size_nzbsrus()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "nzbsrus.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbsRUs>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1793148846);
        }

        [Test]
        public void size_newznab()
        {
            WithConfiguredIndexers();

            var newznabDefs = Builder<NewznabDefinition>.CreateListOfSize(1)
                    .All()
                    .With(n => n.ApiKey = String.Empty)
                    .Build();

            Mocker.GetMock<INewznabService>().Setup(s => s.Enabled()).Returns(newznabDefs.ToList());

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "newznab.xml"));

            //Act
            var parseResults = Mocker.Resolve<Newznab>().FetchRss();

            parseResults[0].Size.Should().Be(1183105773);
        }

        [Test]
        public void size_nzbindex()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbindex.nl/rss/alt.binaries.teevee/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=%23a.b.teevee", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "nzbindex.xml"));

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbindex.nl/rss/alt.binaries.hdtv/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "nzbindex.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbIndex>().FetchRss();

            parseResults[0].Size.Should().Be(587328389);
        }

        [Test]
        public void size_nzbclub()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=102952&st=1&ns=1&q=%23a.b.teevee", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "nzbclub.xml"));

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=5542&st=1&ns=1&q=", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "nzbclub.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbClub>().FetchRss();

            parseResults.Should().HaveCount(2);
            parseResults[0].Size.Should().Be(2652142305);
        }

        [Test]
        public void size_omgwtfnzbs()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user=MockedConfigValue&api=MockedConfigValue&eng=1", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "omgwtfnzbs.xml"));

            //Act
            var parseResults = Mocker.Resolve<Omgwtfnzbs>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(236820890);
        }

        [Test]
        public void Server_Unavailable_503_should_not_log_exception()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Throws(new WebException("503"));

            Mocker.Resolve<NzbsRUs>().FetchRss();

            ExceptionVerification.ExpectedErrors(0);
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void none_503_server_error_should_still_log_error()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Throws(new WebException("some other server error"));

            Mocker.Resolve<NzbsRUs>().FetchRss();

            ExceptionVerification.ExpectedErrors(1);
            ExceptionVerification.ExpectedWarns(0);
        }

        [Test]
        public void indexer_that_isnt_configured_shouldnt_make_an_http_call()
        {
            Mocker.Resolve<NotConfiguredIndexer>().FetchRss();

            Mocker.GetMock<HttpProvider>()
                .Verify(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void newznab_link_should_be_link_to_nzb_not_details()
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "newznab.xml"));

            var fakeSettings = Builder<Indexer>.CreateNew().Build();
            Mocker.GetMock<IIndexerService>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = Mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.NzbUrl.Contains("getnzb"));
            parseResults.Should().NotContain(s => s.NzbUrl.Contains("details"));
        }

        private static void Mark500Inconclusive()
        {
            ExceptionVerification.MarkInconclusive(typeof(WebException));
            ExceptionVerification.MarkInconclusive("System.Net.WebException");
            ExceptionVerification.MarkInconclusive("(503) Server Unavailable.");
            ExceptionVerification.MarkInconclusive("(500) Internal Server Error.");
        }

        [TestCase("wombles.xml", "de-de")]
        public void dateTime_should_parse_when_using_other_cultures(string fileName, string culture)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var fakeSettings = Builder<Indexer>.CreateNew().Build();
            Mocker.GetMock<IIndexerService>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = Mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == mockIndexer.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));
            parseResults.Should().OnlyContain(s => s.Age >= 0);

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }

        [Test]
        public void NzbsRus_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string fileName = "nzbsrus.xml";
            const string expectedString = "nzbdetails";

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var parseResults = Mocker.Resolve<NzbsRUs>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedString);
            }
        }

        [Test]
        public void Newznab_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string fileName = "newznab.xml";
            const string expectedString = "/details/";

            var newznabDefs = Builder<NewznabDefinition>.CreateListOfSize(1)
                    .All()
                    .With(n => n.ApiKey = String.Empty)
                    .Build();

            Mocker.GetMock<INewznabService>().Setup(s => s.Enabled()).Returns(newznabDefs.ToList());

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var parseResults = Mocker.Resolve<Newznab>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedString);
            }
        }

        [Test]
        public void Wombles_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string fileName = "wombles.xml";

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var parseResults = Mocker.Resolve<Wombles>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().BeNull();
            }
        }

        [Test]
        public void FileSharingTalk_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string fileName = "filesharingtalk.xml";
            const string expectedString = "/nzbs/tv";

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var parseResults = Mocker.Resolve<FileSharingTalk>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedString);
            }
        }

        [Test]
        public void NzbIndex_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string expectedString = "release";

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbindex.nl/rss/alt.binaries.teevee/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=%23a.b.teevee", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "nzbindex.xml"));

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbindex.nl/rss/alt.binaries.hdtv/?sort=agedesc&minsize=100&complete=1&max=50&more=1&q=", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "nzbindex.xml"));

            var parseResults = Mocker.Resolve<NzbIndex>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedString);
            }
        }

        [Test]
        public void NzbClub_NzbInfoUrl_should_contain_information_string()
        {
            WithConfiguredIndexers();

            const string fileName = "nzbclub.xml";
            const string expectedString = "nzb_view";

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=102952&st=1&ns=1&q=%23a.b.teevee", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=5542&st=1&ns=1&q=", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "" + fileName));

            var parseResults = Mocker.Resolve<NzbClub>().FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedString);
            }
        }

        [TestCase("30 Rock", "30%20Rock")]
        [TestCase("The Office (US)", "Office%20US")]
        [TestCase("Revenge", "Revenge")]
        [TestCase(" Top Gear ", "Top%20Gear")]
        [TestCase("Breaking   Bad", "Breaking%20Bad")]
        [TestCase("Top Chef (US)", "Top%20Chef%20US")]
        [TestCase("Castle (2009)", "Castle%202009")]
        public void newznab_GetQueryTitle_should_return_expected_result(string seriesTitle, string expected)
        {
            Mocker.Resolve<Newznab>().GetQueryTitle(seriesTitle).Should().Be(expected);
        }

        [Test]
        public void should_get_nzbInfoUrl_for_omgwtfnzbs()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream("http://rss.omgwtfnzbs.org/rss-search.php?catid=19,20&user=MockedConfigValue&api=MockedConfigValue&eng=1", It.IsAny<NetworkCredential>()))
                          .Returns(OpenRead("Files", "Rss", "SizeParsing", "omgwtfnzbs.xml"));

            //Act
            var parseResults = Mocker.Resolve<Omgwtfnzbs>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].NzbInfoUrl.Should().Be("http://omgwtfnzbs.org/details.php?id=OAl4g");
        }
    }
}