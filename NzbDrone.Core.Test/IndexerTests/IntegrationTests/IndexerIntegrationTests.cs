using System.Collections.Generic;
using FluentAssertions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Indexers.NzbClub;
using NzbDrone.Core.Indexers.Nzbx;
using NzbDrone.Core.Indexers.Wombles;
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
        [Explicit]
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
        public void wombles_rss()
        {
            var indexer = new Wombles();

            var result = Subject.FetchRss(indexer);

            ValidateResult(result, skipSize: true, skipInfo: true);
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

            ValidateResult(result);
        }



        private void ValidateResult(IList<ReportInfo> reports, bool skipSize = false, bool skipInfo = false)
        {
            reports.Should().NotBeEmpty();
            reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title));
            reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbUrl));

            if (!skipInfo)
            {
                reports.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.NzbInfoUrl));
            }

            if (!skipSize)
            {
                reports.Should().OnlyContain(c => c.Size > 0);
            }
        }

    }
}