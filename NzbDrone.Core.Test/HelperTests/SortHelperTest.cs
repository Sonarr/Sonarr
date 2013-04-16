using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HelperTests
{
    [TestFixture]
    public class SortHelperTest : CoreTest
    {
        [TestCase("The Office (US)", "Office (US)")]
        [TestCase("A Man in Anger", "Man in Anger")]
        [TestCase("An Idiot Abroad", "Idiot Abroad")]
        [TestCase("American Gladiators", "American Gladiators")]
        [TestCase("Ancient Apocalyps", "Ancient Apocalyps")]
        [TestCase("There Will Be Brawl", "There Will Be Brawl")]
        [TestCase("30 Rock", "30 Rock")]
        [TestCase(null, "")]
        public void SkipArticles(string title, string expected)
        {
            var result = title.IgnoreArticles();
            result.Should().Be(expected);
        }
    }

}