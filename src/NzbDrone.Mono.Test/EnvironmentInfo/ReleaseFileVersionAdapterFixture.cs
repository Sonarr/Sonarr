using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Mono.Disk;
using NzbDrone.Mono.EnvironmentInfo.VersionAdapters;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Mono")]
    public class ReleaseFileVersionAdapterFixture : TestBase<ReleaseFileVersionAdapter>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IDiskProvider>(Mocker.Resolve<DiskProvider>());
        }

        [Test]
        public void should_get_version_info()
        {
            var info = Subject.Read();
            info.FullName.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().NotBeNullOrWhiteSpace();
            info.Version.Should().NotBeNullOrWhiteSpace();
        }
    }
}