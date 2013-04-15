using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.UpdateTests
{
    class GetAvailableUpdateFixture : CoreTest<UpdateService>
    {
        private static readonly Version LatestTestVersion = new Version("0.6.0.3");
        private const string LATEST_TEST_URL = "http://update.nzbdrone.com/_test/NzbDrone.master.0.6.0.3.zip";
        private const string LATEST_TEST_FILE_NAME = "NzbDrone.master.0.6.0.3.zip";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.UpdateUrl).Returns("http://update.nzbdrone.com/_test/");
            Mocker.Resolve<HttpProvider>();
        }

        [TestCase("0.6.0.9")]
        [TestCase("0.7.0.1")]
        [TestCase("1.0.0.0")]
        public void should_return_null_if_latest_is_lower_than_current_version(string currentVersion)
        {

            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.Version).Returns(new Version(currentVersion));

            var updatePackage = Subject.GetAvailableUpdate();

            updatePackage.Should().BeNull();
        }

        [Test]
        public void should_return_null_if_latest_is_equal_to_current_version()
        {
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.Version).Returns(LatestTestVersion);

            var updatePackage = Subject.GetAvailableUpdate();

            updatePackage.Should().BeNull();
        }

        [TestCase("0.0.0.0")]
        [TestCase("0.0.0.1")]
        [TestCase("0.0.10.10")]
        public void should_return_update_if_latest_is_higher_than_current_version(string currentVersion)
        {
            Mocker.GetMock<EnvironmentProvider>().SetupGet(c => c.Version).Returns(new Version(currentVersion));

            var updatePackage = Subject.GetAvailableUpdate();

            updatePackage.Should().NotBeNull();
            updatePackage.Version.Should().Be(LatestTestVersion);
            updatePackage.FileName.Should().BeEquivalentTo(LATEST_TEST_FILE_NAME);
            updatePackage.Url.Should().BeEquivalentTo(LATEST_TEST_URL);
        }
    }
}
