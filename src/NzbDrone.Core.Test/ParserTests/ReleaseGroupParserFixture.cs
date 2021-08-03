using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Series.2009.S01E14.English.HDTV.XviD-LOL", "LOL")]
        [TestCase("Series 2009 S01E14 English HDTV XviD LOL", null)]
        [TestCase("Series Now S05 EXTRAS DVDRip XviD RUNNER", null)]
        [TestCase("Series.Title.S01.EXTRAS.DVDRip.XviD-RUNNER", "RUNNER")]
        [TestCase("2020.Series.2011.12.02.PDTV.XviD-C4TV", "C4TV")]
        [TestCase("The.Series.S03E115.DVDRip.XviD-OSiTV", "OSiTV")]
        [TestCase("Series Title - S01E01 - Pilot [HTDV-480p]", null)]
        [TestCase("Series Title - S01E01 - Pilot [HTDV-720p]", null)]
        [TestCase("Series Title - S01E01 - Pilot [HTDV-1080p]", null)]
        [TestCase("The.Series.Title.S04E13.720p.WEB-DL.AAC2.0.H.264-Cyphanix", "Cyphanix")]
        [TestCase("Series.S02E01.720p.WEB-DL.DD5.1.H.264.mkv", null)]
        [TestCase("Series Title S01E01 Episode Title", null)]
        [TestCase("The Series Title - 2014-06-02 - Thomas Piketty.mkv", null)]
        [TestCase("The Series Title S12E17 May 23, 2014.mp4", null)]
        [TestCase("Reizen Waes - S01E08 - Transistri\u00EB, Zuid-Osseti\u00EB en Abchazi\u00EB SDTV.avi", null)]
        [TestCase("The Series Title 10x11 - Wild Devs Cant Be Broken [rl].avi", "rl")]
        [TestCase("[ www.Torrenting.com ] - Series.S03E14.720p.HDTV.X264-DIMENSION", "DIMENSION")]
        [TestCase("Series S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "2HD")]
        [TestCase("7s-Series-s02e01-720p.mkv", null)]
        [TestCase("The.Series.S09E13.720p.HEVC.x265-MeGusta-Pre", "MeGusta")]
        [TestCase("Series Title - S01E01 - Episode Title [RlsGroup]", "RlsGroup")]
        [TestCase("Red Series S01 E01-E02 1080p AMZN WEBRip DDP5.1 x264 monkee", null)]
        [TestCase("Series.Title.S01E05.The-Aniversary.WEBDL-1080p.mkv", null)]
        [TestCase("Series.Title.S01E05.The-Aniversary.HDTV-1080p.mkv", null)]
        [TestCase("Series US (2010) S04 (1080p BDRip x265 10bit DTS-HD MA 5 1 - WEM)[TAoE]", null)]
        [TestCase("The.Series.S03E04.2160p.Amazon.WEBRip.DTS-HD.MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-X.MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-MA.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.BluRay.DTS-ES.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.Blu-Ray.DTS-ES.5.1.x264", null)]
        [TestCase("SomeShow.S20E13.1080p.Blu-Ray.DTS-ES.5.1.x264-ROUGH [PublicHD]", "ROUGH")]
        [TestCase("SomeShow S01E168 1080p WEB-DL AAC 2.0 x264-Erai-raws", "Erai-raws")]
        [TestCase("The.Good.Series.S05E03.Series.of.Intelligence.1080p.10bit.AMZN.WEB-DL.DDP5.1.HEVC-Vyndros", "Vyndros")]
        [TestCase("[Tenrai-Sensei] Series [BD][1080p][HEVC 10bit x265][Dual Audio]", "Tenrai-Sensei")]
        [TestCase("[Erai-raws] Series - 0955 ~ 1005 [1080p]", "Erai-raws")]
        [TestCase("[Exiled-Destiny] Series Title", "Exiled-Destiny")]

        //[TestCase("", "")]
        public void should_parse_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Show.Name.2009.S01.1080p.BluRay.DTS5.1.x264-D-Z0N3", "D-Z0N3")]
        [TestCase("Show.Name.S01E01.1080p.WEB-DL.H264.Fight-BB.mkv", "Fight-BB")]
        [TestCase("Show Name (2021) Season 1 S01 (1080p BluRay x265 HEVC 10bit AAC 5.1 Tigole) [QxR]", "Tigole")]
        [TestCase("Show Name (2021) Season 1 S01 (1080p BluRay x265 HEVC 10bit AAC 2.0 afm72) [QxR]", "afm72")]
        [TestCase("Show Name (2021) Season 1 S01 (1080p DSNP WEB-DL x265 HEVC 10bit EAC3 5.1 Silence) [QxR]", "Silence")]
        [TestCase("Show Name (2021) Season 1 S01 (1080p BluRay x265 HEVC 10bit AAC 2.0 Panda) [QxR]", "Panda")]
        [TestCase("Show Name (2020) Season 1 S01 (1080p AMZN WEB-DL x265 HEVC 10bit EAC3 2.0 Ghost) [QxR]", "Ghost")]
        [TestCase("Show Name (2020) Season 1 S01 (1080p WEB-DL x265 HEVC 10bit AC3 5.1 MONOLITH) [QxR]", "MONOLITH")]
        [TestCase("The Show S08E09 The Series.1080p.AMZN.WEB-DL.x265.10bit.EAC3.6.0-Qman[UTR]", "UTR")]
        [TestCase("The Show S03E07 Fire and Series[1080p x265 10bit S87 Joy]", "Joy")]
        [TestCase("The Show (2016) - S02E01 - Soul Series #1 (1080p NF WEBRip x265 ImE)", "ImE")]
        [TestCase("The Show (2020) - S02E03 - Fighting His Series(1080p ATVP WEB-DL x265 t3nzin)", "t3nzin")]
        [TestCase("[Anime Time] A Show [BD][Dual Audio][1080p][HEVC 10bit x265][AAC][Eng Sub] [Batch] Title)", "Anime Time")]
        [TestCase("[Project Angel] Anime Series [DVD 480p] [10-bit x265 HEVC | Opus]", "Project Angel")]
        [TestCase("[Hakata Ramen] Show Title - Season 2 - Revival of The Commandments", "Hakata Ramen")]
        public void should_parse_exception_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [Test]
        public void should_not_include_extension_in_release_group()
        {
            const string path = @"C:\Test\Doctor.Series.2005.s01e01.internal.bdrip.x264-archivist.mkv";

            Parser.Parser.ParsePath(path).ReleaseGroup.Should().Be("archivist");
        }

        [TestCase("Series.Title.S02E04.720p.WEBRip.x264-SKGTV English", "SKGTV")]
        [TestCase("Series.Title.S02E04.720p.WEBRip.x264-SKGTV_English", "SKGTV")]
        [TestCase("Series.Title.S02E04.720p.WEBRip.x264-SKGTV.English", "SKGTV")]

        //[TestCase("", "")]
        public void should_not_include_language_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Series.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP", "EVL")]
        [TestCase("Series.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-RP-RP", "EVL")]
        [TestCase("Series.Title.S02E04.720p.WEB-DL.AAC2.0.H.264-EVL-Obfuscated", "EVL")]
        [TestCase("Series.Title.S04E04.720p.BluRay.x264-xHD-NZBgeek", "xHD")]
        [TestCase("Series.Title.S05E11.720p.HDTV.X264-DIMENSION-NZBgeek", "DIMENSION")]
        [TestCase("Series.Title.S04E04.720p.BluRay.x264-xHD-1", "xHD")]
        [TestCase("Series.Title.S05E11.720p.HDTV.X264-DIMENSION-1", "DIMENSION")]
        [TestCase("series.title.s40e11.kevin.hart_sia.720p.hdtv.x264-w4f-sample.mkv", "w4f")]
        [TestCase("The.Series.2017.S05E02.1080p.WEB-DL.DD5.1.H264-EVL-Scrambled", "EVL")]
        [TestCase("Series.S01E08.Haunted.Hayride.720p.AMZN.WEBRip.DDP5.1.x264-NTb-postbot", "NTb")]
        [TestCase("Series.S01E08.Haunted.Hayride.720p.AMZN.WEBRip.DDP5.1.x264-NTb-xpost", "NTb")]
        [TestCase("Series.Title.S08E05.The.Forgotten.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Rakuv", "NTb")]
        [TestCase("The.Series.S30E01.Devs.Not.Dead.1080p.AMZN.WEB-DL.DDP5.1.H264-QOQ-Rakuv02", "QOQ")]
        [TestCase("Lie.To.Developers.S01E13.720p.BluRay.x264-SiNNERS-Rakuvfinhel", "SiNNERS")]
        [TestCase("Who.is.Sonarr.S01E01.INTERNAL.720p.HDTV.x264-aAF-RakuvUS-Obfuscated", "aAF")]
        [TestCase("Deadly.Development.S01E10.Sink.With.Code.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTG-WhiteRev", "NTG")]
        [TestCase("The.Sonarr.Series.S09E12.Developers.REPACK.1080p.AMZN.WEB-DL.DD.5.1.H.264-CasStudio-BUYMORE", "CasStudio")]
        [TestCase("2.Tired.Developers.S02E24.1080p.AMZN.WEBRip.DD5.1.x264-CasStudio-AsRequested", "CasStudio")]
        [TestCase("Series.S04E11.Lines.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-AlternativeToRequested", "NTb")]
        [TestCase("Series.S16E04.Third.Wheel.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-GEROV", "NTb")]
        [TestCase("Series.and.Title.S10E06.Dev.n.Play.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Z0iDS3N", "NTb")]
        [TestCase("Absolute.Series.S02E06.The.House.of.Sonarr.DVDRip.x264-MaG-Chamele0n", "MaG")]
        [TestCase("The.Series.Title.S08E08.1080p.BluRay.x264-ROVERS-4P", "ROVERS")]
        [TestCase("Series.Title.S01E02.720p.BluRay.X264-REWARD-4Planet", "REWARD")]
        [TestCase("Series.S01E01.Rites.of.Passage.1080p.BluRay.x264-DON-AlteZachen", "DON")]
        [TestCase("Series.Title.S04E06.Episode.Name.720p.WEB-DL.DD5.1.H.264-HarrHD-RePACKPOST", "HarrHD")]
        public void should_not_include_repost_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("[FFF] Series Title!! - S01E11 - Someday, With Sonarr", "FFF")]
        [TestCase("[HorribleSubs] Series Title!! - S01E12 - Sonarr Going Well!!", "HorribleSubs")]
        [TestCase("[Anime-Koi] Series Title - S01E06 - Guys From Sonarr", "Anime-Koi")]
        [TestCase("[Anime-Koi] Series Title - S01E07 - A High-Grade Sonarr", "Anime-Koi")]
        [TestCase("[Anime-Koi] Series Title 2 - 01 [h264-720p][28D54E2C]", "Anime-Koi")]

        //[TestCase("Tokyo.Ghoul.02x01.013.HDTV-720p-Anime-Koi", "Anime-Koi")]
        //[TestCase("", "")]
        public void should_parse_anime_release_groups(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Terrible.Anime.Title.001.DBOX.480p.x264-iKaos [v3] [6AFFEF6B]")]
        public void should_not_parse_anime_hash_as_release_group(string title)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().BeNull();
        }
    }
}
