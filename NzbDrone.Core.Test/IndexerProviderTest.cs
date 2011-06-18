// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.ExternalNotification;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using FluentAssertions;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerProviderTest : TestBase
    {
        [Test]
        public void Init_indexer_test()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { mocker.Resolve<MockIndexer>() });
            var settings = indexerProvider.GetSettings(typeof(MockIndexer));
            settings.Enable = true;
            indexerProvider.SaveSettings(settings);

            //Assert
            indexerProvider.GetAllISettings();


            indexerProvider.GetAllISettings().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().HaveCount(1);
        }

        [Test]
        public void Init_indexer_with_disabled_job()
        {
            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());

            //Act
            var indexerProvider = mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { mocker.Resolve<MockIndexer>() });
            var settings = indexerProvider.GetSettings(typeof(MockIndexer));
            settings.Enable = false;
            indexerProvider.SaveSettings(settings);

            //Assert

            indexerProvider.GetAllISettings().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().BeEmpty();
        }
    }

    public class MockIndexer : IndexerBase
    {
        public MockIndexer(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get { return new[] { "www.google.com" }; }
        }

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
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
        public TestUrlIndexer(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
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

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://google.com";
        }
    }

    public class CustomParserIndexer : IndexerBase
    {
        public CustomParserIndexer(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
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

        protected override IList<string> GetSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://www.google.com";
        }

        protected override Model.EpisodeParseResult CustomParser(SyndicationItem item, Model.EpisodeParseResult currentResult)
        {
            if (currentResult == null) currentResult = new EpisodeParseResult();
            currentResult.Language = LanguageType.Finnish;
            return currentResult;
        }
    }

}