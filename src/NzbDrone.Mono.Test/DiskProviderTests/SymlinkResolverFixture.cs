using System.IO;
using FluentAssertions;
using Mono.Unix;
using NUnit.Framework;
using NzbDrone.Mono.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform(Exclude = "Win")]
    public class SymbolicLinkResolverFixture : TestBase<SymbolicLinkResolver>
    {
        [Test]
        public void should_follow_nested_symlinks()
        {
            var rootDir = GetTempFilePath();
            var tempDir1 = Path.Combine(rootDir, "dir1");
            var tempDir2 = Path.Combine(rootDir, "dir2");
            var subDir1 = Path.Combine(tempDir1, "subdir1");
            var file1 = Path.Combine(tempDir2, "file1");
            var file2 = Path.Combine(tempDir2, "file2");

            Directory.CreateDirectory(tempDir1);
            Directory.CreateDirectory(tempDir2);
            File.WriteAllText(file2, "test");

            new UnixSymbolicLinkInfo(subDir1).CreateSymbolicLinkTo("../dir2");
            new UnixSymbolicLinkInfo(file1).CreateSymbolicLinkTo("file2");

            var realPath = Subject.GetCompleteRealPath(Path.Combine(subDir1, "file1"));

            realPath.Should().Be(file2);
        }

        [Test]
        public void should_throw_on_infinite_loop()
        {
            var rootDir = GetTempFilePath();
            var tempDir1 = Path.Combine(rootDir, "dir1");
            var subDir1 = Path.Combine(tempDir1, "subdir1");
            var file1 = Path.Combine(tempDir1, "file1");

            Directory.CreateDirectory(tempDir1);

            new UnixSymbolicLinkInfo(subDir1).CreateSymbolicLinkTo("../../dir1/subdir1/baddir");

            var realPath = Subject.GetCompleteRealPath(file1);

            realPath.Should().Be(file1);
        }
    }
}
