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
        [TestCase("Extant.S01E01.VOSTFR.HDTV.x264-RiDERS")]
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
        [TestCase("Constantine.2014.S01E01.WEBRiP.H264.AAC.5.1-NL.SUBS")]
        [TestCase("Ray Donovan - S01E01.720p.HDtv.x264-Evolve (NLsub)")]
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

        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL")]
        public void should_parse_language_cantonese(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Cantonese.Id);
        }

        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL")]
        public void should_parse_language_mandarin(string postTitle)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Id.Should().Be(Language.Mandarin.Id);
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
        [TestCase("Revolution S01E03 No Quarter 2012 WEB-DL 720p Nordic-philipo mkv")]
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

    }
}
