using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
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
        [Test]
        [TestCase("nzbsorg.xml", 0)]
        [TestCase("nzbsrus.xml", 6)]
        [TestCase("newzbin.xml", 1)]
        [TestCase("nzbmatrix.xml", 1)]
        public void parse_feed_xml(string fileName, int warns)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
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
        public void newzbin_rss_fetch()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\newzbin.xml"));

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

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


            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        [TestCase("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", 3, 19, QualityTypes.DVD)]
        public void custome_parser_partial_success(string title, int season, int episode, QualityTypes quality)
        {
            var mocker = new AutoMoqer();

            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
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


        [Test]
        [TestCase("Adventure.Inc.DVDRip.XviD-OSiTV")]
        public void custome_parser_full_parse(string title)
        {
            var mocker = new AutoMoqer();

            const string summary = "My fake summary";

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
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

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.Resolve<TestUrlIndexer>().FetchRss();

            ExceptionVerification.IgnoreWarns();
        }


        [Test]
        public void nzbsorg_search_returns_valid_results()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgUId)
                .Returns("43516");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NzbsOrgHash)
                .Returns("bc8edb4cc49d4ae440775adec5ac001f");


            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<NzbsOrg>().FetchEpisode("Simpsons", 21, 23);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(r => r.CleanTitle == "simpsons");
            result.Should().OnlyContain(r => r.SeasonNumber == 21);
            result.Should().OnlyContain(r => r.EpisodeNumbers.Contains(23));
        }


        [Test]
        public void newzbin_search_returns_valid_results()
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinUsername)
                .Returns("nzbdrone");

            mocker.GetMock<ConfigProvider>()
                .SetupGet(c => c.NewzbinPassword)
                .Returns("smartar39865");


            mocker.Resolve<HttpProvider>();

            var result = mocker.Resolve<Newzbin>().FetchEpisode("Simpsons", 21, 23);

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
    }
}
