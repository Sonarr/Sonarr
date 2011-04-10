using System;
using AutoMoq;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SyncProviderTest
    {
        [Test]
        public void None_existing_folder_returns_empty_list()
        {
            string path = "d:\\bad folder";

            var mocker = new AutoMoqer();
            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(m => m.FolderExists(path)).Returns(false);

            var result = mocker.Resolve<SyncProvider>().GetUnmappedFolders(path);

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);

            mocker.VerifyAllMocks();
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void empty_folder_path_throws()
        {
            var mocker = new AutoMoqer();
            mocker.Resolve<SyncProvider>().GetUnmappedFolders("");
        }
    }
}