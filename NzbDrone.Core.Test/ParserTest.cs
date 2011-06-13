// ReSharper disable RedundantUsingDirective
using System;
using FluentAssertions;
using NUnit.Framework;
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
    }
}