// ReSharper disable InconsistentNaming

using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class EnviromentProviderTest : TestBase
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
        public void ApplicationPath_should_find_root_in_current_folder()
        {
            Directory.CreateDirectory(EnviromentProvider.ROOT_MARKER);
            enviromentProvider.ApplicationPath.Should().BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Test]
        public void crawl_should_return_null_if_cant_find_root()
        {
            enviromentProvider.CrawlToRoot("C:\\").Should().BeNullOrEmpty();
        }

        [Test]
        public void should_go_up_the_tree_to_find_iis()
        {
            enviromentProvider.ApplicationPath.Should().NotBe(Environment.CurrentDirectory);
            enviromentProvider.ApplicationPath.Should().NotBe(enviromentProvider.StartUpPath);
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

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(EnviromentProvider.ROOT_MARKER))
                Directory.Delete(EnviromentProvider.ROOT_MARKER);
        }
    }
}
