using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class IndexerProviderTest
    // ReSharper disable InconsistentNaming
    {
        [Test]
        public void Download_feed_test()
        {
            var mocker = new AutoMoqer();

            var xmlReader = XmlReader.Create(File.OpenRead(".\\Files\\Rss\\nzbsorg.xml"));

            mocker.GetMock<HttpProvider>()
                .Setup(h => h.DownloadXml(It.IsAny<String>()))
                .Returns(xmlReader);

            var fakeSettings = Builder<IndexerSetting>.CreateNew().Build();
            mocker.GetMock<IndexerProvider>()
                .Setup(c => c.GetSettings(It.IsAny<Type>()))
                .Returns(fakeSettings);

            mocker.Resolve<MockIndexerProvider>().Fetch();
        }

        [Test]
        public void Init_indexer_test()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerProviderBase>() { mocker.Resolve<MockIndexerProvider>() });
            var indexers = indexerProvider.AllIndexers();

            //Assert
            Assert.Count(1, indexers);
        }
    }

    public class MockIndexerProvider : IndexerProviderBase
    {
        public MockIndexerProvider(SeriesProvider seriesProvider, SeasonProvider seasonProvider, EpisodeProvider episodeProvider, ConfigProvider configProvider, HttpProvider httpProvider, IRepository repository, IndexerProvider indexerProvider)
            : base(seriesProvider, seasonProvider, episodeProvider, configProvider, httpProvider, repository, indexerProvider)
        {
        }

        protected override string[] Url
        {
            get { return new[] { "www.google.com" }; }
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
}