// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.Indexers
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IndexerServiceTest : CoreTest<IndexerService>
    {
        [Test]
        public void should_insert_indexer_in_repository_when_it_doesnt_exist()
        {
            Mocker.SetConstant<IEnumerable<IndexerBase>>(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });

            Subject.Init();

            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.Insert(It.IsAny<Indexer>()), Times.Once());
        }

        [Test]
        public void getEnabled_should_not_return_any_when_no_indexers_are_enabled()
        {
            Mocker.SetConstant<IEnumerable<IndexerBase>>(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });

            Mocker.GetMock<IIndexerRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<Indexer> {new Indexer {Id = 1, Type = "", Enable = false, Name = "Fake Indexer"}});

            Subject.GetEnabledIndexers().Should().BeEmpty();
        }

        [Test]
        public void Init_indexer_should_enable_indexer_that_is_enabled_by_default()
        {
            Mocker.SetConstant<IEnumerable<IndexerBase>>(new List<IndexerBase> { Mocker.Resolve<DefaultEnabledIndexer>() });

            Subject.Init();

            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.Insert(It.Is<Indexer>(indexer => indexer.Enable)), Times.Once());

            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.Insert(It.Is<Indexer>(indexer => !indexer.Enable)), Times.Never());
        }

        [Test]
        public void Init_indexer_should_not_enable_indexer_that_is_not_enabled_by_default()
        {
            Mocker.SetConstant<IEnumerable<IndexerBase>>(new List<IndexerBase> { Mocker.Resolve<MockIndexer>() });

            Subject.Init();

            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.Insert(It.Is<Indexer>(indexer => indexer.Enable)), Times.Never());

            Mocker.GetMock<IIndexerRepository>()
                .Verify(v => v.Insert(It.Is<Indexer>(indexer => !indexer.Enable)), Times.Once());
        }
    }

    public class MockIndexer : IndexerBase
    {
        public MockIndexer(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
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
        public TestUrlIndexer(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
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
        public CustomParserIndexer(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
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
        public NotConfiguredIndexer(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
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
        public DefaultEnabledIndexer(HttpProvider httpProvider, IConfigService configService)
            : base(httpProvider, configService)
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