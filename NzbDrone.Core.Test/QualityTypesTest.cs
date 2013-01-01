using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityTypesTest : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] {1, QualityTypes.SDTV},
                        new object[] {2, QualityTypes.DVD},
                        new object[] {4, QualityTypes.HDTV720p},
                        new object[] {5, QualityTypes.WEBDL720p},
                        new object[] {6, QualityTypes.Bluray720p},
                        new object[] {7, QualityTypes.Bluray1080p}
                };

        public static object[] ToIntCases =
                {
                        new object[] {QualityTypes.SDTV, 1},
                        new object[] {QualityTypes.DVD, 2},
                        new object[] {QualityTypes.HDTV720p, 4},
                        new object[] {QualityTypes.WEBDL720p, 5},
                        new object[] {QualityTypes.Bluray720p, 6},
                        new object[] {QualityTypes.Bluray1080p, 7}
                };

        [Test, TestCaseSource("FromIntCases")]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, QualityTypes expected)
        {
            var quality = (QualityTypes)source;
            quality.Should().Be(expected);
        }

        [Test, TestCaseSource("ToIntCases")]
        public void should_be_able_to_convert_qualityTypes_to_int(QualityTypes source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }


        [Test]
        public void Icomparer_greater_test()
        {
            var first = QualityTypes.DVD;
            var second = QualityTypes.Bluray1080p;

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_lesser()
        {
            var first = QualityTypes.DVD;
            var second = QualityTypes.Bluray1080p;

            first.Should().BeLessThan(second);
        }

        [Test]
        public void equal_operand()
        {
            var first = QualityTypes.Bluray1080p;
            var second = QualityTypes.Bluray1080p;

            (first == second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void equal_operand_false()
        {
            var first = QualityTypes.Bluray1080p;
            var second = QualityTypes.Unknown;

            (first == second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand()
        {
            var first = QualityTypes.Bluray1080p;
            var second = QualityTypes.Bluray1080p;

            (first != second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_false()
        {
            var first = QualityTypes.Bluray1080p;
            var second = QualityTypes.Unknown;

            (first != second).Should().BeTrue();
        }

        [Test]
        public void greater_operand()
        {
            var first = QualityTypes.DVD;
            var second = QualityTypes.Bluray1080p;

            (first < second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void lesser_operand()
        {
            var first = QualityTypes.DVD;
            var second = QualityTypes.Bluray1080p;

            (second > first).Should().BeTrue();
            (second >= first).Should().BeTrue();
        }
    }
}
