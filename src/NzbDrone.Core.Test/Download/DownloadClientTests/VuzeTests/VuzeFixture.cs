using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.Download.Clients.Vuze;
using NzbDrone.Core.Test.Download.DownloadClientTests.TransmissionTests;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.VuzeTests
{
    [TestFixture]
    public class VuzeFixture : TransmissionFixtureBase<Vuze>
    {
        [TestCase("14")]
        [TestCase("15")]
        [TestCase("20")]
        public void should_check_protocol_version_number(string version)
        {
            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.GetProtocolVersion(It.IsAny<TransmissionSettings>()))
                  .Returns(version);

            Subject.Test().IsValid.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase("10")]
        [TestCase("foo")]
        public void should_fail_with_unsupported_protocol_version(string version)
        {
            Mocker.GetMock<ITransmissionProxy>()
              .Setup(s => s.GetProtocolVersion(It.IsAny<TransmissionSettings>()))
              .Returns(version);

            Subject.Test().IsValid.Should().BeFalse();
        }

        [Test]
        public void should_have_correct_output_directory()
        {
            WindowsOnly();

            // Vuze reports DownloadDir including the torrent name
            _downloading.DownloadDir = @"C:/Downloads/" + _title;

            GivenTorrents(new List<TransmissionTorrent>
                {
                    _downloading
                });

            var items = Subject.GetItems().ToList();

            items.Should().HaveCount(1);
            items.First().OutputPath.Should().Be(@"C:\Downloads\" + _title);
        }

    }
}