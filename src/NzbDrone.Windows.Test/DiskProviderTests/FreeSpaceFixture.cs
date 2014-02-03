using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            WindowsOnly();
        }
    }
}
