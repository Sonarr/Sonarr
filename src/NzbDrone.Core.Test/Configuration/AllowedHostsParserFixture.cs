using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Configuration
{
    [TestFixture]
    public class AllowedHostsParserFixture : TestBase
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void should_return_empty_list_for_null_or_empty(string value)
        {
            AllowedHostsParser.Parse(value).Should().BeEmpty();
        }

        [Test]
        public void should_split_on_comma_and_semicolon_and_trim_whitespace()
        {
            var result = AllowedHostsParser.Parse("sonarr.local, 192.168.1.5; myhost");

            result.Should().BeEquivalentTo("sonarr.local", "192.168.1.5", "myhost");
        }

        [Test]
        public void should_wrap_bare_ipv6_addresses_in_brackets()
        {
            var result = AllowedHostsParser.Parse("::1, 2001:db8::1, [fe80::1]");

            result.Should().BeEquivalentTo("[::1]", "[2001:db8::1]", "[fe80::1]");
        }

        [Test]
        public void should_not_duplicate_bare_and_bracketed_ipv6_addresses()
        {
            var result = AllowedHostsParser.Parse("::1,[::1]");

            result.Should().BeEquivalentTo("[::1]");
        }

        [Test]
        public void should_remove_empty_entries_and_duplicates()
        {
            var result = AllowedHostsParser.Parse("a,,b, a ");

            result.Should().BeEquivalentTo("a", "b");
        }

        [TestCase("sonarr.local")]
        [TestCase("sonarr.example.co.uk")]
        [TestCase("my-host")]
        [TestCase("192.168.1.5")]
        [TestCase("[::1]")]
        [TestCase("[2001:db8::1]")]
        [TestCase("2001:db8::1")]
        [TestCase("*.example.com")]
        [TestCase("*.example.co.uk")]
        [TestCase("*.subdomain.example.com")]
        [TestCase("*.subdomain.example.co.uk")]
        public void should_be_valid_host(string host)
        {
            AllowedHostsParser.IsValidHost(host).Should().BeTrue();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("http://sonarr.local")]
        [TestCase("sonarr.local:8989")]
        [TestCase("sonarr local")]
        [TestCase("sonarr.*")]
        [TestCase("*")]
        [TestCase("*.")]
        [TestCase("*.192.168.1.5")]
        [TestCase("*.[::1]")]
        [TestCase("*.*.example.com")]
        [TestCase("[not-an-address]")]
        [TestCase(".example.com")]
        [TestCase(".example.co.uk")]
        public void should_not_be_valid_host(string host)
        {
            AllowedHostsParser.IsValidHost(host).Should().BeFalse();
        }
    }
}
