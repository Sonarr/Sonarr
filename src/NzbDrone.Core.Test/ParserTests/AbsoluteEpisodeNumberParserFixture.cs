using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class AbsoluteEpisodeNumberParserFixture : CoreTest
    {
        [TestCase("[SubDESU]_Show_One_07_(1280x720_x264-AAC)_[6B7FD717]", "Show One", 7, 0, 0)]
        [TestCase("[Chihiro]_Show!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Show!!", 6, 0, 0)]
        [TestCase("[Commie]_Some_Anime_Show_-_11_[65F220B4]", "Some Anime Show", 11, 0, 0)]
        [TestCase("[Underwater]_Some_Anime_Show_-_12_(720p)_[5C7BC4F9]", "Some Anime Show", 12, 0, 0)]
        [TestCase("[Commie]_Some_Anime_Show_-_15_[E76552EA]", "Some Anime Show", 15, 0, 0)]
        [TestCase("[HorribleSubs]_Some_Anime_Show_-_33_[720p]", "Some Anime Show", 33, 0, 0)]
        [TestCase("[HorribleSubs]_Some_Anime_Show_-_145_[720p]", "Some Anime Show", 145, 0, 0)]
        [TestCase("[HorribleSubs] Some Anime Show - 13 [1080p].mkv", "Some Anime Show", 13, 0, 0)]
        [TestCase("[Doremi].Some.Anime.Show.8.Go!.31.[1280x720].[C65D4B1F].mkv", "Some Anime Show 8 Go!", 31, 0, 0)]
        [TestCase("[K-F] Some Anime Show 214", "Some Anime Show", 214, 0, 0)]
        [TestCase("[K-F] Some Anime Show S10E14 214", "Some Anime Show", 214, 10, 14)]
        [TestCase("[K-F] Some Anime Show 10x14 214", "Some Anime Show", 214, 10, 14)]
        [TestCase("[K-F] Some Anime Show 214 10x14", "Some Anime Show", 214, 10, 14)]
        [TestCase("Some Anime Show - 031 - The Resolution to Kill [Lunar].avi", "Some Anime Show", 31, 0, 0)]
        [TestCase("Some Anime Show - 031 - The Resolution to Kill [Lunar]", "Some Anime Show", 31, 0, 0)]
        [TestCase("[ACX]Some Anime Show 01 Role Play [Kosaka] [9C57891E].mkv", "Some Anime Show", 1, 0, 0)]
        [TestCase("[SFW-sage] Some Anime Show S3 - 12 [720p][D07C91FC]", "Some Anime Show S3", 12, 0, 0)]
        [TestCase("Some_Anime_Show_e66_time_is_money_part_one_marking_time", "Some Anime Show", 66, 0, 0)]
        [TestCase("[Underwater-FFF] No Series Title No Life - 01 (720p) [27AAA0A0].mkv", "No Series Title No Life", 1, 0, 0)]
        [TestCase("[FroZen] Series Title - 23 [DVD][7F6170E6]", "Series Title", 23, 0, 0)]
        [TestCase("[Commie] Series Title - 32 [0BA19D5B]", "Series Title", 32, 0, 0)]
        [TestCase("[Doki]Series Title - 07 (1280x720 Hi10P AAC) [80AF7DDE]", "Series Title", 7, 0, 0)]
        [TestCase("[HorribleSubs] Series Title - 32 [480p]", "Series Title", 32, 0, 0)]
        [TestCase("[CR] Series Title - 004 [480p][48CE2D0F]", "Series Title", 4, 0, 0)]
        [TestCase("[Chibiki] Series Title!! - 42 [360p][7A4FC77B]", "Series Title!!", 42, 0, 0)]
        [TestCase("[HorribleSubs] Series Title - 32 [1080p]", "Series Title", 32, 0, 0)]
        [TestCase("[HorribleSubs] Series Title! S2 - 07 [720p]", "Series Title! S2", 7, 0, 0)]
        [TestCase("[DeadFish] Series Title - 09v2 [720p][AAC]", "Series Title", 9, 0, 0)]
        [TestCase("[Underwater-FFF] Series Title - 01 (720p) [27AAA0A0]", "Series Title", 1, 0, 0)]
        [TestCase("[S-T-D] Series Title! - 06 (1280x720 10bit AAC) [59B3F2EA].mkv", "Series Title!", 6, 0, 0)]
        [TestCase("Series Title - 010 (720p) [27AAA0A0].mkv", "Series Title", 10, 0, 0)]
        [TestCase("Initial_Series_Title - 01 DVD - Central Anime", "Initial Series Title", 1, 0, 0)]
        [TestCase("Initial_Series_Title_-_01(DVD)_-_(Central_Anime)[5AF6F1E4].mkv", "Initial Series Title", 1, 0, 0)]
        [TestCase("Initial_Series_Title_-_02(DVD)_-_(Central_Anime)[0CA65F00].mkv", "Initial Series Title", 2, 0, 0)]
        [TestCase("Initial_Series_Title - 03 DVD - Central Anime", "Initial Series Title", 3, 0, 0)]
        [TestCase("Initial_Series_Title_-_03(DVD)_-_(Central_Anime)[629BD592].mkv", "Initial Series Title", 3, 0, 0)]
        [TestCase("Initial_Series_Title - 14 DVD - Central Anime", "Initial Series Title", 14, 0, 0)]
        [TestCase("Initial_Series_Title_-_14(DVD)_-_(Central_Anime)[0183D922].mkv", "Initial Series Title", 14, 0, 0)]

//        [TestCase("Initial D - 4th Stage Ep 01.mkv", "Initial D - 4th Stage", 1, 0, 0)]
        [TestCase("[ChihiroDesuYo].Series.Title.-.09.1280x720.10bit.AAC.[24CCE81D]", "Series Title", 9, 0, 0)]
        [TestCase("Series Title - 001 - Fairy Tail", "Series Title", 001, 0, 0)]
        [TestCase("Series Title - 049 - The Day of Fated Meeting", "Series Title", 049, 0, 0)]
        [TestCase("Series Title - 050 - Special Request Watch Out for the Guy You Like!", "Series Title", 050, 0, 0)]
        [TestCase("Series Title - 099 - Natsu vs. Gildarts", "Series Title", 099, 0, 0)]
        [TestCase("Series Title - 100 - Mest", "Series Title", 100, 0, 0)]

//        [TestCase("Fairy Tail - 101 - Mest", "Fairy Tail", 101, 0, 0)] //This gets caught up in the 'see' numbering
        [TestCase("[Exiled-Destiny] Series Title Ep01 (D2201EC5).mkv", "Series Title", 1, 0, 0)]
        [TestCase("[Commie] Series Title - 23 [5396CA24].mkv", "Series Title", 23, 0, 0)]
        [TestCase("[FFF] Series Title - 01 [1FB538B5].mkv", "Series Title", 1, 0, 0)]
        [TestCase("[Hatsuyuki]Series_Title-01[1280x720][122E6EF8]", "Series Title", 1, 0, 0)]
        [TestCase("[CBM]_Series_Title_-_11_-_511_Kinderheim_[6C70C4E4].mkv", "Series Title", 11, 0, 0)]
        [TestCase("[HorribleSubs] Series Title 2 - 05 [720p].mkv", "Series Title 2", 5, 0, 0)]
        [TestCase("[Commie] Series Title 2 - 05 [FCE4D070].mkv", "Series Title 2", 5, 0, 0)]
        [TestCase("[DRONE]Series.Title.100", "Series Title", 100, 0, 0)]
        [TestCase("[RlsGrp]Series.Title.2010.S01E01.001.HDTV-720p.x264-DTS", "Series Title 2010", 1, 1, 1)]
        [TestCase("Series Title - 130 - Found You, Gohan! Harsh Training in the Kaioshin Realm! [Baaro][720p][5A1AD35B].mkv", "Series Title", 130, 0, 0)]
        [TestCase("Series Title - 131 - A Merged Super-Warrior Is Born, His Name Is Gotenks!! [Baaro][720p][32E03F96].mkv", "Series Title", 131, 0, 0)]
        [TestCase("[HorribleSubs] Series Title - 01 [1080p]", "Series Title", 1, 0, 0)]
        [TestCase("[Jumonji-Giri]_[F-B]_Series_Title_Ep04_(0b0e2c10).mkv", "Series Title", 4, 0, 0)]
        [TestCase("[Jumonji-Giri]_[F-B]_Series_Title_Ep08_(8246e542).mkv", "Series Title", 8, 0, 0)]
        [TestCase("Knights Series Title - 01 [1080p 10b DTSHD-MA eng sub].mkv", "Knights Series Title", 1, 0, 0)]
        [TestCase("Series Title (2010) {01} Episode Title (1).hdtv-720p", "Series Title (2010)", 1, 0, 0)]
        [TestCase("[HorribleSubs] Series Title - 20 [720p].mkv", "Series Title", 20, 0, 0)]
        [TestCase("[Hatsuyuki] Series Title (2014) - 017 (115) [1280x720][B2CFBC0F]", "Series Title (2014)", 17, 0, 0)]
        [TestCase("[Hatsuyuki] Series Title (2014) - 018 (116) [1280x720][C4A3B16E]", "Series Title (2014)", 18, 0, 0)]
        [TestCase("Series Title (2014) - 39 (137) [v2][720p.HDTV][Unison Fansub]", "Series Title (2014)", 39, 0, 0)]
        [TestCase("[HorribleSubs] Series Title 21 - 101 [480p].mkv", "Series Title 21", 101, 0, 0)]
        [TestCase("[Cthuyuu].Series.Title.-.03.[720p.H264.AAC][8AD82C3A]", "Series Title", 3, 0, 0)]

        //[TestCase("Series.Title.-.03.(1280x720.HEVC.AAC)", "Series Title", 3, 0, 0)]
        [TestCase("[Cthuyuu] Series Title - 03 [720p H264 AAC][8AD82C3A]", "Series Title", 3, 0, 0)]
        [TestCase("Series Title Episode 56 [VOSTFR V2][720p][AAC]-Mystic Z-Team", "Series Title", 56, 0, 0)]
        [TestCase("[Mystic Z-Team] Series Title Episode 69 [VOSTFR_Finale][1080p][AAC].mp4", "Series Title", 69, 0, 0)]
        [TestCase("[Shark-Raws] Series Title #957 (NBN 1280x720 x264 AAC).mp4", "Series Title", 957, 0, 0)]
        [TestCase("Series Title EP06 720p x265 AOZ.mp4", "Series Title", 6, 0, 0)]
        [TestCase("Series Title 2018 EP06 720p x265 AOZ.mp4", "Series Title 2018", 6, 0, 0)]
        [TestCase("Series Title 2018 06 720p x265 AOZ.mp4", "Series Title 2018", 6, 0, 0)]
        [TestCase("Series Title S03 - EP14 VOSTFR [1080p] [HardSub] Yass'Kun", "Series Title S03", 14, 0, 0)]
        [TestCase("Series Title S3 -  15 VOSTFR [720p]", "Series Title S3", 15, 0, 0)]
        [TestCase("A Series: RE S2 - Episode 4 VOSTFR (1080p)", "A Series: RE S2", 4, 0, 0)]
        [TestCase("To Another Series III - Episode 5 VOSTFR (1080p)", "To Another Series III", 5, 0, 0)]
        [TestCase("[Prout] Show;Title 0 - Episode 5 VOSTFR (BDRip 1920x1080 x264 FLAC)", "Show;Title 0", 5, 0, 0)]
        [TestCase("[BakedFish] Some Show [Anime] - 01 [720p][AAC].mp4", "Some Show [Anime]", 1, 0, 0)]
        [TestCase("Abc x Abc (2011) - 141 - Magician [KaiDubs] [1080p]", "Abc x Abc (2011)", 141, 0, 0)]
        [TestCase("Abc Abc 484 VOSTFR par Abc-Abc (1280*720) - version MQ", "Abc Abc", 484, 0, 0)]
        [TestCase("Abc - Abc Abc Abc - 107 VOSTFR par Fansub-Miracle Sharingan (1920x1080) - HQ_Draft", "Abc - Abc Abc Abc", 107, 0, 0)]
        [TestCase("Abc Abc Abc Abc Episode 10 VOSTFR (1920x1080) Miracle Sharingan Fansub.MKV - Team - (� suivre)", "Abc Abc Abc Abc", 10, 0, 0)]
        [TestCase("[Glenn] Series! 3 - 11 (1080p AAC)[C34B2B3B].mkv", "Series! 3", 11, 0, 0)]
        [TestCase("SeriesTitle.E1135.Lasst.den.Mond.am.Himmel.stehen.GERMAN.1080p.WEBRip.x264-Group", "SeriesTitle", 1135, 0, 0)]
        [TestCase("[HorribleSubs] Series 100 - 07 [1080p].mkv", "Series 100", 7, 0, 0)]
        [TestCase("[HorribleSubs] Series 100 S2 - 07 [1080p].mkv", "Series 100 S2", 7, 0, 0)]
        [TestCase("[abc] Adventure Series: 30 [Web][MKV][h264][720p][AAC 2.0][abc]", "Adventure Series:", 30, 0, 0)]
        [TestCase("[XKsub] Series Title S2 [05][HEVC-10bit 1080p AAC][CHS&CHT&JPN]", "Series Title S2", 5, 0, 0)]
        [TestCase("[Cheetah-Raws] Super Long Anime - 1000 (YTV 1280x720 x264 AAC)", "Super Long Anime", 1000, 0, 0)]
        [TestCase("[DameDesuYo] Another Anime With Special Naming (Season 2) - 33 (1280x720 10bit EAC3) [42A12A76].mkv", "Another Anime With Special Naming", 33, 2, 0)]
        [TestCase("[SubsPlease] Anime Title 300-nen, With Even More Title - 01 (1080p) [8DE44442]", "Anime Title 300-nen, With Even More Title", 1, 0, 0)]
        [TestCase("[Chihiro] Anime Title 300-nen, With Even More Title 02 [720p Hi10P AAC][031FA533]", "Anime Title 300-nen, With Even More Title", 2, 0, 0)]
        [TestCase("[BakeSubs] 86 - 01 [1080p][D40A9E55].mkv", "86", 1, 0, 0)]
        [TestCase("Anime Title the Final - 09 (2021) [SubsPlease] [WEBRip] [HD 1080p]", "Anime Title the Final", 9, 0, 0)]
        [TestCase("Anime Title S21 999", "Anime Title S21", 999, 0, 0)]
        [TestCase("Anime Title S21 1000", "Anime Title S21", 1000, 0, 0)]
        [TestCase("[HatSubs] Anime Title 1004 [E63F2984].mkv", "Anime Title", 1004, 0, 0)]

        //[TestCase("", "", 0, 0, 0)]
        public void should_parse_absolute_numbers(string postTitle, string title, int absoluteEpisodeNumber, int seasonNumber, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.SingleOrDefault().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[DeadFish] Another Anime Show - 01 - Special [BD][720p][AAC]", "Another Anime Show", 1)]
        [TestCase("[DeadFish] Another Anime Show - 01 - OVA [BD][720p][AAC]", "Another Anime Show", 1)]
        [TestCase("[DeadFish] Another Anime Show - 01 - OVD [BD][720p][AAC]", "Another Anime Show", 1)]
        public void should_parse_absolute_specials(string postTitle, string title, int absoluteEpisodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeasonNumber.Should().Be(0);
            result.EpisodeNumbers.SingleOrDefault().Should().Be(0);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
            result.Special.Should().BeTrue();
        }

        [TestCase("[ANBU-AonE]_SeriesTitle_26-27_[F224EF26].avi", "SeriesTitle", 26, 27)]
        [TestCase("[Doutei] Some Good, Anime Show - 01-12 [BD][720p-AAC]", "Some Good, Anime Show", 1, 12)]
        [TestCase("Series Title (2010) - 01-02-03 - Episode Title (1) HDTV-720p", "Series Title (2010)", 1, 3)]
        [TestCase("[RlsGrp] Series Title (2010) - S01E01-02-03 - 001-002-003 - Episode Title HDTV-720p v2", "Series Title (2010)", 1, 3)]
        [TestCase("[RlsGrp] Series Title (2010) - S01E01-02 - 001-002 - Episode Title HDTV-720p v2", "Series Title (2010)", 1, 2)]
        [TestCase("Series Title (2010) - S01E01-02 (001-002) - Episode Title (1) HDTV-720p v2 [RlsGrp]", "Series Title (2010)", 1, 2)]
        [TestCase("[HorribleSubs] Some Anime Show!! (01-25) [1080p] (Batch)", "Some Anime Show!!", 1, 25)]
        [TestCase("Some Anime Show (2011) Episode 99-100 [1080p] [Dual.Audio] [x265]", "Some Anime Show (2011)", 99, 100)]
        [TestCase("Some Anime Show 1-13 (English Dub) [720p]", "Some Anime Show", 1, 13)]
        [TestCase("Series.Title.Ep01-12.Complete.English.AC3.DL.1080p.BluRay.x264", "Series Title", 1, 12)]
        [TestCase("[Judas] Some Anime Show 091-123 [1080p][HEVC x265 10bit][Dual-Audio][Multi-Subs]", "Some Anime Show", 91, 123)]
        [TestCase("[Judas] Some Anime Show - 091-123 [1080p][HEVC x265 10bit][Dual-Audio][Multi-Subs]", "Some Anime Show", 91, 123)]
        [TestCase("[HorribleSubs] Some Anime Show 01 - 119 [1080p] [Batch]", "Some Anime Show", 1, 119)]
        [TestCase("[Erai-raws] Series Title! - 01~10 [1080p][Multiple Subtitle]", "Series Title!", 1, 10)]
        [TestCase("[Erai-raws] Series-Title! 2 - 01~10 [1080p][Multiple Subtitle]", "Series-Title! 2", 1, 10)]
        [TestCase("[Erai-raws] Series Title! - 01 ~ 10 [1080p][Multiple Subtitle]", "Series Title!", 1, 10)]
        [TestCase("[Erai-raws] Series-Title! 2 - 01 ~ 10 [1080p][Multiple Subtitle]", "Series-Title! 2", 1, 10)]

        // [TestCase("", "", 1, 2)]
        public void should_parse_multi_episode_absolute_numbers(string postTitle, string title, int firstAbsoluteEpisodeNumber, int lastAbsoluteEpisodeNumber)
        {
            var absoluteEpisodeNumbers = Enumerable.Range(firstAbsoluteEpisodeNumber, lastAbsoluteEpisodeNumber - firstAbsoluteEpisodeNumber + 1)
                                                        .ToArray();
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Should().BeEquivalentTo(absoluteEpisodeNumbers);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[Vivid] Some Anime Show S01 [Web][MKV][h264 10-bit][1080p][AAC 2.0]", "Some Anime Show", 1)]
        [TestCase("Anime, Title? | Japanse Anime, Title? [Season 1 + EXTRA] [BD 1080p x265 HEVC OPUS] [Dual-Audio]", "Anime, Title | Japanse Anime, Title", 1)]
        [TestCase("[Judas] Japanse Anime, Title (Anime, Title?) (Season 1) [1080p][HEVC x265 10bit][Multi-Subs] (Batch)", "Japanse Anime, Title (Anime, Title)", 1)]
        [TestCase("[Judas] Japanse Anime, Title (Anime, Title?) (Season 1) [1080p][HEVC x265 10bit][Multi-Subs] (Batch)", "Japanse Anime, Title (Anime, Title)", 1)]
        public void should_parse_anime_season_packs(string postTitle, string title, int seasonNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeTrue();
            result.SeasonNumber.Should().Be(seasonNumber);
        }

        [TestCase("[HorribleSubs] Show Slayer - 10.5 [1080p].mkv", "Show Slayer", 10.5)]
        public void should_handle_anime_recap_numbering(string postTitle, string title, double specialEpisodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title);
            result.AbsoluteEpisodeNumbers.Should().BeEmpty();
            result.SpecialAbsoluteEpisodeNumbers.Should().NotBeEmpty();
            result.SpecialAbsoluteEpisodeNumbers.Should().BeEquivalentTo(new[] { (decimal)specialEpisodeNumber });
            result.FullSeason.Should().BeFalse();
        }
    }
}
