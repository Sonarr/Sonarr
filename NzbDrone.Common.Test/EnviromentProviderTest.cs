// ReSharper disable InconsistentNaming

using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class EnviromentProviderTest
    {
        readonly EnviromentProvider enviromentProvider = new EnviromentProvider();

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            enviromentProvider.StartUpPath.Should().NotBeBlank();
            Path.IsPathRooted(enviromentProvider.StartUpPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            enviromentProvider.ApplicationPath.Should().NotBeBlank();
            Path.IsPathRooted(enviromentProvider.ApplicationPath).Should().BeTrue("Path is not rooted");
        }


        [Test]
        public void ApplicationPath_should_find_iis_in_current_folder()
        {
            Directory.CreateDirectory(EnviromentProvider.IIS_FOLDER_NAME);
            enviromentProvider.ApplicationPath.Should().BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Test]
        public void IsProduction_should_return_false_when_run_within_nunit()
        {
            EnviromentProvider.IsProduction.Should().BeFalse();
        }

        [TestCase("0.0.0.0")]
        [TestCase("1.0.0.0")]
        public void Application_version_should_not_be_default(string version)
        {
            enviromentProvider.Version.Should().NotBe(new Version(version));
        }
    }
}
