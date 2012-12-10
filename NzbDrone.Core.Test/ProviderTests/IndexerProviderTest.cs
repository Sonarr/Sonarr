// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;

using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerProviderTest : CoreTest
    {
        [Test]
        public void Init_indexer_test()
        {


            Mocker.SetConstant(TestDbHelper.GetEmptyDatabase());

            //Act
            var indexerProvider = Mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });
            var settings = indexerProvider.GetSettings(typeof(MockIndexer));
            settings.Enable = true;
            indexerProvider.SaveSettings(settings);

            //Assert
            indexerProvider.All();


            indexerProvider.All().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().HaveCount(1);
        }

        [Test]
        public void Init_indexer_with_disabled_job()
        {
            Mocker.SetConstant(TestDbHelper.GetEmptyDatabase());

            //Act
            var indexerProvider = Mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });
            var settings = indexerProvider.GetSettings(typeof(MockIndexer));
            settings.Enable = false;
            indexerProvider.SaveSettings(settings);

            //Assert

            indexerProvider.All().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().BeEmpty();
        }

        [Test]
        public void Init_indexer_should_enable_indexer_that_is_enabled_by_default()
        {
            Mocker.SetConstant(TestDbHelper.GetEmptyDatabase());

            //Act
            var indexerProvider = Mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { Mocker.Resolve<DefaultEnabledIndexer>() });

            //Assert
            indexerProvider.All();
            indexerProvider.All().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().HaveCount(1);
            indexerProvider.GetSettings(typeof(DefaultEnabledIndexer)).Enable.Should().BeTrue();
        }

        [Test]
        public void Init_indexer_should_not_enable_indexer_that_is_not_enabled_by_default()
        {
            Mocker.SetConstant(TestDbHelper.GetEmptyDatabase());

            //Act
            var indexerProvider = Mocker.Resolve<IndexerProvider>();
            indexerProvider.InitializeIndexers(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });

            //Assert
            indexerProvider.All();
            indexerProvider.All().Should().HaveCount(1);
            indexerProvider.GetEnabledIndexers().Should().HaveCount(0);
            indexerProvider.GetSettings(typeof(MockIndexer)).Enable.Should().BeFalse();
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

        public override bool IsConfigured
        {
            get { return true; }
        }

        protected override NetworkCredential Credentials
        {
            get { return null; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return "Mocked Indexer"; }
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
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
            get { return new[] { "http://rss.nzbs.com/rss.php?cat=TV" }; }
        }

        public override bool IsConfigured
        {
            get { return true; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            throw new NotImplementedException();
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://google.com";
        }

        protected override string NzbInfoUrl(SyndicationItem item)
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

        public override bool IsConfigured
        {
            get { return true; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            throw new NotImplementedException();
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return "http://www.google.com";
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return "http://www.google.com";
        }

        protected override EpisodeParseResult CustomParser(SyndicationItem item, EpisodeParseResult currentResult)
        {
            if (currentResult == null) currentResult = new EpisodeParseResult();
            currentResult.Language = LanguageType.Finnish;
            return currentResult;
        }
    }

    public class NotConfiguredIndexer : IndexerBase
    {
        public NotConfiguredIndexer(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        public override string Name
        {
            get { return "NotConfiguredIndexer"; }
        }

        protected override string[] Urls
        {
            get { return new[] { "http://rss.nzbs.com/rss.php?cat=TV" }; }
        }

        public override bool IsConfigured
        {
            get { return false; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            throw new NotImplementedException();
        }

        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            throw new NotImplementedException();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultEnabledIndexer : IndexerBase
    {
        public DefaultEnabledIndexer(HttpProvider httpProvider, ConfigProvider configProvider)
            : base(httpProvider, configProvider)
        {
        }

        protected override string[] Urls
        {
            get { return new[] { "www.google.com" }; }
        }

        public override bool IsConfigured
        {
            get { return true; }
        }

        protected override NetworkCredential Credentials
        {
            get { return null; }
        }

        protected override IList<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            throw new NotImplementedException();
        }

        protected override IList<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return "Mocked Indexer"; }
        }
        
        protected override string NzbDownloadUrl(SyndicationItem item)
        {
            return item.Links[0].Uri.ToString();
        }

        protected override string NzbInfoUrl(SyndicationItem item)
        {
            return item.Links[1].Uri.ToString();
        }

        public override bool EnabledByDefault
        {
            get { return true; }
        }
    }
}