using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class UnicodeReleaseParserFixture : CoreTest
    {
        [TestCase("【喵萌奶茶屋】★10月新番★[哥布林杀手/Anime Series Title][12END][720p][繁体][招募翻译校对]", "Anime Series Title", "喵萌奶茶屋", 12)]
        [TestCase("[桜都字幕组][盾之勇者成名录/Anime Series Title][01][BIG5][720P]", "Anime Series Title", "桜都字幕组", 1)]
        [TestCase("[YMDR][輝夜姬想讓人告白～天才們的戀愛頭腦戰～][Anime Series Title][2019][02][1080p][HEVC][JAP][BIG5][MP4-AAC][繁中]", "Anime Series Title", "YMDR", 2)]
        [TestCase("【DHR百合組】[天使降臨到我身邊！_Anime Series Title][05][繁體][1080P10][WebRip][HEVC][MP4]", "Anime Series Title", "DHR百合組", 5)]
        [TestCase("【傲娇零&自由字幕组】[刀剑神域III UnderWorld/ Anime Series Title ][17][HEVC-10Bit-2160P AAC][外挂GB/BIG5][WEB-Rip][MKV+ass ]", "Anime Series Title", "傲娇零&自由字幕组", 17)]
        [TestCase("【悠哈璃羽字幕社＆拉斯观测组】[刀剑神域Alicization _ Anime Series Title ][17][x264 1080p][CHS]", "Anime Series Title", "悠哈璃羽字幕社＆拉斯观测组", 17)]
        [TestCase("【极影字幕社】 ★04月新番 【巴哈姆特之怒 Virgin Soul】【Anime Series Title】【24】 【END】GB MP4_720P", "Anime Series Title", "极影字幕社", 24)]
        [TestCase("[愛戀&漫貓字幕组][10月新番][關於我轉生後成爲史萊姆那件事][18][720P][BIG5][MP4]", "關於我轉生後成爲史萊姆那件事", "愛戀&漫貓字幕组", 18)]
        [TestCase("【咕咕茶字幕組】★1月新番[天使降臨到了我身邊! / Anime Series Title!][04][1080P][繁體][MP4]", "Anime Series Title!", "咕咕茶字幕組", 4)]
        [TestCase("【千夏字幕组】【天使降临到了我身边！_Anime Series Title】[第05话][1080p_HEVC][简繁外挂]", "Anime Series Title", "千夏字幕组", 5)]
        [TestCase("[星空字幕组] 剃须。然后捡到女高中生。 / Anime Series Title [05][1080p][简日内嵌]", "Anime Series Title", "星空字幕组", 5)]
        [TestCase("【DHR动研字幕组】[多田君不恋爱_Anime Series Title][13完][繁体][720P][MP4]", "Anime Series Title", "DHR动研字幕组", 13)]
        [TestCase("【动漫国字幕组】★01月新番[Anime Series Title～！][01][1080P][简体][MP4]", "Anime Series Title～！", "动漫国字幕组", 1)]
        [TestCase("[风车字幕组][名侦探柯南][857][米花町反复变化之谜（前篇）][简体][MP4][1080P]", "名侦探柯南", "风车字幕组", 857)]
        [TestCase("[风车字幕组][名侦探柯南][857集][米花町反复变化之谜（前篇）][简体][MP4][1080P]", "名侦探柯南", "风车字幕组", 857)]
        [TestCase("【喵萌奶茶屋】★10月新番★[后宫之乌/后宫の乌/Series Title][07][1080p][简日双语][招募翻译校对]", "Series Title", "喵萌奶茶屋", 7)]
        public void should_parse_chinese_anime_releases(string postTitle, string title, string subgroup, int absoluteEpisodeNumber)
        {
            postTitle = XmlCleaner.ReplaceUnicode(postTitle);

            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subgroup);
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[Lilith-Raws] 在地下城尋求邂逅是否搞錯了什麼 / Anime-Series Title S04 - 12 [Baha][WEB-DL][1080p][AVC AAC][CHT][MP4]", "Anime-Series Title S4", "Lilith-Raws", 12)]
        [TestCase("[Lilith-Raws] 魔王學院的不適任者 / Anime-Series Title S02 - 01 [Baha][WEB-DL][1080p][AVC AAC][CHT][MP4]", "Anime-Series Title S2", "Lilith-Raws", 1)]
        [TestCase("[Lilith-Raws x WitEx.io] 盾之勇者成名录 / Anime-Series Title S02 - 03 [Baha][WEB-DL][1080p][AVC AAC][CHT][MP4] [557.3MB]", "Anime-Series Title S2", "Lilith-Raws x WitEx.io", 3)]
        [TestCase("[SweetSub&LoliHouse] 来自深渊 烈日黄金乡 / Anime-Series Title S2 - 07 [WebRip 1080p HEVC-10bit AAC][简繁日内封字幕]", "Anime-Series Title S2", "SweetSub&LoliHouse", 7)]
        [TestCase("[LoliHouse] Love Live! 虹咲学园学园偶像同好会 第二季 / Anime-Series Title S2 - 10 [WebRip 1080p HEVC-10bit AAC][简繁内封字幕]", "Anime-Series Title S2", "LoliHouse", 10)]
        [TestCase("[澄空学园&雪飘工作室&LoliHouse] 辉夜大小姐想让我告白 第三季 / Anime-Series Title S3 - 06 [WebRip 1080p HEVC-10bit AAC][简繁内封字幕]", "Anime-Series Title S3", "澄空学园&雪飘工作室&LoliHouse", 6)]
        [TestCase("[诸神字幕组][致不灭的你 第二季][Anime-Series Title S2][10][简繁日语字幕][1080P][MKV HEVC]", "Anime-Series Title S2", "诸神字幕组", 10)]
        [TestCase("[NC-Raws] 魔王学院的不适任者～史上最强的魔王始祖，转生就读子孙们的学校～第二季 / Anime-Series Title S2 - 01 (Baha 1920x1080 AVC AAC MP4)", "Anime-Series Title S2", "NC-Raws", 1)]
        [TestCase("[Lilith-Raws] Anime-Series Title S02 - 11 [Baha][WEB-DL][1080p][AVC AAC][CHT][MP4].mp4", "Anime-Series Title S2", "Lilith-Raws", 11)]
        [TestCase("[天月搬运组] 不要欺负我，长瀞同学 2nd Attack / Anime-Series Title S02 - 01 [1080P][简繁日外挂]", "Anime-Series Title S2", "天月搬运组", 1)]
        [TestCase("[Skymoon-Raws] 怕痛的我，把防御力点满就对了 第二季 / Anime-Series Title S02 - 01 [ViuTV][WEB-DL][1080p][AVC AAC][繁体外挂][MP4+ASS](正式版本) ", "Anime-Series Title S2", "Skymoon-Raws", 1)]
        [TestCase("[Skymoon-Raws] Anime-Series Title S02 - 01 [ViuTV][CHT][WEB-DL][1080p][AVC AAC][MP4+ASS]", "Anime-Series Title S2", "Skymoon-Raws", 1)]
        [TestCase("[orion origin] Anime-Series Title S02[07][1080p][H264 AAC][CHS][ENG＆JPN stidio]", "Anime-Series Title S2", "orion origin", 7)]
        [TestCase("[UHA-WINGS][Anime-Series Title S02][01][x264 1080p][CHT].mp4", "Anime-Series Title S2", "UHA-WINGS", 1)]
        [TestCase("[Suzuya Raws] 腼腆英雄 东京夺还篇 / Series 2nd Season - 01 [CR WebRip 1080p HEVC-10bit AAC][Multi-Subs]", "Series 2nd Season", "Suzuya Raws", 1)]
        [TestCase("[ANi] SERIES / SERIES 靦腆英雄 - 11 [1080P][Baha][WEB-DL][AAC AVC][CHT][MP4]", "SERIES", "ANi", 11)]
        [TestCase("[Q] 全职高手 第3季 / Series S3 - 09 (1080p HBR HEVC Multi-Sub)", "Series S3", "Q", 9)]
        [TestCase("[Q] 全职高手 第3季 | Series S3 - 09 (1080p HBR HEVC Multi-Sub)", "Series S3", "Q", 9)]
        public void should_parse_chinese_anime_season_episode_releases(string postTitle, string title, string subgroup, int absoluteEpisodeNumber)
        {
            postTitle = XmlCleaner.ReplaceUnicode(postTitle);

            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subgroup);
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[喵萌奶茶屋&LoliHouse]玛娜利亚魔法学院/巴哈姆特之怒Anime Series Title - 03 [WebRip 1080p HEVC-10bit AAC][简繁内封字幕]", "巴哈姆特之怒Anime Series Title", "喵萌奶茶屋&LoliHouse", 3)]
        [TestCase("[悠哈璃羽字幕社&拉斯观测组&LoliHouse] 刀剑神域: Alicization / Anime Series: Title - 17 [WebRip 1080p HEVC-10bit AAC][简繁内封字幕]", "Anime Series: Title", "悠哈璃羽字幕社&拉斯观测组&LoliHouse", 17)]
        [TestCase("[ZERO字幕組]嫁給非人類·Anime-Series Title[11][BIG5][1080p]", "Anime-Series Title", "ZERO字幕組", 11)]
        [TestCase("[Lilith-Raws] 艾梅洛閣下 II 世事件簿 -魔眼蒐集列車 Grace note- / Anime-Series Title - 04 [BiliBili][WEB-DL][1080p][AVC AAC][CHT][MKV]", "Anime-Series Title", "Lilith-Raws", 4)]
        [TestCase("[NC-Raws] 影宅 / Anime-Series Title - 07 [B-Global][WEB-DL][1080p][AVC AAC][CHS_CHT_ENG_TH_SRT][MKV]", "Anime-Series Title", "NC-Raws", 7)]
        [TestCase("[NC-Raws] ANIME-SERIES TITLE－影宅－ / Anime-Series Title - 07 [Baha][WEB-DL][1080p][AVC AAC][CHT][MP4]", "Anime-Series Title", "NC-Raws", 7)]
        [TestCase("[OPFans楓雪動漫][ANIME SERIES 海賊王][第1008話][典藏版][1080P][MKV][簡繁]", "ANIME SERIES", "OPFans", 1008)]
        [TestCase("[Skymoon-Raws][Anime Series 海賊王][1008][ViuTV][WEB-RIP][CHT][SRTx2][1080p][MKV]", "Anime Series", "Skymoon-Raws", 1008)]
        [TestCase("[银光字幕组][名侦探柯南·Series Title][871][信长四五〇事件][繁日][HDrip][X264-AAC][720P][MP4]", "Series Title", "银光字幕组", 871)]
        public void should_parse_unbracketed_chinese_anime_releases(string postTitle, string title, string subgroup, int absoluteEpisodeNumber)
        {
            postTitle = XmlCleaner.ReplaceUnicode(postTitle);

            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subgroup);
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[YMDR][慕留人 -火影忍者新時代-][Anime Series Title-][2017][88-91][1080p][AVC][JAP][BIG5][MP4-AAC][繁中]", "Anime Series Title", "YMDR", new[] { 88, 89, 90, 91 })]
        [TestCase("[诸神字幕组][战栗杀机][ANIME SERIES TITLE][01-24完][简日双语字幕][720P][MP4]", "ANIME SERIES TITLE", "诸神字幕组", new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 })]

        // [TestCase("[orion origin] Anime-Series Title S02 [01-07] [1080p] [H264 AAC] [CHS] [ENG＆JPN stidio]", "Anime-Series Title S2", "orion origin", new[] { 1, 2, 3, 4, 5, 6, 7 })]
        // [TestCase("【漫貓&愛戀字幕組】[五等分的新娘/五等分的花嫁/五等分の花嫁][Anime Series Title][01_03][BIG5][720P][HEVC]", "Anime Series Title", "漫貓&愛戀字幕組", new[] { 1, 2, 3 })]
        public void should_parse_chinese_multiepisode_releases(string postTitle, string title, string subgroup, int[] absoluteEpisodeNumbers)
        {
            postTitle = XmlCleaner.ReplaceUnicode(postTitle);

            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subgroup);
            result.AbsoluteEpisodeNumbers.Should().BeEquivalentTo(absoluteEpisodeNumbers);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[GM-Team][国漫][斗罗大陆][Anime Title][Douro Mainland][2019][215][AVC][GB][1080P]", "Anime Title", 215)]
        [TestCase("[GM-Team][国漫][斗罗大陆][Anime Title][Douro Mainland][2019][215 END][AVC][GB][1080P]", "Anime Title", 215)]
        [TestCase("[GM-Team][国漫][斗罗大陆][Anime Title][2019][215 Fin][AVC][GB][1080P]", "Anime Title", 215)]
        [TestCase("[GM-Team][国漫][Anime Title][Douro Mainland][2019][234][AVC][GB][1080P]", "Anime Title", 234)]
        [TestCase("[GM-Team][国漫][Anime Title][2019][234][AVC][GB][1080P]", "Anime Title", 234)]
        [TestCase("[GM-Team][国漫][Anime Title][2019][234 END][AVC][GB][1080P]", "Anime Title", 234)]
        [TestCase("[GM-Team][国漫][Anime Title][2019][234 Fin][AVC][GB][1080P]", "Anime Title", 234)]
        public void should_parse_gm_team_releases_and_files(string postTitle, string title, int absoluteEpisodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
            result.ReleaseGroup.Should().Be("GM-Team");
        }

        [TestCase("[Subz] My Series - １５８ [h264 10-bit][1080p]", "My Series", 158)]
        public void should_parse_unicode_digits(string postTitle, string title, int absoluteEpisodeNumber)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.SeriesTitle.Should().Be(title);
            result.AbsoluteEpisodeNumbers.Should().NotBeEmpty();
            result.AbsoluteEpisodeNumbers.Should().BeEquivalentTo(new[] { absoluteEpisodeNumber });
            result.SpecialAbsoluteEpisodeNumbers.Should().BeEmpty();
            result.FullSeason.Should().BeFalse();
        }

        [TestCase("[BeanSub][Anime_Series_Title][01][GB][1080P][x264_AAC]", "Anime Series Title", "BeanSub", 1)]
        public void should_parse_false_positive_chinese_anime_releases(string postTitle, string title, string subgroup, int absoluteEpisodeNumber)
        {
            postTitle = XmlCleaner.ReplaceUnicode(postTitle);

            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subgroup);
            result.AbsoluteEpisodeNumbers.Single().Should().Be(absoluteEpisodeNumber);
            result.SeriesTitle.Should().Be(title);
            result.FullSeason.Should().BeFalse();
        }
    }
}
