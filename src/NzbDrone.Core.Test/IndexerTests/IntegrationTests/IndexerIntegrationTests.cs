using System.Collections.Generic;
using FluentAssertions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Eztv;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Indexers.Wombles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NUnit.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Test.Common.Categories;
using System.Linq;

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
        public void wombles_rss()
        {
            var indexer = new Wombles();

            indexer.Definition = new IndexerDefinition
            {
                Name = "Wombles",
                Settings = NullConfig.Instance
            };

            var result = Subject.FetchRss(indexer);

            ValidateResult(result, skipSize: true, skipInfo: true);
        }

        [Test]
        public void extv_rss()
        {
            var indexer = new Eztv();
            indexer.Definition = new IndexerDefinition
            {
                Name = "Eztv",
                Settings = NullConfig.Instance
            };

            var result = Subject.FetchRss(indexer);

            ValidateTorrentResult(result, skipSize: false, skipInfo: true);
        }

        [Test]
        public void nzbsorg_rss()
        {
            var indexer = new Newznab();

            indexer.Definition = new IndexerDefinition();
            indexer.Definition.Name = "nzbs.org";
            indexer.Definition.Settings = new NewznabSettings
            {
                ApiKey = "64d61d3cfd4b75e51d01cbc7c6a78275",
                Url = "http://nzbs.org"
            };

            var result = Subject.FetchRss(indexer);

            ValidateResult(result);
        }
        
        private void ValidateResult(IList<ReleaseInfo> reports, bool skipSize = false, bool skipInfo = false)
        {
            reports.Should().NotBeEmpty();
            reports.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Title));
            reports.Should().NotContain(c => string.IsNullOrWhiteSpace(c.DownloadUrl));
            reports.Should().OnlyContain(c => c.PublishDate.Year > 2000);
            reports.Should().OnlyContain(c => c.DownloadUrl.StartsWith("http"));

            if (!skipInfo)
            {
                reports.Should().NotContain(c => string.IsNullOrWhiteSpace(c.InfoUrl));
            }

            if (!skipSize)
            {
                reports.Should().OnlyContain(c => c.Size > 0);
            }
        }

        private void ValidateTorrentResult(IList<ReleaseInfo> reports, bool skipSize = false, bool skipInfo = false)
        {
            reports.Should().OnlyContain(c => c.GetType() == typeof(TorrentInfo));

            ValidateResult(reports, skipSize, skipInfo);

            reports.Should().OnlyContain(c => c.DownloadUrl.EndsWith(".torrent"));

            reports.Cast<TorrentInfo>().Should().OnlyContain(c => c.MagnetUrl.StartsWith("magnet:"));
            reports.Cast<TorrentInfo>().Should().NotContain(c => string.IsNullOrWhiteSpace(c.InfoHash));
        }

    }
}
