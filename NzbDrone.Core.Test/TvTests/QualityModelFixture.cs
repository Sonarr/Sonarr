using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    
    public class QualityModelFixture : CoreTest
    {
        [Test]
        public void Icomparer_greater_test()
        {
            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_greater_proper()
        {
            var first = new QualityModel(Quality.Bluray1080p, false);
            var second = new QualityModel(Quality.Bluray1080p, true);

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_lesser()
        {
            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void Icomparer_lesser_proper()
        {
            var first = new QualityModel(Quality.DVD, false);
            var second = new QualityModel(Quality.DVD, true);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void equal_operand()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            (first == second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void equal_operand_false()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Unknown, true);

            (first == second).Should().BeFalse();
        }

        [Test]
        public void equal_operand_false_proper()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Bluray1080p, false);

            (first == second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            (first != second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_false()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Unknown, true);

            (first != second).Should().BeTrue();
        }

        [Test]
        public void not_equal_operand_false_proper()
        {
            var first = new QualityModel(Quality.Bluray1080p, true);
            var second = new QualityModel(Quality.Bluray1080p, false);

            (first != second).Should().BeTrue();
        }

        [Test]
        public void greater_operand()
        {
            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            (first < second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void lesser_operand()
        {
            var first = new QualityModel(Quality.DVD, true);
            var second = new QualityModel(Quality.Bluray1080p, true);

            (second > first).Should().BeTrue();
            (second >= first).Should().BeTrue();
        }
    }
}
