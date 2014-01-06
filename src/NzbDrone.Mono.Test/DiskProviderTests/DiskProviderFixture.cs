using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    public class DiskProviderFixture : DiskProviderFixtureBase<DiskProvider>
    {
        public DiskProviderFixture()
        {
            LinuxOnly();
        }
    }
}
