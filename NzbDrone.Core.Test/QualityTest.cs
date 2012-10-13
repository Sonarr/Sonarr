using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityTest : CoreTest
    {
        [Test]
        public void Icomparer_greater_test()
        {
            var first = new QualityModel(QualityTypes.DVD, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_greater_proper()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, false);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            second.Should().BeGreaterThan(first);
        }

        [Test]
        public void Icomparer_lesser()
        {
            var first = new QualityModel(QualityTypes.DVD, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void Icomparer_lesser_proper()
        {
            var first = new QualityModel(QualityTypes.DVD, false);
            var second = new QualityModel(QualityTypes.DVD, true);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void equal_operand()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            (first == second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void equal_operand_false()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Unknown, true);

            (first == second).Should().BeFalse();
        }

        [Test]
        public void equal_operand_false_proper()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, false);

            (first == second).Should().BeFalse();
        }


        [Test]
        public void not_equal_operand()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            (first != second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_false()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Unknown, true);

            (first != second).Should().BeTrue();
        }

        [Test]
        public void not_equal_operand_false_proper()
        {
            var first = new QualityModel(QualityTypes.Bluray1080p, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, false);

            (first != second).Should().BeTrue();
        }

        [Test]
        public void greater_operand()
        {
            var first = new QualityModel(QualityTypes.DVD, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            (first < second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void lesser_operand()
        {
            var first = new QualityModel(QualityTypes.DVD, true);
            var second = new QualityModel(QualityTypes.Bluray1080p, true);

            (second > first).Should().BeTrue();
            (second >= first).Should().BeTrue();
        }

    }
}
