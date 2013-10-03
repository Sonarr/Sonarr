using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc.Model;

namespace NzbDrone.Core.Test
{
    public class XbmcVersionTests
    {
        [TestCase(6, 0, 0)]
        [TestCase(5, 1, 0)]
        [TestCase(5, 0, 1)]
        public void Icomparer_greater_test(int major, int minor, int patch)
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(major, minor, patch);

            second.Should().BeGreaterThan(first);
        }

        [TestCase(4, 5, 5)]
        [TestCase(5, 4, 5)]
        [TestCase(5, 5, 4)]
        public void Icomparer_lesser_test(int major, int minor, int patch)
        {
            var first = new XbmcVersion(5, 5, 5);
            var second = new XbmcVersion(major, minor, patch);

            second.Should().BeLessThan(first);
        }

        [Test]
        public void equal_operand()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(5, 0, 0);

            (first == second).Should().BeTrue();
            (first >= second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void equal_operand_false()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(6, 0, 0);

            (first == second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_false()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(5, 0, 0);

            (first != second).Should().BeFalse();
        }

        [Test]
        public void not_equal_operand_true()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(6, 0, 0);

            (first != second).Should().BeTrue();
        }

        [Test]
        public void greater_operand()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(6, 0, 0);

            (first < second).Should().BeTrue();
            (first <= second).Should().BeTrue();
        }

        [Test]
        public void lesser_operand()
        {
            var first = new XbmcVersion(5, 0, 0);
            var second = new XbmcVersion(6, 0, 0);

            (second > first).Should().BeTrue();
            (second >= first).Should().BeTrue();
        }
    }
}
