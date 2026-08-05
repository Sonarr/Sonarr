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

            // Which season the title refers to is held in the scene mappings, so it is left
            // unset here and resolved by ParsingService. Not 0, that is the special season.
            result.SeasonNumber.Should().Be(-1);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
        }

        // DownloadDecisionComparer reads ParsedEpisodeInfo.Quality while sorting, so leaving it
        // unset fails the whole search with "failed to compare two elements in the array"
        // rather than skipping one release.
        [TestCase("[Group] Monkey Island Curse (BD 1280x720 x264 AAC)")]
        [TestCase("[Group] Monkey Island: Curse [Dual Audio 10bit 1080p][HEVC-x265]")]
        public void should_populate_the_fields_the_decision_engine_reads(string releaseTitle)
        {
            var result = Parser.Parser.ParseSeasonTitle(releaseTitle);

            result.Quality.Should().NotBeNull();
            result.Quality.Quality.Should().NotBeNull();
            result.Languages.Should().NotBeNullOrEmpty();
        }

        // Some groups bracket every part of the name including the title, and a bracketed group
        // can contain another. Both leave metadata behind instead of a title, which is worse
        // than returning nothing because it gets used as a lookup key.
        [TestCase("[Group][Monkey Island: Curse][BDRip][1080P_x265(10bit)-FLAC][ALL]")]
        [TestCase("[Group][Monkey Island Curse][1080p][x264]")]
        public void should_not_return_a_title_when_stripping_leaves_metadata(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        // Groups mark extras inside brackets, which is what stripping metadata removes, so an
        // opening or an OVA would otherwise look exactly like a season pack.
        [TestCase("[Group] Monkey Island Curse (OVA)")]
        [TestCase("[Group] Monkey Island Curse [OVA]")]
        [TestCase("[Group] Monkey Island Curse (Special)")]
        [TestCase("[Group] Monkey Island Curse (NCOP)")]
        [TestCase("[Group] Monkey Island Curse (NCED)")]
        public void should_not_return_a_title_for_an_extra(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        // The helper is not wired into ParseTitle, so anything ParseTitle rejects today it
        // still rejects. Otherwise nothing would ever be unparsable and crap detection, daily
        // date validation and the import path all lose their only signal.
        [TestCase("THIS SHOULD NEVER PARSE")]
        [TestCase("Vrq6e1Aba3U amCjuEgV5R2QvdsLEGYF3YQAQkw8")]
        [TestCase("[Group] Monkey Island Curse (BS11 1280x720 x264 AAC)")]
        public void should_leave_parse_title_rejecting_what_it_rejects_today(string releaseTitle)
        {
            Parser.Parser.ParseTitle(releaseTitle).Should().BeNull();
        }
    }
}
