using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSourceTests
{
    [TestFixture]
    public class TraktProxyFixture : CoreTest<TraktProxy>
    {
        [TestCase("The Simpsons")]
        [TestCase("South Park")]
        [TestCase("Franklin & Bash")]
        public void successful_search(string title)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();
            result[0].Title.Should().Be(title);
        }


        [Test]
        public void no_search_result()
        {
            var result = Subject.SearchForNewSeries(Guid.NewGuid().ToString());
            result.Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_get_series_detail()
        {
            var details = Subject.GetSeriesInfo(75978);

            details.Should().NotBeNull();
            details.Images.Should().NotBeEmpty();
        }

        [Test]
        public void none_unique_season_episode_number()
        {
            var result = Subject.GetEpisodeInfo(75978);//Family guy

            result.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);

            result.Select(c => c.TvDbEpisodeId).Should().OnlyHaveUniqueItems();
        }



        [Test]
        public void should_be_able_to_get_list_of_episodes()
        {
            var details = Subject.GetEpisodeInfo(75978);
            details.Should().NotBeEmpty();
        }
    }
}
