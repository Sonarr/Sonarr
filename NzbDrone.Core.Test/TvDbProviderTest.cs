// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvDbProviderTest : TestBase
    {
        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        public void successful_search(string title)
        {
            var result = new TvDbProvider().SearchSeries(title);

            result.Should().NotBeEmpty();
            result[0].SeriesName.Should().Be(title);
        }






        [Test]
        public void no_search_result()
        {
            //setup
            var tvdbProvider = new TvDbProvider();

            //act
            var result = tvdbProvider.SearchSeries(Guid.NewGuid().ToString());

            //assert
            result.Should().BeEmpty();
        }


        [Test]
        public void none_unique_season_episode_number()
        {
            //setup
            var tvdbProvider = new TvDbProvider();

            //act
            var result = tvdbProvider.GetSeries(75978, true);//Family guy

            //Asserts that when episodes are grouped by Season/Episode each group contains maximum of
            //one item.
            result.Episodes.GroupBy(e => e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                .Max(e => e.Count()).Should().Be(1);

        }

        [Test]
        public void American_dad_fix()
        {
            //setup
            var tvdbProvider = new TvDbProvider();

            //act
            var result = tvdbProvider.GetSeries(73141, true);

            var seasonsNumbers = result.Episodes.Select(e => e.SeasonNumber)
                .Distinct().ToList();

            var seasons = new Dictionary<int, List<TvdbEpisode>>(seasonsNumbers.Count);

            foreach (var season in seasonsNumbers)
            {
                seasons.Add(season, result.Episodes.Where(e => e.SeasonNumber == season).ToList());
            }

            foreach (var episode in result.Episodes)
            {
                Console.WriteLine(episode);
            }

            //assert
            seasonsNumbers.Should().HaveCount(7);
            seasons[1].Should().HaveCount(23);
            seasons[2].Should().HaveCount(19);
            seasons[3].Should().HaveCount(16);
            seasons[4].Should().HaveCount(20);
            seasons[5].Should().HaveCount(18);

            foreach (var season in seasons)
            {
                season.Value.Should().OnlyHaveUniqueItems();
            }

            //Make sure no episode number is skipped
            foreach (var season in seasons)
            {
                for (int i = 1; i < season.Value.Count; i++)
                {
                    season.Value.Should().Contain(c => c.EpisodeNumber == i, "Can't find Episode S{0:00}E{1:00}",
                                            season.Value[0].SeasonNumber, i);
                }
            }


        }
    }
}