using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class AbsoluteEpisodeNumberParserFixture : CoreTest
    {
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
//        [TestCase("One Piece S10E14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 10x14 214", "One Piece", 214, 10, 14)]
//        [TestCase("One Piece 214 10x14", "One Piece", 214, 10, 14)]
//        [TestCase("214 One Piece 10x14", "One Piece", 214, 10, 14)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar].avi", "Bleach", 31, 0, 0)]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar]", "Bleach", 31, 0, 0)]
        [TestCase("[ACX]Hack Sign 01 Role Play [Kosaka] [9C57891E].mkv", "Hack Sign", 1, 0, 0)]
        [TestCase("[SFW-sage] Bakuman S3 - 12 [720p][D07C91FC]", "Bakuman S3", 12, 0, 0)]
        [TestCase("ducktales_e66_time_is_money_part_one_marking_time", "DuckTales", 66, 0, 0)]
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0].mkv", "No Game No Life", 1, 0, 0)]
        [TestCase("[FroZen] Miyuki - 23 [DVD][7F6170E6]", "Miyuki", 23, 0, 0)]
        [TestCase("[Commie] Yowamushi Pedal - 32 [0BA19D5B]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[Doki] Mahouka Koukou no Rettousei - 07 (1280x720 Hi10P AAC) [80AF7DDE]", "Mahouka Koukou no Rettousei", 7, 0, 0)]
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [480p]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[CR] Sailor Moon - 004 [480p][48CE2D0F]", "Sailor Moon", 4, 0, 0)]
        [TestCase("[Chibiki] Puchimas!! - 42 [360p][7A4FC77B]", "Puchimas", 42, 0, 0)]
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [1080p]", "Yowamushi Pedal", 32, 0, 0)]
        [TestCase("[HorribleSubs] Love Live! S2 - 07 [720p]", "Love Live! S2", 7, 0, 0)]
        [TestCase("[DeadFish] Onee-chan ga Kita - 09v2 [720p][AAC]", "Onee-chan ga Kita", 9, 0, 0)]
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0]", "No Game No Life", 1, 0, 0)]
        [TestCase("[S-T-D] Soul Eater Not! - 06 (1280x720 10bit AAC) [59B3F2EA].mkv", "Soul Eater Not!", 6, 0, 0)]
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0].mkv", "No Game No Life", 1, 0, 0)]
        [TestCase("No Game No Life - 010 (720p) [27AAA0A0].mkv", "No Game No Life", 10, 0, 0)]
        [TestCase("Initial D Fifth Stage - 01 DVD - Central Anime", "Initial D Fifth Stage", 1, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_01(DVD)_-_(Central_Anime)[5AF6F1E4].mkv", "Initial D Fifth Stage", 1, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_02(DVD)_-_(Central_Anime)[0CA65F00].mkv", "Initial D Fifth Stage", 2, 0, 0)]
        [TestCase("Initial D Fifth Stage - 03 DVD - Central Anime", "Initial D Fifth Stage", 3, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_03(DVD)_-_(Central_Anime)[629BD592].mkv", "Initial D Fifth Stage", 3, 0, 0)]
        [TestCase("Initial D Fifth Stage - 14 DVD - Central Anime", "Initial D Fifth Stage", 14, 0, 0)]
        [TestCase("Initial_D_Fifth_Stage_-_14(DVD)_-_(Central_Anime)[0183D922].mkv", "Initial D Fifth Stage", 14, 0, 0)]
//        [TestCase("Initial D - 4th Stage Ep 01.mkv", "Initial D - 4th Stage", 1, 0, 0)]
        [TestCase("[ChihiroDesuYo].No.Game.No.Life.-.09.1280x720.10bit.AAC.[24CCE81D]", "No.Game.No.Life", 9, 0, 0)]
        [TestCase("Fairy Tail - 001 - Fairy Tail", "Fairy Tail", 001, 0, 0)]
        [TestCase("Fairy Tail - 049 - The Day of Fated Meeting", "Fairy Tail", 049, 0, 0)]
        [TestCase("Fairy Tail - 050 - Special Request Watch Out for the Guy You Like!", "Fairy Tail", 050, 0, 0)]
        [TestCase("Fairy Tail - 099 - Natsu vs. Gildarts", "Fairy Tail", 099, 0, 0)]
        [TestCase("Fairy Tail - 100 - Mest", "Fairy Tail", 100, 0, 0)]
//        [TestCase("Fairy Tail - 101 - Mest", "Fairy Tail", 100, 0, 0)] //This gets caught up in the 'see' numbering
        public void should_parse_absolute_numbers(string postTitle, string title, int absoluteEpisodeNumber, int seasonNumber, int episodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeasonNumber.Should().Be(seasonNumber);
            result.EpisodeNumbers.SingleOrDefault().Should().Be(episodeNumber);
            result.SeriesTitle.Should().Be(title.CleanSeriesTitle());
            result.FullSeason.Should().BeFalse();
        }
    }
}
