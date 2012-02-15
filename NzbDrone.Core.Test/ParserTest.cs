// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Contract;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ParserTest : CoreTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         * [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
         * [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
         * [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "", 7, new[] { 22, 23 }, 2)]
         */

        [TestCase("Sonny.With.a.Chance.S02E15", "Sonny.With.a.Chance", 2, 15)]
        [TestCase("Two.and.a.Half.Me.103.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 1, 3)]
        [TestCase("Two.and.a.Half.Me.113.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 1, 13)]
        [TestCase("Two.and.a.Half.Me.1013.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 10, 13)]
        [TestCase("Chuck.4x05.HDTV.XviD-LOL", "Chuck", 4, 5)]
        [TestCase("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", "The.Girls.Next.Door", 3, 6)]
        [TestCase("Degrassi.S10E27.WS.DSR.XviD-2HD", "Degrassi", 10, 27)]
        [TestCase("Parenthood.2010.S02E14.HDTV.XviD-LOL", "Parenthood 2010", 2, 14)]
        [TestCase("Hawaii Five 0 S01E19 720p WEB DL DD5 1 H 264 NT", "Hawaii Five", 1, 19)]
        [TestCase("The Event S01E14 A Message Back 720p WEB DL DD5 1 H264 SURFER", "The Event", 1, 14)]
        [TestCase("Adam Hills In Gordon St Tonight S01E07 WS PDTV XviD FUtV", "Adam Hills In Gordon St Tonight", 1, 7)]
        [TestCase("Adam Hills In Gordon St Tonight S01E07 WS PDTV XviD FUtV", "Adam Hills In Gordon St Tonight", 1, 7)]
        [TestCase("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", "Adventure.Inc", 3, 19)]
        [TestCase("S03E09 WS PDTV XviD FUtV", "", 3, 9)]
        [TestCase("5x10 WS PDTV XviD FUtV", "", 5, 10)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD-LOL", "Castle 2009", 1, 14)]
        [TestCase("Pride.and.Prejudice.1995.S03E20.HDTV.XviD-LOL", "Pride and Prejudice 1995", 3, 20)]
        [TestCase("The.Office.S03E115.DVDRip.XviD-OSiTV", "The.Office", 3, 115)]
        [TestCase(@"Parks and Recreation - S02E21 - 94 Meetings - 720p TV.mkv", "Parks and Recreation", 2, 21)]
        [TestCase(@"24-7 Penguins-Capitals- Road to the NHL Winter Classic - S01E03 - Episode 3.mkv", "24-7 Penguins-Capitals- Road to the NHL Winter Classic", 1, 3)]
        [TestCase("Adventure.Inc.S03E19.DVDRip.\"XviD\"-OSiTV", "Adventure.Inc", 3, 19)]
        [TestCase("Hawaii Five-0 (2010) - 1x05 - Nalowale (Forgotten/Missing)", "Hawaii Five-0 (2010)", 1, 5)]
        [TestCase("Hawaii Five-0 (2010) - 1x05 - Title", "Hawaii Five-0 (2010)", 1, 5)]
        [TestCase("House - S06E13 - 5 to 9 [DVD]", "House", 6, 13)]
        [TestCase("The Mentalist - S02E21 - 18-5-4", "The Mentalist", 2, 21)]
        [TestCase("Breaking.In.S01E07.21.0.Jump.Street.720p.WEB-DL.DD5.1.h.264-KiNGS", "Breaking In", 1, 7)]
        [TestCase("CSI525", "CSI", 5, 25)]
        [TestCase("King of the Hill - 10x12 - 24 Hour Propane People [SDTV]", "King of the Hill", 10, 12)]
        [TestCase("Brew Masters S01E06 3 Beers For Batali DVDRip XviD SPRiNTER", "Brew Masters", 1, 6)]
        [TestCase("24 7 Flyers Rangers Road to the NHL Winter Classic Part01 720p HDTV x264 ORENJI", "24 7 Flyers Rangers Road to the NHL Winter Classic", 1, 1)]
        [TestCase("24 7 Flyers Rangers Road to the NHL Winter Classic Part 02 720p HDTV x264 ORENJI", "24 7 Flyers Rangers Road to the NHL Winter Classic", 1, 2)]
        [TestCase("24-7 Flyers-Rangers- Road to the NHL Winter Classic - S01E01 - Part 1", "24 7 Flyers Rangers Road to the NHL Winter Classic", 1, 1)]
        [TestCase("The.Kennedys.Part.2.DSR.XviD-SYS", "The Kennedys", 1, 2)]
        [TestCase("the-pacific-e07-720p", "The Pacific", 1, 7)]
        [TestCase("S6E02-Unwrapped-(Playing With Food) - [DarkData]", "", 6, 2)]
        [TestCase("S06E03-Unwrapped-(Number Ones Unwrapped) - [DarkData]", "", 6, 3)]
        [TestCase("The Mentalist S02E21 18 5 4 720p WEB DL DD5 1 h 264 EbP", "The Mentalist", 2, 21)]
        [TestCase("01x04 - Halloween, Part 1 - 720p WEB-DL", "", 1, 4)]
        public void ParseTitle_single(string postTitle, string title, int seasonNumber, int episodeNumber)
        {
            var result = Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.First().Should().Be(episodeNumber);
            result.CleanTitle.Should().Be(Parser.NormalizeTitle(title));
            result.OriginalString.Should().Be(postTitle);
        }

        [Test]
        [TestCase(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
        [TestCase(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Battlestar Galactica (2003)\Season 2\S02E21.avi", 2, 21)]
        [TestCase("C:/Test/TV/Chuck.4x05.HDTV.XviD-LOL", 4, 5)]
        [TestCase(@"P:\TV Shows\House\Season 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
        [TestCase(@"S:\TV Drop\House - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\1012 - 24 Hour Propane People.avi", 10, 12)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\Hour Propane People.avi", 10, 12)]
        public void PathParse_tests(string path, int season, int episode)
        {
            var result = Parser.ParsePath(path);
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers[0].Should().Be(episode);
            result.OriginalString.Should().Be(path);

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void unparsable_path_should_report_the_path()
        {
            Parser.ParsePath("C:\\").Should().BeNull();
            
            MockedRestProvider.Verify(c => c.PostData(It.IsAny<string>(), It.IsAny<ParseErrorReport>()), Times.Exactly(2));

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void unparsable_title_should_report_title()
        {
            const string TITLE = "SOMETHING";

            Parser.ParseTitle(TITLE).Should().BeNull();

            MockedRestProvider.Verify(c => c.PostData(It.IsAny<string>(), It.Is<ParseErrorReport>(r => r.Title == TITLE)), Times.Once());

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", QualityTypes.DVD, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", QualityTypes.DVD, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", QualityTypes.DVD, false)]
        [TestCase("Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", QualityTypes.HDTV, false)]
        [TestCase("this has no extention or periods HDTV", QualityTypes.SDTV, false)]
        [TestCase("Chuck.S04E05.HDTV.XviD-LOL", QualityTypes.SDTV, false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", QualityTypes.DVD, false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", QualityTypes.DVD, false)]
        [TestCase("The.Girls.Next.Door.S03E06.HDTV-WiDE", QualityTypes.SDTV, false)]
        [TestCase("Degrassi.S10E27.WS.DSR.XviD-2HD", QualityTypes.SDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", QualityTypes.WEBDL, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p", QualityTypes.HDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.mkv", QualityTypes.HDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.avi", QualityTypes.SDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.xvid", QualityTypes.SDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15.divx", QualityTypes.SDTV, false)]
        [TestCase("Sonny.With.a.Chance.S02E15", QualityTypes.Unknown, false)]
        [TestCase("Chuck - S01E04 - So Old - Playdate - 720p TV.mkv", QualityTypes.HDTV, false)]
        [TestCase("Chuck - S22E03 - MoneyBART - HD TV.mkv", QualityTypes.HDTV, false)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", QualityTypes.Bluray720p, false)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", QualityTypes.Bluray1080p, false)]
        [TestCase("Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", QualityTypes.WEBDL, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.DVD, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.DVD, false)]
        [TestCase("Law & Order: Special Victims Unit - 11x11 - Quickie", QualityTypes.Unknown, false)]
        [TestCase("(<a href=\"http://www.newzbin.com/browse/post/6076286/nzb/\">NZB</a>)", QualityTypes.Unknown, false)]
        [TestCase("S07E23 - [HDTV].mkv ", QualityTypes.HDTV, false)]
        [TestCase("S07E23 - [WEBDL].mkv ", QualityTypes.WEBDL, false)]
        [TestCase("S07E23.mkv ", QualityTypes.HDTV, false)]
        [TestCase("S07E23 .avi ", QualityTypes.SDTV, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", QualityTypes.DVD, false)]
        [TestCase("WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", QualityTypes.Bluray720p, false)]
        [TestCase("The Voice S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", QualityTypes.Unknown, false)]
        [TestCase("Nikita S02E01 HDTV XviD 2HD", QualityTypes.SDTV, false)]
        [TestCase("Gossip Girl S05E11 PROPER HDTV XviD 2HD", QualityTypes.SDTV, true)]
        public void quality_parse(string postTitle, object quality, bool proper)
        {
            var result = Parser.ParseQuality(postTitle);
            result.QualityType.Should().Be(quality);
            result.Proper.Should().Be(proper);
        }

        [Test]
        public void parsing_our_own_quality_enum()
        {
            var qualityEnums = Enum.GetValues(typeof(QualityTypes));


            foreach (var qualityEnum in qualityEnums)
            {
                var fileName = String.Format("My series S01E01 [{0}]", qualityEnum);
                var result = Parser.ParseQuality(fileName);
                result.QualityType.Should().Be(qualityEnum);
            }
        }

        [Timeout(1000)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "WEEDS", 3, new[] { 1, 2, 3, 4, 5, 6 }, 6)]
        [TestCase("Two.and.a.Half.Men.103.104.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Men", 1, new[] { 3, 4 }, 2)]
        [TestCase("Weeds.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Weeds", 3, new[] { 1, 2 }, 2)]
        [TestCase("The Borgias S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Borgias", 1, new[] { 1, 2 }, 2)]
        [TestCase("White.Collar.2x04.2x05.720p.BluRay-FUTV", "White.Collar", 2, new[] { 4, 5 }, 2)]
        [TestCase("Desperate.Housewives.S07E22E23.720p.HDTV.X264-DIMENSION", "Desperate.Housewives", 7, new[] { 22, 23 }, 2)]
        [TestCase("Desparate Housewives - S07E22 - S07E23 - And Lots of Security.. [HDTV].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 }, 2)]
        [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
        [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "", 7, new[] { 22, 23 }, 2)]
        [TestCase("2x04x05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 }, 2)]
        [TestCase("S02E04E05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 }, 2)]
        public void TitleParse_multi(string postTitle, string title, int season, int[] episodes, int count)
        {
            var result = Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers.Should().HaveSameCount(episodes);
            result.EpisodeNumbers.Should().BeEquivalentTo(result.EpisodeNumbers);
            result.CleanTitle.Should().Be(Parser.NormalizeTitle(title));
            result.EpisodeNumbers.Count.Should().Be(count);
            result.OriginalString.Should().Be(postTitle);
        }


        [TestCase("Conan 2011 04 18 Emma Roberts HDTV XviD BFF", "Conan", 2011, 04, 18)]
        [TestCase("The Tonight Show With Jay Leno 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "The Tonight Show With Jay Leno", 2011, 04, 15)]
        [TestCase("The.Daily.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "The.Daily.Show", 2010, 10, 11)]
        [TestCase("The Daily Show - 2011-04-12 - Gov. Deval Patrick", "The.Daily.Show", 2011, 04, 12)]
        [TestCase("2011.01.10 - Denis Leary - HD TV.mkv", "", 2011, 1, 10)]
        [TestCase("2011.03.13 - Denis Leary - HD TV.mkv", "", 2011, 3, 13)]
        [TestCase("The Tonight Show with Jay Leno - 2011-06-16 - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo", "The Tonight Show with Jay Leno", 2011, 6, 16)]
        public void parse_daily_episodes(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.ParseTitle(postTitle);
            var airDate = new DateTime(year, month, day);
            result.CleanTitle.Should().Be(Parser.NormalizeTitle(title));
            result.AirDate.Should().Be(airDate);
            result.EpisodeNumbers.Should().BeNull();
            result.OriginalString.Should().Be(postTitle);
        }

        [Test]
        public void parse_daily_should_fail_if_episode_is_far_in_future()
        {
            var title = string.Format("{0:yyyy.MM.dd} - Denis Leary - HD TV.mkv", DateTime.Now.AddDays(2));

            Parser.ParseTitle(title).Should().BeNull();
        }


        [TestCase("30.Rock.Season.04.HDTV.XviD-DIMENSION", "30.Rock", 4)]
        [TestCase("Parks.and.Recreation.S02.720p.x264-DIMENSION", "Parks.and.Recreation", 2)]
        [TestCase("The.Office.US.S03.720p.x264-DIMENSION", "The.Office.US", 3)]
        [TestCase(@"Sons.of.Anarchy.S03.720p.BluRay-CLUE\REWARD", "Sons.of.Anarchy", 3)]
        public void full_season_release_parse(string postTitle, string title, int season)
        {
            var result = Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.CleanTitle.Should().Be(Parser.NormalizeTitle(title));
            result.EpisodeNumbers.Count.Should().Be(0);
            result.FullSeason.Should().BeTrue();
            result.OriginalString.Should().Be(postTitle);
        }

        [TestCase("Conan", "conan")]
        [TestCase("The Tonight Show With Jay Leno", "tonightshowwithjayleno")]
        [TestCase("The.Daily.Show", "dailyshow")]
        [TestCase("Castle (2009)", "castle2009")]
        [TestCase("Parenthood.2010", "parenthood2010")]
        public void series_name_normalize(string parsedSeriesName, string seriesName)
        {
            var result = Parser.NormalizeTitle(parsedSeriesName);
            result.Should().Be(seriesName);
        }



        [TestCase("CaPitAl", "capital")]
        [TestCase("peri.od", "period")]
        [TestCase("this.^&%^**$%@#$!That", "thisthat")]
        [TestCase("test/test", "testtest")]
        [TestCase("90210", "90210")]
        [TestCase("24", "24")]
        public void Normalize_Title(string dirty, string clean)
        {
            var result = Parser.NormalizeTitle(dirty);
            result.Should().Be(clean);
        }


        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void Normalize_removed_common_words(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}.word",
                                "word {0} word",
                                "word-{0}-word",
                                "{0}.word.word",
                                "{0}-word-word",
                                "{0} word word",
                                "word.word.{0}",
                                "word-word-{0}",
                                "word-word {0}",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = String.Format(s, word);
                Parser.NormalizeTitle(dirty).Should().Be("wordword");
            }

        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void Normalize_not_removed_common_words_in_the_middle(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}word",
                                "word {0}word",
                                "word-{0}word",
                                "word{0}.word",
                                "word{0}-word",
                                "word{0}-word",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = String.Format(s, word);
                Parser.NormalizeTitle(dirty).Should().Be(("word" + word.ToLower() + "word"));
            }

        }

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Men - Season 1 [Bluray720p]", "madmen")]
        [TestCase("Mad Men - Season 1 [Bluray1080p]", "madmen")]
        [TestCase("The Daily Show With Jon Stewart -", "dailyshowwithjonstewart")]
        [TestCase("The Venture Bros. (2004)", "venturebros2004")]
        [TestCase("Castle (2011)", "castle2011")]
        public void parse_series_name(string postTitle, string title)
        {
            var result = Parser.ParseSeriesName(postTitle);
            result.Should().Be(Parser.NormalizeTitle(title));
        }


        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", LanguageType.English)]
        [TestCase("Castle.2009.S01E14.French.HDTV.XviD-LOL", LanguageType.French)]
        [TestCase("Castle.2009.S01E14.Spanish.HDTV.XviD-LOL", LanguageType.Spanish)]
        [TestCase("Castle.2009.S01E14.German.HDTV.XviD-LOL", LanguageType.German)]
        [TestCase("Castle.2009.S01E14.Germany.HDTV.XviD-LOL", LanguageType.English)]
        [TestCase("Castle.2009.S01E14.Italian.HDTV.XviD-LOL", LanguageType.Italian)]
        [TestCase("Castle.2009.S01E14.Danish.HDTV.XviD-LOL", LanguageType.Danish)]
        [TestCase("Castle.2009.S01E14.Dutch.HDTV.XviD-LOL", LanguageType.Dutch)]
        [TestCase("Castle.2009.S01E14.Japanese.HDTV.XviD-LOL", LanguageType.Japanese)]
        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL", LanguageType.Cantonese)]
        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL", LanguageType.Mandarin)]
        [TestCase("Castle.2009.S01E14.Korean.HDTV.XviD-LOL", LanguageType.Korean)]
        [TestCase("Castle.2009.S01E14.Russian.HDTV.XviD-LOL", LanguageType.Russian)]
        [TestCase("Castle.2009.S01E14.Polish.HDTV.XviD-LOL", LanguageType.Polish)]
        [TestCase("Castle.2009.S01E14.Vietnamese.HDTV.XviD-LOL", LanguageType.Vietnamese)]
        [TestCase("Castle.2009.S01E14.Swedish.HDTV.XviD-LOL", LanguageType.Swedish)]
        [TestCase("Castle.2009.S01E14.Norwegian.HDTV.XviD-LOL", LanguageType.Norwegian)]
        [TestCase("Castle.2009.S01E14.Finnish.HDTV.XviD-LOL", LanguageType.Finnish)]
        [TestCase("Castle.2009.S01E14.Turkish.HDTV.XviD-LOL", LanguageType.Turkish)]
        [TestCase("Castle.2009.S01E14.Portuguese.HDTV.XviD-LOL", LanguageType.Portuguese)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD-LOL", LanguageType.English)]
        public void parse_language(string postTitle, LanguageType language)
        {
            var result = Parser.ParseLanguage(postTitle);
            result.Should().Be(language);
        }

        [TestCase("Hawaii Five 0 S01 720p WEB DL DD5 1 H 264 NT", "Hawaii Five", 1)]
        [TestCase("30 Rock S03 WS PDTV XviD FUtV", "30 Rock", 3)]
        [TestCase("The Office Season 4 WS PDTV XviD FUtV", "The Office", 4)]
        [TestCase("Eureka Season 1 720p WEB DL DD 5 1 h264 TjHD", "Eureka", 1)]
        [TestCase("The Office Season4 WS PDTV XviD FUtV", "The Office", 4)]
        [TestCase("Eureka S 01 720p WEB DL DD 5 1 h264 TjHD", "Eureka", 1)]
        [TestCase("Doctor Who Confidential   Season 3", "Doctor Who Confidential", 3)]
        public void parse_season_info(string postTitle, string seriesName, int seasonNumber)
        {
            var result = Parser.ParseTitle(postTitle);

            result.CleanTitle.Should().Be(Parser.NormalizeTitle(seriesName));
            result.SeasonNumber.Should().Be(seasonNumber);
            result.FullSeason.Should().BeTrue();
            result.OriginalString.Should().Be(postTitle);
        }

        [TestCase("5.64 GB", 6055903887)]
        [TestCase("5.54 GiB", 5948529705)]
        [TestCase("398.62 MiB", 417983365)]
        [TestCase("7,162.1MB", 7510006170)]
        [TestCase("162.1MB", 169974170)]
        [TestCase("398.62 MB", 417983365)]
        public void parse_size(string sizeString, long expectedSize)
        {
            var result = Parser.GetReportSize(sizeString);

            result.Should().Be(expectedSize);
        }

        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Punky Brewster S01 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Instant Star S03 EXTRAS DVDRip XviD OSiTV")]
        public void parse_season_extras(string postTitle)
        {
            var result = Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("Lie.to.Me.S03.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("The.Middle.S02.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("CSI.S11.SUBPACK.DVDRip.XviD-REWARD")]
        public void parse_season_subpack(string postTitle)
        {
            var result = Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("Fussball Bundesliga 10e2011e30 Spieltag FC Bayern Muenchen vs Bayer 04 Leverkusen German WS dTV XviD WoGS")]
        public void unparsable_should_log_error_but_not_throw(string title)
        {
            Parser.ParseTitle(title);
            ExceptionVerification.IgnoreWarns();
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
