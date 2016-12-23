using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class LevenshteinDistanceFixture : TestBase
    {
        [TestCase("", "", 0)]
        [TestCase("abc", "abc", 0)]
        [TestCase("abc", "abcd", 1)]
        [TestCase("abcd", "abc", 1)]
        [TestCase("abc", "abd", 1)]
        [TestCase("abc", "adc", 1)]
        [TestCase("abcdefgh", "abcghdef", 4)]
        [TestCase("a.b.c.", "abc", 3)]
        [TestCase("Agents Of SHIELD", "Marvel's Agents Of S.H.I.E.L.D.", 15)]
        [TestCase("Agents of cracked", "Agents of shield", 6)]
        [TestCase("ABCxxx", "ABC1xx", 1)]
        [TestCase("ABC1xx", "ABCxxx", 1)]
        public void LevenshteinDistance(string text, string other, int expected)
        {
            text.LevenshteinDistance(other).Should().Be(expected);
        }

        [TestCase("", "", 0)]
        [TestCase("abc", "abc", 0)]
        [TestCase("abc", "abcd", 1)]
        [TestCase("abcd", "abc", 3)]
        [TestCase("abc", "abd", 3)]
        [TestCase("abc", "adc", 3)]
        [TestCase("abcdefgh", "abcghdef", 8)]
        [TestCase("a.b.c.", "abc", 0)]
        [TestCase("Agents of shield", "Marvel's Agents Of S.H.I.E.L.D.", 9)]
        [TestCase("Agents of shield", "Agents of cracked", 14)]
        [TestCase("Agents of shield", "the shield", 24)]
        [TestCase("ABCxxx", "ABC1xx", 3)]
        [TestCase("ABC1xx", "ABCxxx", 3)]
        public void LevenshteinDistanceClean(string text, string other, int expected)
        {
            text.ToLower().LevenshteinDistanceClean(other.ToLower()).Should().Be(expected);
        }
    }
}
