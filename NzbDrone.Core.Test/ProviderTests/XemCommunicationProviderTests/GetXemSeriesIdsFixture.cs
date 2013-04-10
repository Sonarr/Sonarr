using System;
using System.IO;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.XemCommunicationProviderTests
{
    [TestFixture]
    
    public class GetXemSeriesIdsFixture : CoreTest
    {
        private void WithFailureJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Xem", "Failure.txt"));
        }

        private void WithIdsJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Xem", "Ids.txt"));
        }

        private void WithMappingsJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files", "Xem", "Mappings.txt"));
        }

        [Test]
        public void should_throw_when_failure_is_found()
        {
            WithFailureJson();
            Assert.Throws<Exception>(() => Mocker.Resolve<XemCommunicationProvider>().GetXemSeriesIds());
        }

        [Test]
        public void should_get_list_of_int()
        {
            WithIdsJson();
            Mocker.Resolve<XemCommunicationProvider>().GetXemSeriesIds().Should().NotBeEmpty();
        }

        [Test]
        public void should_have_two_ids()
        {
            WithIdsJson();
            Mocker.Resolve<XemCommunicationProvider>().GetXemSeriesIds().Should().HaveCount(2);
        }
    }
}