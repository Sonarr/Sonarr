using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class IndexerProviderTest
    // ReSharper disable InconsistentNaming
    {
        [Test]
        [Row("nzbsorg.xml")]
        [Row("nzbsrus.xml")]
        [Row("newzbin.xml")]
        [Row("nzbmatrix.xml")]
        public void parse_feed_xml(string fileName)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<HttpProvider>()
                          .Setup(h => h.DownloadStream(It.IsAny<String>(), It.IsAny<NetworkCredential>()))
                          .Returns(File.OpenRead(".\\Files\\Rss\\" + fileName));

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            var exceptions = mocker.Resolve<MockIndexerProvider>().Fetch();

            foreach (var exception in exceptions)
            {
                Console.WriteLine(exception.ToString());
            }

            Assert.IsEmpty(exceptions);
        }



        [Test]
        [Row("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", 3, 19, QualityTypes.DVD)]
        public void parse_feed_test_success(string title, int season, int episode, QualityTypes quality)
        {
            var mocker = new AutoMoqer();

            var summary = "My fake summary";

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.FindSeries(It.IsAny<String>()))
                .Returns(Builder<Series>.CreateNew().Build());


            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .With(c => c.Summary = new TextSyndicationContent(summary))
                .Build();

            var result = mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNotNull(result);
            Assert.AreEqual(summary, result.EpisodeTitle);
        }

        [Test]
        [Row("Adventure.Inc.DVDRip.XviD-OSiTV")]
        public void parse_feed_test_fail(string title)
        {
            var mocker = new AutoMoqer();



            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.GetMock<SeriesProvider>(MockBehavior.Strict);


            var fakeRssItem = Builder<SyndicationItem>.CreateNew()
                .With(c => c.Title = new TextSyndicationContent(title))
                .Build();

            var result = mocker.Resolve<CustomParserIndexer>().ParseFeed(fakeRssItem);

            Assert.IsNull(result);
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
        }

        [Test]
        public void Init_indexer_test()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerProviderBase>() { mocker.Resolve<MockIndexerProvider>() });
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

            var indexer = mocker.Resolve<MockIndexerProvider>();
            indexer.ProcessItem(new SyndicationItem { Title = new TextSyndicationContent("Adventure.Inc.S01E18.DVDRip.XviD-OSiTV") });
        }
    }

    public class MockIndexerProvider : IndexerProviderBase
    {
        public MockIndexerProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            EpisodeProvider episodeProvider, ConfigProvider configProvider,
            HttpProvider httpProvider, IndexerProvider indexerProvider,
            HistoryProvider historyProvider, SabProvider sabProvider, IEnumerable<ExternalNotificationProviderBase> externalNotificationProvider)
            : base(seriesProvider, seasonProvider, episodeProvider,
            configProvider, httpProvider, indexerProvider, historyProvider,
            sabProvider, externalNotificationProvider)
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

        public override bool SupportsBacklog
        {
            get { return false; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }
    }

    public class TestUrlIndexer : IndexerProviderBase
    {
        public TestUrlIndexer(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            EpisodeProvider episodeProvider, ConfigProvider configProvider,
            HttpProvider httpProvider, IndexerProvider indexerProvider,
            HistoryProvider historyProvider, SabProvider sabProvider, IEnumerable<ExternalNotificationProviderBase> externalNotificationProvider)
            : base(seriesProvider, seasonProvider, episodeProvider,
            configProvider, httpProvider, indexerProvider, historyProvider,
            sabProvider, externalNotificationProvider)
        {
        }

        public override string Name
        {
            get { return "All Urls"; }
        }

        public override bool SupportsBacklog
        {
            get { return false; }
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

    public class CustomParserIndexer : IndexerProviderBase
    {
        public CustomParserIndexer(SeriesProvider seriesProvider, SeasonProvider seasonProvider,
            EpisodeProvider episodeProvider, ConfigProvider configProvider,
            HttpProvider httpProvider, IndexerProvider indexerProvider,
            HistoryProvider historyProvider, SabProvider sabProvider, IEnumerable<ExternalNotificationProviderBase> externalNotificationProvider)
            : base(seriesProvider, seasonProvider, episodeProvider,
            configProvider, httpProvider, indexerProvider, historyProvider,
            sabProvider, externalNotificationProvider)
        {
        }

        public override string Name
        {
            get { return "Custom parser"; }
        }

        public override bool SupportsBacklog
        {
            get { return false; }
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
            currentResult.EpisodeTitle = item.Summary.Text;
            return currentResult;
        }
    }

}