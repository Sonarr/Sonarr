using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Mono.EnvironmentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Mono")]
    public class MonoPlatformInfoFixture : TestBase<MonoPlatformInfo>
    {
        [Test]
        public void should_get_framework_version()
        {
            Subject.Version.Major.Should().Be(4);
            Subject.Version.Minor.Should().BeOneOf(0, 5, 6);
        }
    }
}
