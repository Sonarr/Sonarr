// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using MbUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class TvDbProviderTest : TestBase
    {
        [Test]
        [Row("The Simpsons")]
        [Row("Family Guy")]
        [Row("South Park")]
        [Row("clone high, usa")]
        public void successful_search(string title)
        {
            var tvCont = new TvDbProvider();
            var result = tvCont.SearchSeries(title);

            Assert.IsNotEmpty(result);
            Assert.AreEqual(title, result[0].SeriesName, StringComparison.InvariantCultureIgnoreCase);
        }

        [Test]
        [Row("The Simpsons")]
        [Row("Family Guy")]
        [Row("South Park")]
        public void successful_title_lookup(string title)
        {
            var tvCont = new TvDbProvider();
            var result = tvCont.GetSeries(title);

            Assert.AreEqual(title, result.SeriesName, StringComparison.InvariantCultureIgnoreCase);
        }


        [Test]
        [Row(new object[] { "CAPITAL", "capital", true })]
        [Row(new object[] { "Something!!", "Something", true })]
        [Row(new object[] { "Simpsons 2000", "Simpsons", true })]
        [Row(new object[] { "Simp222sons", "Simpsons", true })]
        [Row(new object[] { "Simpsons", "The Simpsons", true })]
        [Row(new object[] { "Law and order", "Law & order", true })]
        [Row(new object[] { "xxAndxx", "xxxx", false })]
        [Row(new object[] { "Andxx", "xx", false })]
        [Row(new object[] { "xxAnd", "xx", false })]
        [Row(new object[] { "Thexx", "xx", false })]
        [Row(new object[] { "Thexx", "xx", false })]
        [Row(new object[] { "xxThexx", "xxxxx", false })]
        [Row(new object[] { "Simpsons The", "Simpsons", true })]
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
            Assert.IsEmpty(result);
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
            Assert.Count(7, seasons);
            Assert.Count(23, seasons1);
            Assert.Count(19, seasons2);
            Assert.Count(16, seasons3);
            Assert.Count(20, seasons4);
            Assert.Count(18, seasons5);

            Assert.Distinct(seasons1.Select(s => s.EpisodeNumber));
            Assert.Distinct(seasons2.Select(s => s.EpisodeNumber));
            Assert.Distinct(seasons3.Select(s => s.EpisodeNumber));
            Assert.Distinct(seasons4.Select(s => s.EpisodeNumber));
            Assert.Distinct(seasons5.Select(s => s.EpisodeNumber));
            Assert.Distinct(seasons6.Select(s => s.EpisodeNumber));

        }
    }
}