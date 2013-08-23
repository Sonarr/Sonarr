

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
        public static object[] QualityParserCases =
        {
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", Quality.DVD, false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", Quality.DVD, false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", Quality.DVD, false },
            new object[] { "Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", Quality.HDTV720p, false },
            new object[] { "Chuck S11E03 has no periods or extension HDTV", Quality.SDTV, false },
            new object[] { "Chuck.S04E05.HDTV.XviD-LOL", Quality.SDTV, false },
            new object[] { "The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", Quality.DVD, false },
            new object[] { "The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", Quality.DVD, false },
            new object[] { "The.Girls.Next.Door.S03E06.HDTV-WiDE", Quality.SDTV, false },
            new object[] { "Degrassi.S10E27.WS.DSR.XviD-2HD", Quality.SDTV, false },
            new object[] { "Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", Quality.WEBDL720p, false },
            new object[] { "Sonny.With.a.Chance.S02E15.720p", Quality.HDTV720p, false },
            new object[] { "Sonny.With.a.Chance.S02E15.mkv", Quality.HDTV720p, false },
            new object[] { "Sonny.With.a.Chance.S02E15.avi", Quality.SDTV, false },
            new object[] { "Sonny.With.a.Chance.S02E15.xvid", Quality.SDTV, false },
            new object[] { "Sonny.With.a.Chance.S02E15.divx", Quality.SDTV, false },
            new object[] { "Sonny.With.a.Chance.S02E15", Quality.Unknown, false },
            new object[] { "Chuck - S01E04 - So Old - Playdate - 720p TV.mkv", Quality.HDTV720p, false },
            new object[] { "Chuck - S22E03 - MoneyBART - HD TV.mkv", Quality.HDTV720p, false },
            new object[] { "Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", Quality.Bluray720p, false },
            new object[] { "Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", Quality.Bluray1080p, false },
            new object[] { "Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", Quality.WEBDL720p, false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", Quality.DVD, false },
            new object[] { "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", Quality.DVD, false },
            new object[] { "Law & Order: Special Victims Unit - 11x11 - Quickie", Quality.Unknown, false },
            new object[] { "S07E23 - [HDTV-720p].mkv ", Quality.HDTV720p, false },
            new object[] { "S07E23 - [WEBDL].mkv ", Quality.WEBDL720p, false },
            new object[] { "S07E23.mkv ", Quality.HDTV720p, false },
            new object[] { "S07E23 .avi ", Quality.SDTV, false },
            new object[] { "WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", Quality.DVD, false },
            new object[] { "WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", Quality.Bluray720p, false },
            new object[] { "The Voice S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", Quality.RAWHD, false },
            new object[] { "Nikita S02E01 HDTV XviD 2HD", Quality.SDTV, false },
            new object[] { "Gossip Girl S05E11 PROPER HDTV XviD 2HD", Quality.SDTV, true },
            new object[] { "The Jonathan Ross Show S02E08 HDTV x264 FTP", Quality.SDTV, false },
            new object[] { "White.Van.Man.2011.S02E01.WS.PDTV.x264-TLA", Quality.SDTV, false },
            new object[] { "White.Van.Man.2011.S02E01.WS.PDTV.x264-REPACK-TLA", Quality.SDTV, true },
            new object[] { "WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3-REPACK.-HELLYWOOD.avi", Quality.DVD, true },
            new object[] { "Pawn Stars S04E87 REPACK 720p HDTV x264 aAF", Quality.HDTV720p, true },
            new object[] { "The Real Housewives of Vancouver S01E04 DSR x264 2HD", Quality.SDTV, false },
            new object[] { "Vanguard S01E04 Mexicos Death Train DSR x264 MiNDTHEGAP", Quality.SDTV, false },
            new object[] { "Vanguard S01E04 Mexicos Death Train 720p WEB DL", Quality.WEBDL720p, false },
            new object[] { "Hawaii Five 0 S02E21 720p WEB DL DD5 1 H 264", Quality.WEBDL720p, false },
            new object[] { "Castle S04E22 720p WEB DL DD5 1 H 264 NFHD", Quality.WEBDL720p, false },
            new object[] { "Fringe S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", Quality.WEBDL720p, false },
            new object[] { "CSI NY S09E03 1080p WEB DL DD5 1 H264 NFHD", Quality.WEBDL1080p, false },
            new object[] { "Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 NFHD", Quality.WEBDL1080p, false },
            new object[] { "Criminal.Minds.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", Quality.WEBDL1080p, false },
            new object[] { "Its.Always.Sunny.in.Philadelphia.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", Quality.WEBDL1080p, true },
            new object[] { "Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", Quality.WEBDL1080p, true },
            new object[] { "Glee.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", Quality.WEBDL1080p, false },
            new object[] { "Elementary.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", Quality.WEBDL480p, false },
            new object[] { "Glee.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", Quality.WEBDL480p, false },
            new object[] { "The.Big.Bang.Theory.S06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", Quality.WEBDL480p, false },
            new object[] { "The.Big.Bang.Theory.S06E11.The.Santa.Simulation.1080p.WEB-DL.DD5.1.H.264", Quality.WEBDL1080p, false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", Quality.HDTV1080p, false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", Quality.HDTV1080p, false },
            new object[] { "DEXTER.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", Quality.HDTV1080p, true },
            new object[] { "Dexter - S01E01 - Title [HDTV]", Quality.HDTV720p, false },
            new object[] { "Dexter - S01E01 - Title [HDTV-720p]", Quality.HDTV720p, false },
            new object[] { "Dexter - S01E01 - Title [HDTV-1080p]", Quality.HDTV1080p, false },
            new object[] { "POI S02E11 1080i HDTV DD5.1 MPEG2-TrollHD", Quality.RAWHD, false },
            new object[] { "How I Met Your Mother S01E18 Nothing Good Happens After 2 A.M. 720p HDTV DD5.1 MPEG2-TrollHD", Quality.RAWHD, false },
            new object[] { "Arrested.Development.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", Quality.WEBDL1080p, false },
            new object[] { "Arrested.Development.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", Quality.WEBDL720p, false },
            new object[] { "Sons.Of.Anarchy.S02E13.1080p.BluRay.x264-AVCDVD", Quality.Bluray1080p, false }
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

        [Test, TestCaseSource("QualityParserCases")]
        public void quality_parse(string postTitle, Quality quality, bool proper)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Quality.Quality.Should().Be(quality);
            result.Quality.Proper.Should().Be(proper);
        }

        [Test, TestCaseSource("SelfQualityParserCases")]
        public void parsing_our_own_quality_enum(Quality quality)
        {
            var fileName = String.Format("My series S01E01 [{0}]", quality);
            var result = Parser.Parser.ParseTitle(fileName);
            result.Quality.Quality.Should().Be(quality);
        }
    }
}
