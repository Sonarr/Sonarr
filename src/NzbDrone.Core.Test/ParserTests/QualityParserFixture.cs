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
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [480p]", false)]
        [TestCase("[CR] Sailor Moon - 004 [480p][48CE2D0F]", false)]
        [TestCase("[Hatsuyuki] Naruto Shippuuden - 363 [848x480][ADE35E38]", false)]
        [TestCase("Muppet.Babies.S03.TVRip.XviD-NOGRP", false)]
        [TestCase("[HorribleSubs] ID INVADED - 03 [360p].mkv", false)]
        public void should_parse_sdtv_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.SDTV, proper);
        }

        [TestCase("The.Shield.S01E13.NTSC.x264-CtrlSD", false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", false)]
        [TestCase("The.Girls.Next.Door.S03E06.DVD.Rip.XviD-WiDE", false)]
        [TestCase("the.shield.1x13.circles.ws.xvidvd-tns", false)]
        [TestCase("the_x-files.9x18.sunshine_days.ac3.ws_dvdrip_xvid-fov.avi", false)]
        [TestCase("[FroZen] Miyuki - 23 [DVD][7F6170E6]", false)]
        [TestCase("[AniDL] Vandread - 26 -[360p][DVD][D - A][Exiled - Destiny]", false)]
        public void should_parse_dvd_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.DVD, proper);
        }

        [TestCase("Elementary.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Glee.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", false)]
        [TestCase("The.Big.Bang.Theory.S06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Da.Vincis.Demons.S02E04.480p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("Incorporated.S01E08.Das.geloeschte.Ich.German.Dubbed.DL.AmazonHD.x264-TVS", false)]
        [TestCase("Haters.Back.Off.S01E04.Rod.Trip.mit.meinem.Onkel.German.DL.NetflixUHD.x264", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][480p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        public void should_parse_webdl480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL480p, proper);
        }

        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3-REPACK.-HELLYWOOD.avi", true)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.X-viD.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.XviD.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Hannibal.S01E05.576p.BluRay.DD5.1.x264-HiSD", false)]
        [TestCase("Hannibal.S01E05.480p.BluRay.DD5.1.x264-HiSD", false)]
        [TestCase("Heidi Girl of the Alps (BD)(640x480(RAW) (BATCH 1) (1-13)", false)]
        [TestCase("[Doki] Clannad - 02 (848x480 XviD BD MP3) [95360783]", false)]
        public void should_parse_bluray480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray480p, proper);
        }

        [TestCase("Clarissa.Explains.It.All.S02E10.480p.HULU.WEBRip.x264-Puffin", false)]
        [TestCase("Duck.Dynasty.S10E14.Techs.And.Balances.480p.AE.WEBRip.AAC2.0.x264-SEA", false)]
        [TestCase("Series.Title.1x04.ITA.WEBMux.x264-NovaRip", false)]
        public void should_parse_webrip480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip480p, proper);
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
        [TestCase("[Underwater-FFF] No Game No Life - 01 (720p) [27AAA0A0]", false)]
        [TestCase("[Doki] Mahouka Koukou no Rettousei - 07 (1280x720 Hi10P AAC) [80AF7DDE]", false)]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", false)]
        [TestCase("[HorribleSubs]_Fairy_Tail_-_145_[720p]", false)]
        [TestCase("[Eveyuu] No Game No Life - 10 [Hi10P 1280x720 H264][10B23BD8]", false)]
        [TestCase("Hells.Kitchen.US.S12E17.HR.WS.PDTV.X264-DIMENSION", false)]
        [TestCase("Survivorman.The.Lost.Pilots.Summer.HR.WS.PDTV.x264-DHD", false)]
        [TestCase("Victoria S01E07 - Motor zmen (CZ)[TvRip][HEVC][720p]", false)]
        [TestCase("flashpoint.S05E06.720p.HDTV.x264-FHD", false)]
        [TestCase("Series.Title.1x01.ITA.720p.x264-RlsGrp [01/54] - \"series.title.1x01.ita.720p.x264-rlsgrp.nfo\"", false)]
        public void should_parse_hdtv720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.HDTV720p, proper);
        }

        [TestCase("Under the Dome S01E10 Let the Games Begin 1080p", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", false)]
        [TestCase("DEXTER.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", true)]
        [TestCase("Dexter - S01E01 - Title [HDTV-1080p]", false)]
        [TestCase("[HorribleSubs] Yowamushi Pedal - 32 [1080p]", false)]
        [TestCase("Victoria S01E07 - Motor zmen (CZ)[TvRip][HEVC][1080p]", false)]
        [TestCase("Sword Art Online Alicization 04 vostfr FHD", false)]
        [TestCase("Goblin Slayer 04 vostfr FHD.mkv", false)]
        [TestCase("[Onii-ChanSub] SSSS.Gridman - 02 vostfr (FHD 1080p 10bits).mkv", false)]
        [TestCase("[Miaou] Akanesasu Shoujo 02 VOSTFR FHD 10 bits", false)]
        [TestCase("[mhastream.com]_Episode_05_FHD.mp4", false)]
        [TestCase("[Kousei]_One_Piece_ - _609_[FHD][648A87C7].mp4", false)]
        [TestCase("Presunto culpable 1x02 Culpabilidad [HDTV 1080i AVC MP2 2.0 Sub][GrupoHDS]", false)]
        [TestCase("Cuéntame cómo pasó - 19x15 [344] Cuarenta años de baile [HDTV 1080i AVC MP2 2.0 Sub][GrupoHDS]", false)]
        [TestCase("Super.Robot.Monkey.Team.Hyperforce.Go.S01E02.Depths.of.Fear.1080i.HDTV.DD5.1.H.264-NOGRP", false)]
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

        [TestCase("Vanguard S01E04 Mexicos Death Train 720p WEB DL", false)]
        [TestCase("Hawaii Five 0 S02E21 720p WEB DL DD5 1 H 264", false)]
        [TestCase("Castle S04E22 720p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Chuck - S11E06 - D-Yikes! - 720p WEB-DL.mkv", false)]
        [TestCase("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", false)]
        [TestCase("S07E23 - [WEBDL].mkv ", false)]
        [TestCase("Fringe S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", false)]
        [TestCase("House.S04.720p.Web-Dl.Dd5.1.h264-P2PACK", false)]
        [TestCase("Da.Vincis.Demons.S02E04.720p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("CSI.Miami.S04E25.720p.iTunesHD.AVC-TVS", false)]
        [TestCase("Castle.S06E23.720p.WebHD.h264-euHD", false)]
        [TestCase("The.Nightly.Show.2016.03.14.720p.WEB.x264-spamTV", false)]
        [TestCase("The.Nightly.Show.2016.03.14.720p.WEB.h264-spamTV", false)]
        [TestCase("Incorporated.S01E08.Das.geloeschte.Ich.German.DD51.Dubbed.DL.720p.AmazonHD.x264-TVS", false)]
        [TestCase("Marco.Polo.S01E11.One.Hundred.Eyes.2015.German.DD51.DL.720p.NetflixUHD.x264.NewUp.by.Wunschtante", false)]
        [TestCase("Hush 2016 German DD51 DL 720p NetflixHD x264-TVS", false)]
        [TestCase("Community.6x10.Basic.RV.Repair.and.Palmistry.ITA.ENG.720p.WEB-DLMux.H.264-GiuseppeTnT", false)]
        [TestCase("Community.6x11.Modern.Espionage.ITA.ENG.720p.WEB.DLMux.H.264-GiuseppeTnT", false)]
        [TestCase("The Walking Dead 2010 S09E13 [MKV / H.264 / AC3/AAC / WEB / Dual Áudio / Inglês / 720p]", false)]
        [TestCase("into.the.badlands.s03e16.h264.720p-web-handbrake.mkv", false)]
        [TestCase("BrainDead.S01E01.The.Insanity.Principle.720p.WEB-DL.DD5.1.H.264-BD", false)]
        [TestCase("Jerks.S03E05.Griebnitzsee.German.720p.MaxdomeHD.AVC-TVS", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][720p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        public void should_parse_webdl720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL720p, proper);
        }

        [TestCase("Arrested.Development.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", false)]
        [TestCase("American.Gods.S01E07.A.Prayer.For.Mad.Sweeney.720p.AMZN.WEBRip.DD5.1.x264-NTb", false)]
        [TestCase("LEGO.Star.Wars.The.Freemaker.Adventures.S07E01.A.New.Home.720p.DSNY.WEBRip.AAC2.0.x264-TVSmash", false)]
        [TestCase("Series.Title.1x04.ITA.720p.WEBMux.x264-NovaRip", false)]
        public void should_parse_webrip720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip720p, proper);
        }

        [TestCase("CSI NY S09E03 1080p WEB DL DD5 1 H264 NFHD", false)]
        [TestCase("Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Criminal.Minds.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", false)]
        [TestCase("Its.Always.Sunny.in.Philadelphia.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", true)]
        [TestCase("Two and a Half Men S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", true)]
        [TestCase("Glee.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", false)]
        [TestCase("The.Big.Bang.Theory.S06E11.The.Santa.Simulation.1080p.WEB-DL.DD5.1.H.264", false)]
        [TestCase("Rosemary's.Baby.S01E02.Night.2.[WEBDL-1080p].mkv", false)]
        [TestCase("The.Nightly.Show.2016.03.14.1080p.WEB.x264-spamTV", false)]
        [TestCase("The.Nightly.Show.2016.03.14.1080p.WEB.h264-spamTV", false)]
        [TestCase("Psych.S01.1080p.WEB-DL.AAC2.0.AVC-TrollHD", false)]
        [TestCase("Series Title S06E08 1080p WEB h264-EXCLUSIVE", false)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB DD5 1 H 264-EXCLUSIVE", true)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB H 264-EXCLUSIVE", true)]
        [TestCase("The.Simpsons.S25E21.Pay.Pal.1080p.WEB-DL.DD5.1.H.264-NTb", false)]
        [TestCase("Incorporated.S01E08.Das.geloeschte.Ich.German.DD51.Dubbed.DL.1080p.AmazonHD.x264-TVS", false)]
        [TestCase("Death.Note.2017.German.DD51.DL.1080p.NetflixHD.x264-TVS", false)]
        [TestCase("Played.S01E08.Pro.Gamer.1440p.BKPL.WEB-DL.H.264-LiGHT", false)]
        [TestCase("Good.Luck.Charlie.S04E11.Teddy's.Choice.FHD.1080p.Web-DL", false)]
        [TestCase("Outlander.S04E03.The.False.Bride.1080p.NF.WEB.DDP5.1.x264-NTb[rartv]", false)]
        [TestCase("Legacies.S02E02.This.Year.Will.Be.Different.1080p.AMZN.WEB...", false)]
        [TestCase("Legacies.S02E02.This.Year.Will.Be.Different.1080p.AMZN.WEB.", false)]
        [TestCase("Series Title - S01E11 2020 1080p Viva MKV WEB", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][1080p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        public void should_parse_webdl1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL1080p, proper);
        }

        [TestCase("Arrested.Development.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", false)]
        [TestCase("Blue.Bloods.S07E20.1080p.AMZN.WEBRip.DDP5.1.x264-ViSUM ac3.(NLsub)", false)]
        [TestCase("Better.Call.Saul.S03E09.1080p.NF.WEBRip.DD5.1.x264-ViSUM", false)]
        [TestCase("The Walking Dead S09E13 1.54 GB WEB-RIP 1080p Dual-Audio 2019 MKV", false)]
        [TestCase("Series.Title.1x04.ITA.1080p.WEBMux.x264-NovaRip", false)]
        [TestCase("Series.Title.2019.S02E07.Chapter.15.The.Believer.4Kto1080p.DSNYP.Webrip.x265.10bit.EAC3.5.1.Atmos.GokiTAoE", false)]
        public void should_parse_webrip1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip1080p, proper);
        }

        [TestCase("The.Nightly.Show.2016.03.14.2160p.WEB.x264-spamTV", false)]
        [TestCase("The.Nightly.Show.2016.03.14.2160p.WEB.h264-spamTV", false)]
        [TestCase("The.Nightly.Show.2016.03.14.2160p.WEB.PROPER.h264-spamTV", true)]
        [TestCase("House.of.Cards.US.s05e13.4K.UHD.WEB.DL", false)]
        [TestCase("House.of.Cards.US.s05e13.UHD.4K.WEB.DL", false)]
        [TestCase("[HorribleSubs] Series Title! S01 [Web][MKV][h264][2160p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        public void should_parse_webdl2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBDL2160p, proper);
        }

        [TestCase("CASANOVA S01E01.2160P AMZN WEBRIP DD2.0 HI10P X264-TROLLUHD", false)]
        [TestCase("JUST ADD MAGIC S01E01.2160P AMZN WEBRIP DD2.0 X264-TROLLUHD", false)]
        [TestCase("The.Man.In.The.High.Castle.S01E01.2160p.AMZN.WEBRip.DD2.0.Hi10p.X264-TrollUHD", false)]
        [TestCase("The Man In the High Castle S01E01 2160p AMZN WEBRip DD2.0 Hi10P x264-TrollUHD", false)]
        [TestCase("House.of.Cards.US.S05E08.Chapter.60.2160p.NF.WEBRip.DD5.1.x264-NTb.NLsubs", false)]
        [TestCase("Bill Nye Saves the World S01 2160p Netflix WEBRip DD5.1 x264-TrollUHD", false)]
        public void should_parse_webrip2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.WEBRip2160p, proper);
        }

        [TestCase("WEEDS.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Chuck - S01E03 - Come Fly With Me - 720p BluRay.mkv", false)]
        [TestCase("The Big Bang Theory.S03E01.The Electric Can Opener Fluctuation.m2ts", false)]
        [TestCase("Revolution.S01E02.Chained.Heat.[Bluray720p].mkv", false)]
        [TestCase("[FFF] DATE A LIVE - 01 [BD][720p-AAC][0601BED4]", false)]
        [TestCase("[coldhell] Pupa v3 [BD720p][03192D4C]", false)]
        [TestCase("[RandomRemux] Nobunagun - 01 [720p BD][043EA407].mkv", false)]
        [TestCase("[Kaylith] Isshuukan Friends Specials - 01 [BD 720p AAC][B7EEE164].mkv", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.720p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Elysium]Lucky.Star.01(BD.720p.AAC.DA)[0BB96AD8].mkv", false)]
        [TestCase("Battlestar.Galactica.S01E01.33.720p.HDDVD.x264-SiNNERS.mkv", false)]
        [TestCase("The.Expanse.S01E07.RERIP.720p.BluRay.x264-DEMAND", true)]
        [TestCase("Sans.Laisser.De.Traces.FRENCH.720p.BluRay.x264-FHD", false)]
        [TestCase("Orphan.Black.1x01.Selezione.Naturale.ITA.720p.BDMux.x264-NovaRip", false)]
        public void should_parse_bluray720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray720p, proper);
        }

        [TestCase("Chuck - S01E03 - Come Fly With Me - 1080p BluRay.mkv", false)]
        [TestCase("Sons.Of.Anarchy.S02E13.1080p.BluRay.x264-AVCDVD", false)]
        [TestCase("Revolution.S01E02.Chained.Heat.[Bluray1080p].mkv", false)]
        [TestCase("[FFF] Namiuchigiwa no Muromi-san - 10 [BD][1080p-FLAC][0C4091AF]", false)]
        [TestCase("[coldhell] Pupa v2 [BD1080p][5A45EABE].mkv", false)]
        [TestCase("[Kaylith] Isshuukan Friends Specials - 01 [BD 1080p FLAC][429FD8C7].mkv", false)]
        [TestCase("[Zurako] Log Horizon - 01 - The Apocalypse (BD 1080p AAC) [7AE12174].mkv", false)]
        [TestCase("WEEDS.S03E01-06.DUAL.1080p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Coalgirls]_Durarara!!_01_(1920x1080_Blu-ray_FLAC)_[8370CB8F].mkv", false)]
        [TestCase("Planet.Earth.S01E11.Ocean.Deep.1080p.HD-DVD.DD.VC1-TRB", false)]
        [TestCase("Spirited Away(2001) Bluray FHD Hi10P.mkv", false)]
        [TestCase("V for Vendetta 2005 1080p UHD BluRay DD+7.1 x264-LoRD.mkv", false)]
        [TestCase("Rise.Of.The.Planet.Of.The.Apes.2011.1080p.UHD.BluRay.DD5.1.HDR.x265-CtrlHD.mkv", false)]
        [TestCase("Rise.Of.The.Planet.Of.The.Apes.2011.UHD.BluRay.DD5.1.HDR.x265-CtrlHD/ctrlhd-rotpota-1080p.mkv", false)]
        public void should_parse_bluray1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray1080p, proper);
        }

        [TestCase("Yuri!!! on ICE - S01E12[JP BD Remux][ENG subs]", false)]
        [TestCase("Agents.of.S.H.I.E.L.D.S01E08.The.Well.BluRay.1080p.AVC.DTS-HD.MA.5.1.REMUX-FraMeSToR", false)]
        [TestCase("Miami.Vice.2x11.Nato.Per.La.Truffa.Bluray.Remux.AVC.1080p.AC3.ITA", false)]
        [TestCase("Miami.Vice.2x11.Nato.Per.La.Truffa.Bluray.Remux.AVC.AC3.ITA", false)]
        [TestCase("Arrow.S03E01.The.Calm.1080p.DTS-HD.MA.5.1.AVC.REMUX-FraMeSToR", false)]
        public void should_parse_bluray1080p_remux_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray1080pRemux, proper);
        }

        [TestCase("House.of.Cards.US.s05e13.4K.UHD.Bluray", false)]
        [TestCase("House.of.Cards.US.s05e13.UHD.4K.Bluray", false)]
        [TestCase("[DameDesuYo] Backlog Bundle - Part 1 (BD 4K 8bit FLAC)", false)]
        [TestCase("X-Men.Days.of.Future.Past.2014.2160p.UHD.BluRay.X265-IAMABLE.mkv", false)]
        public void should_parse_bluray2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray2160p, proper);
        }

        [TestCase("Yuri!!! on ICE - S01E12[JP BD 2160p Remux][ENG subs]", false)]
        [TestCase("Agents.of.S.H.I.E.L.D.S01E08.The.Well.BluRay.2160p.AVC.DTS-HD.MA.5.1.REMUX-FraMeSToR", false)]
        [TestCase("Miami.Vice.2x11.Nato.Per.La.Truffa.Bluray.Remux.AVC.2160p.AC3.ITA", false)]
        [TestCase("[Dolby Vision] Game.of.Thrones.S07.MULTi.UHD.BLURAY.REMUX.DV-NoTag", false)]
        public void should_parse_bluray2160p_remux_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Bluray2160pRemux, proper);
        }

        [TestCase("POI S02E11 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("How I Met Your Mother S01E18 Nothing Good Happens After 2 A.M. 720p HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("The Voice S01E11 The Finals 1080i HDTV DD5.1 MPEG2-TrollHD", false)]
        [TestCase("Californication.S07E11.1080i.HDTV.DD5.1.MPEG2-NTb.ts", false)]
        [TestCase("Game of Thrones S04E10 1080i HDTV MPEG2 DD5.1-CtrlHD.ts", false)]
        [TestCase("VICE.S02E05.1080i.HDTV.DD2.0.MPEG2-NTb.ts", false)]
        [TestCase("Show - S03E01 - Episode Title Raw-HD.ts", false)]
        [TestCase("Saturday.Night.Live.Vintage.S10E09.Eddie.Murphy.The.Honeydrippers.1080i.UPSCALE.HDTV.DD5.1.MPEG2-zebra", false)]
        [TestCase("The.Colbert.Report.2011-08-04.1080i.HDTV.MPEG-2-CtrlHD", false)]
        public void should_parse_raw_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.RAWHD, proper);
        }

        [TestCase("Sonny.With.a.Chance.S02E15", false)]
        [TestCase("Law & Order: Special Victims Unit - 11x11 - Quickie", false)]
        [TestCase("Series.Title.S01E01.webm", false)]
        [TestCase("Droned.S01E01.The.Web.MT-dd", false)]
        public void quality_parse(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Quality.Unknown, proper);
        }

        [Test, TestCaseSource(nameof(SelfQualityParserCases))]
        public void parsing_our_own_quality_enum_name(Quality quality)
        {
            var fileName = string.Format("My series S01E01 [{0}]", quality.Name);
            var result = QualityParser.ParseQuality(fileName);
            result.Quality.Should().Be(quality);
        }

        [Test, TestCaseSource(nameof(OtherSourceQualityParserCases))]
        public void should_parse_quality_from_other_source(string qualityString, Quality quality)
        {
            foreach (var c in new char[] { '-', '.', ' ', '_' })
            {
                var title = string.Format("My series S01E01 {0}", qualityString.Replace(' ', c));

                ParseAndVerifyQuality(title, quality, false);
            }
        }

        [TestCase("Dexter - S01E01 - Title [HDTV-1080p]")]
        [TestCase("Saturday.Night.Live.Vintage.S10E09.Eddie.Murphy.The.Honeydrippers.1080i.UPSCALE.HDTV.DD5.1.MPEG2-zebra")]
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

        [TestCase("[CR] Sailor Moon - 004 [480p][48CE2D0F]")]
        [TestCase("Series.Title.S01E01.848x480")]
        [TestCase("Series.Title.S01E01.1280x720")]
        [TestCase("Series.Title.S01E01.1920x1080")]
        public void should_parse_resolution_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Unknown);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("White.Van.Man.2011.S02E01.WS.PDTV.x264-REPACK-TLA")]
        [TestCase("Series.Title.S01E01.Bluray")]
        [TestCase("Series.Title.S01E01.HD.TV")]
        [TestCase("Series.Title.S01E01.SD.TV")]
        public void should_parse_source_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Unknown);
        }

        [TestCase("Revolution.S01E02.Chained.Heat.mkv")]
        [TestCase("Dexter - S01E01 - Title.avi")]
        [TestCase("the_x-files.9x18.sunshine_days.avi")]
        [TestCase("[CR] Sailor Moon - 004 [48CE2D0F].avi")]
        public void should_parse_quality_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [TestCase("Revolution.S01E02.Chained.Heat.1080p.mkv")]
        [TestCase("Dexter - S01E01 - Title.720p.avi")]
        public void should_parse_resolution_from_name_and_source_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Series Title S04E87 REPACK 720p HDTV x264 aAF", true)]
        [TestCase("Series.Title.S04E87.REPACK.720p.HDTV.x264-aAF", true)]
        [TestCase("Series.Title.S04E87.PROPER.720p.HDTV.x264-aAF", false)]
        [TestCase("The.Expanse.S01E07.RERIP.720p.BluRay.x264-DEMAND", true)]
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
