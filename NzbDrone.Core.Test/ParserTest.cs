// ReSharper disable RedundantUsingDirective
using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ParserTest : TestBase
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * The.Kennedys.Part.2.DSR.XviD-SYS
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
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
        //[Row(@"Season 4\07 WS PDTV XviD FUtV", "", 4, 7)]
        [TestCase("The.Office.S03E115.DVDRip.XviD-OSiTV", "The.Office", 3, 115)]
        [TestCase(@"Parks and Recreation - S02E21 - 94 Meetings - 720p TV.mkv", "Parks and Recreation", 2, 21)]
        [TestCase(@"24-7 Penguins-Capitals- Road to the NHL Winter Classic - S01E03 - Episode 3.mkv", "24-7 Penguins-Capitals- Road to the NHL Winter Classic", 1, 3)]
        [TestCase("Adventure.Inc.S03E19.DVDRip.\"XviD\"-OSiTV", "Adventure.Inc", 3, 19)]
        [TestCase("C:/Test/TV/Chuck.4x05.HDTV.XviD-LOL", "Chuck", 4, 5)]
        [TestCase("Hawaii Five-0 (2010) - 1x05 - Nalowale (Forgotten/Missing)", "Hawaii Five-0 (2010)", 1, 5)]
        [TestCase("Hawaii Five-0 (2010) - 1x05 - Title", "Hawaii Five-0 (2010)", 1, 5)]
        public void episode_parse(string postTitle, string title, int season, int episode)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumbers[0]);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(1, result.EpisodeNumbers.Count);
        }

        [Test]
        [TestCase(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
        [TestCase(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Battlestar Galactica (2003)\Season 2\S02E21.avi", 2, 21)]
        public void file_path_parse(string path, int season, int episode)
        {
            var result = Parser.ParseEpisodeInfo(path);
            result.EpisodeNumbers.Should().HaveCount(1);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumbers[0]);
        }

        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", QualityTypes.DVD)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", QualityTypes.DVD)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", QualityTypes.DVD)]
        [TestCase("Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", QualityTypes.HDTV)]
        [TestCase("this has no extention or periods HDTV", QualityTypes.SDTV)]
        [TestCase("Chuck.S04E05.HDTV.XviD-LOL", QualityTypes.SDTV)]
        [TestCase("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", QualityTypes.DVD)]
        [TestCase("The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", QualityTypes.DVD)]
        [TestCase("The.Girls.Next.Door.S03E06.HDTV-WiDE", QualityTypes.SDTV)]
        [TestCase("Degrassi.S10E27.WS.DSR.XviD-2HD", QualityTypes.SDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", QualityTypes.WEBDL)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p", QualityTypes.HDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15.mkv", QualityTypes.HDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15.avi", QualityTypes.SDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15.xvid", QualityTypes.SDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15.divx", QualityTypes.SDTV)]
        [TestCase("Sonny.With.a.Chance.S02E15", QualityTypes.Unknown)]
        [TestCase("Chuck - S01E04 - So Old - Playdate - 720p TV.mkv", QualityTypes.HDTV)]
        [TestCase("Chuck - S22E03 - MoneyBART - HD TV.mkv", QualityTypes.HDTV)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", QualityTypes.Bluray720p)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", QualityTypes.Bluray1080p)]
        [TestCase("Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", QualityTypes.WEBDL)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.DVD)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.DVD)]
        [TestCase("Law & Order: Special Victims Unit - 11x11 - Quickie", QualityTypes.Unknown)]
        [TestCase("(<a href=\"http://www.newzbin.com/browse/post/6076286/nzb/\">NZB</a>)", QualityTypes.Unknown)]
        [TestCase("S07E23 - [HDTV].mkv ", QualityTypes.HDTV)]
        [TestCase("S07E23 - [WEBDL].mkv ", QualityTypes.WEBDL)]
        [TestCase("S07E23.mkv ", QualityTypes.HDTV)]
        [TestCase("S07E23 .avi ", QualityTypes.SDTV)]
        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", QualityTypes.DVD)]
        [TestCase("WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", QualityTypes.Bluray720p)]
        public void quality_parse(string postTitle, object quality)
        {
            var result = Parser.ParseQuality(postTitle);
            Assert.AreEqual(quality, result.QualityType);
        }

        [Test]
        public void parsing_our_own_quality_enum()
        {
            var qualityEnums = Enum.GetValues(typeof(QualityTypes));


            foreach (var qualityEnum in qualityEnums)
            {
                if (qualityEnum.ToString() == QualityTypes.Unknown.ToString()) continue;

                var extention = "mkv";

                if (qualityEnum.ToString() == QualityTypes.SDTV.ToString() || qualityEnum.ToString() == QualityTypes.DVD.ToString())
                {
                    extention = "avi";
                }

                var fileName = String.Format("My series S01E01 [{0}].{1}", qualityEnum, extention);
                var result = Parser.ParseQuality(fileName);
                Assert.AreEqual(qualityEnum, result.QualityType);
            }
        }
    
        [Timeout(1000)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "WEEDS", 3, new[] { 1, 2, 3, 4, 5, 6 }, 6)]
        [TestCase("Two.and.a.Half.Men.103.104.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Men", 1, new[] { 3, 4 }, 2)]
        [TestCase("Weeds.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Weeds", 3, new[] { 1, 2 }, 2)]
        [TestCase("The Borgias S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Borgias", 1, new[] { 1, 2 }, 2)]
        [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
        [TestCase("White.Collar.2x04.2x05.720p.BluRay-FUTV", "White.Collar", 2, new[] { 4, 5 }, 2)]
        [TestCase("Desperate.Housewives.S07E22E23.720p.HDTV.X264-DIMENSION", "Desperate.Housewives", 7, new[] { 22, 23 }, 2)]
        //[Row("The.Kennedys.Part.1.and.Part.2.DSR.XviD-SYS", 1, new[] { 1, 2 })]
        [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "", 7, new[] { 22, 23 }, 2)]
        [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
        [TestCase("Desparate Housewives - S07E22 - S07E23 - And Lots of Security.. [HDTV].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 }, 2)]
        public void episode_multipart_parse(string postTitle, string title, int season, int[] episodes, int count)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            result.EpisodeNumbers.Should().HaveSameCount(episodes);
            result.EpisodeNumbers.Should().BeEquivalentTo(result.EpisodeNumbers);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(count, result.EpisodeNumbers.Count);
        }

       
        [TestCase("Conan 2011 04 18 Emma Roberts HDTV XviD BFF", "Conan", 2011, 04, 18)]
        [TestCase("The Tonight Show With Jay Leno 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "The Tonight Show With Jay Leno", 2011, 04, 15)]
        [TestCase("The.Daily.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "The.Daily.Show", 2010, 10, 11)]
        [TestCase("The Daily Show - 2011-04-12 - Gov. Deval Patrick", "The.Daily.Show", 2011, 04, 12)]
        [TestCase("2011.01.10 - Denis Leary - HD TV.mkv", "", 2011, 1, 10)]
        [TestCase("2011.03.13 - Denis Leary - HD TV.mkv", "", 2011, 3, 13)]
        [TestCase("The Tonight Show with Jay Leno - 2011-06-16 - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo", "The Tonight Show with Jay Leno", 2011, 6, 16)]
        public void episode_daily_parse(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            var airDate = new DateTime(year, month, day);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(airDate, result.AirDate);
            Assert.IsNull(result.EpisodeNumbers);
        }

       
        [TestCase("30.Rock.Season.04.HDTV.XviD-DIMENSION", "30.Rock", 4)]
        [TestCase("Parks.and.Recreation.S02.720p.x264-DIMENSION", "Parks.and.Recreation", 2)]
        [TestCase("The.Office.US.S03.720p.x264-DIMENSION", "The.Office.US", 3)]
        public void full_season_release_parse(string postTitle, string title, int season)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(0, result.EpisodeNumbers.Count);
        }
      
        [TestCase("Conan", "conan")]
        [TestCase("The Tonight Show With Jay Leno", "tonightshowwithjayleno")]
        [TestCase("The.Daily.Show", "dailyshow")]
        [TestCase("Castle (2009)", "castle2009")]
        [TestCase("Parenthood.2010", "parenthood2010")]
        public void series_name_normalize(string parsedSeriesName, string seriesName)
        {
            var result = Parser.NormalizeTitle(parsedSeriesName);
            Assert.AreEqual(seriesName, result);
        }

        [TestCase(@"c:\test\", @"c:\test")]
        [TestCase(@"c:\\test\\", @"c:\test")]
        [TestCase(@"C:\\Test\\", @"C:\Test")]
        [TestCase(@"C:\\Test\\Test\", @"C:\Test\Test")]
        [TestCase(@"\\Testserver\Test\", @"\\Testserver\Test")]
        public void Normalize_Path(string dirty, string clean)
        {
            var result = Parser.NormalizePath(dirty);
            Assert.AreEqual(clean, result);
        }
      
        [TestCase("CaPitAl", "capital")]
        [TestCase("peri.od", "period")]
        [TestCase("this.^&%^**$%@#$!That", "thisthat")]
        [TestCase("test/test", "testtest")]
        public void Normalize_Title(string dirty, string clean)
        {
            var result = Parser.NormalizeTitle(dirty);
            Assert.AreEqual(clean, result);
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
                Assert.AreEqual("wordword", Parser.NormalizeTitle(dirty));
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
                Assert.AreEqual("word" + word.ToLower() + "word", Parser.NormalizeTitle(dirty));
            }

        }

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("This Isn't a Valid Post", "")]
        public void parse_series_name(string postTitle, string title)
        {
            var result = Parser.ParseSeriesName(postTitle);
            Assert.AreEqual(Parser.NormalizeTitle(title), result);
        }

        [TestCase("Chuck - 4x05 - Title [Proper]", true)]
        [TestCase("Law & Order - 4x05 - Title", false)]
        [TestCase("30.Rock.S04E05.asdjasdj.proper.", true)]
        public void parse_proper(string postTitle, bool proper)
        {
            var result = Parser.ParseProper(postTitle);
            Assert.AreEqual(proper, result);
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
            Assert.AreEqual(language, result);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Path can not be null or empty")]
        public void normalize_path_exception_empty()
        {
            Parser.NormalizePath("");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Path can not be null or empty")]
        public void normalize_path_exception_null()
        {
            Parser.NormalizePath(null);
        }

        [TestCase("Hawaii Five 0 S01E19 720p WEB DL DD5 1 H 264 NT", "Hawaii Five", 1)]
        [TestCase("Chuck.4x05.HDTV.XviD-LOL", "Chuck", 4)]
        [TestCase("S03E09 WS PDTV XviD FUtV", "", 3)]
        [TestCase("5x10 WS PDTV XviD FUtV", "", 5)]
        public void parse_season_info(string postTitle, string seriesName, int seasonNumber)
        {
            var result = Parser.ParseSeasonInfo(postTitle);

            Assert.AreEqual(Parser.NormalizeTitle(seriesName), result.SeriesTitle);
            Assert.AreEqual(seasonNumber, result.SeasonNumber);
        }

        [Test]
        public void parse_season_info_null()
        {
            var result = Parser.ParseSeasonInfo("This is not a valid post");

            Assert.AreEqual(null, result);
        }
    }
}