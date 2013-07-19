using NUnit.Framework;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.EnsureTest
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {
        [TestCase(@"p:\TV Shows\file with, comma.mkv")]
        public void EnsureWindowsPath(string path)
        {
            Ensure.That(() => path).IsValidPath();
        }
    }
}
