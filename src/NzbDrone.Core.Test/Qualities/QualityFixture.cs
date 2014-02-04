using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFixture : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] {1, Quality.SDTV},
                        new object[] {2, Quality.DVD},
                        new object[] {4, Quality.HDTV720p},
                        new object[] {5, Quality.WEBDL720p},
                        new object[] {6, Quality.Bluray720p},
                        new object[] {7, Quality.Bluray1080p}
                };

        public static object[] ToIntCases =
                {
                        new object[] {Quality.SDTV, 1},
                        new object[] {Quality.DVD, 2},
                        new object[] {Quality.HDTV720p, 4},
                        new object[] {Quality.WEBDL720p, 5},
                        new object[] {Quality.Bluray720p, 6},
                        new object[] {Quality.Bluray1080p, 7}
                };

        [Test, TestCaseSource("FromIntCases")]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, Quality expected)
        {
            var quality = (Quality)source;
            quality.Should().Be(expected);
        }

        [Test, TestCaseSource("ToIntCases")]
        public void should_be_able_to_convert_qualityTypes_to_int(Quality source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        public static List<QualityProfileItem> GetDefaultQualities(params Quality[] allowed)
        {
            var qualities = new List<Quality>
            {
                Quality.SDTV,
                Quality.WEBDL480p,
                Quality.DVD,
                Quality.HDTV720p,
                Quality.HDTV1080p,
                Quality.RAWHD,
                Quality.WEBDL720p,
                Quality.Bluray720p,
                Quality.WEBDL1080p,
                Quality.Bluray1080p
            };

            if (allowed.Length == 0)
                allowed = qualities.ToArray();

            var items = qualities
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new QualityProfileItem { Quality = v, Allowed = allowed.Contains(v) }).ToList();

            return items;
        }
    }
}
