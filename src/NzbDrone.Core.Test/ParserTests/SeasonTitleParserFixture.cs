using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SeasonTitleParserFixture : CoreTest
    {
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (BS11 1280x720 x264 AAC)", "Dr. Stone Stone Wars")]
        [TestCase("[S3rNx] Dr. Stone Stone Wars (BDRip 1920x1080 x264 FLAC)", "Dr. Stone Stone Wars")]
        [TestCase("[DmonHiro] Dr.Stone - Stone Wars (BD, 720p)", "Dr.Stone - Stone Wars")]
        [TestCase("Dr. Stone Stone Wars [Bluray.1080p.HEVC.AAC.ITA.FLAC.JAP.Sub.Ita]", "Dr. Stone Stone Wars")]
        [TestCase("[Cleo] Dr. Stone: Stone Wars [Dual Audio 10bit 1080p][HEVC-x265]", "Dr. Stone: Stone Wars")]
        public void should_return_the_title_when_there_is_no_season_or_episode(string releaseTitle, string expectedTitle)
        {
            var result = Parser.Parser.ParseSeasonTitle(releaseTitle);

            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(expectedTitle);
            result.FullSeason.Should().BeTrue();

            // Which season the title refers to is held in the scene mappings, so it is left
            // unset here and resolved by ParsingService.
            result.SeasonNumber.Should().Be(0);
            result.EpisodeNumbers.Should().BeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
        }

        // DownloadDecisionComparer reads ParsedEpisodeInfo.Quality while sorting, so leaving it
        // unset fails the whole search with "failed to compare two elements in the array"
        // rather than skipping one release.
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (BD 1280x720 x264 AAC)")]
        [TestCase("[Cleo] Dr. Stone: Stone Wars [Dual Audio 10bit 1080p][HEVC-x265]")]
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
        [TestCase("[FY-Raws][Dr.STONE: Stone Wars][BDRip][1080P_x265(10bit)-FLAC][ALL]")]
        [TestCase("[Group][Some Title][1080p][x265(10bit)]")]
        public void should_not_return_a_title_when_stripping_leaves_metadata(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        // Groups mark extras inside brackets, which is what stripping metadata removes, so an
        // opening or an OVA would otherwise look exactly like a season pack.
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (OVA)")]
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars [OVA]")]
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (Special)")]
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (NCOP)")]
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (NCED)")]
        public void should_not_return_a_title_for_an_extra(string releaseTitle)
        {
            Parser.Parser.ParseSeasonTitle(releaseTitle).Should().BeNull();
        }

        // The helper is not wired into ParseTitle, so anything ParseTitle rejects today it
        // still rejects. Otherwise nothing would ever be unparsable and crap detection, daily
        // date validation and the import path all lose their only signal.
        [TestCase("THIS SHOULD NEVER PARSE")]
        [TestCase("Vrq6e1Aba3U amCjuEgV5R2QvdsLEGYF3YQAQkw8")]
        [TestCase("[Ohys-Raws] Dr. Stone Stone Wars (BS11 1280x720 x264 AAC)")]
        public void should_leave_parse_title_rejecting_what_it_rejects_today(string releaseTitle)
        {
            Parser.Parser.ParseTitle(releaseTitle).Should().BeNull();
        }
    }
}
