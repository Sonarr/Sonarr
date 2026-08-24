using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Host;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class ForwardedHeadersConfiguratorFixture
    {
        private static ForwardedHeadersOptions GetOptions(string trustedNetworks)
        {
            var configFileProvider = new Mock<IConfigFileProvider>();
            configFileProvider.SetupGet(c => c.TrustedNetworks).Returns(trustedNetworks);

            var options = new ForwardedHeadersOptions();

            ForwardedHeadersConfigurator.Configure(options, configFileProvider.Object);

            return options;
        }

        private static string[] GetKnownNetworks(ForwardedHeadersOptions options)
        {
            return options.KnownNetworks.Select(n => $"{n.Prefix}/{n.PrefixLength}").ToArray();
        }

        [Test]
        public void should_forward_for_proto_and_host_headers()
        {
            GetOptions("10.0.0.0/8").ForwardedHeaders
                .Should()
                .Be(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void should_only_trust_loopback_when_no_networks_are_configured(string trustedNetworks)
        {
            GetKnownNetworks(GetOptions(trustedNetworks)).Should().BeEquivalentTo("127.0.0.1/8");
        }

        [Test]
        public void should_trust_configured_networks_in_addition_to_loopback()
        {
            GetKnownNetworks(GetOptions("10.0.0.0/8, 172.17.0.1,fc00::/7"))
                .Should()
                .BeEquivalentTo("127.0.0.1/8", "10.0.0.0/8", "172.17.0.1/32", "fc00::/7");
        }

        [Test]
        public void should_skip_invalid_networks()
        {
            GetKnownNetworks(GetOptions("10.0.0.0/8,not-an-address,10.1.2.3/8,0.0.0.0/0,192.168.0.0/16"))
                .Should()
                .BeEquivalentTo("127.0.0.1/8", "10.0.0.0/8", "192.168.0.0/16");
        }

        [Test]
        public void should_skip_empty_entries()
        {
            GetKnownNetworks(GetOptions("10.0.0.0/8,, ,192.168.0.0/16,"))
                .Should()
                .BeEquivalentTo("127.0.0.1/8", "10.0.0.0/8", "192.168.0.0/16");
        }
    }
}
