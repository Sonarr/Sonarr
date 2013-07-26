using NUnit.Framework;
using NzbDrone.Common.EnsureThat;
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
            Ensure.That(() => path).IsValidPath();
        }


        [TestCase(@"/var/user/file with, comma.mkv")]
        public void EnsureLinuxPath(string path)
        {
            LinuxOnly();
            Ensure.That(() => path).IsValidPath();
        }
    }
}
