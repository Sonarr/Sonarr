using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
        [TestCase("Title.the.Series.2009.S01E14.English.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.Germany.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD-LOL")]
        [TestCase("Title.the.Italian.Series.S01E01.The.Family.720p.HDTV.x264-FTP")]
        [TestCase("Title.the.Italy.Series.S02E01.720p.HDTV.x264-TLA")]
        [TestCase("Series Title - S01E01 - Pilot.en.sub")]
        [TestCase("Series Title - S01E01 - Pilot.eng.sub")]
        [TestCase("Series Title - S01E01 - Pilot.English.sub")]
        [TestCase("Series Title - S01E01 - Pilot.english.sub")]
        [TestCase("Spanish Killroy was Here S02E02 Flodden 720p AMZN WEB-DL DDP5 1 H 264-NTb")]
        [TestCase("Title.the.Spanish.Series.S02E02.1080p.WEB.H264-CAKES")]
        [TestCase("Title.the.Spanish.Series.S02E06.Field.of.Cloth.of.Gold.1080p.AMZN.WEBRip.DDP5.1.x264-NTb")]
        public void should_parse_language_english(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Should().Be(Language.English);
        }

        [TestCase("Series Title - S01E01 - Pilot.sub")]
        public void should_parse_subtitle_language_unknown(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.Unknown);
        }

        [TestCase("Series Title - S01E01 - Pilot.en.sub")]
        [TestCase("Series Title - S01E01 - Pilot.EN.sub")]
        [TestCase("Series Title - S01E01 - Pilot.eng.sub")]
        [TestCase("Series Title - S01E01 - Pilot.ENG.sub")]
        [TestCase("Series Title - S01E01 - Pilot.English.sub")]
        [TestCase("Series Title - S01E01 - Pilot.english.sub")]
        [TestCase("Series Title - S01E01 - Pilot.en.cc.sub")]
        [TestCase("Series Title - S01E01 - Pilot.en.sdh.sub")]
        [TestCase("Series Title - S01E01 - Pilot.en.forced.sub")]
        [TestCase("Series Title - S01E01 - Pilot.en.sdh.forced.sub")]
        public void should_parse_subtitle_language_english(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.English);
        }

        [TestCase("Title.the.Series.2009.S01E14.French.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.The.1x13.Tueurs.De.Flics.FR.DVDRip.XviD")]
        [TestCase("Title.S01.720p.VF.WEB-DL.AAC2.0.H.264-BTN")]
        [TestCase("Title.S01.720p.VF2.WEB-DL.AAC2.0.H.264-BTN")]
        [TestCase("Title.S01.720p.VFF.WEB-DL.AAC2.0.H.264-BTN")]
        [TestCase("Title.S01.720p.VFQ.WEB-DL.AAC2.0.H.264-BTN")]
        [TestCase("Title.S01.720p.TRUEFRENCH.WEB-DL.AAC2.0.H.264-BTN")]
        public void should_parse_language_french(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.French.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Spanish.HDTV.XviD-LOL")]
        [TestCase("Series Title - Temporada 1 [HDTV 720p][Cap.101][AC3 5.1 Castellano][www.pctnew.ORG]")]
        [TestCase("Series Title - Temporada 2 [HDTV 720p][Cap.206][AC3 5.1 Español Castellano]")]
        public void should_parse_language_spanish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Spanish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.German.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP")]
        [TestCase("The Series Title - S02E16 - Kampfhaehne - mkv - by Videomann")]
        [TestCase("Series.Title.S01E03.Ger.Dub.AAC.1080p.WebDL.x264-TKP21")]
        public void should_parse_language_german(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.German.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Italian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.1x19.ita.720p.bdmux.x264-novarip")]
        public void should_parse_language_italian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Italian.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Danish.HDTV.XviD-LOL")]
        public void should_parse_language_danish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Danish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Dutch.HDTV.XviD-LOL")]
        public void should_parse_language_dutch(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Dutch.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Japanese.HDTV.XviD-LOL")]
        public void should_parse_language_japanese(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Japanese.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Icelandic.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S01E03.1080p.WEB-DL.DD5.1.H.264-SbR Icelandic")]
        public void should_parse_language_icelandic(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Icelandic.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Chinese.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.Cantonese.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.Mandarin.HDTV.XviD-LOL")]
        [TestCase("[abc] My Series - 01 [CHS]")]
        [TestCase("[abc] My Series - 01 [CHT]")]
        [TestCase("[abc] My Series - 01 [BIG5]")]
        [TestCase("[abc] My Series - 01 [GB]")]
        [TestCase("[abc] My Series - 01 [繁中]")]
        [TestCase("[abc] My Series - 01 [繁体]")]
        [TestCase("[abc] My Series - 01 [简繁外挂]")]
        [TestCase("[abc] My Series - 01 [简繁内封字幕]")]
        [TestCase("[ABC字幕组] My Series - 01 [HDTV]")]
        [TestCase("[喵萌奶茶屋&LoliHouse] 拳愿阿修罗 / Kengan Ashura - 17 [WebRip 1080p HEVC-10bit AAC][中日双语字幕]")]
        public void should_parse_language_chinese(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Chinese.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Korean.HDTV.XviD-LOL")]
        public void should_parse_language_korean(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Korean.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Russian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S01E01.1080p.WEB-DL.Rus.Eng.TVKlondike")]
        public void should_parse_language_russian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Russian.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Polish.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.PL.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.PLLEK.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.PL-LEK.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.LEKPL.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.LEK-PL.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.PLDUB.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.PL-DUB.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.DUBPL.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.DUB-PL.HDTV.XviD-LOL")]
        public void should_parse_language_polish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Polish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Vietnamese.HDTV.XviD-LOL")]
        public void should_parse_language_vietnamese(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Vietnamese.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Swedish.HDTV.XviD-LOL")]
        public void should_parse_language_swedish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Swedish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Norwegian.HDTV.XviD-LOL")]
        public void should_parse_language_norwegian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Norwegian.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Finnish.HDTV.XviD-LOL")]
        public void should_parse_language_finnish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Finnish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Turkish.HDTV.XviD-LOL")]
        public void should_parse_language_turkish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Turkish.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Portuguese.HDTV.XviD-LOL")]
        public void should_parse_language_portuguese(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Portuguese.Id);
        }

        [TestCase("Title.the.Series.S01E01.FLEMISH.HDTV.x264-BRiGAND")]
        public void should_parse_language_flemish(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Flemish.Id);
        }

        [TestCase("Title.the.Series.S03E13.Greek.PDTV.XviD-Ouzo")]
        public void should_parse_language_greek(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Greek.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.HUNDUB-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.ENG.HUN-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.HUN-LOL")]
        public void should_parse_language_hungarian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Hungarian.Id);
        }

        [TestCase("Title.the.Series.S01-03.DVDRip.HebDub")]
        public void should_parse_language_hebrew(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Hebrew.Id);
        }

        [TestCase("Title.the.Series.S05E01.WEBRip.x264.AC3.LT.EN-CNN")]
        public void should_parse_language_lithuanian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Lithuanian.Id);
        }

        [TestCase("Title.the.Series.​S07E11.​WEB Rip.​XviD.​Louige-​CZ.​EN.​5.​1")]
        public void should_parse_language_czech(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Czech.Id);
        }

        [TestCase("Series Title.S01.ARABIC.COMPLETE.720p.NF.WEBRip.x264-PTV")]
        public void should_parse_language_arabic(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Arabic.Id);
        }

        [TestCase("The Shadow Series S01 E01-08 WebRip Dual Audio [Hindi 5.1 + English 5.1] 720p x264 AAC ESub")]
        [TestCase("The Final Sonarr (2020) S04 Complete 720p NF WEBRip [Hindi+English] Dual audio")]
        public void should_parse_language_hindi(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Hindi.Id);
        }

        [TestCase("Title.the.Series.2009.S01E14.Bulgarian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.BGAUDIO.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.BG.AUDIO.HDTV.XviD-LOL")]
        public void should_parse_language_bulgarian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Bulgarian.Id);
        }

        [TestCase("Series Title S01E01 Malayalam.1080p.WebRip.AVC.5.1-Rjaa")]
        [TestCase("Series Title S01E01 Malayalam DVDRip XviD 5.1 ESub MTR")]
        [TestCase("Series.Title.S01E01.DVDRip.1CD.Malayalam.Xvid.MP3 @Mastitorrents")]
        public void should_parse_language_malayalam(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Malayalam.Id);
        }

        [TestCase("Гало(Сезон 1, серії 1-5) / SeriesTitle(Season 1, episodes 1-5) (2022) WEBRip-AVC Ukr/Eng")]
        [TestCase("Архів 81 (Сезон 1) / Series 81 (Season 1) (2022) WEB-DLRip-AVC Ukr/Eng | Sub Ukr/Eng")]
        [TestCase("Книга Боби Фетта(Сезон 1) / Series Title(Season 1) (2021) WEB-DLRip Ukr/Eng")]
        public void should_parse_language_ukrainian(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Id.Should().Be(Language.Ukrainian.Id);
        }

        [TestCase("Title.the.Russian.Series.S01E07.Cold.Action.HDTV.XviD-Droned")]
        [TestCase("Title.the.Russian.Series.S01E07E08.Cold.Action.HDTV.XviD-Droned")]
        [TestCase("Title.the.Russian.Series.S01.1080p.WEBRip.DDP5.1.x264-Drone")]
        [TestCase("Title.the.Spanish.Series.S02E08.Peace.1080p.AMZN.WEBRip.DDP5.1.x264-NTb")]
        [TestCase("Title The Spanish S02E02 Flodden 720p AMZN WEB-DL DDP5 1 H 264-NTb")]
        public void should_not_parse_series_or_episode_title(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Name.Should().Be(Language.English.Name);
        }
    }
}
