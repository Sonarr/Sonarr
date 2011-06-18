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

        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        public void successful_title_lookup(string title)
        {
            var tvCont = new TvDbProvider();
            var result = tvCont.GetSeries(title);

            result.SeriesName.Should().Be(title);
        }



        [TestCase(new object[] { "CAPITAL", "capital", true })]
        [TestCase(new object[] { "Something!!", "Something", true })]
        [TestCase(new object[] { "Simpsons 2000", "Simpsons", true })]
        [TestCase(new object[] { "Simp222sons", "Simpsons", true })]
        [TestCase(new object[] { "Simpsons", "The Simpsons", true })]
        [TestCase(new object[] { "Law and order", "Law & order", true })]
        [TestCase(new object[] { "xxAndxx", "xxxx", false })]
        [TestCase(new object[] { "Andxx", "xx", false })]
        [TestCase(new object[] { "xxAnd", "xx", false })]
        [TestCase(new object[] { "Thexx", "xx", false })]
        [TestCase(new object[] { "Thexx", "xx", false })]
        [TestCase(new object[] { "xxThexx", "xxxxx", false })]
        [TestCase(new object[] { "Simpsons The", "Simpsons", true })]
        public void Name_match_test(string a, string b, bool match)
        {
            bool result = TvDbProvider.IsTitleMatch(a, b);

            Assert.AreEqual(match, result, "{0} , {1}", a, b);
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
        public void no_result_title_lookup()
        {
            //setup
            var tvdbProvider = new TvDbProvider();

            //act
            var result = tvdbProvider.GetSeries("clone high");

            //assert
            Assert.IsNull(result);
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

            var seasons = new List<List<TvdbEpisode>>(seasonsNumbers.Count);

            foreach (var season in seasonsNumbers)
            {
                seasons.Insert(season, result.Episodes.Where(e => e.SeasonNumber == season).ToList());
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
                season.Should().OnlyHaveUniqueItems();
            }

            //Make sure no episode number is skipped
            foreach (var season in seasons)
            {
                for (int i = 1; i < season.Count; i++)
                {
                    season.Should().Contain(c => c.EpisodeNumber == i, "Can't find Episode S{0:00}E{1:00}",
                                            season[0].SeasonNumber, i);
                }
            }


        }
    }
}