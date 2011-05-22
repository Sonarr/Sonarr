// ReSharper disable RedundantUsingDirective
using System;
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
        [Row(new object[] {"CAPITAL", "capital", true})]
        [Row(new object[] {"Something!!", "Something", true})]
        [Row(new object[] {"Simpsons 2000", "Simpsons", true})]
        [Row(new object[] {"Simp222sons", "Simpsons", true})]
        [Row(new object[] {"Simpsons", "The Simpsons", true})]
        [Row(new object[] {"Law and order", "Law & order", true})]
        [Row(new object[] {"xxAndxx", "xxxx", false})]
        [Row(new object[] {"Andxx", "xx", false})]
        [Row(new object[] {"xxAnd", "xx", false})]
        [Row(new object[] {"Thexx", "xx", false})]
        [Row(new object[] {"Thexx", "xx", false})]
        [Row(new object[] {"xxThexx", "xxxxx", false})]
        [Row(new object[] {"Simpsons The", "Simpsons", true})]
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
    }
}