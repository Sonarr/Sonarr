using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SeasonTitleParserFixture : CoreTest
    {
        [TestCase("[Group] Monkey Island Curse (BS11 1280x720 x264 AAC)", "Monkey Island Curse")]
        [TestCase("[Group] Monkey Island Curse (BDRip 1920x1080 x264 FLAC) [ABCD1234]", "Monkey Island Curse")]
        [TestCase("[Group] Monkey.Island - Curse (BD, 720p)", "Monkey.Island - Curse")]
        [TestCase("Monkey Island Curse [Bluray.1080p.HEVC.AAC.ITA.FLAC.JAP.Sub.Ita]", "Monkey Island Curse")]
        [TestCase("[Group] Monkey Island: Curse [Dual Audio 10bit 1080p][HEVC-x265]", "Monkey Island: Curse")]
        public void should_return_the_title_when_there_is_no_season_or_episode(string releaseTitle, string expectedTitle)
        {
            var result = Parser.Parser.ParseSeasonTitle(releaseTitle);

            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(expectedTitle);
            result.FullSeason.Should().BeTrue();
            result.IsSeasonTitleOnly.Should().BeTrue();
            result.SeasonNumber.Should().Be(-1);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
        }

        [TestCase("[Group] Monkey Island Curse (BD 1280x720 x264 AAC)")]
        [TestCase("[Group] Monkey Island: Curse [Dual Audio 10bit 1080p][HEVC-x265]")]
        public void should_populate_the_fields_the_decision_engine_reads(string releaseTitle)
        {
            var result = Parser.Parser.ParseSeasonTitle(releaseTitle);

            result.Quality.Should().NotBeNull();
            result.Quality.Quality.Should().NotBeNull();
            result.Languages.Should().NotBeNullOrEmpty();
        }

        [TestCase("[Group][Monkey Island: Curse][BDRip][1080P_x265(10bit)-FLAC][ALL]")]
        [TestCase("[Group][Monkey Island Curse][1080p][x264]")]
        public void should_not_return_a_title_when_stripping_leaves_metadata(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        [TestCase("[Group] Monkey Island Curse (OVA)")]
        [TestCase("[Group] Monkey Island Curse [OVA]")]
        [TestCase("[Group] Monkey Island Curse (Special)")]
        [TestCase("[Group] Monkey Island Curse (NCOP)")]
        [TestCase("[Group] Monkey Island Curse (NCED)")]
        public void should_not_return_a_title_for_an_extra(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        [TestCase("Monkey Island S02E05 1080p WEB-DL")]
        [TestCase("[Group] Monkey Island - 05 (1080p)")]
        [TestCase("Monkey Island S02 1080p WEB-DL")]
        public void should_not_flag_a_release_the_regexes_can_parse(string releaseTitle)
        {
            Parser.Parser.ParseTitle(releaseTitle).IsSeasonTitleOnly.Should().BeFalse();
        }

        [TestCase("THIS SHOULD NEVER PARSE")]
        [TestCase("Vrq6e1Aba3U amCjuEgV5R2QvdsLEGYF3YQAQkw8")]
        [TestCase("[Group] Monkey Island Curse (BS11 1280x720 x264 AAC)")]
        public void should_leave_parse_title_rejecting_what_it_rejects_today(string releaseTitle)
        {
            Parser.Parser.ParseTitle(releaseTitle).Should().BeNull();
        }
    }
}
