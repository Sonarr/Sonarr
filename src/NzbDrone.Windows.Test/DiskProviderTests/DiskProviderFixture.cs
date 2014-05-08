using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Win")]
    public class DiskProviderFixture : DiskProviderFixtureBase<DiskProvider>
    {
        public DiskProviderFixture()
        {
            WindowsOnly();
        }
    }
}
