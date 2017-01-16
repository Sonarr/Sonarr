using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
        }

        [TestCase("The Simpsons", "The Simpsons")]
        [TestCase("South Park", "South Park")]
        [TestCase("Franklin & Bash", "Franklin & Bash")]
        [TestCase("House", "House")]
        [TestCase("Mr. D", "Mr. D")]
        [TestCase("Rob & Big", "Rob & Big")]
        [TestCase("M*A*S*H", "M*A*S*H")]
        //[TestCase("imdb:tt0436992", "Doctor Who (2005)")]
        [TestCase("tvdb:78804", "Doctor Who (2005)")]
        [TestCase("tvdbid:78804", "Doctor Who (2005)")]
        [TestCase("tvdbid: 78804 ", "Doctor Who (2005)")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewSeries(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tvdbid:")]
        [TestCase("tvdbid: 99999999999999999999")]
        [TestCase("tvdbid: 0")]
        [TestCase("tvdbid: -12")]
        [TestCase("tvdbid:289578")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD;EF")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewSeries(term);
            result.Should().BeEmpty();
            
            ExceptionVerification.IgnoreWarns();
        }
    }
}
