// ReSharper disable InconsistentNaming

using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class EnvironmentProviderTest : TestBase
    {
        readonly EnvironmentProvider environmentProvider = new EnvironmentProvider();

        [Test]
        public void StartupPath_should_not_be_empty()
        {
            environmentProvider.StartUpPath.Should().NotBeBlank();
            Path.IsPathRooted(environmentProvider.StartUpPath).Should().BeTrue("Path is not rooted");

        }

        [Test]
        public void ApplicationPath_should_not_be_empty()
        {
            environmentProvider.ApplicationPath.Should().NotBeBlank();
            Path.IsPathRooted(environmentProvider.ApplicationPath).Should().BeTrue("Path is not rooted");
        }


        [Test]
        public void ApplicationPath_should_find_root_in_current_folder()
        {
            Directory.CreateDirectory(EnvironmentProvider.ROOT_MARKER);
            environmentProvider.ApplicationPath.Should().BeEquivalentTo(Directory.GetCurrentDirectory());
        }

        [Test]
        public void crawl_should_return_null_if_cant_find_root()
        {
            environmentProvider.CrawlToRoot("C:\\").Should().BeNullOrEmpty();
        }

        [Test]
        public void should_go_up_the_tree_to_find_iis()
        {
            environmentProvider.ApplicationPath.Should().NotBe(Environment.CurrentDirectory);
            environmentProvider.ApplicationPath.Should().NotBe(environmentProvider.StartUpPath);
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
            environmentProvider.Version.Should().NotBe(new Version(version));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(EnvironmentProvider.ROOT_MARKER))
                Directory.Delete(EnvironmentProvider.ROOT_MARKER);
        }
    }
}
