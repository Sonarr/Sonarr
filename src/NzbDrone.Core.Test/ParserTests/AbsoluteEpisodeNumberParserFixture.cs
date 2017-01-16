using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class AbsoluteEpisodeNumberParserFixture : CoreTest
    {
        [TestCase("[SubDESU]_High_School_DxD_07_(1280x720_x264-AAC)_[6B7FD717]", "High School DxD", 7, 0, 0)]
        [TestCase("[Chihiro]_Working!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Working!!", 6, 0, 0)]
        [TestCase("[Commie]_Senki_Zesshou_Symphogear_-_11_[65F220B4]", "Senki Zesshou Symphogear", 11, 0, 0)]
        [TestCase("[Underwater]_Rinne_no_Lagrange_-_12_(720p)_[5C7BC4F9]", "Rinne no Lagrange", 12, 0, 0)]
        [TestCase("[Commie]_Rinne_no_Lagrange_-_15_[E76552EA]", "Rinne no Lagrange", 15, 0, 0)]
        [TestCase("[HorribleSubs]_Hunter_X_Hunter_-_33_[720p]", "Hunter X Hunter", 33, 0, 0)]
        [TestCase("[HorribleSubs]_Fairy_Tail_-_145_[720p]", "Fairy Tail", 145, 0, 0)]
        [TestCase("[HorribleSubs] Tonari no Kaibutsu-kun - 13 [1080p].mkv", "Tonari no Kaibutsu-kun", 13, 0, 0)]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", "Yes Pretty Cure 5 Go Go!", 31, 0, 0)]
        [TestCase("[K-F] One Piece 214", "One Piece", 214, 0, 0)]
        [TestCase("[K-F] One Piece S10E14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 10x14 214", "One Piece", 214, 10, 14)]
        [TestCase("[K-F] One Piece 214 10x14", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece S10E14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 10x14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 214 10x14", "One Piece", 214, 10, 14)]
//        [TestCase("214 One Piece 10x14", "One Piece", 214, 10, 14)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar].avi", "Bleach", 31, 0, 0)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar]", "Bleach", 31, 0, 0)]
        [TestCase("[ACX]Hack Sign 01 Role Play [Kosaka] [9C57891E].mkv", "Hack Sign", 1, 0, 0)]
        [TestCase("[SFW-sage] Bakuman S3 - 12 [720p][D07C91FC]", "Bakuman S3", 12, 0, 0)]
        [TestCase("ducktales_e66_time_is_money_part_one_marking_time", "ducktales", 66, 0, 0)]
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0].mkv", "No Game No Life", 1, 0, 0)]
        [TestCase("[FroZen] Miyuki - 23 [DVD][7F6170E6]", "Miyuki", 23, 0, 0)]
        [TestCase("[Commie] Yowamushi Pedal - 32 [0BA19D5B]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[Doki] Mahouka Koukou no Rettousei - 07 (1280x720 Hi10P AAC) [80AF7DDE]", "Mahouka Koukou no Rettousei", 7, 0, 0)]
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [480p]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[CR] Sailor Moon - 004 [480p][48CE2D0F]", "Sailor Moon", 4, 0, 0)]
        [TestCase("[Chibiki] Puchimas!! - 42 [360p][7A4FC77B]", "Puchimas!!", 42, 0, 0)]
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [1080p]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[HorribleSubs] Love Live! S2 - 07 [720p]", "Love Live! S2", 7, 0, 0)]
        [TestCase("[DeadFish] Onee-chan ga Kita - 09v2 [720p][AAC]", "Onee-chan ga Kita", 9, 0, 0)]
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0]", "No Game No Life", 1, 0, 0)]
        [TestCase("[S-T-D] Soul Eater Not! - 06 (1280x720 10bit AAC) [59B3F2EA].mkv", "Soul Eater Not!", 6, 0, 0)]
        [TestCase("No Game No Life - 010 (720p) [27AAA0A0].mkv", "No Game No Life", 10, 0, 0)]
        [TestCase("Initial D Fifth Stage - 01 DVD - Central Anime", "Initial D Fifth Stage", 1, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_01(DVD)_-_(Central_Anime)[5AF6F1E4].mkv", "Initial D Fifth Stage", 1, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_02(DVD)_-_(Central_Anime)[0CA65F00].mkv", "Initial D Fifth Stage", 2, 0, 0)]
        [TestCase("Initial D Fifth Stage - 03 DVD - Central Anime", "Initial D Fifth Stage", 3, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_03(DVD)_-_(Central_Anime)[629BD592].mkv", "Initial D Fifth Stage", 3, 0, 0)]
        [TestCase("Initial D Fifth Stage - 14 DVD - Central Anime", "Initial D Fifth Stage", 14, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_14(DVD)_-_(Central_Anime)[0183D922].mkv", "Initial D Fifth Stage", 14, 0, 0)]
//        [TestCase("Initial D - 4th Stage Ep 01.mkv", "Initial D - 4th Stage", 1, 0, 0)]
        [TestCase("[ChihiroDesuYo].No.Game.No.Life.-.09.1280x720.10bit.AAC.[24CCE81D]", "No Game No Life", 9, 0, 0)]
        [TestCase("Fairy Tail - 001 - Fairy Tail", "Fairy Tail", 001, 0, 0)]
        [TestCase("Fairy Tail - 049 - The Day of Fated Meeting", "Fairy Tail", 049, 0, 0)]
        [TestCase("Fairy Tail - 050 - Special Request Watch Out for the Guy You Like!", "Fairy Tail", 050, 0, 0)]
        [TestCase("Fairy Tail - 099 - Natsu vs. Gildarts", "Fairy Tail", 099, 0, 0)]
        [TestCase("Fairy Tail - 100 - Mest", "Fairy Tail", 100, 0, 0)]
//        [TestCase("Fairy Tail - 101 - Mest", "Fairy Tail", 101, 0, 0)] //This gets caught up in the 'see' numbering
        [TestCase("[Exiled-Destiny] Angel Beats Ep01 (D2201EC5).mkv", "Angel Beats", 1, 0, 0)]
        [TestCase("[Commie] Nobunaga the Fool - 23 [5396CA24].mkv", "Nobunaga the Fool", 23, 0, 0)]
        [TestCase("[FFF] Seikoku no Dragonar - 01 [1FB538B5].mkv", "Seikoku no Dragonar", 1, 0, 0)]
        [TestCase("[Hatsuyuki]Fate_Zero-01[1280x720][122E6EF8]", "Fate Zero", 1, 0, 0)]
        [TestCase("[CBM]_Monster_-_11_-_511_Kinderheim_[6C70C4E4].mkv", "Monster", 11, 0, 0)]
        [TestCase("[HorribleSubs] Log Horizon 2 - 05 [720p].mkv", "Log Horizon 2", 5, 0, 0)]
        [TestCase("[Commie] Log Horizon 2 - 05 [FCE4D070].mkv", "Log Horizon 2", 5, 0, 0)]
        [TestCase("[DRONE]Series.Title.100", "Series Title", 100, 0, 0)]
        [TestCase("[RlsGrp]Series.Title.2010.S01E01.001.HDTV-720p.x264-DTS", "Series Title 2010", 1, 1, 1)]
        [TestCase("Dragon Ball Kai - 130 - Found You, Gohan! Harsh Training in the Kaioshin Realm! [Baaro][720p][5A1AD35B].mkv", "Dragon Ball Kai", 130, 0, 0)]
        [TestCase("Dragon Ball Kai - 131 - A Merged Super-Warrior Is Born, His Name Is Gotenks!! [Baaro][720p][32E03F96].mkv", "Dragon Ball Kai", 131, 0, 0)]
        [TestCase("[HorribleSubs] Magic Kaito 1412 - 01 [1080p]", "Magic Kaito 1412", 1, 0, 0)]
        [TestCase("[Jumonji-Giri]_[F-B]_Kagihime_Monogatari_Eikyuu_Alice_Rondo_Ep04_(0b0e2c10).mkv", "Kagihime Monogatari Eikyuu Alice Rondo", 4, 0, 0)]
        [TestCase("[Jumonji-Giri]_[F-B]_Kagihime_Monogatari_Eikyuu_Alice_Rondo_Ep08_(8246e542).mkv", "Kagihime Monogatari Eikyuu Alice Rondo", 8, 0, 0)]
        [TestCase("Knights of Sidonia - 01 [1080p 10b DTSHD-MA eng sub].mkv", "Knights of Sidonia", 1, 0, 0)]
        [TestCase("Series Title (2010) {01} Episode Title (1).hdtv-720p", "Series Title (2010)", 1, 0, 0)]
        [TestCase("[HorribleSubs] Shirobako - 20 [720p].mkv", "Shirobako", 20, 0, 0)]
        [TestCase("[Hatsuyuki] Dragon Ball Kai (2014) - 017 (115) [1280x720][B2CFBC0F]", "Dragon Ball Kai (2014)", 17, 0, 0)]
        [TestCase("[Hatsuyuki] Dragon Ball Kai (2014) - 018 (116) [1280x720][C4A3B16E]", "Dragon Ball Kai (2014)", 18, 0, 0)]
        [TestCase("Dragon Ball Kai (2014) - 39 (137) [v2][720p.HDTV][Unison Fansub]", "Dragon Ball Kai (2014)", 39, 0, 0)]
        [TestCase("[HorribleSubs] Eyeshield 21 - 101 [480p].mkv", "Eyeshield 21", 101, 0, 0)]
        [TestCase("[Cthuyuu].Taimadou.Gakuen.35.Shiken.Shoutai.-.03.[720p.H264.AAC][8AD82C3A]", "Taimadou Gakuen 35 Shiken Shoutai", 3, 0, 0)]
        //[TestCase("Taimadou.Gakuen.35.Shiken.Shoutai.-.03.(1280x720.HEVC.AAC)", "Taimadou Gakuen 35 Shiken Shoutai", 3, 0, 0)]
        [TestCase("[Cthuyuu] Taimadou Gakuen 35 Shiken Shoutai - 03 [720p H264 AAC][8AD82C3A]", "Taimadou Gakuen 35 Shiken Shoutai", 3, 0, 0)]
        [TestCase("Dragon Ball Super Episode 56 [VOSTFR V2][720p][AAC]-Mystic Z-Team", "Dragon Ball Super", 56, 0, 0)]
        [TestCase("[Mystic Z-Team] Dragon Ball Super Episode 69 [VOSTFR_Finale][1080p][AAC].mp4", "Dragon Ball Super", 69, 0, 0)]
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

        [TestCase("[DeadFish] Kenzen Robo Daimidaler - 01 - Special [BD][720p][AAC]", "Kenzen Robo Daimidaler", 1)]
        [TestCase("[DeadFish] Kenzen Robo Daimidaler - 01 - OVA [BD][720p][AAC]", "Kenzen Robo Daimidaler", 1)]
        [TestCase("[DeadFish] Kenzen Robo Daimidaler - 01 - OVD [BD][720p][AAC]", "Kenzen Robo Daimidaler", 1)]
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

        [TestCase("[ANBU-AonE]_Naruto_26-27_[F224EF26].avi", "Naruto", new[] { 26, 27 })]
        [TestCase("[Doutei] Recently, My Sister is Unusual - 01-12 [BD][720p-AAC]", "Recently, My Sister is Unusual", new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        [TestCase("Series Title (2010) - 01-02-03 - Episode Title (1) HDTV-720p", "Series Title (2010)", new [] { 1, 2, 3 })]
        [TestCase("[RlsGrp] Series Title (2010) - S01E01-02-03 - 001-002-003 - Episode Title HDTV-720p v2", "Series Title (2010)", new[] { 1, 2, 3 })]
        [TestCase("[RlsGrp] Series Title (2010) - S01E01-02 - 001-002 - Episode Title HDTV-720p v2", "Series Title (2010)", new[] { 1, 2 })]
        [TestCase("Series Title (2010) - S01E01-02 (001-002) - Episode Title (1) HDTV-720p v2 [RlsGrp]", "Series Title (2010)", new[] { 1, 2 })]
        [TestCase("[HorribleSubs] Haikyuu!! (01-25) [1080p] (Batch)", "Haikyuu!!", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 })]
        public void should_parse_multi_episode_absolute_numbers(string postTitle, string title, int[] absoluteEpisodeNumbers)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Should().BeEquivalentTo(absoluteEpisodeNumbers);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }
    }
}
