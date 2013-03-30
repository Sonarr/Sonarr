using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]

    public class TvDbProxyFixture : CoreTest<TvDbProxy>
    {
        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        [TestCase("Franklin & Bash")]
        public void successful_search(string title)
        {
            var result = Subject.SearchSeries(title);

            result.Should().NotBeEmpty();
            result[0].Title.Should().Be(title);
        }


        [Test]
        public void no_search_result()
        {
            var result = Subject.SearchSeries(Guid.NewGuid().ToString());
            result.Should().BeEmpty();
        }

        [Test]
        public void none_unique_season_episode_number()
        {
            var result = Subject.GetEpisodes(75978);//Family guy

            result.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);

            result.Select(c => c.TvDbEpisodeId).Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void should_be_able_to_get_series_detail()
        {
            var details = Subject.GetSeries(75978);

            details.Should().NotBeNull();
            details.Covers.Value.Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_get_list_of_episodes()
        {
            var details = Subject.GetEpisodes(75978);
            details.Should().NotBeEmpty();
        }
    }
}