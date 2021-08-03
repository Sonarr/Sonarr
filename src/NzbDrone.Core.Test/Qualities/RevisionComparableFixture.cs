using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class RevisionComparableFixture : CoreTest
    {
        [Test]
        public void should_be_greater_when_first_quality_is_a_real()
        {
            var first = new Revision(real: 1);
            var second = new Revision();

            first.Should().BeGreaterThan(second);
        }

        [Test]
        public void should_be_greater_when_first_quality_is_a_proper()
        {
            var first = new Revision(version: 2);
            var second = new Revision();

            first.Should().BeGreaterThan(second);
        }

        [Test]
        public void should_be_greater_when_first_is_a_proper_for_a_real()
        {
            var first = new Revision(real: 1, version: 2);
            var second = new Revision(real: 1);

            first.Should().BeGreaterThan(second);
        }

        [Test]
        public void should_be_lesser_when_second_quality_is_a_real()
        {
            var first = new Revision();
            var second = new Revision(real: 1);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void should_be_lesser_when_second_quality_is_a_proper()
        {
            var first = new Revision();
            var second = new Revision(version: 2);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void should_be_lesser_when_second_is_a_proper_for_a_real()
        {
            var first = new Revision(real: 1);
            var second = new Revision(real: 1, version: 2);

            first.Should().BeLessThan(second);
        }

        [Test]
        public void should_be_equal_when_both_real_and_version_match()
        {
            var first = new Revision();
            var second = new Revision();

            first.CompareTo(second).Should().Be(0);
        }

        [Test]
        public void should_be_equal_when_both_real_and_version_match_for_real()
        {
            var first = new Revision(real: 1);
            var second = new Revision(real: 1);

            first.CompareTo(second).Should().Be(0);
        }

        [Test]
        public void should_be_equal_when_both_real_and_version_match_for_real_proper()
        {
            var first = new Revision(version: 2, real: 1);
            var second = new Revision(version: 2, real: 1);

            first.CompareTo(second).Should().Be(0);
        }

        [Test]
        public void equal_operator_tests()
        {
            var first = new Revision();
            var second = new Revision();

            (first > second).Should().BeFalse();
            (first < second).Should().BeFalse();
            (first != second).Should().BeFalse();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
            (first == second).Should().BeTrue();
        }

        [Test]
        public void greater_than_operator_tests()
        {
            var first = new Revision(version: 2);
            var second = new Revision();

            (first > second).Should().BeTrue();
            (first < second).Should().BeFalse();
            (first != second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeFalse();
            (first == second).Should().BeFalse();
        }

        [Test]
        public void less_than_operator_tests()
        {
            var first = new Revision();
            var second = new Revision(version: 2);

            (first > second).Should().BeFalse();
            (first < second).Should().BeTrue();
            (first != second).Should().BeTrue();
            (first >= second).Should().BeFalse();
            (first <= second).Should().BeTrue();
            (first == second).Should().BeFalse();
        }

        [Test]
        public void operating_on_nulls()
        {
            (new Revision() < null).Should().BeFalse();
            (new Revision() <= null).Should().BeFalse();
            (new Revision() > null).Should().BeTrue();
            (new Revision() >= null).Should().BeTrue();

            (new Revision() > null).Should().BeTrue();
            (new Revision() >= null).Should().BeTrue();
            (new Revision() < null).Should().BeFalse();
            (new Revision() <= null).Should().BeFalse();
        }
    }
}
