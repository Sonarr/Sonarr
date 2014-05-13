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
    public class IndexerIntegrationTests : CoreTest<Wombles>
    {
        [SetUp]
        public void SetUp()
        {
            UseRealHttp();
        }

        [Test]
        public void wombles_rss()
        {
            Subject.Definition = new IndexerDefinition
            {
                Name = "Wombles",
                Settings = NullConfig.Instance
            };

            var result = Subject.FetchRecent();

            ValidateResult(result, skipSize: true, skipInfo: true);
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
