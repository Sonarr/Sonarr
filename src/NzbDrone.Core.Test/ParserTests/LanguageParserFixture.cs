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
        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.Germany.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.HDTV.XviD-LOL")]
        [TestCase("Two.Greedy.Italians.S01E01.The.Family.720p.HDTV.x264-FTP")]
        [TestCase("The.Trip.To.Italy.S02E01.720p.HDTV.x264-TLA")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.en.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.eng.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.English.sub")]
        [TestCase("2 Broke Girls - S01E01 - Pilot.english.sub")]
        [TestCase("The Spanish Princess S02E02 Flodden 720p AMZN WEB-DL DDP5 1 H 264-NTb")]
        [TestCase("The.Spanish.Princess.S02E02.1080p.WEB.H264-CAKES")]
        [TestCase("The.Spanish.Princess.S02E06.Field.of.Cloth.of.Gold.1080p.AMZN.WEBRip.DDP5.1.x264-NTb")]
        public void should_parse_language_english(string postTitle)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Should().Be(Language.English);
        }

        [TestCase("2 Broke Girls - S01E01 - Pilot.sub")]
        public void should_parse_subtitle_language_unknown(string fileName)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(Language.Unknown);
        }

        [TestCase("Castle.2009.S01E14.French.HDTV.XviD-LOL")]
        [TestCase("Shield,.The.1x13.Tueurs.De.Flics.FR.DVDRip.XviD")]
        public void should_parse_language_french(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.French.Id);
        }

        [TestCase("Castle.2009.S01E14.Spanish.HDTV.XviD-LOL")]
        public void should_parse_language_spanish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Spanish.Id);
        }

        [TestCase("Castle.2009.S01E14.German.HDTV.XviD-LOL")]
        [TestCase("Burn.Notice.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP")]
        [TestCase("Elementary - S02E16 - Kampfhaehne - mkv - by Videomann")]
        public void should_parse_language_german(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.German.Id);
        }

        [TestCase("Castle.2009.S01E14.Italian.HDTV.XviD-LOL")]
        [TestCase("person.of.interest.1x19.ita.720p.bdmux.x264-novarip")]
        public void should_parse_language_italian(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Italian.Id);
        }

        [TestCase("Castle.2009.S01E14.Danish.HDTV.XviD-LOL")]
        public void should_parse_language_danish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Danish.Id);
        }

        [TestCase("Castle.2009.S01E14.Dutch.HDTV.XviD-LOL")]
        public void should_parse_language_dutch(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Dutch.Id);
        }

        [TestCase("Castle.2009.S01E14.Japanese.HDTV.XviD-LOL")]
        public void should_parse_language_japanese(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Japanese.Id);
        }

        [TestCase("Castle.2009.S01E14.Icelandic.HDTV.XviD-LOL")]
        [TestCase("S.B.S01E03.1080p.WEB-DL.DD5.1.H.264-SbR Icelandic")]
        public void should_parse_language_icelandic(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Icelandic.Id);
        }

        [TestCase("Castle.2009.S01E14.Chinese.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL")]
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
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Chinese.Id);
        }

        [TestCase("Castle.2009.S01E14.Korean.HDTV.XviD-LOL")]
        public void should_parse_language_korean(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Korean.Id);
        }

        [TestCase("Castle.2009.S01E14.Russian.HDTV.XviD-LOL")]
        [TestCase("True.Detective.S01E01.1080p.WEB-DL.Rus.Eng.TVKlondike")]
        public void should_parse_language_russian(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Russian.Id);
        }

        [TestCase("Castle.2009.S01E14.Polish.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.PL.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.PLLEK.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.PL-LEK.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.LEKPL.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.LEK-PL.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.PLDUB.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.PL-DUB.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.DUBPL.HDTV.XviD-LOL")]
        [TestCase("Castle.2009.S01E14.DUB-PL.HDTV.XviD-LOL")]
        public void should_parse_language_polish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Polish.Id);
        }

        [TestCase("Castle.2009.S01E14.Vietnamese.HDTV.XviD-LOL")]
        public void should_parse_language_vietnamese(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Vietnamese.Id);
        }

        [TestCase("Castle.2009.S01E14.Swedish.HDTV.XviD-LOL")]
        public void should_parse_language_swedish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Swedish.Id);
        }

        [TestCase("Castle.2009.S01E14.Norwegian.HDTV.XviD-LOL")]
        public void should_parse_language_norwegian(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Norwegian.Id);
        }

        [TestCase("Castle.2009.S01E14.Finnish.HDTV.XviD-LOL")]
        public void should_parse_language_finnish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Finnish.Id);
        }

        [TestCase("Castle.2009.S01E14.Turkish.HDTV.XviD-LOL")]
        public void should_parse_language_turkish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Turkish.Id);
        }

        [TestCase("Castle.2009.S01E14.Portuguese.HDTV.XviD-LOL")]
        public void should_parse_language_portuguese(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Portuguese.Id);
        }
        
        [TestCase("Salamander.S01E01.FLEMISH.HDTV.x264-BRiGAND")]
        public void should_parse_language_flemish(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Flemish.Id);
        }

        [TestCase("H.Polukatoikia.S03E13.Greek.PDTV.XviD-Ouzo")]
        public void should_parse_language_greek(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Greek.Id);
        }

        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUNDUB-LOL")]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.ENG.HUN-LOL")]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUN-LOL")]
        public void should_parse_language_hungarian(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Hungarian.Id);
        }

        [TestCase("Avatar.The.Last.Airbender.S01-03.DVDRip.HebDub")]
        public void should_parse_language_hebrew(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Hebrew.Id);
        }

        [TestCase("Prison.Break.S05E01.WEBRip.x264.AC3.LT.EN-CNN")]
        public void should_parse_language_lithuanian(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Lithuanian.Id);
        }

        [TestCase("The.​Walking.​Dead.​S07E11.​WEB Rip.​XviD.​Louige-​CZ.​EN.​5.​1")]
        public void should_parse_language_czech(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Czech.Id);
        }

        [TestCase("Russian.Puppets.S01E07.Cold.Action.HDTV.XviD-Droned")]
        [TestCase("Russian.Puppets.S01E07E08.Cold.Action.HDTV.XviD-Droned")]
        [TestCase("Russian.Puppets.S01.1080p.WEBRip.DDP5.1.x264-Drone")]
        [TestCase("The.Spanish.Princess.S02E08.Peace.1080p.AMZN.WEBRip.DDP5.1.x264-NTb")]
        [TestCase("The Spanish Princess S02E02 Flodden 720p AMZN WEB-DL DDP5 1 H 264-NTb")]
        public void should_not_parse_series_or_episode_title(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Name.Should().Be(Language.English.Name);
        }
    }
}
