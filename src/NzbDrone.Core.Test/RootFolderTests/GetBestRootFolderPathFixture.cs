using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.RootFolderTests
{
    [TestFixture]
    public class GetBestRootFolderPathFixture : CoreTest<RootFolderService>
    {
        private void GivenRootFolders(params string[] paths)
        {
            Mocker.GetMock<IRootFolderRepository>()
                .Setup(s => s.All())
                .Returns(paths.Select(p => new RootFolder { Path = p }));
        }

        [Test]
        public void should_return_root_folder_that_is_parent_path()
        {
            GivenRootFolders(@"C:\Test\TV".AsOsAgnostic(), @"D:\Test\TV".AsOsAgnostic());
            Subject.GetBestRootFolderPath(@"C:\Test\TV\Series Title".AsOsAgnostic()).Should().Be(@"C:\Test\TV".AsOsAgnostic());
        }

        [Test]
        public void should_return_root_folder_that_is_grandparent_path()
        {
            GivenRootFolders(@"C:\Test\TV".AsOsAgnostic(), @"D:\Test\TV".AsOsAgnostic());
            Subject.GetBestRootFolderPath(@"C:\Test\TV\S\Series Title".AsOsAgnostic()).Should().Be(@"C:\Test\TV".AsOsAgnostic());
        }

        [Test]
        public void should_get_parent_path_from_os_path_if_matching_root_folder_is_not_found()
        {
            var seriesPath = @"T:\Test\TV\Series Title".AsOsAgnostic();

            GivenRootFolders(@"C:\Test\TV".AsOsAgnostic(), @"D:\Test\TV".AsOsAgnostic());
            Subject.GetBestRootFolderPath(seriesPath).Should().Be(@"T:\Test\TV".AsOsAgnostic());
        }

        [Test]
        public void should_get_parent_path_from_os_path_if_matching_root_folder_is_not_found_for_posix_path()
        {
            WindowsOnly();

            var seriesPath = "/mnt/tv/Series Title";

            GivenRootFolders(@"C:\Test\TV".AsOsAgnostic(), @"D:\Test\TV".AsOsAgnostic());
            Subject.GetBestRootFolderPath(seriesPath).Should().Be(@"/mnt/tv");
        }

        [Test]
        public void should_get_parent_path_from_os_path_if_matching_root_folder_is_not_found_for_windows_path()
        {
            PosixOnly();

            var seriesPath = @"T:\Test\TV\Series Title";

            GivenRootFolders(@"C:\Test\TV".AsOsAgnostic(), @"D:\Test\TV".AsOsAgnostic());
            Subject.GetBestRootFolderPath(seriesPath).Should().Be(@"T:\Test\TV");
        }
    }
}
