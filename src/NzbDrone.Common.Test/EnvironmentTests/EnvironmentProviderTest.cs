using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EnvironmentTests
{
    [TestFixture]
    public class BuildInfoTest : TestBase
    {
        [TestCase("0.0.0.0")]
        [TestCase("1.0.0.0")]
        public void Application_version_should_not_be_default(string version)
        {
            BuildInfo.Version.Should().NotBe(new Version(version));
        }
    }
}
