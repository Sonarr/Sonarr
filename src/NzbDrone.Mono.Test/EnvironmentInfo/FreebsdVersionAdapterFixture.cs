using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Mono.EnvironmentInfo.VersionAdapters;
using NzbDrone.Test.Common;
using Workarr.EnvironmentInfo;
using Workarr.Processes;

namespace NzbDrone.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Linux")]
    public class FreebsdVersionAdapterFixture : TestBase<FreebsdVersionAdapter>
    {
        [SetUp]
        public void Setup()
        {
            if (OsInfo.Os != Os.Bsd)
            {
                throw new IgnoreException("BSD Only");
            }

            Mocker.SetConstant<IProcessProvider>(Mocker.Resolve<ProcessProvider>());
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
