using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Nyaa;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.IndexerTests.IntegrationTests
{
    [IntegrationTest]
    public class IndexerIntegrationTests : CoreTest
    {
        private SingleEpisodeSearchCriteria _singleSearchCriteria;
        private AnimeEpisodeSearchCriteria _animeSearchCriteria;

        [SetUp]
        public void SetUp()
        {
            UseRealHttp();

            _singleSearchCriteria = new SingleEpisodeSearchCriteria()
                {
                    SceneTitles = new List<string> { "Person of Interest" },
                    SeasonNumber = 1,
                    EpisodeNumber = 1
                };

            _animeSearchCriteria = new AnimeEpisodeSearchCriteria()
            {
                SceneTitles = new List<string> { "Steins;Gate" },
                AbsoluteEpisodeNumber = 1
            };
        }

        private void ValidateTorrentResult(IList<ReleaseInfo> reports, bool hasSize = false, bool hasInfoUrl = false, bool hasMagnet = false)
        {
            reports.Should().OnlyContain(c => c.GetType() == typeof(TorrentInfo));

            ValidateResult(reports, hasSize, hasInfoUrl);

            reports.Should().OnlyContain(c => c.DownloadProtocol == DownloadProtocol.Torrent);

            if (hasMagnet)
            {
                reports.Cast<TorrentInfo>().Should().OnlyContain(c => c.MagnetUrl.StartsWith("magnet:"));
            }
        }

        private void ValidateResult(IList<ReleaseInfo> reports, bool hasSize = false, bool hasInfoUrl = false)
        {
            reports.Should().NotBeEmpty();
            reports.Should().OnlyContain(c => c.Title.IsNotNullOrWhiteSpace());
            reports.Should().OnlyContain(c => c.PublishDate.Year > 2000);
            reports.Should().OnlyContain(c => c.DownloadUrl.IsNotNullOrWhiteSpace());
            reports.Should().OnlyContain(c => c.DownloadUrl.StartsWith("http"));

            if (hasInfoUrl)
            {
                reports.Should().OnlyContain(c => c.InfoUrl.IsNotNullOrWhiteSpace());
            }

            if (hasSize)
            {
                reports.Should().OnlyContain(c => c.Size > 0);
            }
        }
    }
}
