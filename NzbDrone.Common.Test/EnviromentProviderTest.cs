// ReSharper disable InconsistentNaming

using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class EnviromentProviderTest
    {
        readonly EnviromentProvider enviromentController = new EnviromentProvider();

        [Test]

        public void Is_user_interactive_should_be_false()
        {
            enviromentController.IsUserInteractive.Should().BeTrue();
        }

        [Test]
        public void Log_path_should_not_be_empty()
        {
            enviromentController.LogPath.Should().NotBeBlank();
            Path.IsPathRooted(enviromentController.LogPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            enviromentController.StartUpPath.Should().NotBeBlank();
            Path.IsPathRooted(enviromentController.StartUpPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            enviromentController.ApplicationPath.Should().NotBeBlank();
            Path.IsPathRooted(enviromentController.ApplicationPath).Should().BeTrue("Path is not rooted");
        }

        [Test]
        public void IsProduction_should_return_false_when_run_within_nunit()
        {
            EnviromentProvider.IsProduction.Should().BeFalse();
        }
    }
}
