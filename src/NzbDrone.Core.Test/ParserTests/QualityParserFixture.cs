using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
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
            new object[] { Quality.Bluray480p },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.HDTV1080p },
            new object[] { Quality.HDTV2160p },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.WEBDL1080p },
            new object[] { Quality.WEBDL2160p },
            new object[] { Quality.Bluray720p },
            new object[] { Quality.Bluray1080p },
            new object[] { Quality.Bluray2160p },
            new object[] { Quality.Bluray1080pRemux },
            new object[] { Quality.Bluray2160pRemux },
        };

        public static object[] OtherSourceQualityParserCases =
        {
            new object[] { "SD TV", Quality.SDTV },
            new object[] { "SD DVD",  Quality.DVD },
            new object[] { "480p WEB-DL", Quality.WEBDL480p },
            new object[] { "HD TV", Quality.HDTV720p },
            new object[] { "1080p HD TV", Quality.HDTV1080p },
            new object[] { "2160p HD TV", Quality.HDTV2160p },
            new object[] { "720p WEB-DL", Quality.WEBDL720p },
            new object[] { "1080p WEB-DL", Quality.WEBDL1080p },
            new object[] { "2160p WEB-DL", Quality.WEBDL2160p },
            new object[] { "720p BluRay", Quality.Bluray720p },
            new object[] { "1080p BluRay", Quality.Bluray1080p },
            new object[] { "2160p BluRay", Quality.Bluray2160p },
            new object[] { "1080p Remux", Quality.Bluray1080pRemux },
            new object[] { "2160p Remux", Quality.Bluray2160pRemux },
        };

        [TestCase("S07E23 .avi ", false)]
        [TestCase("The.Series.S01E13.x264-CtrlSD", false)]
        [TestCase("The Series S02E01 HDTV XviD 2HD", false)]
        [TestCase("The Series S05E11 PROPER HDTV XviD 2HD", true)]
        [TestCase("The Series Show S02E08 HDTV x264 FTP", false)]
        [TestCase("The.Series.2011.S02E01.WS.PDTV.x264-TLA", false)]
        [TestCase("The.Series.2011.S02E01.WS.PDTV.x264-REPACK-TLA", true)]
        [TestCase("The Series S01E04 DSR x264 2HD", false)]
        [TestCase("The Series S01E04 Series Death Train DSR x264 MiNDTHEGAP", false)]
        [TestCase("The Series S11E03 has no periods or extension HDTV", false)]
        [TestCase("The.Series.S04E05.HDTV.XviD-LOL", false)]
        [TestCase("The.Series.S02E15.avi", false)]
        [TestCase("The.Series.S02E15.xvid", false)]
        [TestCase("The.Series.S02E15.divx", false)]
        [TestCase("The.Series.S03E06.HDTV-WiDE", false)]
        [TestCase("Series.S10E27.WS.DSR.XviD-2HD", false)]
        [TestCase("[HorribleSubs] The Series - 32 [480p]", false)]
        [TestCase("[CR] The Series - 004 [480p][48CE2D0F]", false)]
        [TestCase("[Hatsuyuki] The Series - 363 [848x480][ADE35E38]", false)]
        [TestCase("The.Series.S03.TVRip.XviD-NOGRP", false)]
        [TestCase("[HorribleSubs] The Series - 03 [360p].mkv", false)]
        [TestCase("[SubsPlease] Series Title (540p) [AB649D32].mkv", false)]
        [TestCase("[Erai-raws] Series Title [540p][Multiple Subtitle].mkv", false)]
        public void should_parse_sdtv_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.SDTV, proper);
        }

        [TestCase("The.Series.S01E13.NTSC.x264-CtrlSD", false)]
        [TestCase("The.Series.S03E06.DVDRip.XviD-WiDE", false)]
        [TestCase("The.Series.S03E06.DVD.Rip.XviD-WiDE", false)]
        [TestCase("the.Series.1x13.circles.ws.xvidvd-tns", false)]
        [TestCase("the_Series.9x18.sunshine_days.ac3.ws_dvdrip_xvid-fov.avi", false)]
        [TestCase("[FroZen] Series - 23 [DVD][7F6170E6]", false)]
        [TestCase("[AniDL] Series - 26 -[360p][DVD][D - A][Exiled - Destiny]", false)]
        public void should_parse_dvd_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.DVD, proper);
        }

        [TestCase("The.Series.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", false)]
        [TestCase("The.Series.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", false)]
        [TestCase("The.SeriesS06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", false)]
        [TestCase("The.Series.S02E04.480p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("The.Series.S01E08.Das.geloeschte.Ich.German.Dubbed.DL.AmazonHD.x264-TVS", false)]
        [TestCase("The.Series.S01E04.Rod.Trip.mit.meinem.Onkel.German.DL.NetflixUHD.x264", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][480p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        public void should_parse_webdl480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL480p, proper);
        }

        [TestCase("SERIES.S03E01-06.DUAL.XviD.Bluray.AC3-REPACK.-HELLYWOOD.avi", true)]
        [TestCase("SERIES.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", false)]
        [TestCase("SERIES.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", false)]
        [TestCase("SERIES.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", false)]
        [TestCase("SERIES.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false)]
        [TestCase("SERIES.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("The.Series.S01E05.576p.BluRay.DD5.1.x264-HiSD", false)]
        [TestCase("The.Series.S01E05.480p.BluRay.DD5.1.x264-HiSD", false)]
        [TestCase("The Series (BD)(640x480(RAW) (BATCH 1) (1-13)", false)]
        [TestCase("[Doki] Series - 02 (848x480 XviD BD MP3) [95360783]", false)]
        public void should_parse_bluray480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray480p, proper);
        }

        [TestCase("The.Series.S02E10.480p.HULU.WEBRip.x264-Puffin", false)]
        [TestCase("The.Series.S10E14.Techs.And.Balances.480p.AE.WEBRip.AAC2.0.x264-SEA", false)]
        [TestCase("Series.Title.1x04.ITA.WEBMux.x264-NovaRip", false)]
        public void should_parse_webrip480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip480p, proper);
        }

        [TestCase("Series - S01E01 - Title [HDTV]", false)]
        [TestCase("Series - S01E01 - Title [HDTV-720p]", false)]
        [TestCase("The Series S04E87 REPACK 720p HDTV x264 aAF", true)]
        [TestCase("The.Series.S02E15.720p", false)]
        [TestCase("S07E23 - [HDTV-720p].mkv ", false)]
        [TestCase("Series - S22E03 - MoneyBART - HD TV.mkv", false)]
        [TestCase("S07E23.mkv ", false)]
        [TestCase("The.Series.S08E05.720p.HDTV.X264-DIMENSION", false)]
        [TestCase("The.Series.S02E15.mkv", false)]
        [TestCase(@"E:\Downloads\tv\The.Series.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", false)]
        [TestCase("The.Series.S01E08.Tourmaline.Nepal.720p.HDTV.x264-DHD", false)]
        [TestCase("[Underwater-FFF] The Series - 01 (720p) [27AAA0A0]", false)]
        [TestCase("[Doki] The Series - 07 (1280x720 Hi10P AAC) [80AF7DDE]", false)]
        [TestCase("[Doremi].The.Series.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", false)]
        [TestCase("[HorribleSubs]_Series_Title_-_145_[720p]", false)]
        [TestCase("[Eveyuu] Series Title - 10 [Hi10P 1280x720 H264][10B23BD8]", false)]
        [TestCase("The.Series.US.S12E17.HR.WS.PDTV.X264-DIMENSION", false)]
        [TestCase("The.Series.The.Lost.Sonarr.Summer.HR.WS.PDTV.x264-DHD", false)]
        [TestCase("The Series S01E07 - Motor zmen (CZ)[TvRip][HEVC][720p]", false)]
        [TestCase("The.Series.S05E06.720p.HDTV.x264-FHD", false)]
        [TestCase("Series.Title.1x01.ITA.720p.x264-RlsGrp [01/54] - \"series.title.1x01.ita.720p.x264-rlsgrp.nfo\"", false)]
        [TestCase("[TMS-Remux].Series.Title.X.21.720p.[76EA1C53].mkv", false)]
        public void should_parse_hdtv720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV720p, proper);
        }

        [TestCase("Under the Series S01E10 Let the Sonarr Begin 1080p", false)]
        [TestCase("Series.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", false)]
        [TestCase("Series.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", false)]
        [TestCase("Series.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", true)]
        [TestCase("Series - S01E01 - Title [HDTV-1080p]", false)]
        [TestCase("[HorribleSubs] Series Title - 32 [1080p]", false)]
        [TestCase("Series S01E07 - Sonarr zmen (CZ)[TvRip][HEVC][1080p]", false)]
        [TestCase("The Online Series Alicization 04 vostfr FHD", false)]
        [TestCase("Series Slayer 04 vostfr FHD.mkv", false)]
        [TestCase("[Onii-ChanSub] The.Series - 02 vostfr (FHD 1080p 10bits).mkv", false)]
        [TestCase("[Miaou] Series Title 02 VOSTFR FHD 10 bits", false)]
        [TestCase("[mhastream.com]_Episode_05_FHD.mp4", false)]
        [TestCase("[Kousei]_One_Series_ - _609_[FHD][648A87C7].mp4", false)]
        [TestCase("Series culpable 1x02 Culpabilidad [HDTV 1080i AVC MP2 2.0 Sub][GrupoHDS]", false)]
        [TestCase("Series cómo pasó - 19x15 [344] Cuarenta años de baile [HDTV 1080i AVC MP2 2.0 Sub][GrupoHDS]", false)]
        [TestCase("Super.Seires.Go.S01E02.Depths.of.Sonarr.1080i.HDTV.DD5.1.H.264-NOGRP", false)]
        public void should_parse_hdtv1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV1080p, proper);
        }

        [TestCase("My Title - S01E01 - EpTitle [HEVC 4k DTSHD-MA-6ch]", false)]
        [TestCase("My Title - S01E01 - EpTitle [HEVC-4k DTSHD-MA-6ch]", false)]
        [TestCase("My Title - S01E01 - EpTitle [4k HEVC DTSHD-MA-6ch]", false)]
        public void should_parse_hdtv2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV2160p, proper);
        }

        [TestCase("Series S01E04 Mexicos Death Train 720p WEB DL", false)]
        [TestCase("Series Five 0 S02E21 720p WEB DL DD5 1 H 264", false)]
        [TestCase("Series S04E22 720p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Series - S11E06 - D-Yikes! - 720p WEB-DL.mkv", false)]
        [TestCase("The.Series.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", false)]
        [TestCase("S07E23 - [WEBDL].mkv ", false)]
        [TestCase("Series S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", false)]
        [TestCase("Series.S04.720p.Web-Dl.Dd5.1.h264-P2PACK", false)]
        [TestCase("Da.Series.Shows.S02E04.720p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("Series.Miami.S04E25.720p.iTunesHD.AVC-TVS", false)]
        [TestCase("Series.S06E23.720p.WebHD.h264-euHD", false)]
        [TestCase("Series.Title.2016.03.14.720p.WEB.x264-spamTV", false)]
        [TestCase("Series.Title.2016.03.14.720p.WEB.h264-spamTV", false)]
        [TestCase("Series.S01E08.Das.geloeschte.Ich.German.DD51.Dubbed.DL.720p.AmazonHD.x264-TVS", false)]
        [TestCase("Series.Polo.S01E11.One.Hundred.Sonarrs.2015.German.DD51.DL.720p.NetflixUHD.x264.NewUp.by.Wunschtante", false)]
        [TestCase("Series 2016 German DD51 DL 720p NetflixHD x264-TVS", false)]
        [TestCase("Series.6x10.Basic.Sonarr.Repair.and.Replace.ITA.ENG.720p.WEB-DLMux.H.264-GiuseppeTnT", false)]
        [TestCase("Series.6x11.Modern.Spy.ITA.ENG.720p.WEB.DLMux.H.264-GiuseppeTnT", false)]
        [TestCase("The Series Was Dead 2010 S09E13 [MKV / H.264 / AC3/AAC / WEB / Dual Áudio / Inglês / 720p]", false)]
        [TestCase("into.the.Series.s03e16.h264.720p-web-handbrake.mkv", false)]
        [TestCase("Series.S01E01.The.Sonarr.Principle.720p.WEB-DL.DD5.1.H.264-BD", false)]
        [TestCase("Series.S03E05.Griebnitzsee.German.720p.MaxdomeHD.AVC-TVS", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][720p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("Series.Title.S04E13.960p.WEB-DL.AAC2.0.H.264-squalor", false)]
        [TestCase("Series.Title.S16.DP.WEB.720p.DDP.5.1.H.264.PLEX", false)]
        public void should_parse_webdl720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL720p, proper);
        }

        [TestCase("Series.Title.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", false)]
        [TestCase("Series.Title.S01E07.A.Prayer.For.Mad.Sweeney.720p.AMZN.WEBRip.DD5.1.x264-NTb", false)]
        [TestCase("Series.Title.S07E01.A.New.Home.720p.DSNY.WEBRip.AAC2.0.x264-TVSmash", false)]
        [TestCase("Series.Title.1x04.ITA.720p.WEBMux.x264-NovaRip", false)]
        public void should_parse_webrip720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip720p, proper);
        }

        [TestCase("Series S09E03 1080p WEB DL DD5 1 H264 NFHD", false)]
        [TestCase("Two and a Half Developers of the Series S10E03 1080p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Series.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", false)]
        [TestCase("Its.Always.Sonarrs.Fault.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", true)]
        [TestCase("This is an Easter Egg S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", true)]
        [TestCase("Series.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", false)]
        [TestCase("The.Big.Easter.Theory.S06E11.The.Sonarr.Simulation.1080p.WEB-DL.DD5.1.H.264", false)]
        [TestCase("Sonarr's.Baby.S01E02.Night.2.[WEBDL-1080p].mkv", false)]
        [TestCase("Series.Title.2016.03.14.1080p.WEB.x264-spamTV", false)]
        [TestCase("Series.Title.2016.03.14.1080p.WEB.h264-spamTV", false)]
        [TestCase("Series.S01.1080p.WEB-DL.AAC2.0.AVC-TrollHD", false)]
        [TestCase("Series Title S06E08 1080p WEB h264-EXCLUSIVE", false)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB DD5 1 H 264-EXCLUSIVE", true)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB H 264-EXCLUSIVE", true)]
        [TestCase("The.Series.S25E21.Pay.No1.1080p.WEB-DL.DD5.1.H.264-NTb", false)]
        [TestCase("Series.S01E08.Das.geloeschte.Ich.German.DD51.Dubbed.DL.1080p.AmazonHD.x264-TVS", false)]
        [TestCase("Death.Series.2017.German.DD51.DL.1080p.NetflixHD.x264-TVS", false)]
        [TestCase("Series.S01E08.Pro.Gamer.1440p.BKPL.WEB-DL.H.264-LiGHT", false)]
        [TestCase("Series.Title.S04E11.Teddy's.Choice.FHD.1080p.Web-DL", false)]
        [TestCase("Series.S04E03.The.False.Bride.1080p.NF.WEB.DDP5.1.x264-NTb[rartv]", false)]
        [TestCase("Series.Title.S02E02.This.Year.Will.Be.Different.1080p.AMZN.WEB...", false)]
        [TestCase("Series.Title.S02E02.This.Year.Will.Be.Different.1080p.AMZN.WEB.", false)]
        [TestCase("Series Title - S01E11 2020 1080p Viva MKV WEB", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][1080p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("[LostYears] Series Title - 01-17 (WEB 1080p x264 10-bit AAC) [Dual-Audio]", false)]
        [TestCase("Series.and.Titles.S01.1080p.NF.WEB.DD2.0.x264-SNEAkY", false)]
        public void should_parse_webdl1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL1080p, proper);
        }

        [TestCase("Series.Title.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", false)]
        [TestCase("Series.Title.S07E20.1080p.AMZN.WEBRip.DDP5.1.x264-ViSUM ac3.(NLsub)", false)]
        [TestCase("Series.Title.S03E09.1080p.NF.WEBRip.DD5.1.x264-ViSUM", false)]
        [TestCase("The Series 42 S09E13 1.54 GB WEB-RIP 1080p Dual-Audio 2019 MKV", false)]
        [TestCase("Series.Title.1x04.ITA.1080p.WEBMux.x264-NovaRip", false)]
        [TestCase("Series.Title.2019.S02E07.Chapter.15.The.Believer.4Kto1080p.DSNYP.Webrip.x265.10bit.EAC3.5.1.Atmos.GokiTAoE", false)]
        public void should_parse_webrip1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip1080p, proper);
        }

        [TestCase("Series.Title.2016.03.14.2160p.WEB.x264-spamTV", false)]
        [TestCase("Series.Title.2016.03.14.2160p.WEB.h264-spamTV", false)]
        [TestCase("Series.Title.2016.03.14.2160p.WEB.PROPER.h264-spamTV", true)]
        [TestCase("House.of.Sonarr.AK.s05e13.4K.UHD.WEB.DL", false)]
        [TestCase("House.of.Sonarr.AK.s05e13.UHD.4K.WEB.DL", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][2160p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("Series Title S02 2013 WEB-DL 4k H265 AAC 2Audio-HDSWEB", false)]
        public void should_parse_webdl2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL2160p, proper);
        }

        [TestCase("Series S01E01.2160P AMZN WEBRIP DD2.0 HI10P X264-TROLLUHD", false)]
        [TestCase("JUST ADD SONARR S01E01.2160P AMZN WEBRIP DD2.0 X264-TROLLUHD", false)]
        [TestCase("The.Man.In.The.Series.S01E01.2160p.AMZN.WEBRip.DD2.0.Hi10p.X264-TrollUHD", false)]
        [TestCase("The Man In the Series S01E01 2160p AMZN WEBRip DD2.0 Hi10P x264-TrollUHD", false)]
        [TestCase("House.of.Sonarr.AK.S05E08.Chapter.60.2160p.NF.WEBRip.DD5.1.x264-NTb.NLsubs", false)]
        [TestCase("Sonarr Saves the World S01 2160p Netflix WEBRip DD5.1 x264-TrollUHD", false)]
        public void should_parse_webrip2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip2160p, proper);
        }

        [TestCase("SERIES.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Series - S01E03 - Come Fly With Me - 720p BluRay.mkv", false)]
        [TestCase("The Big Series.S03E01.The Sonarr Can Opener.m2ts", false)]
        [TestCase("Series.S01E02.Chained.Sonarr.[Bluray720p].mkv", false)]
        [TestCase("[FFF] DATE A Sonarr Dev - 01 [BD][720p-AAC][0601BED4]", false)]
        [TestCase("[coldhell] Series v3 [BD720p][03192D4C]", false)]
        [TestCase("[RandomRemux] Series - 01 [720p BD][043EA407].mkv", false)]
        [TestCase("[Kaylith] Series Friends Specials - 01 [BD 720p AAC][B7EEE164].mkv", false)]
        [TestCase("SERIES.S03E01-06.DUAL.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("SERIES.S03E01-06.DUAL.720p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Elysium]Lucky.Series.01(BD.720p.AAC.DA)[0BB96AD8].mkv", false)]
        [TestCase("Series.Galaxy.S01E01.33.720p.HDDVD.x264-SiNNERS.mkv", false)]
        [TestCase("The.Series.S01E07.RERIP.720p.BluRay.x264-DEMAND", true)]
        [TestCase("Sans.Series.De.Traces.FRENCH.720p.BluRay.x264-FHD", false)]
        [TestCase("Series.Black.1x01.Selezione.Naturale.ITA.720p.BDMux.x264-NovaRip", false)]
        public void should_parse_bluray720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray720p, proper);
        }

        [TestCase("Series - S01E03 - Come Fly With Me - 1080p BluRay.mkv", false)]
        [TestCase("Sonarr.Of.Series.S02E13.1080p.BluRay.x264-AVCDVD", false)]
        [TestCase("Series.S01E02.Chained.Heat.[Bluray1080p].mkv", false)]
        [TestCase("[FFF] Series no Muromi-san - 10 [BD][1080p-FLAC][0C4091AF]", false)]
        [TestCase("[coldhell] Series v2 [BD1080p][5A45EABE].mkv", false)]
        [TestCase("[Kaylith] Series Friends Specials - 01 [BD 1080p FLAC][429FD8C7].mkv", false)]
        [TestCase("[Zurako] Log Series - 01 - The Sonarr (BD 1080p AAC) [7AE12174].mkv", false)]
        [TestCase("SERIES.S03E01-06.DUAL.1080p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Coalgirls]_Series!!_01_(1920x1080_Blu-ray_FLAC)_[8370CB8F].mkv", false)]
        [TestCase("Planet.Series.S01E11.Code.Deep.1080p.HD-DVD.DD.VC1-TRB", false)]
        [TestCase("Series Away(2001) Bluray FHD Hi10P.mkv", false)]
        [TestCase("S for Series 2005 1080p UHD BluRay DD+7.1 x264-LoRD.mkv", false)]
        [TestCase("Series.Title.2011.1080p.UHD.BluRay.DD5.1.HDR.x265-CtrlHD.mkv", false)]
        [TestCase("Fall.Of.The.Release.Groups.S02E13.1080p.BDLight.x265-AVCDVD", false)]
        public void should_parse_bluray1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray1080p, proper);
        }

        [TestCase("Series!!! on ICE - S01E12[JP BD Remux][ENG subs]", false)]
        [TestCase("Series.Title.S01E08.The.Well.BluRay.1080p.AVC.DTS-HD.MA.5.1.REMUX-FraMeSToR", false)]
        [TestCase("Series.Title.2x11.Nato.Per.La.Truffa.Bluray.Remux.AVC.1080p.AC3.ITA", false)]
        [TestCase("Series.Title.2x11.Nato.Per.La.Truffa.Bluray.Remux.AVC.AC3.ITA", false)]
        [TestCase("Series.Title.S03E01.The.Calm.1080p.DTS-HD.MA.5.1.AVC.REMUX-FraMeSToR", false)]
        public void should_parse_bluray1080p_remux_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray1080pRemux, proper);
        }

        [TestCase("Series.Title.US.s05e13.4K.UHD.Bluray", false)]
        [TestCase("Series.Title.US.s05e13.UHD.4K.Bluray", false)]
        [TestCase("[DameDesuYo] Series Bundle - Part 1 (BD 4K 8bit FLAC)", false)]
        [TestCase("Series.Title.2014.2160p.UHD.BluRay.X265-IAMABLE.mkv", false)]
        [TestCase("Series.Title.2014.2160p.UHD.BluRay.X265-IAMABLE.mkv", false)]
        [TestCase("Series.Title.S05EO1.Episode.Title.2160p.BDRip.AAC.7.1.HDR10.x265.10bit-Markll", false)]
        public void should_parse_bluray2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray2160p, proper);
        }

        [TestCase("Series!!! on ICE - S01E12[JP BD 2160p Remux][ENG subs]", false)]
        [TestCase("Series.Title.S01E08.The.Sonarr.BluRay.2160p.AVC.DTS-HD.MA.5.1.REMUX-FraMeSToR", false)]
        [TestCase("Series.Title.2x11.Nato.Per.The.Sonarr.Bluray.Remux.AVC.2160p.AC3.ITA", false)]
        [TestCase("[Dolby Vision] Sonarr.of.Series.S07.MULTi.UHD.BLURAY.REMUX.DV-NoTag", false)]
        public void should_parse_bluray2160p_remux_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray2160pRemux, proper);
        }

        [TestCase("POI S02E11 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("How I Met Your Developer S01E18 Nothing Good Happens After Sonarr 720p HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("The Series S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("Series.Title.S07E11.1080i.HDTV.DD5.1.MPEG2-NTb.ts", false)]
        [TestCase("Game of Series S04E10 1080i HDTV MPEG2 DD5.1-CtrlHD.ts", false)]
        [TestCase("Series.Title.S02E05.1080i.HDTV.DD2.0.MPEG2-NTb.ts", false)]
        [TestCase("Show - S03E01 - Episode Title Raw-HD.ts", false)]
        [TestCase("Series.Title.S10E09.Title.1080i.UPSCALE.HDTV.DD5.1.MPEG2-zebra", false)]
        [TestCase("Series.Title.2011-08-04.1080i.HDTV.MPEG-2-CtrlHD", false)]
        public void should_parse_raw_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.RAWHD, proper);
        }

        [TestCase("The.Series.S02E15", false)]
        [TestCase("Series.Title - 11x11 - Title", false)]
        [TestCase("Series.Title.S01E01.webm", false)]
        [TestCase("Series.Title.S01E01.The.Web.MT-dd", false)]
        public void quality_parse(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Unknown, proper);
        }

        [Test]
        [TestCaseSource(nameof(SelfQualityParserCases))]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = string.Format("My series S01E01 [{0}]", quality.Name);
            var result = QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }

        [Test]
        [TestCaseSource(nameof(OtherSourceQualityParserCases))]
        public void should_parse_quality_from_other_source(string qualityString, Quality quality)
        {
            foreach (var c in new char[] { '-', '.', ' ', '_' })
            {
                var title = string.Format("My series S01E01 {0}", qualityString.Replace(' ', c));

                ParseAndVerifyQuality(title, quality, false);
            }
        }

        [TestCase("Series - S01E01 - Title [HDTV-1080p]")]
        [TestCase("Series.Title.S10E09.Episode.Title.1080i.UPSCALE.HDTV.DD5.1.MPEG2-zebra")]
        [TestCase("Series.Title.S01E01.Bluray720p")]
        [TestCase("Series.Title.S01E01.Bluray1080p")]
        [TestCase("Series.Title.S01E01.Bluray2160p")]
        [TestCase("Series.Title.S01E01.848x480.dvd")]
        [TestCase("Series.Title.S01E01.848x480.Bluray")]
        [TestCase("Series.Title.S01E01.1280x720.Bluray")]
        [TestCase("Series.Title.S01E01.1920x1080.Bluray")]
        public void should_parse_full_quality_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("[CR] Series Title - 004 [480p][48CE2D0F]")]
        [TestCase("Series.Title.S01E01.848x480")]
        [TestCase("Series.Title.S01E01.1280x720")]
        [TestCase("Series.Title.S01E01.1920x1080")]
        public void should_parse_resolution_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Unknown);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Series.Title.2011.S02E01.WS.PDTV.x264-REPACK-TLA")]
        [TestCase("Series.Title.S01E01.Bluray")]
        [TestCase("Series.Title.S01E01.HD.TV")]
        [TestCase("Series.Title.S01E01.SD.TV")]
        public void should_parse_source_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Unknown);
        }

        [TestCase("Series.Title.S01E02.Chained.Heat.mkv")]
        [TestCase("Series - S01E01 - Title.avi")]
        [TestCase("Series.Title.9x18.sunshine_days.avi")]
        [TestCase("[CR] Series Title - 004 [48CE2D0F].avi")]
        public void should_parse_quality_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [TestCase("Series.Title.S01E02.Chained.Heat.1080p.mkv")]
        [TestCase("Series - S01E01 - Title.720p.avi")]
        public void should_parse_resolution_from_name_and_source_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Series Title S04E87 REPACK 720p HDTV x264 aAF", true)]
        [TestCase("Series.Title.S04E87.REPACK.720p.HDTV.x264-aAF", true)]
        [TestCase("Series.Title.S04E87.PROPER.720p.HDTV.x264-aAF", false)]
        [TestCase("Series.Title.S01E07.RERIP.720p.BluRay.x264-DEMAND", true)]
        public void should_be_able_to_parse_repack(string title, bool isRepack)
        {
            var result = QualityParser.ParseQuality(title);
            result.Revision.Version.Should().Be(2);
            result.Revision.IsRepack.Should().Be(isRepack);
        }

        private void ParseAndVerifyQuality(string title, Quality quality, bool proper)
        {
            var result = QualityParser.ParseQuality(title);
            result.Quality.Should().Be(quality);

            var version = proper ? 2 : 1;
            result.Revision.Version.Should().Be(version);
        }
    }
}
