// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerProviderTest : TestBase
    {
        [Test]
        [Row("nzbsorg.xml", 0)]
        [Row("nzbsrus.xml", 6)]
        [Row("newzbin.xml", 1)]
        [Row("nzbmatrix.xml", 1)]
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
            var parseResults = mockIndexer.Fetch();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Assert.DoesNotContain(Uri.PathAndQuery, "//");
            }


            Assert.IsNotEmpty(parseResults);

            Assert.ForAll(parseResults, s => Assert.AreEqual(mockIndexer.Name, s.Indexer));
            Assert.ForAll(parseResults, s => Assert.AreNotEqual("", s.NzbTitle));
            Assert.ForAll(parseResults, s => Assert.AreNotEqual(null, s.NzbTitle));

            ExceptionVerification.ExcpectedWarns(warns);
        }



        [Test]
        public void newzbin()
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
            var parseResults = newzbinProvider.Fetch();

            foreach (var episodeParseResult in parseResults)
            {
                var Uri = new Uri(episodeParseResult.NzbUrl);
                Assert.DoesNotContain(Uri.PathAndQuery, "//");
            }


            Assert.IsNotEmpty(parseResults);
            Assert.ForAll(parseResults, s => Assert.AreEqual(newzbinProvider.Name, s.Indexer));
            Assert.ForAll(parseResults, s => Assert.AreNotEqual("", s.NzbTitle));
            Assert.ForAll(parseResults, s => Assert.AreNotEqual(null, s.NzbTitle));

            ExceptionVerification.ExcpectedWarns(1);
        }






        [Test]
        [Row("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", 3, 19, QualityTypes.DVD)]
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
            Assert.AreEqual(summary, result.EpisodeTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumbers[0]);
            Assert.AreEqual(quality, result.Quality);
        }

        [Test]
        [Row("Adventure.Inc.DVDRip.XviD-OSiTV")]
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
            Assert.AreEqual(summary, result.EpisodeTitle);
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

            mocker.Resolve<TestUrlIndexer>().Fetch();

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void Init_indexer_test()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { mocker.Resolve<MockIndexer>() });
            var indexers = indexerProvider.All();

            //Assert
            Assert.Count(1, indexers);
        }

        [Test]
        public void unmapped_series_shouldnt_call_any_providers()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries(It.IsAny<String>()))
                .Returns<Series>(null);

            var indexer = mocker.Resolve<MockIndexer>();
            //indexer.ProcessItem(new SyndicationItem { Title = new TextSyndicationContent("Adventure.Inc.S01E18.DVDRip.XviD-OSiTV") });
        }
    }

    public class MockIndexer : IndexerBase
    {
        public MockIndexer(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider)
            : base(httpProvider, configProvider, indexerProvider)
        {
        }

        protected override string[] Urls
        {
            get { return new[] { "www.google.com" }; }
        }

        protected override NetworkCredential Credentials
        {
            get { return null; }
        }

        public override string Name
        {
            get { return "Mocked Indexer"; }
        }


        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }
    }

    public class TestUrlIndexer : IndexerBase
    {
        public TestUrlIndexer(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider)
            : base(httpProvider, configProvider, indexerProvider)
        {
        }

        public override string Name
        {
            get { return "All Urls"; }
        }

        protected override string[] Urls
        {
            get { return new[] { "http://rss.nzbmatrix.com/rss.php?cat=TV" }; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://google.com";
        }
    }

    public class CustomParserIndexer : IndexerBase
    {
        public CustomParserIndexer(HttpProvider httpProvider, ConfigProvider configProvider, IndexerProvider indexerProvider)
            : base(httpProvider, configProvider, indexerProvider)
        {
        }

        public override string Name
        {
            get { return "Custom parser"; }
        }



        protected override string[] Urls
        {
            get { return new[] { "http://www.google.com" }; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://www.google.com";
        }

        protected override Model.EpisodeParseResult CustomParser(SyndicationItem item, Model.EpisodeParseResult currentResult)
        {
            if (currentResult == null) currentResult = new EpisodeParseResult();
            currentResult.EpisodeTitle = item.Summary.Text;
            return currentResult;
        }
    }

}