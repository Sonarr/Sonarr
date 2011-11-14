using System;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.UpdateProviderTests
{
    class GetAvilableUpdateFixture : CoreTest
    {
        private AutoMoqer _mocker = null;

        private static Version _latestsTestVersion = new Version("0.6.0.3");
        private static string _latestsTestUrl = "http://update.nzbdrone.com/_test/NzbDrone.master.0.6.0.3.zip";
        private static string _latestsTestFileName = "NzbDrone.master.0.6.0.3.zip";

        [SetUp]
        public void setup()
        {
            _mocker = new AutoMoqer(MockBehavior.Strict);

            _mocker.GetMock<ConfigProvider>().SetupGet(c => c.UpdateUrl).Returns("http://update.nzbdrone.com/_test/");
            _mocker.Resolve<HttpProvider>();
        }

        [TestCase("0.6.0.9")]
        [TestCase("0.7.0.1")]
        [TestCase("1.0.0.0")]
        public void should_return_null_if_latests_is_lower_than_current_version(string currentVersion)
        {
            _mocker.GetMock<EnviromentProvider>().SetupGet(c => c.Version).Returns(new Version(currentVersion));

            var updatePackage = _mocker.Resolve<UpdateProvider>().GetAvilableUpdate();

            updatePackage.Should().BeNull();
        }

        [Test]
        public void should_return_null_if_latests_is_equal_to_current_version()
        {
            _mocker.GetMock<EnviromentProvider>().SetupGet(c => c.Version).Returns(_latestsTestVersion);

            var updatePackage = _mocker.Resolve<UpdateProvider>().GetAvilableUpdate();

            updatePackage.Should().BeNull();
        }

        [TestCase("0.0.0.0")]
        [TestCase("0.0.0.1")]
        [TestCase("0.0.10.10")]
        public void should_return_update_if_latests_is_higher_than_current_version(string currentVersion)
        {
            _mocker.GetMock<EnviromentProvider>().SetupGet(c => c.Version).Returns(new Version(currentVersion));

            var updatePackage = _mocker.Resolve<UpdateProvider>().GetAvilableUpdate();

            updatePackage.Should().NotBeNull();
            updatePackage.Version.Should().Be(_latestsTestVersion);
            updatePackage.FileName.Should().BeEquivalentTo(_latestsTestFileName);
            updatePackage.Url.Should().BeEquivalentTo(_latestsTestUrl);
        }
    }
}
