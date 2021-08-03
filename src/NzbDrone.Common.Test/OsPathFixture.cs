using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    public class OsPathFixture : TestBase
    {
        [TestCase(@"C:\rooted\windows\path\", OsPathKind.Windows)]
        [TestCase(@"C:\rooted\windows\path", OsPathKind.Windows)]
        [TestCase(@"C:\", OsPathKind.Windows)]
        [TestCase(@"C:", OsPathKind.Windows)]
        [TestCase(@"\\rooted\unc\path\", OsPathKind.Windows)]
        [TestCase(@"\\rooted\unc\path", OsPathKind.Windows)]
        [TestCase(@"\relative\windows\path\", OsPathKind.Windows)]
        [TestCase(@"\relative\windows\path", OsPathKind.Windows)]
        [TestCase(@"relative\windows\path\", OsPathKind.Windows)]
        [TestCase(@"relative\windows\path", OsPathKind.Windows)]
        [TestCase(@"relative\", OsPathKind.Windows)]
        [TestCase(@"relative", OsPathKind.Unknown)]
        [TestCase("/rooted/linux/path/", OsPathKind.Unix)]
        [TestCase("/rooted/linux/path", OsPathKind.Unix)]
        [TestCase("/", OsPathKind.Unix)]
        [TestCase("linux/path", OsPathKind.Unix)]
        [TestCase(@"Castle:unrooted+linux+path", OsPathKind.Unknown)]
        public void should_auto_detect_kind(string path, OsPathKind kind)
        {
            var result = new OsPath(path);

            result.Kind.Should().Be(kind);

            if (kind == OsPathKind.Windows)
            {
                result.IsWindowsPath.Should().BeTrue();
                result.IsUnixPath.Should().BeFalse();
            }
            else if (kind == OsPathKind.Unix)
            {
                result.IsWindowsPath.Should().BeFalse();
                result.IsUnixPath.Should().BeTrue();
            }
            else
            {
                result.IsWindowsPath.Should().BeFalse();
                result.IsUnixPath.Should().BeFalse();
            }
        }

        [Test]
        public void should_add_directory_slash()
        {
            var osPath = new OsPath(@"C:\rooted\windows\path\");

            osPath.Directory.Should().NotBeNull();
            osPath.Directory.ToString().Should().Be(@"C:\rooted\windows\");
        }

        [TestCase(@"C:\rooted\windows\path", @"C:\rooted\windows\")]
        [TestCase(@"C:\rooted\windows\path\", @"C:\rooted\windows\")]
        [TestCase(@"C:\rooted", @"C:\")]
        [TestCase(@"C:", null)]
        [TestCase("/rooted/linux/path", "/rooted/linux/")]
        [TestCase("/rooted", "/")]
        [TestCase("/", null)]
        public void should_return_parent_directory(string path, string expectedParent)
        {
            var osPath = new OsPath(path);

            osPath.Directory.Should().NotBeNull();
            osPath.Directory.Should().Be(new OsPath(expectedParent));
        }

        [Test]
        public void should_return_empty_as_parent_of_root_unc()
        {
            var osPath = new OsPath(@"\\unc");

            osPath.Directory.IsEmpty.Should().BeTrue();
        }

        [TestCase(@"C:\rooted\windows\path")]
        [TestCase(@"C:")]
        [TestCase(@"\\blaat")]
        [TestCase("/rooted/linux/path")]
        [TestCase("/")]
        public void should_detect_rooted_ospaths(string path)
        {
            var osPath = new OsPath(path);

            osPath.IsRooted.Should().BeTrue();
        }

        [TestCase(@"\rooted\windows\path")]
        [TestCase(@"rooted\windows\path")]
        [TestCase(@"path")]
        [TestCase("linux/path")]
        [TestCase(@"Castle:unrooted+linux+path")]
        [TestCase(@"C:unrooted+linux+path")]
        public void should_detect_unrooted_ospaths(string path)
        {
            var osPath = new OsPath(path);

            osPath.IsRooted.Should().BeFalse();
        }

        [TestCase(@"C:\rooted\windows\path", "path")]
        [TestCase(@"C:", "C:")]
        [TestCase(@"\\blaat", "blaat")]
        [TestCase("/rooted/linux/path", "path")]
        [TestCase("/", null)]
        [TestCase(@"\rooted\windows\path\", "path")]
        [TestCase(@"rooted\windows\path", "path")]
        [TestCase(@"path", "path")]
        [TestCase("linux/path", "path")]
        public void should_return_filename(string path, string expectedFilePath)
        {
            var osPath = new OsPath(path);

            osPath.FileName.Should().Be(expectedFilePath);
        }

        [Test]
        public void should_compare_windows_ospathkind_case_insensitive()
        {
            var left = new OsPath(@"C:\rooted\Windows\path");
            var right = new OsPath(@"C:\rooted\windows\path");

            left.Should().Be(right);
        }

        [Test]
        public void should_compare_unix_ospathkind_case_sensitive()
        {
            var left = new OsPath(@"/rooted/Linux/path");
            var right = new OsPath(@"/rooted/linux/path");

            left.Should().NotBe(right);
        }

        [Test]
        public void should_not_ignore_trailing_slash_during_compare()
        {
            var left = new OsPath(@"/rooted/linux/path/");
            var right = new OsPath(@"/rooted/linux/path");

            left.Should().NotBe(right);
        }

        [TestCase(@"C:\Test", @"sub", @"C:\Test\sub")]
        [TestCase(@"C:\Test", @"sub\test", @"C:\Test\sub\test")]
        [TestCase(@"C:\Test\", @"\sub", @"C:\Test\sub")]
        [TestCase(@"C:\Test", @"sub\", @"C:\Test\sub\")]
        [TestCase(@"C:\Test", @"C:\Test2\sub", @"C:\Test2\sub")]
        [TestCase(@"/Test", @"sub", @"/Test/sub")]
        [TestCase(@"/Test", @"sub/", @"/Test/sub/")]
        [TestCase(@"/Test/", @"sub/test/", @"/Test/sub/test/")]
        [TestCase(@"/Test/", @"/Test2/", @"/Test2/")]
        [TestCase(@"C:\Test", "", @"C:\Test")]
        public void should_combine_path(string left, string right, string expectedResult)
        {
            var osPathLeft = new OsPath(left);
            var osPathRight = new OsPath(right);

            var result = osPathLeft + osPathRight;

            result.FullPath.Should().Be(expectedResult);
        }

        [Test]
        public void should_fix_slashes_windows()
        {
            var osPath = new OsPath(@"C:/on/windows/transmission\uses/forward/slashes");

            osPath.Kind.Should().Be(OsPathKind.Windows);
            osPath.FullPath.Should().Be(@"C:\on\windows\transmission\uses\forward\slashes");
        }

        [Test]
        public void should_fix_slashes_unix()
        {
            var osPath = new OsPath(@"/just/a/test\to\verify the/slashes\");

            osPath.Kind.Should().Be(OsPathKind.Unix);
            osPath.FullPath.Should().Be(@"/just/a/test/to/verify the/slashes/");
        }

        [Test]
        public void should_fix_double_slashes_unix()
        {
            var osPath = new OsPath(@"/just/a//test////to/verify the/slashes/");

            osPath.Kind.Should().Be(OsPathKind.Unix);
            osPath.FullPath.Should().Be(@"/just/a/test/to/verify the/slashes/");
        }

        [Test]
        public void should_combine_mixed_slashes()
        {
            var left = new OsPath(@"C:/on/windows/transmission");
            var right = new OsPath(@"uses/forward/slashes", OsPathKind.Unknown);

            var osPath = left + right;

            osPath.Kind.Should().Be(OsPathKind.Windows);
            osPath.FullPath.Should().Be(@"C:\on\windows\transmission\uses\forward\slashes");
        }

        [TestCase(@"C:\Test\Data\", @"C:\Test\Data\Sub\Folder", @"Sub\Folder")]
        [TestCase(@"C:\Test\Data\", @"C:\Test\Data2\Sub\Folder", @"..\Data2\Sub\Folder")]
        [TestCase(@"/parent/folder", @"/parent/folder/Sub/Folder", @"Sub/Folder")]
        public void should_create_relative_path(string parent, string child, string expected)
        {
            var left = new OsPath(child);
            var right = new OsPath(parent);

            var osPath = left - right;

            osPath.Kind.Should().Be(OsPathKind.Unknown);
            osPath.FullPath.Should().Be(expected);
        }

        [Test]
        public void should_parse_null_as_empty()
        {
            var result = new OsPath(null);

            result.FullPath.Should().BeEmpty();
            result.IsEmpty.Should().BeTrue();
        }

        [TestCase(@"C:\Test\", @"C:\Test", true)]
        [TestCase(@"C:\Test\", @"C:\Test\Contains\", true)]
        [TestCase(@"C:\Test\", @"C:\Other\", false)]
        public void should_evaluate_contains(string parent, string child, bool expectedResult)
        {
            var left = new OsPath(parent);
            var right = new OsPath(child);

            var result = left.Contains(right);

            result.Should().Be(expectedResult);
        }
    }
}
