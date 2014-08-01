using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc
{
    [TestFixture]
    public class GetJsonVersionFixture : CoreTest<XbmcService>
    {
        private XbmcSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = Builder<XbmcSettings>.CreateNew()
                                             .Build();
        }

        private void GivenVersionResponse(String response)
        {
            Mocker.GetMock<IXbmcJsonApiProxy>()
                  .Setup(s => s.GetJsonVersion(_settings))
                  .Returns(response);
        }

        [TestCase(3)]
        [TestCase(2)]
        [TestCase(0)]
        public void should_get_version_from_major_only(int number)
        {
            GivenVersionResponse("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":" + number + "}}");

            Subject.GetJsonVersion(_settings).Should().Be(new XbmcVersion(number));
        }

        [TestCase(5, 0, 0)]
        [TestCase(6, 0, 0)]
        [TestCase(6, 1, 0)]
        [TestCase(6, 0, 23)]
        [TestCase(0, 0, 0)]
        public void should_get_version_from_semantic_version(int major, int minor, int patch)
        {
            GivenVersionResponse("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":{\"version\":{\"major\":" + major + ",\"minor\":" + minor + ",\"patch\":" + patch + "}}}");

            Subject.GetJsonVersion(_settings).Should().Be(new XbmcVersion(major, minor, patch));
        }

        [Test]
        public void should_get_version_zero_when_an_error_is_received()
        {
            GivenVersionResponse("{\"error\":{\"code\":-32601,\"message\":\"Method not found.\"},\"id\":10,\"jsonrpc\":\"2.0\"}");

            Subject.GetJsonVersion(_settings).Should().Be(new XbmcVersion(0));
        }
    }
}