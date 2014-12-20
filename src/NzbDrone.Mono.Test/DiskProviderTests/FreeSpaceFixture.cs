using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Mono")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            MonoOnly();
        }
    }
}
