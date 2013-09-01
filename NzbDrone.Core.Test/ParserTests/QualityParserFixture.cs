

using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]

    public class QualityParserFixture : CoreTest
    {
        public static object[] SdtvCases =
        {
            new object[] { "S07E23 .avi ", false },
            new object[] {"The.Shield.S01E13.x264-CtrlSD", false},
            new object[] { "Nikita S02E01 HDTV XviD 2HD", false },
            new object[] { "Gossip Girl S05E11 PROPER HDTV XviD 2HD", true },
            new object[] { "The Jonathan Ross Show S02E08 HDTV x264 FTP", false },
            new object[] { "White.Van.Man.2011.S02E01.WS.PDTV.x264-TLA", false },
            new object[] { "White.Van.Man.2011.S02E01.WS.PDTV.x264-REPACK-TLA", true },
            new object[] { "The Real Housewives of Vancouver S01E04 DSR x264 2HD", false },
            new object[] { "Vanguard S01E04 Mexicos Death Train DSR x264 MiNDTHEGAP", false },
            new object[] { "Chuck S11E03 has no periods or extension HDTV", false },
            new object[] { "Chuck.S04E05.HDTV.XviD-LOL", false },
            new object[] { "Sonny.With.a.Chance.S02E15.avi", false },
            new object[] { "Sonny.With.a.Chance.S02E15.xvid", false },
            new object[] { "Sonny.With.a.Chance.S02E15.divx", false },
            new object[] { "The.Girls.Next.Door.S03E06.HDTV-WiDE", false },
            new object[] { "Degrassi.S10E27.WS.DSR.XviD-2HD", false },
        };

        public static object[] DvdCases =
        {
            new object[] { "WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3-REPACK.-HELLYWOOD.avi", true },
            new object[] { "The.Shield.S01E13.NTSC.x264-CtrlSD", false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false },
            new object[] { "WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", false },
            new object[] { "The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", false },
            new object[] { "The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", false },
        };

        public static object[] Webdl480pCases =
        {
            new object[] { "Elementary.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", false },
            new object[] { "Glee.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", false },
            new object[] { "The.Big.Bang.Theory.S06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", false },            
        };

        public static object[] Hdtv720pCases =
        {
            new object[] { "Dexter - S01E01 - Title [HDTV]", false },
            new object[] { "Dexter - S01E01 - Title [HDTV-720p]", false },
            new object[] { "Pawn Stars S04E87 REPACK 720p HDTV x264 aAF", true },
            new object[] { "Sonny.With.a.Chance.S02E15.720p", false },
            new object[] { "S07E23 - [HDTV-720p].mkv ", false },
            new object[] { "Chuck - S22E03 - MoneyBART - HD TV.mkv", false },            
            new object[] { "S07E23.mkv ", false },
            new object[] { "Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", false },
            new object[] { "Sonny.With.a.Chance.S02E15.mkv", false },        
        };

        public static object[] Hdtv1080pCases =
        {
            new object[] { "Under the Dome S01E10 Let the Games Begin 1080p", false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", true },
            new object[] { "Dexter - S01E01 - Title [HDTV-1080p]", false },
        };

        public static object[] Webdl720pCases =
        {
            new object[] { "Arrested.Development.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", false },
            new object[] { "Vanguard S01E04 Mexicos Death Train 720p WEB DL", false },
            new object[] { "Hawaii Five 0 S02E21 720p WEB DL DD5 1 H 264", false },
            new object[] { "Castle S04E22 720p WEB DL DD5 1 H 264 NFHD", false },
            new object[] { "Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", false },
            new object[] { "Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", false },
            new object[] { "S07E23 - [WEBDL].mkv ", false },
            new object[] { "Fringe S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", false },
        };

        public static object[] Webdl1080pCases =
        {
            new object[] { "Arrested.Development.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", false },
            new object[] { "CSI NY S09E03 1080p WEB DL DD5 1 H264 NFHD", false },
            new object[] { "Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 NFHD", false },
            new object[] { "Criminal.Minds.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", false },
            new object[] { "Its.Always.Sunny.in.Philadelphia.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", true },
            new object[] { "Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", true },
            new object[] { "Glee.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", false },
            new object[] { "The.Big.Bang.Theory.S06E11.The.Santa.Simulation.1080p.WEB-DL.DD5.1.H.264", false },        
        };

        public static object[] Bluray720pCases =
        {
            new object[] { "WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", false },
            new object[] { "Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv",  false },
        };

        public static object[] Bluray1080pCases =
        {
            new object[] { "Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", false },
            new object[] { "Sons.Of.Anarchy.S02E13.1080p.BluRay.x264-AVCDVD", false },
        };

        public static object[] RawCases =
        {
            new object[] { "POI S02E11 1080i HDTV DD5.1 MPEG2-TrollHD", false },
            new object[] { "How I Met Your Mother S01E18 Nothing Good Happens After 2 A.M. 720p HDTV DD5.1 MPEG2-TrollHD", false },
            new object[] { "The Voice S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", false },
        };

        public static object[] UnknownCases =
        {
            new object[] { "Sonny.With.a.Chance.S02E15", false },
            new object[] { "Law & Order: Special Victims Unit - 11x11 - Quickie", false },

        };

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

        [Test, TestCaseSource("SdtvCases")]
        public void should_parse_sdtv_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.SDTV);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("DvdCases")]
        public void should_parse_dvd_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.DVD);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Webdl480pCases")]
        public void should_parse_webdl480p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.WEBDL480p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Hdtv720pCases")]
        public void should_parse_hdtv720p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.HDTV720p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Hdtv1080pCases")]
        public void should_parse_hdtv1080p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.HDTV1080p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Webdl720pCases")]
        public void should_parse_webdl720p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.WEBDL720p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Webdl1080pCases")]
        public void should_parse_webdl1080p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.WEBDL1080p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Bluray720pCases")]
        public void should_parse_bluray720p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.Bluray720p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("Bluray1080pCases")]
        public void should_parse_bluray1080p_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.Bluray1080p);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("RawCases")]
        public void should_parse_raw_quality(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.RAWHD);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("UnknownCases")]
        public void quality_parse(string postTitle, bool proper)
        {
            var result = Parser.QualityParser.ParseQuality(postTitle);
            result.Quality.Should().Be(Quality.Unknown);
            result.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("SelfQualityParserCases")]
        public void parsing_our_own_quality_enum(Quality quality)
        {
            var fileName = String.Format("My series S01E01 [{0}]", quality);
            var result = Parser.QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }
    }
}
