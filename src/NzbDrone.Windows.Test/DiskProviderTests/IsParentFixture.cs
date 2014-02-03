using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    public class IsParentFixtureFixture : IsParentFixtureBase<DiskProvider>
    {
        public IsParentFixtureFixture()
        {
            WindowsOnly();
        }
    }
}
