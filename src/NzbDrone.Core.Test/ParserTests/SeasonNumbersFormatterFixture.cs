using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SeasonNumbersFormatterFixture
    {
        [TestCase(new[] { 2 }, "Season 2")]
        [TestCase(new[] { 2, 2 }, "Season 2")]
        [TestCase(new[] { 1, 2, 3 }, "Seasons 1-3")]
        [TestCase(new[] { 3, 1, 2 }, "Seasons 1-3")]
        [TestCase(new[] { 1, 3, 5 }, "Seasons 1, 3, 5")]
        public void should_format_season_numbers(int[] seasonNumbers, string expected)
        {
            SeasonNumbersFormatter.Format(seasonNumbers).Should().Be(expected);
        }

        [TestCase(new[] { 2 }, "Season 02")]
        [TestCase(new[] { 1, 2 }, "Seasons 01-02")]
        [TestCase(new[] { 1, 3 }, "Seasons 01, 03")]
        public void should_format_season_numbers_with_number_format(int[] seasonNumbers, string expected)
        {
            SeasonNumbersFormatter.Format(seasonNumbers, "00").Should().Be(expected);
        }
    }
}
