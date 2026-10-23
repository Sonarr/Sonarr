using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications;

namespace NzbDrone.Core.Test.NotificationTests
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
    }
}
