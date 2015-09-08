using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class LanguageParserFixture : CoreTest
    {
        [TestCase("Castle.2009.S01E14.English.HDTV.XviD-LOL", Language.English, false)]
        [TestCase("Castle.2009.S01E14.French.HDTV.XviD-LOL", Language.French, false)]
        [TestCase("Castle.2009.S01E14.Spanish.HDTV.XviD-LOL", Language.Spanish, false)]
        [TestCase("Castle.2009.S01E14.German.HDTV.XviD-LOL", Language.German, false)]
        [TestCase("Castle.2009.S01E14.Germany.HDTV.XviD-LOL", Language.English, false)]
        [TestCase("Castle.2009.S01E14.Italian.HDTV.XviD-LOL", Language.Italian, false)]
        [TestCase("Castle.2009.S01E14.Danish.HDTV.XviD-LOL", Language.Danish, false)]
        [TestCase("Castle.2009.S01E14.Dutch.HDTV.XviD-LOL", Language.Dutch, false)]
        [TestCase("Castle.2009.S01E14.Japanese.HDTV.XviD-LOL", Language.Japanese, false)]
        [TestCase("Castle.2009.S01E14.Cantonese.HDTV.XviD-LOL", Language.Cantonese, false)]
        [TestCase("Castle.2009.S01E14.Mandarin.HDTV.XviD-LOL", Language.Mandarin)]
        [TestCase("Castle.2009.S01E14.Korean.HDTV.XviD-LOL", Language.Korean, false)]
        [TestCase("Castle.2009.S01E14.Russian.HDTV.XviD-LOL", Language.Russian, false)]
        [TestCase("Castle.2009.S01E14.Polish.HDTV.XviD-LOL", Language.Polish, false)]
        [TestCase("Castle.2009.S01E14.Vietnamese.HDTV.XviD-LOL", Language.Vietnamese, false)]
        [TestCase("Castle.2009.S01E14.Swedish.HDTV.XviD-LOL", Language.Swedish, false)]
        [TestCase("Castle.2009.S01E14.Norwegian.HDTV.XviD-LOL", Language.Norwegian, false)]
        [TestCase("Castle.2009.S01E14.Finnish.HDTV.XviD-LOL", Language.Finnish, false)]
        [TestCase("Castle.2009.S01E14.Turkish.HDTV.XviD-LOL", Language.Turkish, false)]
        [TestCase("Castle.2009.S01E14.Portuguese.HDTV.XviD-LOL", Language.Portuguese, false)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD-LOL", Language.English, false)]
        [TestCase("person.of.interest.1x19.ita.720p.bdmux.x264-novarip", Language.Italian, false)]
        [TestCase("Salamander.S01E01.FLEMISH.HDTV.x264-BRiGAND", Language.Flemish, false)]
        [TestCase("H.Polukatoikia.S03E13.Greek.PDTV.XviD-Ouzo", Language.Greek, false)]
        [TestCase("Burn.Notice.S04E15.Brotherly.Love.GERMAN.DUBBED.WS.WEBRiP.XviD.REPACK-TVP", Language.German, true)]
        [TestCase("Ray Donovan - S01E01.720p.HDtv.x264-Evolve (NLsub)", Language.Dutch, true)]
        [TestCase("Shield,.The.1x13.Tueurs.De.Flics.FR.DVDRip.XviD", Language.French, false)]
        [TestCase("True.Detective.S01E01.1080p.WEB-DL.Rus.Eng.TVKlondike", Language.Russian, false)]
        [TestCase("The.Trip.To.Italy.S02E01.720p.HDTV.x264-TLA", Language.English, false)]
        [TestCase("Revolution S01E03 No Quarter 2012 WEB-DL 720p Nordic-philipo mkv", Language.Norwegian, false)]
        [TestCase("Extant.S01E01.VOSTFR.HDTV.x264-RiDERS", Language.French, true)]
        [TestCase("Constantine.2014.S01E01.WEBRiP.H264.AAC.5.1-NL.SUBS", Language.Dutch, true)]
        [TestCase("Elementary - S02E16 - Kampfhaehne - mkv - by Videomann", Language.German, false)]
        [TestCase("Two.Greedy.Italians.S01E01.The.Family.720p.HDTV.x264-FTP", Language.English, false)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUNDUB-LOL", Language.Hungarian, true)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.ENG.HUN-LOL", Language.Hungarian, false)]
        [TestCase("Castle.2009.S01E14.HDTV.XviD.HUN-LOL", Language.Hungarian, false)]
        public void should_parse_language(string postTitle, Language language, bool isSubtitled)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Language.Should().Be(language);
            result.IsSubtitled.Should().Be(isSubtitled);
        }
     }
}
