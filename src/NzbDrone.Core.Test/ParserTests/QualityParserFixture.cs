using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]

    public class QualityParserFixture : CoreTest
    {
        public static object[] SelfQualityParserCases =
        {
            new object[] { Quality.SDTV },
            new object[] { Quality.DVD },
            new object[] { Quality.WEBDL480p },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.WEBDL1080p },
            new object[] { Quality.Bluray720p },
            new object[] { Quality.Bluray1080p }
        };

        [TestCase("S07E23 .avi ", false)]
        [TestCase("The.Shield.S01E13.x264-CtrlSD", false)]
        [TestCase("Nikita S02E01 HDTV XviD 2HD", false)]
        [TestCase("Gossip Girl S05E11 PROPER HDTV XviD 2HD", true)]
        [TestCase("The Jonathan Ross Show S02E08 HDTV x264 FTP", false)]
        [TestCase("White.Van.Man.2011.S02E01.WS.PDTV.x264-TLA", false)]
        [TestCase("White.Van.Man.2011.S02E01.WS.PDTV.x264-REPACK-TLA", true)]
        [TestCase("The Real Housewives of Vancouver S01E04 DSR x264 2HD", false)]
        [TestCase("Vanguard S01E04 Mexicos Death Train DSR x264 MiNDTHEGAP", false)]
        [TestCase("Chuck S11E03 has no periods or extension HDTV", false)]
        [TestCase("Chuck.S04E05.HDTV.XviD-LOL", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.avi", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.xvid", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.divx", false)]
        [TestCase("The.Girls.Next.Door.S03E06.HDTV-WiDE", false)]
        [TestCase("Degrassi.S10E27.WS.DSR.XviD-2HD", false)]
        public void should_parse_sdtv_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.SDTV, proper);
        }

        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3-REPACK.-HELLYWOOD.avi", true)]
        [TestCase("The.Shield.S01E13.NTSC.x264-CtrlSD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", false)]
        [TestCase("the.shield.1x13.circles.ws.xvidvd-tns", false)]
        public void should_parse_dvd_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.DVD, proper);
        }

        [TestCase("Elementary.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Glee.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", false)]
        [TestCase("The.Big.Bang.Theory.S06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", false)]  
        public void should_parse_webdl480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL480p, proper);
        }

        [TestCase("Dexter - S01E01 - Title [HDTV]", false)]
        [TestCase("Dexter - S01E01 - Title [HDTV-720p]", false)]
        [TestCase("Pawn Stars S04E87 REPACK 720p HDTV x264 aAF", true)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p", false)]
        [TestCase("S07E23 - [HDTV-720p].mkv ", false)]
        [TestCase("Chuck - S22E03 - MoneyBART - HD TV.mkv", false)]
        [TestCase("S07E23.mkv ", false)]
        [TestCase("Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.mkv", false)]
        [TestCase(@"E:\Downloads\tv\The.Big.Bang.Theory.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", false)]
        [TestCase("Gem.Hunt.S01E08.Tourmaline.Nepal.720p.HDTV.x264-DHD", false)]
        public void should_parse_hdtv720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV720p, proper);
        }

        [TestCase("Under the Dome S01E10 Let the Games Begin 1080p", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", true)]
        [TestCase("Dexter - S01E01 - Title [HDTV-1080p]", false)]
        public void should_parse_hdtv1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV1080p, proper);
        }

        [TestCase("Arrested.Development.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", false)]
        [TestCase("Vanguard S01E04 Mexicos Death Train 720p WEB DL", false)]
        [TestCase("Hawaii Five 0 S02E21 720p WEB DL DD5 1 H 264", false)]
        [TestCase("Castle S04E22 720p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", false)]
        [TestCase("S07E23 - [WEBDL].mkv ", false)]
        [TestCase("Fringe S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", false)]
        public void should_parse_webdl720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL720p, proper);
        }

        [TestCase("Arrested.Development.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", false)]
        [TestCase("CSI NY S09E03 1080p WEB DL DD5 1 H264 NFHD", false)]
        [TestCase("Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Criminal.Minds.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", false)]
        [TestCase("Its.Always.Sunny.in.Philadelphia.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", true)]
        [TestCase("Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", true)]
        [TestCase("Glee.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", false)]
        [TestCase("The.Big.Bang.Theory.S06E11.The.Santa.Simulation.1080p.WEB-DL.DD5.1.H.264", false)]
        public void should_parse_webdl1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL1080p, proper);
        }

        [TestCase("WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", false)]
        [TestCase("The Big Bang Theory.S03E01.The Electric Can Opener Fluctuation.m2ts", false)]
        public void should_parse_bluray720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray720p, proper);
        }

        [TestCase("Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", false)]
        [TestCase("Sons.Of.Anarchy.S02E13.1080p.BluRay.x264-AVCDVD", false)]
        public void should_parse_bluray1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray1080p, proper);
        }

        [TestCase("POI S02E11 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("How I Met Your Mother S01E18 Nothing Good Happens After 2 A.M. 720p HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("The Voice S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        public void should_parse_raw_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.RAWHD, proper);
        }

        [TestCase("Sonny.With.a.Chance.S02E15", false)]
        [TestCase("Law & Order: Special Victims Unit - 11x11 - Quickie", false)]
        public void quality_parse(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Unknown, proper);
        }

        [Test, TestCaseSource("SelfQualityParserCases")]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = String.Format("My series S01E01 [{0}]", quality.Name);
            var result = Parser.QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }

        private void ParseAndVerifyQuality(string title, Quality quality, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(title);
            result.Quality.Should().Be(quality);
            result.Proper.Should().Be(proper);
        }
    }
}
