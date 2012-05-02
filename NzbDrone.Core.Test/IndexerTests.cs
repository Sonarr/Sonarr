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
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.ProviderTests;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerTests : CoreTest
    {
        [TestCase("nzbsorg.xml")]
        [TestCase("nzbsrus.xml")]
        [TestCase("newzbin.xml")]
        [TestCase("nzbmatrix.xml")]
        [TestCase("newznab.xml")]
        [TestCase("wombles.xml")]
        [TestCase("filesharingtalk.xml")]
        [TestCase("nzbindex.xml")]
        [TestCase("nzbclub.xml")]
        public void parse_feed_xml(string fileName)
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
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

        private void WithConfiguredIndexers()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbMatrixApiKey).Returns("MockedConfigValue");
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbMatrixUsername).Returns("MockedConfigValue");

            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NewzbinUsername).Returns("MockedConfigValue");
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NewzbinPassword).Returns("MockedConfigValue");

            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbsOrgHash).Returns("MockedConfigValue");
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbsOrgUId).Returns("MockedConfigValue");

            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbsrusHash).Returns("MockedConfigValue");
            Mocker.GetMock<ConfigProvider>().SetupGet(c => c.NzbsrusUId).Returns("MockedConfigValue");
        }

        [Test]
        public void newzbin_parses_languae()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\newbin_none_english.xml"));



            var newzbin = Mocker.Resolve<Newzbin>();
            var parseResults = newzbin.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }


            parseResults.Should().NotBeEmpty();
            parseResults.Should().NotContain(e => e.Language == LanguageType.English);
        }

        [Test]
        public void newzbin_rss_fetch()
        {
            Mocker.Resolve<HttpProvider>();
            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            Mocker.GetMock<ConfigProvider>()
             .SetupGet(c => c.NewzbinUsername)
             .Returns("nzbdrone");

            Mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinPassword)
                .Returns("smartar39865");

            var newzbinProvider = Mocker.Resolve<Newzbin>();
            var parseResults = newzbinProvider.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }


            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == newzbinProvider.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));

            Mark500Inconclusive();
            ExceptionVerification.IgnoreWarns();
        }


        [TestCase("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", 3, 19, QualityTypes.DVD)]
        public void custome_parser_partial_success(string title, int season, int episode, QualityTypes quality)
        {
            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
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
            Assert.AreEqual(quality, result.Quality.QualityType);
        }


        [TestCase("Adventure.Inc.DVDRip.XviD-OSiTV")]
        public void custome_parser_full_parse(string title)
        {
            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
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

        [Test]
        public void downloadFeed()
        {
            Mocker.Resolve<HttpProvider>();

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            Mocker.Resolve<TestUrlIndexer>().FetchRss();

            Mark500Inconclusive();
            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("simpsons", 21, 23)]
        [TestCase("Hawaii Five-0 (2010)", 1, 5)]
        [TestCase("In plain Sight", 1, 4)]
        public void nzbsorg_search_returns_valid_results(string title, int season, int episode)
        {
            Mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgUId)
                .Returns("43516");

            Mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgHash)
                .Returns("bc8edb4cc49d4ae440775adec5ac001f");

            Mocker.Resolve<HttpProvider>();

            var result = Mocker.Resolve<NzbsOrg>().FetchEpisode(title, season, episode);

            Mark500Inconclusive();

            result.Should().NotBeEmpty();
        }

        [TestCase("simpsons", 21, 23)]
        [TestCase("Hawaii Five-0 (2010)", 1, 1)]
        [TestCase("In plain Sight", 1, 11, Ignore = true)]
        public void newzbin_search_returns_valid_results(string title, int season, int episode)
        {
            Mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinUsername)
                .Returns("nzbdrone");

            Mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinPassword)
                .Returns("smartar39865");

            Mocker.Resolve<HttpProvider>();

            var result = Mocker.Resolve<Newzbin>().FetchEpisode(title, season, episode);

            Mark500Inconclusive();
            result.Should().NotBeEmpty();
        }

        [TestCase("simpsons", 21, 23)]
        [TestCase("The walking dead", 2, 10)]
        public void nzbmatrix_search_returns_valid_results(string title, int season, int episode)
        {
            WithConfiguredIndexers();


            Mocker.Resolve<HttpProvider>();

            var result = Mocker.Resolve<NzbMatrix>().FetchEpisode(title, season, episode);

            Mark500Inconclusive();

            result.Should().NotBeEmpty();
        }

        [Test]
        public void nzbmatrix_multi_word_search_returns_valid_results()
        {
            WithConfiguredIndexers();

            Mocker.Resolve<HttpProvider>();

            var result = Mocker.Resolve<NzbMatrix>().FetchEpisode("Blue Bloods", 1, 19);

            Mark500Inconclusive();

            result.Should().NotBeEmpty();
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

        [TestCase("hawaii five-0 (2010)", "hawaii+five+0+2010")]
        [TestCase("this& that", "this+that")]
        [TestCase("this&    that", "this+that")]
        [TestCase("grey's anatomy", "greys+anatomy")]
        public void get_query_title_nzbmatrix_should_replace_apostrophe_with_empty_string(string raw, string clean)
        {
            var result = Mocker.Resolve<NzbMatrix>().GetQueryTitle(raw);
            result.Should().Be(clean);
        }

        [Test]
        public void size_newzbin()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\newzbin.xml"));

            //Act
            var parseResults = Mocker.Resolve<Newzbin>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1295620506);
        }

        [Test]
        public void size_nzbmatrix()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbmatrix.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbMatrix>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1331439862);
        }

        [Test]
        public void size_nzbsorg()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbsorg.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbsOrg>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1793148846);
        }

        [Test]
        public void size_nzbsrus()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbsrus.xml"));

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

            Mocker.GetMock<NewznabProvider>().Setup(s => s.Enabled()).Returns(newznabDefs.ToList());

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\newznab.xml"));

            //Act
            var parseResults = Mocker.Resolve<Newznab>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1183105773);
        }

        [Test]
        public void size_nzbindex()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbindex.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbIndex>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(587328389);
        }

        [Test]
        public void size_nzbclub()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbclub.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbClub>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(2652142305);
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
                          .Returns(File.OpenRead(".\\Files\\Rss\\newznab.xml"));

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = Mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.NzbUrl.Contains("getnzb"));
            parseResults.Should().NotContain(s => s.NzbUrl.Contains("details"));
        }

        [Test]
        public void nzbmatrix_should_use_age_from_custom()
        {
            WithConfiguredIndexers();

            var expectedAge = DateTime.Now.Subtract(new DateTime(2011, 4, 25, 15, 6, 58)).Days;

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbmatrix.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbMatrix>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Age.Should().Be(expectedAge);
        }

        private static void Mark500Inconclusive()
        {
            ExceptionVerification.MarkInconclusive(typeof(WebException));
            ExceptionVerification.MarkInconclusive("System.Net.WebException");
            ExceptionVerification.MarkInconclusive("(503) Server Unavailable.");
            ExceptionVerification.MarkInconclusive("(500) Internal Server Error.");
        }

        [Test]
        public void title_preparse_nzbindex()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbindex.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbIndex>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].CleanTitle.Should().Be("britainsgotmoretalent");
        }

        [Test]
        public void title_preparse_nzbclub()
        {
            WithConfiguredIndexers();

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbclub.xml"));

            //Act
            var parseResults = Mocker.Resolve<NzbIndex>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].CleanTitle.Should().Be("britainsgottalent");
        }

        [TestCase("wombles.xml", "de-de")]
        public void dateTime_should_parse_when_using_other_cultures(string fileName, string culture)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
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

        [TestCase("nzbsorg.xml", "info")]
        [TestCase("nzbsrus.xml", "info")]
        [TestCase("newzbin.xml", "info")]
        [TestCase("nzbmatrix.xml", "info")]
        [TestCase("newznab.xml", "info")]
        [TestCase("wombles.xml", "info")]
        [TestCase("filesharingtalk.xml", "info")]
        [TestCase("nzbindex.xml", "info")]
        [TestCase("nzbclub.xml", "info")]
        public void NzbInfoUrl_should_contain_information_string(string fileName, string expectedContent)
        {
            Mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            Mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = Mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                episodeParseResult.NzbInfoUrl.Should().Contain(expectedContent);
            }
            
            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == mockIndexer.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.OriginalString));
            parseResults.Should().OnlyContain(s => s.Age >= 0);
        }
    }
}
