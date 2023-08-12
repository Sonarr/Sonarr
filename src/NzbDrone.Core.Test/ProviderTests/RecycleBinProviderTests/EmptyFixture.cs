using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.RecycleBinProviderTests
{
    [TestFixture]

    public class EmptyFixture : CoreTest
    {
        private const string RecycleBin = @"C:\Test\RecycleBin";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(RecycleBin);

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetDirectories(RecycleBin))
                    .Returns(new[] { @"C:\Test\RecycleBin\Folder1", @"C:\Test\RecycleBin\Folder2", @"C:\Test\RecycleBin\Folder3" });

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(RecycleBin, false))
                    .Returns(new[] { @"C:\Test\RecycleBin\File1.avi", @"C:\Test\RecycleBin\File2.mkv" });
        }

        [Test]
        public void should_return_if_recycleBin_not_configured()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RecycleBin).Returns(string.Empty);

            Mocker.Resolve<RecycleBinProvider>().Empty();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetDirectories(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_delete_all_folders()
        {
            Mocker.Resolve<RecycleBinProvider>().Empty();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Exactly(3));
        }

        [Test]
        public void should_delete_all_files()
        {
            Mocker.Resolve<RecycleBinProvider>().Empty();

            Mocker.GetMock<IDiskProvider>().Verify(v => v.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
