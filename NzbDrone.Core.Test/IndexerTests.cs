using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerTests : TestBase
    {

        [TestCase("nzbsorg.xml", 2)]
        [TestCase("nzbsrus.xml", 9)]
        [TestCase("newzbin.xml", 1)]
        [TestCase("nzbmatrix.xml", 2)]
        public void parse_feed_xml(string fileName, int warns)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var mockIndexer = mocker.Resolve<MockIndexer>();
            var parseResults = mockIndexer.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }


            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == mockIndexer.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.NzbTitle));

            ExceptionVerification.ExcpectedWarns(warns);
        }

        [Test]
        public void newzbin_parses_languae()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\newbin_none_english.xml"));

            

            var newzbin = mocker.Resolve<Newzbin>();
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
            var mocker = new AutoMoqer();
            mocker.Resolve<HttpProvider>();
            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.GetMock<ConfigProvider>()
             .SetupGet(c => c.NewzbinUsername)
             .Returns("nzbdrone");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinPassword)
                .Returns("smartar39865");

            var newzbinProvider = mocker.Resolve<Newzbin>();
            var parseResults = newzbinProvider.FetchRss();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Uri.PathAndQuery.Should().NotContain("//");
            }


            parseResults.Should().NotBeEmpty();
            parseResults.Should().OnlyContain(s => s.Indexer == newzbinProvider.Name);
            parseResults.Should().OnlyContain(s => !String.IsNullOrEmpty(s.NzbTitle));

            ExceptionVerification.IgnoreWarns();
        }


        [TestCase("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", 3, 19, QualityTypes.DVD)]
        public void custome_parser_partial_success(string title, int season, int episode, QualityTypes quality)
        {
            var mocker = new AutoMoqer();

            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .With(c => c.Summary = new TextSyndicationContent(summary))
                .Build();

            var result = mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNotNull(result);
            Assert.AreEqual(LanguageType.Finnish, result.Language);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumbers[0]);
            Assert.AreEqual(quality, result.Quality.QualityType);
        }


        [TestCase("Adventure.Inc.DVDRip.XviD-OSiTV")]
        public void custome_parser_full_parse(string title)
        {
            var mocker = new AutoMoqer();

            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .With(c => c.Summary = new TextSyndicationContent(summary))
                .Build();

            var result = mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNotNull(result);
            Assert.AreEqual(LanguageType.Finnish, result.Language);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void downloadFeed()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(new HttpProvider());

            var fakeSettings = Builder<IndexerDefinition>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.Resolve<TestUrlIndexer>().FetchRss();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("simpsons", 21, 23)]
        [TestCase("Hawaii Five-0 (2010)", 1, 5)]
        [TestCase("In plain Sight", 1, 4)]
        public void nzbsorg_search_returns_valid_results(string title, int season, int episode)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgUId)
                .Returns("43516");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgHash)
                .Returns("bc8edb4cc49d4ae440775adec5ac001f");

            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<NzbsOrg>().FetchEpisode(title, season, episode);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == Parser.NormalizeTitle(title));
            result.Should().OnlyContain(r => r.SeasonNumber == season);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(episode));
        }

        [TestCase("simpsons", 21, 23)]
        [TestCase("Hawaii Five-0 (2010)", 1, 1)]
        [TestCase("In plain Sight", 1, 11)]
        public void newzbin_search_returns_valid_results(string title, int season, int episode)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinUsername)
                .Returns("nzbdrone");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinPassword)
                .Returns("smartar39865");

            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<Newzbin>().FetchEpisode(title, season, episode);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == Parser.NormalizeTitle(title));
            result.Should().OnlyContain(r => r.SeasonNumber == season);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(episode));
        }

        [Test]
        public void nzbmatrix_search_returns_valid_results()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbMatrixUsername)
                .Returns("");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbMatrixApiKey)
                .Returns("");

            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<NzbMatrix>().FetchEpisode("Simpsons", 21, 23);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == "simpsons");
            result.Should().OnlyContain(r => r.SeasonNumber == 21);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(23));
        }

        [Test]
        public void nzbsorg_multi_word_search_returns_valid_results()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgUId)
                .Returns("43516");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgHash)
                .Returns("bc8edb4cc49d4ae440775adec5ac001f");

            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<NzbsOrg>().FetchEpisode("Blue Bloods", 1, 19);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == "bluebloods");
            result.Should().OnlyContain(r => r.SeasonNumber == 1);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(19));

        }

        [Test]
        public void nzbmatrix_multi_word_search_returns_valid_results()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbMatrixUsername)
                .Returns("");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbMatrixApiKey)
                .Returns("");

            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<NzbMatrix>().FetchEpisode("Blue Bloods", 1, 19);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == "bluebloods");
            result.Should().OnlyContain(r => r.SeasonNumber == 1);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(19));
        }


        [TestCase("hawaii five-0 (2010)", "hawaii+five+0+2010")]
        [TestCase("this& that", "this+that")]
        [TestCase("this&    that", "this+that")]
        public void get_query_title(string raw, string clean)
        {
            var result = IndexerBase.GetQueryTitle(raw);

            result.Should().Be(clean);
        }

        [Test]
        public void size_newzbin()
        {
            //Setup
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\newzbin.xml"));

            //Act
            var parseResults = mocker.Resolve<Newzbin>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1295620506);
        }

        [Test]
        public void size_nzbmatrix()
        {
            //Setup
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbmatrix.xml"));

            //Act
            var parseResults = mocker.Resolve<NzbMatrix>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1331439862);
        }

        [Test]
        public void size_nzbsorg()
        {
            //Setup
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbsorg.xml"));

            //Act
            var parseResults = mocker.Resolve<NzbsOrg>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1793148846);
        }

        [Test]
        public void size_nzbsrus()
        {
            //Setup
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\SizeParsing\\nzbsrus.xml"));

            //Act
            var parseResults = mocker.Resolve<NzbsRUs>().FetchRss();

            parseResults.Should().HaveCount(1);
            parseResults[0].Size.Should().Be(1793148846);
        }
    }
}
