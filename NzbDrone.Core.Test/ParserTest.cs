using System;
using System.Threading;
using MbUnit.Framework;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ParserTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * The.Kennedys.Part.2.DSR.XviD-SYS
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         */

        [Test]
        [Row("Sonny.With.a.Chance.S02E15", "Sonny.With.a.Chance", 2, 15)]
        [Row("Two.and.a.Half.Me.103.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 1, 3)]
        [Row("Two.and.a.Half.Me.113.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 1, 13)]
        [Row("Two.and.a.Half.Me.1013.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Me", 10, 13)]
        [Row("Chuck.4x05.HDTV.XviD-LOL", "Chuck", 4, 5)]
        [Row("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", "The.Girls.Next.Door", 3, 6)]
        [Row("Degrassi.S10E27.WS.DSR.XviD-2HD", "Degrassi", 10, 27)]
        [Row("Parenthood.2010.S02E14.HDTV.XviD-LOL", "Parenthood 2010", 2, 14)]
        [Row("Hawaii Five 0 S01E19 720p WEB DL DD5 1 H 264 NT", "Hawaii Five", 1, 19)]
        [Row("The Event S01E14 A Message Back 720p WEB DL DD5 1 H264 SURFER", "The Event", 1, 14)]
        [Row("Adam Hills In Gordon St Tonight S01E07 WS PDTV XviD FUtV", "Adam Hills In Gordon St Tonight", 1, 7)]
        [Row("Adam Hills In Gordon St Tonight S01E07 WS PDTV XviD FUtV", "Adam Hills In Gordon St Tonight", 1, 7)]
        [Row("Adventure.Inc.S03E19.DVDRip.XviD-OSiTV", "Adventure.Inc", 3, 19)]
        [Row("S03E09 WS PDTV XviD FUtV", "", 3, 9)]
        [Row("5x10 WS PDTV XviD FUtV", "", 5, 10)]
        [Row("Castle.2009.S01E14.HDTV.XviD-LOL", "Castle 2009", 1, 14)]
        [Row("Pride.and.Prejudice.1995.S03E20.HDTV.XviD-LOL", "Pride and Prejudice 1995", 3, 20)]
        //[Row(@"Season 4\07 WS PDTV XviD FUtV", "", 4, 7)]
        [Row("The.Office.S03E115.DVDRip.XviD-OSiTV", "The.Office", 3, 115)]
        [Row(@"Parks and Recreation - S02E21 - 94 Meetings - 720p TV.mkv", "Parks and Recreation", 2, 21)]
        public void episode_parse(string postTitle, string title, int season, int episode)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.Episodes[0]);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(1, result.Episodes.Count);
        }

        [Test]
        [Row(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
        [Row(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
        [Row(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
        [Row(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
        public void file_path_parse(string path, int season, int episode)
        {
            var result = Parser.ParseEpisodeInfo(path);
            Assert.Count(1, result.Episodes);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.Episodes[0]);
        }


        [Test]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", QualityTypes.BDRip)]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", QualityTypes.BDRip)]
        [Row("Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", QualityTypes.HDTV)]
        [Row("Chuck.S04E05.HDTV.XviD-LOL", QualityTypes.TV)]
        [Row("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", QualityTypes.DVD)]
        [Row("Degrassi.S10E27.WS.DSR.XviD-2HD", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", QualityTypes.WEBDL)]
        [Row("Sonny.With.a.Chance.S02E15.720p", QualityTypes.HDTV)]
        [Row("Sonny.With.a.Chance.S02E15.mkv", QualityTypes.HDTV)]
        [Row("Sonny.With.a.Chance.S02E15.avi", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.xvid", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.divx", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15", QualityTypes.Unknown)]
        [Row("Chuck - S01E04 - So Old - Playdate - 720p TV.mkv", QualityTypes.HDTV)]
        [Row("Chuck - S22E03 - MoneyBART - HD TV.mkv", QualityTypes.HDTV)]
        [Row("Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", QualityTypes.Bluray720)]
        [Row("Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", QualityTypes.Bluray1080)]
        [Row("Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", QualityTypes.WEBDL)]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.BDRip)]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", QualityTypes.BDRip)]
        [Row("Law & Order: Special Victims Unit - 11x11 - Quickie", QualityTypes.Unknown)]
        public void quality_parse(string postTitle, object quality)
        {
            var result = Parser.ParseEpisodeInfo(postTitle).Quality;
            Assert.AreEqual(quality, result);
        }

        [Test]
        [Timeout(1)]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "WEEDS", 3, new[] { 1, 2, 3, 4, 5, 6 }, 6)]
        [Row("Two.and.a.Half.Men.103.104.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Men", 1, new[] { 3, 4 }, 2)]
        [Row("Weeds.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Weeds", 3, new[] { 1, 2 }, 2)]
        [Row("The Borgias S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Borgias", 1, new[] { 1, 2 }, 2)]
        [Row("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
        [Row("White.Collar.2x04.2x05.720p.BluRay-FUTV", "White.Collar", 2, new[] { 4, 5 }, 2)]
        //[Row("The.Kennedys.Part.1.and.Part.2.DSR.XviD-SYS", 1, new[] { 1, 2 })]
        public void episode_multipart_parse(string postTitle, string title, int season, int[] episodes, int count)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.Count(episodes.Length, result.Episodes);
            Assert.AreElementsEqualIgnoringOrder(episodes, result.Episodes);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(count, result.Episodes.Count);
        }

        [Test]
        [Row("Conan 2011 04 18 Emma Roberts HDTV XviD BFF", "Conan", 2011, 04, 18)]
        [Row("The Tonight Show With Jay Leno 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "The Tonight Show With Jay Leno", 2011, 04, 15)]
        [Row("The.Daily.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "The.Daily.Show", 2010, 10, 11)]
        [Row("The Daily Show - 2011-04-12 - Gov. Deval Patrick", "The.Daily.Show", 2011, 04, 12)]
        public void episode_daily_parse(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.ParseEpisodeInfo(postTitle);
            var airDate = new DateTime(year, month, day);
            Assert.AreEqual(Parser.NormalizeTitle(title), result.CleanTitle);
            Assert.AreEqual(airDate, result.AirDate);
        }

        [Test]
        [Row("Conan", "conan")]
        [Row("The Tonight Show With Jay Leno", "tonightshowwithjayleno")]
        [Row("The.Daily.Show", "dailyshow")]
        [Row("Castle (2009)", "castle2009")]
        [Row("Parenthood.2010", "parenthood2010")]
        public void series_name_normalize(string parsedSeriesName, string seriesName)
        {
            var result = Parser.NormalizeTitle(parsedSeriesName);
            Assert.AreEqual(seriesName, result);
        }

        [Test]
        [Row(@"c:\test\", @"c:\test")]
        [Row(@"c:\\test\\", @"c:\test")]
        [Row(@"C:\\Test\\", @"C:\Test")]
        [Row(@"C:\\Test\\Test\", @"C:\Test\Test")]
        [Row(@"\\Testserver\Test\", @"\\Testserver\Test")]
        public void Normalize_Path(string dirty, string clean)
        {
            var result = Parser.NormalizePath(dirty);
            Assert.AreEqual(clean, result);
        }

        [Test]
        [Row("CaPitAl", "capital")]
        [Row("peri.od", "period")]
        [Row("this.^&%^**$%@#$!That", "thisthat")]
        public void Normalize_Title(string dirty, string clean)
        {
            var result = Parser.NormalizeTitle(dirty);
            Assert.AreEqual(clean, result);
        }

        [Test]
        [Row("the")]
        [Row("and")]
        [Row("or")]
        [Row("a")]
        [Row("an")]
        [Row("of")]
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

        [Test]
        [Row("the")]
        [Row("and")]
        [Row("or")]
        [Row("a")]
        [Row("an")]
        [Row("of")]
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