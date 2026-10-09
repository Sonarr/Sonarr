using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ParsedEpisodeInfoFixture : CoreTest
    {
        [Test]
        public void should_be_possible_scene_season_special_for_e00_in_a_regular_season()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeasonNumbers = [2], EpisodeNumbers = [0] };

            parsedEpisodeInfo.IsPossibleSceneSeasonSpecial.Should().BeTrue();
        }

        [Test]
        public void should_not_be_possible_scene_season_special_in_season_zero()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeasonNumbers = [0], EpisodeNumbers = [0] };

            parsedEpisodeInfo.IsPossibleSceneSeasonSpecial.Should().BeFalse();
        }

        [Test]
        public void should_not_be_possible_scene_season_special_without_a_season()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeasonNumbers = [], EpisodeNumbers = [0] };

            parsedEpisodeInfo.IsPossibleSceneSeasonSpecial.Should().BeFalse();
        }

        [Test]
        public void should_not_be_possible_special_episode_without_a_season_or_series_title()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeriesTitle = "", SeasonNumbers = [], EpisodeNumbers = [7] };

            parsedEpisodeInfo.IsPossibleSpecialEpisode.Should().BeFalse();
        }

        [Test]
        public void should_be_possible_special_episode_in_season_zero_without_series_title()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeriesTitle = "", SeasonNumbers = [0], EpisodeNumbers = [7] };

            parsedEpisodeInfo.IsPossibleSpecialEpisode.Should().BeTrue();
        }

        [TestCase(new[] { 1, 2, 3 }, "Season 01-03")]
        [TestCase(new[] { 2 }, "Season 02")]
        [TestCase(new int[0], "[Unknown Season]")]
        public void should_format_full_season(int[] seasonNumbers, string expected)
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeriesTitle = "Series Title", SeasonNumbers = [.. seasonNumbers], FullSeason = true };

            parsedEpisodeInfo.ToString().Should().Contain(expected);
        }

        [Test]
        public void should_format_episode_with_first_season()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo { SeriesTitle = "Series Title", SeasonNumbers = [2], EpisodeNumbers = [5, 6] };

            parsedEpisodeInfo.ToString().Should().Contain("S02E05-06");
        }
    }
}
