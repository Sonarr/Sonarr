using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SceneCheckerFixture
    {
        [TestCase("Series.Title.S04E13.Helen.Keller.The.Musical.720p.WEBRip.AAC2.0.H.264-GC")]
        [TestCase("Series.Title.S07E02.720p.WEB-DL.DD5.1.H.264-pcsyndicate")]
        [TestCase("Series.Title.2009.S05E06.Baby.Shower.720p.WEB-DL.DD5.1.H.264-iT00NZ")]
        [TestCase("Series.Title.S04E17.720p.HDTV.X264-DIMENSION")]
        [TestCase("Series.Title.S04.720p.HDTV.X264-DIMENSION")]
        public void should_return_true_for_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeTrue();
        }

        [TestCase("S08E05 - Virtual In-Stanity [WEBDL-720p]")]
        [TestCase("S08E05 - Virtual In-Stanity.With.Dots [WEBDL-720p]")]
        [TestCase("Something")]
        [TestCase("86de66b7ef385e2fa56a3e41b98481ea1658bfab")]
        [TestCase("Series.Title.S04E17.720p.HDTV.X264", Description = "no group")]
        [TestCase("S04E17.720p.HDTV.X264-DIMENSION", Description = "no series title")]
        [TestCase("Series.Title.S04E17-DIMENSION", Description = "no quality")]
        [TestCase("Series.Title.720p.HDTV.X264-DIMENSION", Description = "no episode")]
        public void should_return_false_for_non_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeFalse();
        }
    }
}
