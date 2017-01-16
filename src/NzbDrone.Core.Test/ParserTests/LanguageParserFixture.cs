using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
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
        [TestCase("Ray Donovan - S01E01.720p.HDtv.x264-Evolve (NLsub)", Language.Dutch)]
        [TestCase("Shield,.The.1x13.Tueurs.De.Flics.FR.DVDRip.XviD", Language.French)]
        [TestCase("True.Detective.S01E01.1080p.WEB-DL.Rus.Eng.TVKlondike", Language.Russian)]
        [TestCase("The.Trip.To.Italy.S02E01.720p.HDTV.x264-TLA", Language.English)]
        [TestCase("Revolution S01E03 No Quarter 2012 WEB-DL 720p Nordic-philipo mkv", Language.Norwegian)]
        [TestCase("Extant.S01E01.VOSTFR.HDTV.x264-RiDERS", Language.French)]
        [TestCase("Constantine.2014.S01E01.WEBRiP.H264.AAC.5.1-NL.SUBS", Language.Dutch)]
        [TestCase("Elementary - S02E16 - Kampfhaehne - mkv - by Videomann", Language.German)]
        [TestCase("Two.Greedy.Italians.S01E01.The.Family.720p.HDTV.x264-FTP", Language.English)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUNDUB-LOL", Language.Hungarian)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.ENG.HUN-LOL", Language.Hungarian)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUN-LOL", Language.Hungarian)]
        public void should_parse_language(string postTitle, Language language)
        {
            var result = LanguageParser.ParseLanguage(postTitle);
            result.Should().Be(language);
        }

        [TestCase("2 Broke Girls - S01E01 - Pilot.en.sub", Language.English)]
        [TestCase("2 Broke Girls - S01E01 - Pilot.eng.sub", Language.English)]
        [TestCase("2 Broke Girls - S01E01 - Pilot.sub", Language.Unknown)]
        public void should_parse_subtitle_language(string fileName, Language language)
        {
            var result = LanguageParser.ParseSubtitleLanguage(fileName);
            result.Should().Be(language);
        }
    }
}
