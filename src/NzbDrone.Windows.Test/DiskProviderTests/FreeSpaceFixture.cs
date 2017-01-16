using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Windows.Disk;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Win")]
    public class FreeSpaceFixture : FreeSpaceFixtureBase<DiskProvider>
    {
        public FreeSpaceFixture()
        {
            WindowsOnly();
        }
    }
}
