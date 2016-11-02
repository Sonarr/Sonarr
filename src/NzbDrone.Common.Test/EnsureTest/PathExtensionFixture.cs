using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EnsureTest
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {
        [TestCase(@"p:\TV Shows\file with, comma.mkv")]
        [TestCase(@"\\serer\share\file with, comma.mkv")]
        public void EnsureWindowsPath(string path)
        {
            WindowsOnly();
            Ensure.That(path, () => path).IsValidPath();
        }


        [TestCase(@"/var/user/file with, comma.mkv")]
        public void EnsureLinuxPath(string path)
        {
            MonoOnly();
            Ensure.That(path, () => path).IsValidPath();
        }

        [Test]
        public void GetAncestorFolders_should_return_all_ancestors_in_path()
        {
            var path = @"C:\Test\TV\Series Title".AsOsAgnostic();
            var result = path.GetAncestorFolders();

            result.Count.Should().Be(4);
            result[0].Should().Be(@"C:\".AsOsAgnostic());
            result[1].Should().Be(@"Test".AsOsAgnostic());
            result[2].Should().Be(@"TV".AsOsAgnostic());
            result[3].Should().Be(@"Series Title".AsOsAgnostic());
        }
    }
}
