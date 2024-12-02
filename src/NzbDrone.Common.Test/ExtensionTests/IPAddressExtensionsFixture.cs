using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test.ExtensionTests
{
    [TestFixture]
    public class IPAddressExtensionsFixture
    {
        [TestCase("::1")]
        [TestCase("10.64.5.1")]
        [TestCase("127.0.0.1")]
        [TestCase("172.16.0.1")]
        [TestCase("192.168.5.1")]
        public void should_return_true_for_local_ip_address(string ipAddress)
        {
            IPAddress.Parse(ipAddress).IsLocalAddress().Should().BeTrue();
        }

        [TestCase("1.2.3.4")]
        [TestCase("172.55.0.1")]
        [TestCase("192.55.0.1")]
        [TestCase("100.64.0.1")]
        [TestCase("100.127.255.254")]
        public void should_return_false_for_public_ip_address(string ipAddress)
        {
            IPAddress.Parse(ipAddress).IsLocalAddress().Should().BeFalse();
        }

        [TestCase("100.64.0.1")]
        [TestCase("100.127.255.254")]
        [TestCase("100.100.100.100")]
        public void should_return_true_for_cgnat_ip_address(string ipAddress)
        {
            IPAddress.Parse(ipAddress).IsCgnatIpAddress().Should().BeTrue();
        }

        [TestCase("1.2.3.4")]
        [TestCase("192.168.5.1")]
        [TestCase("100.63.255.255")]
        [TestCase("100.128.0.0")]
        public void should_return_false_for_non_cgnat_ip_address(string ipAddress)
        {
            IPAddress.Parse(ipAddress).IsCgnatIpAddress().Should().BeFalse();
        }
    }
}
