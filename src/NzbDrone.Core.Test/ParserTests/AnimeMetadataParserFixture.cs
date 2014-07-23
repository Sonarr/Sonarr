using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class AnimeMetadataParserFixture : CoreTest
    {
        [TestCase("[SubDESU]_High_School_DxD_07_(1280x720_x264-AAC)_[6B7FD717]", "SubDESU", "6B7FD717")]
        [TestCase("[Chihiro]_Working!!_-_06_[848x480_H.264_AAC][859EEAFA]", "Chihiro", "859EEAFA")]
        [TestCase("[Underwater]_Rinne_no_Lagrange_-_12_(720p)_[5C7BC4F9]", "Underwater", "5C7BC4F9")]
        [TestCase("[HorribleSubs]_Hunter_X_Hunter_-_33_[720p]", "HorribleSubs", "")]
        [TestCase("[HorribleSubs] Tonari no Kaibutsu-kun - 13 [1080p].mkv", "HorribleSubs", "")]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F].mkv", "Doremi", "C65D4B1F")]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].[C65D4B1F]", "Doremi", "C65D4B1F")]
        [TestCase("[Doremi].Yes.Pretty.Cure.5.Go.Go!.31.[1280x720].mkv", "Doremi", "")]
        [TestCase("[K-F] One Piece 214", "K-F", "")]
        [TestCase("[K-F] One Piece S10E14 214", "K-F", "")]
        [TestCase("[K-F] One Piece 10x14 214", "K-F", "")]
        [TestCase("[K-F] One Piece 214 10x14", "K-F", "")]
        [TestCase("Bleach - 031 - The Resolution to Kill [Lunar].avi", "Lunar", "")]
        [TestCase("[ACX]Hack Sign 01 Role Play [Kosaka] [9C57891E].mkv", "ACX", "9C57891E")]
        [TestCase("[S-T-D] Soul Eater Not! - 06 (1280x720 10bit AAC) [59B3F2EA].mkv", "S-T-D", "59B3F2EA")]
        public void should_parse_absolute_numbers(string postTitle, string subGroup, string hash)
        {
            var result = Parser.Parser.ParseTitle(postTitle);
            result.Should().NotBeNull();
            result.ReleaseGroup.Should().Be(subGroup);
            result.ReleaseHash.Should().Be(hash);
        }
    }
}
