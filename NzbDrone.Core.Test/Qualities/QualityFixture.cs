using System.Linq;
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


        [Test]
        public void Icomparer_greater_test()
        {
            var first = Quality.DVD;
            var second = Quality.Bluray1080p;

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_lesser()
        {
            var first = Quality.DVD;
            var second = Quality.Bluray1080p;

            first.Should().BeLessThan(second);
        }

        [Test]
        public void equal_operand()
        {
            var first = Quality.Bluray1080p;
            var second = Quality.Bluray1080p;

            (first == second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void equal_operand_false()
        {
            var first = Quality.Bluray1080p;
            var second = Quality.Unknown;

            (first == second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand()
        {
            var first = Quality.Bluray1080p;
            var second = Quality.Bluray1080p;

            (first != second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_false()
        {
            var first = Quality.Bluray1080p;
            var second = Quality.Unknown;

            (first != second).Should().BeTrue();
        }

        [Test]
        public void greater_operand()
        {
            var first = Quality.DVD;
            var second = Quality.Bluray1080p;

            (first < second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void lesser_operand()
        {
            var first = Quality.DVD;
            var second = Quality.Bluray1080p;

            (second > first).Should().BeTrue();
            (second >= first).Should().BeTrue();
        }
    }
}
