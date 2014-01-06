using NUnit.Framework;
using NzbDrone.Common.Test.DiskProviderTests;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    public class IsParentFixtureFixture : IsParentFixtureBase<DiskProvider>
    {
        public IsParentFixtureFixture()
        {
            LinuxOnly();
        }
    }
}
