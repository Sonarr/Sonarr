using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Clients.DownloadStation;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class SerialNumberProviderFixture : CoreTest<SerialNumberProvider>
    {
        protected DownloadStationSettings _settings;

        [SetUp]
        protected void Setup()
        {
            _settings = new DownloadStationSettings();
        }

        private void GivenValidResponse()
        {
            Mocker.GetMock<IDSMInfoProxy>()
                  .Setup(d => d.GetSerialNumber(It.IsAny<DownloadStationSettings>()))
                  .Returns("serial");
        }

        private void GivenInvalidResponse()
        {
            Mocker.GetMock<IDSMInfoProxy>()
                  .Setup(d => d.GetSerialNumber(It.IsAny<DownloadStationSettings>()))
                  .Throws(new DownloadClientException("Serial response invalid"));
        }

        [Test]
        public void should_return_hashedserialnumber()
        {
            GivenValidResponse();

            var serial = Subject.GetSerialNumber(_settings);

            // This hash should remain the same for 'serial', so don't update the test if you change HashConverter, fix the code instead.
            serial.Should().Be("50DE66B735D30738618568294742FCF1DFA52A47");

            Mocker.GetMock<IDSMInfoProxy>()
                  .Verify(d => d.GetSerialNumber(It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void should_cache_serialnumber()
        {
            GivenValidResponse();

            var serial1 = Subject.GetSerialNumber(_settings);
            var serial2 = Subject.GetSerialNumber(_settings);

            serial2.Should().Be(serial1);

            Mocker.GetMock<IDSMInfoProxy>()
                  .Verify(d => d.GetSerialNumber(It.IsAny<DownloadStationSettings>()), Times.Once());
        }

        [Test]
        public void should_throw_if_serial_number_unavailable()
        {
            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.GetSerialNumber(_settings));

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
