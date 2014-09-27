using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSourceTests
{
    [TestFixture]
    [IntegrationTest]
    public class TvdbProxyFixture : CoreTest<TvdbProxy>
    {
        [TestCase(88031)]
        [TestCase(179321)]
        public void should_be_able_to_get_series_detail(int tvdbId)
        {
            UseRealHttp();

            var episodes = Subject.GetEpisodeInfo(tvdbId);

            ValidateEpisodes(episodes);
        }

        private void ValidateEpisodes(List<Episode> episodes)
        {
            episodes.Should().NotBeEmpty();

            episodes.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);

            episodes.Should().Contain(c => c.SeasonNumber > 0);

            episodes.Should().OnlyContain(c => c.SeasonNumber > 0 || c.EpisodeNumber > 0);
        }


    }
}
