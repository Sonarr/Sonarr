using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Network;

namespace NzbDrone.Common.Test.NetworkTests
{
    [TestFixture]
    public class IPNetworkParserFixture
    {
        [TestCase("10.0.0.0/8", "10.0.0.0", 8)]
        [TestCase("172.16.0.0/12", "172.16.0.0", 12)]
        [TestCase("192.168.0.0/16", "192.168.0.0", 16)]
        [TestCase("10.0.0.5/32", "10.0.0.5", 32)]
        [TestCase("fc00::/7", "fc00::", 7)]
        [TestCase("fe80::/10", "fe80::", 10)]
        [TestCase("2001:db8::1/128", "2001:db8::1", 128)]
        public void should_parse_network_in_cidr_notation(string value, string expectedAddress, int expectedPrefixLength)
        {
            IPNetworkParser.TryParse(value, out var address, out var prefixLength).Should().BeTrue();

            address.Should().Be(IPAddress.Parse(expectedAddress));
            prefixLength.Should().Be(expectedPrefixLength);
        }

        [TestCase("172.17.0.1", "172.17.0.1", 32)]
        [TestCase("2001:db8::1", "2001:db8::1", 128)]
        public void should_use_single_host_prefix_length_for_bare_address(string value, string expectedAddress, int expectedPrefixLength)
        {
            IPNetworkParser.TryParse(value, out var address, out var prefixLength).Should().BeTrue();

            address.Should().Be(IPAddress.Parse(expectedAddress));
            prefixLength.Should().Be(expectedPrefixLength);
        }

        [TestCase(" 10.0.0.0/8 ")]
        [TestCase("10.0.0.0 / 8")]
        [TestCase("\t172.17.0.1")]
        public void should_ignore_surrounding_whitespace(string value)
        {
            IPNetworkParser.TryParse(value, out _, out _).Should().BeTrue();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("not-an-address")]
        [TestCase("10.0.0.0/")]
        [TestCase("/8")]
        [TestCase("10.0.0.0/8/8")]
        [TestCase("10.0.0.0/eight")]
        [TestCase("10.0.0.0/-1")]
        [TestCase("10.0.0.0/33")]
        [TestCase("fc00::/-1")]
        [TestCase("fc00::/129")]
        [TestCase("10.0.0.0/128")]
        [TestCase("0.0.0.0/0")]
        [TestCase("::/0")]
        [TestCase("10.0.0.0/0")]
        [TestCase("10")]
        [TestCase("10/8")]
        [TestCase("10.1/8")]
        [TestCase("10.1.2/8")]
        [TestCase("10.1.2.3/8")]
        [TestCase("172.17.0.0/12")]
        [TestCase("192.168.1.1/24")]
        [TestCase("fc00::1/7")]
        [TestCase("2001:db8::1/32")]
        [TestCase("example.com")]
        [TestCase("example.com/24")]
        public void should_not_parse_invalid_network(string value)
        {
            IPNetworkParser.TryParse(value, out var address, out var prefixLength).Should().BeFalse();

            address.Should().BeNull();
            prefixLength.Should().Be(0);
        }

        [TestCase("10.0.0.0/8", true)]
        [TestCase("10.0.0.0/8,192.168.0.0/16", true)]
        [TestCase("10.0.0.0/8, 172.17.0.1", true)]
        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("   ", true)]
        [TestCase("10.0.0.0/8,garbage", false)]
        [TestCase("garbage", false)]
        [TestCase("10.0.0.0/8,", true)]
        [TestCase(",10.0.0.0/8", true)]
        [TestCase("10.0.0.0/8, ,192.168.0.0/16", true)]
        [TestCase(",,", true)]
        [TestCase("0.0.0.0/0", false)]
        [TestCase("10.0.0.0/8,10.1.2.3/8", false)]
        public void should_validate_list(string value, bool expected)
        {
            IPNetworkParser.IsValidList(value).Should().Be(expected);
        }

        [TestCase("10.0.0.0/8,", "10.0.0.0/8")]
        [TestCase(",10.0.0.0/8", "10.0.0.0/8")]
        [TestCase("10.0.0.0/8,,192.168.0.0/16", "10.0.0.0/8, 192.168.0.0/16")]
        [TestCase("  10.0.0.0/8 ,  172.17.0.1  ", "10.0.0.0/8, 172.17.0.1")]
        [TestCase("10.0.0.0/8", "10.0.0.0/8")]
        [TestCase(",,", "")]
        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("   ", "")]
        public void should_normalize_list(string value, string expected)
        {
            IPNetworkParser.NormalizeList(value).Should().Be(expected);
        }
    }
}
