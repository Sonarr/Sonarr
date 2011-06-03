// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [NUnit.Framework.TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvDbProviderTest : TestBase
    {
        [Test]
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
        [TestCase("The Simpsons")]
        [TestCase("Family Guy")]
        [TestCase("South Park")]
        public void successful_title_lookup(string title)
        {
            var tvCont = new TvDbProvider();
            var result = tvCont.GetSeries(title);

            result.SeriesName.Should().Be(title);
        }


        [Test]
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

            var seasons = result.Episodes.Select(e => e.SeasonNumber)
                .Distinct().ToList();



            var seasons1 = result.Episodes.Where(e => e.SeasonNumber == 1).ToList();
            var seasons2 = result.Episodes.Where(e => e.SeasonNumber == 2).ToList();
            var seasons3 = result.Episodes.Where(e => e.SeasonNumber == 3).ToList();
            var seasons4 = result.Episodes.Where(e => e.SeasonNumber == 4).ToList();
            var seasons5 = result.Episodes.Where(e => e.SeasonNumber == 5).ToList();
            var seasons6 = result.Episodes.Where(e => e.SeasonNumber == 6).ToList();


            foreach (var episode in result.Episodes)
            {
                Console.WriteLine(episode);
            }

            //assert
            seasons.Should().HaveCount(7);
            seasons1.Should().HaveCount(23);
            seasons2.Should().HaveCount(19);
            seasons3.Should().HaveCount(16);
            seasons4.Should().HaveCount(20);
            seasons5.Should().HaveCount(18);

            seasons1.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();
            seasons2.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();
            seasons3.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();
            seasons4.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();
            seasons5.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();
            seasons6.Select(s => s.EpisodeNumber).Should().OnlyHaveUniqueItems();

        }
    }
}