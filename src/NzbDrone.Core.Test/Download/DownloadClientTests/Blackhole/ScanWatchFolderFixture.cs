using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Blackhole;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.Blackhole
{
    [TestFixture]
    public class ScanWatchFolderFixture : CoreTest<ScanWatchFolder>
    {
        protected readonly string _title = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE";
        protected string _completedDownloadFolder = @"c:\blackhole\completed".AsOsAgnostic();

        protected void GivenCompletedItem()
        {
            var targetDir = Path.Combine(_completedDownloadFolder, _title);
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetDirectories(_completedDownloadFolder))
                .Returns(new[] { targetDir });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFiles(targetDir, SearchOption.AllDirectories))
                .Returns(new[] { Path.Combine(targetDir, "somefile.mkv") });

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFileSize(It.IsAny<string>()))
                .Returns(1000000);

            Mocker.GetMock<IDiskScanService>().Setup(c => c.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                  .Returns<string, IEnumerable<string>, bool>((b, s, c) => s.ToList());
        }

        protected void GivenChangedItem()
        {
            var currentSize = Mocker.GetMock<IDiskProvider>().Object.GetFileSize("abc");

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.GetFileSize(It.IsAny<string>()))
                .Returns(currentSize + 1);
        }

        private void VerifySingleItem(DownloadItemStatus status)
        {
            var items = Subject.GetItems(_completedDownloadFolder, TimeSpan.FromMilliseconds(50)).ToList();

            items.Count.Should().Be(1);
            items.First().Status.Should().Be(status);
        }

        [Test]
        public void GetItems_should_considered_locked_files_queued()
        {
            GivenCompletedItem();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.IsFileLocked(It.IsAny<string>()))
                .Returns(true);

            Thread.Sleep(60);

            VerifySingleItem(DownloadItemStatus.Downloading);
        }

        [Test]
        public void GetItems_should_considered_changing_files_queued()
        {
            GivenCompletedItem();

            VerifySingleItem(DownloadItemStatus.Downloading);

            // If we keep changing the file every 20ms we should stay Downloading.
            for (int i = 0; i < 10; i++)
            {
                TestLogger.Info("Iteration {0}", i);

                GivenChangedItem();

                VerifySingleItem(DownloadItemStatus.Downloading);

                Thread.Sleep(10);
            }

            // Until it remains unchanged for >=50ms.
            Thread.Sleep(60);

            VerifySingleItem(DownloadItemStatus.Completed);
        }
    }
}
