using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class EnvironmentProviderTest : TestBase<EnvironmentProvider>
    {

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            Subject.StartUpPath.Should().NotBeBlank();
            Path.IsPathRooted(Subject.StartUpPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            Subject.WorkingDirectory.Should().NotBeBlank();
            Path.IsPathRooted(Subject.WorkingDirectory).Should().BeTrue("Path is not rooted");
        }



        [Test]
        public void IsProduction_should_return_false_when_run_within_nunit()
        {
            EnvironmentProvider.IsProduction.Should().BeFalse();
        }

        [TestCase("0.0.0.0")]
        [TestCase("1.0.0.0")]
        public void Application_version_should_not_be_default(string version)
        {
            Subject.Version.Should().NotBe(new Version(version));
        }

    }
}
