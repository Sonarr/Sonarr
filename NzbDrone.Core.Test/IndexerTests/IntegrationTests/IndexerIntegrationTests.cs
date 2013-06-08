using System.Collections.Generic;
using FluentAssertions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Indexers.NzbClub;
using NzbDrone.Core.Indexers.Nzbx;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NUnit.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.IndexerTests.IntegrationTests
{
    [IntegrationTest]
    public class IndexerIntegrationTests : CoreTest<FetchFeedService>
    {
        [SetUp]
        public void SetUp()
        {
            UseRealHttp();
            
        }

        [Test]
        public void nzbclub_rss()
        {
            var indexer = new NzbClub();

            var result = Subject.FetchRss(indexer);

            ValidateResult(result);
        }

        [Test]
        public void nzbx_rss()
        {
            var indexer = new Nzbx();

            var result = Subject.FetchRss(indexer);

            ValidateResult(result);
        }



        [Test]
        [Explicit("needs newznab api key")]
        public void nzbsorg_rss()
        {
            var indexer = new Newznab();
            indexer.Settings = new NewznabSettings
                {
                    ApiKey = "",
                    Url = "http://nzbs.org"
                };

            indexer.InstanceDefinition = new IndexerDefinition();

            var result = Subject.FetchRss(indexer);

            result.Should().NotBeEmpty();
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title));
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbUrl));

            //TODO: uncomment these after moving to restsharp for rss
            //result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbInfoUrl));
            //result.Should().OnlyContain(c => c.Size > 0);

        }



        private void ValidateResult(IList<ReportInfo> reports)
        {
            reports.Should().NotBeEmpty();
            reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title));
            reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbInfoUrl));
            reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbUrl));
            reports.Should().OnlyContain(c => c.Size > 0);
        }

    }
}