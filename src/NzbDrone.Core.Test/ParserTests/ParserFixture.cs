using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Expansive;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class ParserFixture : CoreTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         * [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
         * [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
         * [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 }, 2)]
         * (Game of Thrones s03 e - "Game of Thrones Season 3 Episode 10"
         * The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-06.34.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
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
        [TestCase("CSI.525", "CSI", 5, 25)]
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
        [TestCase("extras.s03.e05.ws.dvdrip.xvid-m00tv", "Extras", 3, 5)]
        [TestCase("castle.2009.416.hdtv-lol", "Castle 2009", 4, 16)]
        [TestCase("hawaii.five-0.2010.217.hdtv-lol", "Hawaii Five-0 (2010)", 2, 17)]
        [TestCase("Looney Tunes - S1936E18 - I Love to Singa", "Looney Tunes", 1936, 18)]
        [TestCase("American_Dad!_-_7x6_-_The_Scarlett_Getter_[SDTV]", "American Dad!", 7, 6)]
        [TestCase("Falling_Skies_-_1x1_-_Live_and_Learn_[HDTV-720p]", "Falling Skies", 1, 1)]
        [TestCase("Top Gear - 07x03 - 2005.11.70", "Top Gear", 7, 3)]
        [TestCase("Hatfields and McCoys 2012 Part 1 REPACK 720p HDTV x264 2HD", "Hatfields and McCoys 2012", 1, 1)]
        [TestCase("Glee.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", "Glee", 4, 9)]
        [TestCase("S08E20 50-50 Carla [DVD]", "", 8, 20)]
        [TestCase("Cheers S08E20 50-50 Carla [DVD]", "Cheers", 8, 20)]
        [TestCase("S02E10 6-50 to SLC [SDTV]", "", 2, 10)]
        [TestCase("Franklin & Bash S02E10 6-50 to SLC [SDTV]", "Franklin & Bash", 2, 10)]
        [TestCase("The_Big_Bang_Theory_-_6x12_-_The_Egg_Salad_Equivalency_[HDTV-720p]", "The Big Bang Theory", 6, 12)]
        [TestCase("Top_Gear.19x06.720p_HDTV_x264-FoV", "Top Gear", 19, 6)]
        [TestCase("Portlandia.S03E10.Alexandra.720p.WEB-DL.AAC2.0.H.264-CROM.mkv", "Portlandia", 3, 10)]
        [TestCase("(Game of Thrones s03 e - \"Game of Thrones Season 3 Episode 10\"", "Game of Thrones", 3, 10)]
        [TestCase("House.Hunters.International.S05E607.720p.hdtv.x264", "House.Hunters.International", 5, 607)]
        [TestCase("Adventure.Time.With.Finn.And.Jake.S01E20.720p.BluRay.x264-DEiMOS", "Adventure.Time.With.Finn.And.Jake", 1, 20)]
        [TestCase("Hostages.S01E04.2-45.PM.[HDTV-720p].mkv", "Hostages", 1, 4)]
        public void ParseTitle_single(string postTitle, string title, int seasonNumber, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.First().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
        }

        [TestCase(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
        [TestCase(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
        [TestCase(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
        [TestCase(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
        [TestCase(@"D:\shares\TV Shows\Battlestar Galactica (2003)\Season 2\S02E21.avi", 2, 21)]
        [TestCase("C:/Test/TV/Chuck.4x05.HDTV.XviD-LOL", 4, 5)]
        [TestCase(@"P:\TV Shows\House\Season 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
        [TestCase(@"S:\TV Drop\House - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
        [TestCase(@"/TV Drop/House - 10x11 - Title [SDTV]/1011 - Title.avi", 10, 11)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\1012 - 24 Hour Propane People.avi", 10, 12)]
        [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/1012 - 24 Hour Propane People.avi", 10, 12)]
        [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\Hour Propane People.avi", 10, 12)]
        [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/Hour Propane People.avi", 10, 12)]
        [TestCase(@"E:\Downloads\tv\The.Big.Bang.Theory.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", 1, 1)]
        [TestCase(@"C:\Test\Unsorted\The.Big.Bang.Theory.S01E01.720p.HDTV\tbbt101.avi", 1, 1)]
        public void PathParse_tests(string path, int season, int episode)
        {
            var result = Parser.Parser.ParsePath(path);
            result.EpisodeNumbers.Should().HaveCount(1);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers[0].Should().Be(episode);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("THIS SHOULD NEVER PARSE")]
        public void unparsable_title_should_log_warn_and_return_null(string title)
        {
            Parser.Parser.ParseTitle(title).Should().BeNull();
        }

        //[Timeout(1000)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", "WEEDS", 3, new[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase("Two.and.a.Half.Men.103.104.720p.HDTV.X264-DIMENSION", "Two.and.a.Half.Men", 1, new[] { 3, 4 })]
        [TestCase("Weeds.S03E01.S03E02.720p.HDTV.X264-DIMENSION", "Weeds", 3, new[] { 1, 2 })]
        [TestCase("The Borgias S01e01 e02 ShoHD On Demand 1080i DD5 1 ALANiS", "The Borgias", 1, new[] { 1, 2 })]
        [TestCase("White.Collar.2x04.2x05.720p.BluRay-FUTV", "White.Collar", 2, new[] { 4, 5 })]
        [TestCase("Desperate.Housewives.S07E22E23.720p.HDTV.X264-DIMENSION", "Desperate.Housewives", 7, new[] { 22, 23 })]
        [TestCase("Desparate Housewives - S07E22 - S07E23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 })]
        [TestCase("S03E01.S03E02.720p.HDTV.X264-DIMENSION", "", 3, new[] { 1, 2 })]
        [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 })]
        [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 })]
        [TestCase("2x04x05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E04E05.720p.BluRay-FUTV", "", 2, new[] { 4, 5 })]
        [TestCase("S02E03-04-05.720p.BluRay-FUTV", "", 2, new[] { 3, 4, 5 })]
        [TestCase("Breakout.Kings.S02E09-E10.HDTV.x264-ASAP", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Breakout Kings - 2x9-2x10 - Served Cold [SDTV] ", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Breakout Kings - 2x09-2x10 - Served Cold [SDTV] ", "Breakout Kings", 2, new[] { 9, 10 })]
        [TestCase("Hell on Wheels S02E09 E10 HDTV x264 EVOLVE", "Hell on Wheels", 2, new[] { 9, 10 })]
        [TestCase("Hell.on.Wheels.S02E09-E10.720p.HDTV.x264-EVOLVE", "Hell on Wheels", 2, new[] { 9, 10 })]
        [TestCase("Grey's Anatomy - 8x01_02 - Free Falling", "Grey's Anatomy", 8, new [] { 1,2 })]
        [TestCase("8x01_02 - Free Falling", "", 8, new[] { 1, 2 })]
        [TestCase("Kaamelott.S01E91-E100", "Kaamelott", 1,new[] { 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 })]
        public void TitleParse_multi(string postTitle, string title, int season, int[] episodes)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.EpisodeNumbers.Should().BeEquivalentTo(episodes);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
        }


        [TestCase("Conan 2011 04 18 Emma Roberts HDTV XviD BFF", "Conan", 2011, 04, 18)]
        [TestCase("The Tonight Show With Jay Leno 2011 04 15 1080i HDTV DD5 1 MPEG2 TrollHD", "The Tonight Show With Jay Leno", 2011, 04, 15)]
        [TestCase("The.Daily.Show.2010.10.11.Johnny.Knoxville.iTouch-MW", "The.Daily.Show", 2010, 10, 11)]
        [TestCase("The Daily Show - 2011-04-12 - Gov. Deval Patrick", "The.Daily.Show", 2011, 04, 12)]
        [TestCase("2011.01.10 - Denis Leary - HD TV.mkv", "", 2011, 1, 10)]
        [TestCase("2011.03.13 - Denis Leary - HD TV.mkv", "", 2011, 3, 13)]
        [TestCase("The Tonight Show with Jay Leno - 2011-06-16 - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo", "The Tonight Show with Jay Leno", 2011, 6, 16)]
        [TestCase("2020.NZ.2012.16.02.PDTV.XviD-C4TV", "2020nz", 2012, 2, 16)]
        [TestCase("2020.NZ.2012.13.02.PDTV.XviD-C4TV", "2020nz", 2012, 2, 13)]
        [TestCase("2020.NZ.2011.12.02.PDTV.XviD-C4TV", "2020nz", 2011, 12, 2)]
        public void parse_daily_episodes(string postTitle, string title, int year, int month, int day)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            var airDate = new DateTime(year, month, day);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
            result.AirDate.Should().Be(airDate.ToString(Episode.AIR_DATE_FORMAT));
            result.EpisodeNumbers.Should().BeNull();
        }

        [TestCase("[SubDESU]_High_School_DxD_07_(1280x720_x264-AAC)_[6B7FD717]", "High School DxD", 7, 0, 0)]
        [TestCase("[Chihiro]_Working!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Working!!", 6, 0, 0)]
        [TestCase("[Commie]_Senki_Zesshou_Symphogear_-_11_[65F220B4]", "Senki_Zesshou_Symphogear", 11, 0, 0)]
        [TestCase("[Underwater]_Rinne_no_Lagrange_-_12_(720p)_[5C7BC4F9]", "Rinne_no_Lagrange", 12, 0, 0)]
        [TestCase("[Commie]_Rinne_no_Lagrange_-_15_[E76552EA]", "Rinne_no_Lagrange", 15, 0, 0)]
        [TestCase("[HorribleSubs]_Hunter_X_Hunter_-_33_[720p]", "Hunter_X_Hunter", 33, 0, 0)]
        [TestCase("[HorribleSubs]_Fairy_Tail_-_145_[720p]", "Fairy_Tail", 145, 0, 0)]
        [TestCase("[HorribleSubs] Tonari no Kaibutsu-kun - 13 [1080p].mkv", "Tonari no Kaibutsu-kun", 13, 0, 0)]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", "Yes.Pretty.Cure.5.Go.Go!", 31, 0, 0)]
        [TestCase("[K-F] One Piece 214", "One Piece", 214, 0, 0)]
        [TestCase("[K-F] One Piece S10E14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 10x14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 214 10x14", "One Piece", 214, 10, 14)]
        [TestCase("One Piece S10E14 214", "One Piece", 214, 10, 14)]
        [TestCase("One Piece 10x14 214", "One Piece", 214, 10, 14)]
        [TestCase("One Piece 214 10x14", "One Piece", 214, 10, 14)]
        [TestCase("214 One Piece 10x14", "One Piece", 214, 10, 14)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar].avi", "Bleach", 31, 0, 0)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar]", "Bleach", 31, 0, 0)]
        [TestCase("[ACX]Hack Sign 01 Role Play [Kosaka] [9C57891E].mkv", "Hack Sign", 1, 0, 0)]
        [TestCase("[SFW-sage] Bakuman S3 - 12 [720p][D07C91FC]", "Bakuman S3", 12, 0, 0)]
        [TestCase("ducktales_e66_time_is_money_part_one_marking_time", "DuckTales", 66, 0, 0)]
        public void parse_absolute_numbers(string postTitle, string title, int absoluteEpisodeNumber, int seasonNumber, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.First().Should().Be(absoluteEpisodeNumber);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.FirstOrDefault().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
        }


        [TestCase("Conan {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("The Tonight Show With Jay Leno {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("The.Daily.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("The Daily Show - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - Denis Leary - HD TV.mkv")]
        [TestCase("The Tonight Show with Jay Leno - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_ancient_daily_series(string title)
        {
            var yearTooLow = title.Expand(new { year = 1950, month = 10, day = 14 });
            Parser.Parser.ParseTitle(yearTooLow).Should().BeNull();
        }


        [TestCase("Conan {year} {month} {day} Emma Roberts HDTV XviD BFF")]
        [TestCase("The Tonight Show With Jay Leno {year} {month} {day} 1080i HDTV DD5 1 MPEG2 TrollHD")]
        [TestCase("The.Daily.Show.{year}.{month}.{day}.Johnny.Knoxville.iTouch-MW")]
        [TestCase("The Daily Show - {year}-{month}-{day} - Gov. Deval Patrick")]
        [TestCase("{year}.{month}.{day} - Denis Leary - HD TV.mkv")]
        [TestCase("The Tonight Show with Jay Leno - {year}-{month}-{day} - Larry David, \"Bachelorette\" Ashley Hebert, Pitbull with Ne-Yo")]
        [TestCase("2020.NZ.{year}.{month}.{day}.PDTV.XviD-C4TV")]
        public void should_not_accept_future_dates(string title)
        {
            var twoDaysFromNow = DateTime.Now.AddDays(2);

            var validDate = title.Expand(new { year = twoDaysFromNow.Year, month = twoDaysFromNow.Month.ToString("00"), day = twoDaysFromNow.Day.ToString("00") });

            Parser.Parser.ParseTitle(validDate).Should().BeNull();
        }

        [Test]
        public void parse_daily_should_fail_if_episode_is_far_in_future()
        {
            var title = string.Format("{0:yyyy.MM.dd} - Denis Leary - HD TV.mkv", DateTime.Now.AddDays(2));

            Parser.Parser.ParseTitle(title).Should().BeNull();
        }

        [TestCase("30.Rock.Season.04.HDTV.XviD-DIMENSION", "30.Rock", 4)]
        [TestCase("Parks.and.Recreation.S02.720p.x264-DIMENSION", "Parks.and.Recreation", 2)]
        [TestCase("The.Office.US.S03.720p.x264-DIMENSION", "The.Office.US", 3)]
        [TestCase(@"Sons.of.Anarchy.S03.720p.BluRay-CLUE\REWARD", "Sons.of.Anarchy", 3)]
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "Adventure Time", 2)]
        public void full_season_release_parse(string postTitle, string title, int season)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.SeasonNumber.Should().Be(season);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
            result.EpisodeNumbers.Length.Should().Be(0);
            result.FullSeason.Should().BeTrue();
        }

        [TestCase("Conan", "conan")]
        [TestCase("The Tonight Show With Jay Leno", "tonightshowwithjayleno")]
        [TestCase("The.Daily.Show", "dailyshow")]
        [TestCase("Castle (2009)", "castle2009")]
        [TestCase("Parenthood.2010", "parenthood2010")]
        [TestCase("Law_and_Order_SVU", "lawordersvu")]
        public void series_name_normalize(string parsedSeriesName, string seriesName)
        {
            var result = parsedSeriesName.CleanSeriesTitle();
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
            var result = dirty.CleanSeriesTitle();
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
                dirty.CleanSeriesTitle().Should().Be("wordword");
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
                dirty.CleanSeriesTitle().Should().Be(("word" + word.ToLower() + "word"));
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
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "adventuretime")]
        public void parse_series_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseSeriesName(postTitle);
            result.Should().Be(title.CleanSeriesTitle());
        }

        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", Language.English)]
        [TestCase("Castle.2009.S01E14.French.HDTV.XviD-LOL", Language.French)]
        [TestCase("Castle.2009.S01E14.Spanish.HDTV.XviD-LOL", Language.Spanish)]
        [TestCase("Castle.2009.S01E14.German.HDTV.XviD-LOL", Language.German)]
        [TestCase("Castle.2009.S01E14.Germany.HDTV.XviD-LOL", Language.English)]
        [TestCase("Castle.2009.S01E14.Italian.HDTV.XviD-LOL", Language.Italian)]
        [TestCase("Castle.2009.S01E14.Danish.HDTV.XviD-LOL", Language.Danish)]
        [TestCase("Castle.2009.S01E14.Dutch.HDTV.XviD-LOL", Language.Dutch)]
        [TestCase("Castle.2009.S01E14.Japanese.HDTV.XviD-LOL", Language.Japanese)]
        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL", Language.Cantonese)]
        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL", Language.Mandarin)]
        [TestCase("Castle.2009.S01E14.Korean.HDTV.XviD-LOL", Language.Korean)]
        [TestCase("Castle.2009.S01E14.Russian.HDTV.XviD-LOL", Language.Russian)]
        [TestCase("Castle.2009.S01E14.Polish.HDTV.XviD-LOL", Language.Polish)]
        [TestCase("Castle.2009.S01E14.Vietnamese.HDTV.XviD-LOL", Language.Vietnamese)]
        [TestCase("Castle.2009.S01E14.Swedish.HDTV.XviD-LOL", Language.Swedish)]
        [TestCase("Castle.2009.S01E14.Norwegian.HDTV.XviD-LOL", Language.Norwegian)]
        [TestCase("Castle.2009.S01E14.Finnish.HDTV.XviD-LOL", Language.Finnish)]
        [TestCase("Castle.2009.S01E14.Turkish.HDTV.XviD-LOL", Language.Turkish)]
        [TestCase("Castle.2009.S01E14.Portuguese.HDTV.XviD-LOL", Language.Portuguese)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD-LOL", Language.English)]
        [TestCase("person.of.interest.1x19.ita.720p.bdmux.x264-novarip", Language.Italian)]
        [TestCase("Salamander.S01E01.FLEMISH.HDTV.x264-BRiGAND", Language.Flemish)]
        [TestCase("H.Polukatoikia.S03E13.Greek.PDTV.XviD-Ouzo", Language.Greek)]
        [TestCase("Burn.Notice.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP", Language.German)]
        [TestCase("Ray Donovan - S01E01.720p.HDtv.x264-Evolve (NLsub)", Language.Norwegian)]
        [TestCase("Shield,.The.1x13.Tueurs.De.Flics.FR.DVDRip.XviD", Language.French)]
        public void parse_language(string postTitle, Language language)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Should().Be(language);
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
            var result = Parser.Parser.ParseTitle(postTitle);

            result.SeriesTitle.Should().Be(seriesName.CleanSeriesTitle());
            result.SeasonNumber.Should().Be(seasonNumber);
            result.FullSeason.Should().BeTrue();
        }

        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Punky Brewster S01 EXTRAS DVDRip XviD RUNNER")]
        [TestCase("Instant Star S03 EXTRAS DVDRip XviD OSiTV")]
        public void parse_season_extras(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("Lie.to.Me.S03.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("The.Middle.S02.SUBPACK.DVDRip.XviD-REWARD")]
        [TestCase("CSI.S11.SUBPACK.DVDRip.XviD-REWARD")]
        public void parse_season_subpack(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);

            result.Should().BeNull();
        }

        [TestCase("76El6LcgLzqb426WoVFg1vVVVGx4uCYopQkfjmLe")]
        [TestCase("Vrq6e1Aba3U amCjuEgV5R2QvdsLEGYF3YQAQkw8")]
        [TestCase("TDAsqTea7k4o6iofVx3MQGuDK116FSjPobMuh8oB")]
        [TestCase("yp4nFodAAzoeoRc467HRh1mzuT17qeekmuJ3zFnL")]
        [TestCase("oxXo8S2272KE1 lfppvxo3iwEJBrBmhlQVK1gqGc")]
        [TestCase("dPBAtu681Ycy3A4NpJDH6kNVQooLxqtnsW1Umfiv")]
        [TestCase("password - \"bdc435cb-93c4-4902-97ea-ca00568c3887.337\" yEnc")]
        public void should_not_parse_crap(string title)
        {
            Parser.Parser.ParseTitle(title).Should().BeNull();
            ExceptionVerification.IgnoreWarns();
        }
    }
}
