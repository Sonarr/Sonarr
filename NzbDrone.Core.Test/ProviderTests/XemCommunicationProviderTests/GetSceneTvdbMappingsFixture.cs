using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.XemCommunicationProviderTests
{
    [TestFixture]
    
    public class GetSceneTvdbMappingsFixture : CoreTest
    {
        private void WithFailureJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files","Xem","Failure.txt"));
        }

        private void WithIdsJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files","Xem","Ids.txt"));
        }

        private void WithMappingsJson()
        {
            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString(It.IsAny<String>()))
                    .Returns(ReadAllText("Files","Xem","Mappings.txt"));
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