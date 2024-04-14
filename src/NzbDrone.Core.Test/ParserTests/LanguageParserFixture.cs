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
        [TestCase("Series Title - S01E01 - Pilot.eng.sub")]
        public void should_parse_subtitle_language_english(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.English);
        }

        [TestCase("Spanish Killroy was Here S02E02 Flodden 720p AMZN WEB-DL DDP5 1 H 264-NTb")]
        [TestCase("Title.the.Spanish.Series.S02E02.1080p.WEB.H264-CAKES")]
        [TestCase("Title.the.Spanish.Series.S02E06.Field.of.Cloth.of.Gold.1080p.AMZN.WEBRip.DDP5.1.x264-NTb")]
        [TestCase("Title.the.Series.2009.S01E14.Germany.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD-LOL")]
        [TestCase("Title.the.Italian.Series.S01E01.The.Family.720p.HDTV.x264-FTP")]
        [TestCase("Title.the.Italy.Series.S02E01.720p.HDTV.x264-TLA")]
        [TestCase("Series Title - S01E01 - Pilot.en.sub")]
        public void should_parse_language_unknown(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Unknown);
        }

        [TestCase("Title.the.Series.2009.S01E14.English.HDTV.XviD-LOL")]
        [TestCase("Series Title - S01E01 - Pilot.English.sub")]
        [TestCase("Series Title - S01E01 - Pilot.english.sub")]
        [TestCase("Series S02 (1999–2003)[BDRemux 1080p AVC Esp DD2.0,Ing DTS-HD5.1,Cat DD2.0 Subs][HD-Olimpo][PACK]")]
        public void should_parse_language_english(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.English);
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
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.French);
        }

        [TestCase("Title.the.Series.2009.S01E14.Spanish.HDTV.XviD-LOL")]
        [TestCase("Series Title - Temporada 1 [HDTV 720p][Cap.101][AC3 5.1 Castellano][www.pctnew.ORG]")]
        [TestCase("Series Title - Temporada 2 [HDTV 720p][Cap.206][AC3 5.1 Español Castellano]")]
        public void should_parse_language_spanish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Spanish);
        }

        [TestCase("Title.the.Series.2009.S01E14.German.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP")]
        [TestCase("The Series Title - S02E16 - Kampfhaehne - mkv - by Videomann")]
        [TestCase("Series.Title.S01E03.Ger.Dub.AAC.1080p.WebDL.x264-TKP21")]
        public void should_parse_language_german(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.German);
        }

        [TestCase("Title.the.Series.2009.S01E14.Italian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.1x19.ita.720p.bdmux.x264-novarip")]
        public void should_parse_language_italian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Italian);
        }

        [TestCase("Title.the.Series.2009.S01E14.Danish.HDTV.XviD-LOL")]
        public void should_parse_language_danish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Danish);
        }

        [TestCase("Title.the.Series.2009.S01E14.Dutch.HDTV.XviD-LOL")]
        public void should_parse_language_dutch(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Dutch);
        }

        [TestCase("Title.the.Series.2009.S01E14.Japanese.HDTV.XviD-LOL")]
        public void should_parse_language_japanese(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Japanese);
        }

        [TestCase("Title.the.Series.2009.S01E14.Icelandic.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S01E03.1080p.WEB-DL.DD5.1.H.264-SbR Icelandic")]
        public void should_parse_language_icelandic(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Icelandic);
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
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Chinese);
        }

        [TestCase("Title.the.Series.2009.S01E14.Korean.HDTV.XviD-LOL")]
        public void should_parse_language_korean(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Korean);
        }

        [TestCase("Title.the.Series.2009.S01E08.2160p.WEB-DL.LAV.ENG")]
        [TestCase("Title.the.Series.S01.COMPLETE.2009.1080p.WEB-DL.x264.AVC.AAC.LT.LV.RU")]
        [TestCase("Title.the.Series.S03.1080p.WEB.x264.LAT.ENG")]
        [TestCase("Title.the.Series.S02E02.LATViAN.1080p.WEB.XviD-LOL")]
        public void should_parse_language_latvian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Latvian);
        }

        [TestCase("Title.the.Series.2009.S01E14.Russian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.S01E01.1080p.WEB-DL.Rus.Eng.TVKlondike")]
        [TestCase("Title.the.Series.S01.COMPLETE.2009.1080p.WEB-DL.x264.AVC.AAC.LT.LV.RU")]
        public void should_parse_language_russian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Russian);
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
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Polish);
        }

        [TestCase("Title.the.Series.2009.S01E14.Vietnamese.HDTV.XviD-LOL")]
        public void should_parse_language_vietnamese(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Vietnamese);
        }

        [TestCase("Title.the.Series.2009.S01E14.Swedish.HDTV.XviD-LOL")]
        public void should_parse_language_swedish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Swedish);
        }

        [TestCase("Title.the.Series.2009.S01E14.Norwegian.HDTV.XviD-LOL")]
        public void should_parse_language_norwegian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Norwegian);
        }

        [TestCase("Title.the.Series.2009.S01E14.Finnish.HDTV.XviD-LOL")]
        public void should_parse_language_finnish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Finnish);
        }

        [TestCase("Title.the.Series.2009.S01E14.Turkish.HDTV.XviD-LOL")]
        public void should_parse_language_turkish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Turkish);
        }

        [TestCase("Title.the.Series.2009.S01E14.Portuguese.HDTV.XviD-LOL")]
        public void should_parse_language_portuguese(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Portuguese);
        }

        [TestCase("Title.the.Series.S01E01.FLEMISH.HDTV.x264-BRiGAND")]
        public void should_parse_language_flemish(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Flemish);
        }

        [TestCase("Title.the.Series.S03E13.Greek.PDTV.XviD-Ouzo")]
        public void should_parse_language_greek(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Greek);
        }

        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.HUNDUB-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.ENG.HUN-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.HDTV.XviD.HUN-LOL")]
        public void should_parse_language_hungarian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Hungarian);
        }

        [TestCase("Title.the.Series.S01-03.DVDRip.HebDub")]
        public void should_parse_language_hebrew(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Hebrew);
        }

        [TestCase("Title.the.Series.S05E01.WEBRip.x264.AC3.LT.EN-CNN")]
        public void should_parse_language_lithuanian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Lithuanian);
        }

        [TestCase("Title.the.Series.​S07E11.​WEB Rip.​XviD.​Louige-​CZ.​EN.​5.​1")]
        public void should_parse_language_czech(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Czech);
        }

        [TestCase("Series Title.S01.ARABIC.COMPLETE.720p.NF.WEBRip.x264-PTV")]
        public void should_parse_language_arabic(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Arabic);
        }

        [TestCase("The Shadow Series S01 E01-08 WebRip Dual Audio [Hindi 5.1] 720p x264 AAC ESub")]
        [TestCase("The Final Sonarr (2020) S04 Complete 720p NF WEBRip [Hindi] Dual audio")]
        public void should_parse_language_hindi(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Hindi);
        }

        [TestCase("Title.the.Series.2009.S01E14.Bulgarian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.BGAUDIO.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.BG.AUDIO.HDTV.XviD-LOL")]
        public void should_parse_language_bulgarian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Bulgarian);
        }

        [TestCase("Series Title S01E01 Malayalam.1080p.WebRip.AVC.5.1-Rjaa")]
        [TestCase("Series Title S01E01 Malayalam DVDRip XviD 5.1 ESub MTR")]
        [TestCase("Series.Title.S01E01.DVDRip.1CD.Malayalam.Xvid.MP3 @Mastitorrents")]
        public void should_parse_language_malayalam(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Malayalam);
        }

        [TestCase("Гало(Сезон 1, серії 1-5) / SeriesTitle(Season 1, episodes 1-5) (2022) WEBRip-AVC Ukr/Eng")]
        [TestCase("Архів 81 (Сезон 1) / Series 81 (Season 1) (2022) WEB-DLRip-AVC Ukr/Eng | Sub Ukr/Eng")]
        [TestCase("Книга Боби Фетта(Сезон 1) / Series Title(Season 1) (2021) WEB-DLRip Ukr/Eng")]
        public void should_parse_language_ukrainian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Ukrainian);
        }

        [TestCase("Title.the.Series.2022.S02E22.Slovak.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2021.S01E11.HDTV.XviD.ENG.SK-LOL")]
        public void should_parse_language_slovak(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Slovak);
        }

        [TestCase("Thai.Series.Title.S01.THAI.1080p.WEBRip.x265-RARBG")]
        [TestCase("Series.Title.S02.THAI.1080p.NF.WEBRip.DDP2.0.x264-PAAI[rartv]")]
        [TestCase("Series.Title.S01.THAI.1080p.NF.WEBRip.DDP5.1.x264-NTG[rartv]")]
        public void should_parse_language_thai(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Thai);
        }

        [TestCase("Title.the.Series.2009.S01E14.Brazilian.HDTV.XviD-LOL")]
        [TestCase("Title.the.Series.2009.S01E14.Dublado.HDTV.XviD-LOL")]
        public void should_parse_language_portuguese_brazil(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.PortugueseBrazil);
        }

        [TestCase("Series.Title.S01.2019.720p_Eng-Spa(Latino)_MovieClubMx")]
        [TestCase("Series.Title.1.WEB-DL.720p.Complete.Latino.YG")]
        [TestCase("Series.Title.S08E01.1080p.WEB.H264.Latino.YG")]
        [TestCase("Series Title latino")]
        [TestCase("Series Title (Temporada 11 Completa) Audio Dual Ingles/Latino 1920x1080")]
        [TestCase("series title 7x4 audio latino")]
        public void should_parse_language_spanish_latino(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.SpanishLatino);
        }

        [TestCase("TV.Series.S01.720p.WEB-DL.RoDubbed-RLSGRP")]
        [TestCase("TV Series S22E36 ROMANIAN 1080p WEB-DL AAC 2.0 H.264-RLSGRP")]
        public void should_parse_language_romanian(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Romanian);
        }

        [TestCase("Series S02 (2009–2013)[BDRemux 1080p AVC Cat.DD2.0,Subs][HD-Olimpo][PACK]")]
        [TestCase("Series S02 (2009–2013)[BDRemux 1080p AVC Esp.DD2.0,Ing.DD2.0,Cat.DD2.0,Subs][HD-Olimpo][PACK]")]
        [TestCase("Series S02 (1999–2003)[BDRemux 1080p AVC Esp DD2.0,Ing DTS-HD5.1,Cat DD2.0 Subs][HD-Olimpo][PACK]")]
        [TestCase("Series S01 [WEB-DL NF 1080p EAC3 2.0 esp cat Subs][HDOlimpo]")]
        [TestCase("Series S02E08 M+ WEBDL 1080p SPA-CAT DD5.1 SUBS x264")]
        [TestCase("Series Title S02 (2021) [WEB-DL 1080p Castellano DD 5.1 - Catalán DD 5.1 Subs] [HDOlimpo]")]
        public void should_parse_language_catalan(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().Contain(Language.Catalan);
        }

        [TestCase("The Shadow Series S01 E01-08 WebRip Dual Audio [Hindi 5.1 + English 5.1] 720p x264 AAC ESub")]
        [TestCase("The Final Sonarr (2020) S04 Complete 720p NF WEBRip [Hindi+English] Dual audio")]
        public void should_parse_hindi_and_english(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().BeEquivalentTo(new[] { Language.Hindi, Language.English });
    }

        [TestCase("Series S01 [WEB-DL NF 1080p EAC3 2.0 esp cat Subs][HDOlimpo]")]
        [TestCase("Series S02E08 M+ WEBDL 1080p SPA-CAT DD5.1 SUBS x264")]
        [TestCase("Series Title S02 (2021) [WEB-DL 1080p Castellano DD 5.1 - Catalán DD 5.1 Subs] [HDOlimpo]")]
        public void should_parse_spanish_and_catalan(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().BeEquivalentTo(new[] { Language.Spanish, Language.Catalan });
        }

        [TestCase("Series S02 (2009–2013)[BDRemux 1080p AVC Esp.DD2.0,Ing.DD2.0,Cat.DD2.0,Subs][HD-Olimpo][PACK]")]
        [TestCase("Series S02 (1999–2003)[BDRemux 1080p AVC Esp DD2.0,Ing DTS-HD5.1,Cat DD2.0 Subs][HD-Olimpo][PACK]")]
        public void should_parse_english_spanish_and_catalan(string postTitle)
        {
            var result = LanguageParser.ParseLanguages(postTitle);
            result.Should().BeEquivalentTo(new[] { Language.English, Language.Spanish, Language.Catalan });
        }

        [TestCase("Series.Title.S01E01.German.DL.1080p.BluRay.x264-RlsGrp")]
        [TestCase("Series.Title.S01E01.GERMAN.DL.1080P.WEB.H264-RlsGrp")]
        [TestCase("Series.Title.2023.S01E01.German.DL.EAC3.1080p.DSNP.WEB.H264-RlsGrp")]
        public void should_add_original_language_to_german_release_with_dl_tag(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Languages.Count.Should().Be(2);
            result.Languages.Should().Contain(Language.German);
            result.Languages.Should().Contain(Language.Original);
        }

        [TestCase("Series.Title.2023.S01E01.GERMAN.1080P.WEB-DL.H264-RlsGrp")]
        [TestCase("Series.Title.2023.S01E01.GERMAN.1080P.WEB.DL.H264-RlsGrp")]
        [TestCase("Series Title 2023 S01E01 GERMAN 1080P WEB DL H264-RlsGrp")]
        [TestCase("Series.Title.2023.S01E01.GERMAN.1080P.WEBDL.H264-RlsGrp")]
        public void should_not_add_original_language_to_german_release_when_title_contains_web_dl(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Languages.Count.Should().Be(1);
            result.Languages.Should().Contain(Language.German);
        }

        [TestCase("Series.Title.2023.S01.German.ML.EAC3.1080p.NF.WEB.H264-RlsGrp")]
        public void should_add_original_language_and_english_to_german_release_with_ml_tag(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Languages.Count.Should().Be(3);
            result.Languages.Should().Contain(Language.German);
            result.Languages.Should().Contain(Language.Original);
            result.Languages.Should().Contain(Language.English);
        }

        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.default.eng.forced.ass", new[] { "default", "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.default.testtitle.forced.ass", new[] { "default", "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.eng.testtitle.forced.ass", new[] { "default", "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.forced.eng.ass", new[] { "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].eng.forced.testtitle.ass", new[] { "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].forced.eng.testtitle.ass", new[] { "forced" }, "testtitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.default.fra.forced.ass", new[] { "default", "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.default.testtitle.forced.ass", new[] { "default", "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.fra.testtitle.forced.ass", new[] { "default", "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.forced.fra.ass", new[] { "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].fra.forced.testtitle.ass", new[] { "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].forced.fra.testtitle.ass", new[] { "forced" }, "testtitle", "French")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].ru-something-else.srt", new string[0], "something-else", "Russian")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].Full Subtitles.eng.ass", new string[0], "Full Subtitles", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].mytitle - 1.en.ass", new string[0], "mytitle", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].mytitle 1.en.ass", new string[0], "mytitle 1", "English")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].mytitle.en.ass", new string[0], "mytitle", "English")]
        public void should_parse_title_and_tags(string postTitle, string[] expectedTags, string expectedTitle, string expectedLanguage)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(postTitle);

            subtitleTitleInfo.LanguageTags.Should().BeEquivalentTo(expectedTags);
            subtitleTitleInfo.Title.Should().BeEquivalentTo(expectedTitle);
            subtitleTitleInfo.Language.Should().BeEquivalentTo((Language)expectedLanguage);
        }

        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.forced.ass")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].default.ass")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].ass")]
        [TestCase("Name (2020) - S01E20 - [AAC 2.0].testtitle.ass")]
        public void should_not_parse_false_title(string postTitle)
        {
            var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(postTitle);
            subtitleTitleInfo.Language.Should().Be(Language.Unknown);
            subtitleTitleInfo.LanguageTags.Should().BeEmpty();
            subtitleTitleInfo.RawTitle.Should().BeNull();
        }
    }
}
