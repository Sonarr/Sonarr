using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFixture : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] {0, Quality.Unknown},
                        new object[] {1, Quality.SDTV},
                        new object[] {2, Quality.DVD},
                        new object[] {3, Quality.WEBDL1080p},
                        new object[] {4, Quality.HDTV720p},
                        new object[] {5, Quality.WEBDL720p},
                        new object[] {6, Quality.Bluray720p},
                        new object[] {7, Quality.Bluray1080p},
                        new object[] {8, Quality.WEBDL480p},
                        new object[] {9, Quality.HDTV1080p},
                        new object[] {10, Quality.RAWHD},
                        new object[] {16, Quality.HDTV2160p},
                        new object[] {18, Quality.WEBDL2160p},
                        new object[] {19, Quality.Bluray2160p},
                };

        public static object[] ToIntCases =
                {
                        new object[] {Quality.Unknown, 0},
                        new object[] {Quality.SDTV, 1},
                        new object[] {Quality.DVD, 2},
                        new object[] {Quality.WEBDL1080p, 3},
                        new object[] {Quality.HDTV720p, 4},
                        new object[] {Quality.WEBDL720p, 5},
                        new object[] {Quality.Bluray720p, 6},
                        new object[] {Quality.Bluray1080p, 7},
                        new object[] {Quality.WEBDL480p, 8},
                        new object[] {Quality.HDTV1080p, 9},
                        new object[] {Quality.RAWHD, 10},
                        new object[] {Quality.HDTV2160p, 16},
                        new object[] {Quality.WEBDL2160p, 18},
                        new object[] {Quality.Bluray2160p, 19},
                };

        [Test, TestCaseSource(nameof(FromIntCases))]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, Quality expected)
        {
            var quality = (Quality)source;
            quality.Should().Be(expected);
        }

        [Test, TestCaseSource(nameof(ToIntCases))]
        public void should_be_able_to_convert_qualityTypes_to_int(Quality source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        public static List<ProfileQualityItem> GetDefaultQualities(params Quality[] allowed)
        {
            var qualities = new List<Quality>
            {
                Quality.Unknown,
                Quality.SDTV,
                Quality.WEBDL480p,
                Quality.DVD,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.HDTV2160p,
                Quality.RAWHD,
                Quality.WEBDL720p,
                Quality.WEBDL1080p,
                Quality.WEBDL2160p,
                Quality.Bluray720p,
                Quality.Bluray1080p,
                Quality.Bluray2160p,
            };

            if (allowed.Length == 0)
                allowed = qualities.ToArray();

            var items = qualities
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new ProfileQualityItem { Quality = v, Allowed = allowed.Contains(v) }).ToList();

            return items;
        }
    }
}
