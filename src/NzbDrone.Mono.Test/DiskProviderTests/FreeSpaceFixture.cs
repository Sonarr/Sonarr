using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            LinuxOnly();
        }
    }
}
