using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class UrlHostFixture
    {
        [TestCase("::1", "[::1]")]
        [TestCase("fd3a:9a29:b136:eeee:aaaa::6", "[fd3a:9a29:b136:eeee:aaaa::6]")]
        [TestCase("[::1]", "[::1]")]
        [TestCase("192.168.0.1", "192.168.0.1")]
        [TestCase("sonarr.tv", "sonarr.tv")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void should_bracket_ipv6_addresses(string input, string expected)
        {
            input.ToUrlHost().Should().Be(expected);
        }

        [TestCase("[::1]", "::1")]
        [TestCase("[fd3a:9a29:b136:eeee:aaaa::6]", "fd3a:9a29:b136:eeee:aaaa::6")]
        [TestCase("::1", "::1")]
        [TestCase("192.168.0.1", "192.168.0.1")]
        [TestCase("sonarr.tv", "sonarr.tv")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void should_remove_brackets_from_ipv6_addresses(string input, string expected)
        {
            input.FromUrlHost().Should().Be(expected);
        }

        [TestCase("localhost")]
        [TestCase("LocalHost")]
        [TestCase("127.0.0.1")]
        [TestCase("127.1.2.3")]
        [TestCase("::1")]
        [TestCase("[::1]")]
        public void should_be_localhost_address(string input)
        {
            input.IsLocalhostAddress().Should().BeTrue();
        }

        [TestCase("192.168.0.1")]
        [TestCase("[fd3a:9a29:b136:eeee:aaaa::6]")]
        [TestCase("sonarr.tv")]
        [TestCase("")]
        [TestCase(null)]
        public void should_not_be_localhost_address(string input)
        {
            input.IsLocalhostAddress().Should().BeFalse();
        }
    }
}
