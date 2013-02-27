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
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.XemCommunicationProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class GetSceneTvdbMappingsFixture : CoreTest
    {
        private void WithFailureJson()
        {
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(File.ReadAllText(@".\Files\Xem\Failure.txt"));
        }

        private void WithIdsJson()
        {
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(File.ReadAllText(@".\Files\Xem\Ids.txt"));
        }

        private void WithMappingsJson()
        {
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(File.ReadAllText(@".\Files\Xem\Mappings.txt"));
        }

        [Test]
        public void should_throw_when_failure_is_found()
        {
            WithFailureJson();
            Assert.Throws<Exception>(() => Mocker.Resolve<XemCommunicationProvider>().GetSceneTvdbMappings(12345));
        }

        [Test]
        public void should_get_list_of_mappings()
        {
            WithMappingsJson();
            Mocker.Resolve<XemCommunicationProvider>().GetSceneTvdbMappings(12345).Should().NotBeEmpty();
        }

        [Test]
        public void should_have_two_mappings()
        {
            WithMappingsJson();
            Mocker.Resolve<XemCommunicationProvider>().GetSceneTvdbMappings(12345).Should().HaveCount(2);
        }

        [Test]
        public void should_have_expected_results()
        {
            WithMappingsJson();
            var results = Mocker.Resolve<XemCommunicationProvider>().GetSceneTvdbMappings(12345);
            var first = results.First();
            first.Scene.Absolute.Should().Be(1);
            first.Scene.Season.Should().Be(1);
            first.Scene.Episode.Should().Be(1);
            first.Tvdb.Absolute.Should().Be(1);
            first.Tvdb.Season.Should().Be(1);
            first.Tvdb.Episode.Should().Be(1);
        }
    }
}