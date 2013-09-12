using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [Test]
        public void no_update_when_version_higher()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(10,0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(1, 0)).Should().NotBeNull();
        }
    }
}
