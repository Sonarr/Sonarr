using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetTests
{
    [TestFixture]
    public class NzbgetFixture : DownloadClientFixtureBase<Nzbget>
    {
        private NzbgetQueueItem _queued;
        private NzbgetHistoryItem _failed;
        private NzbgetHistoryItem _completed;
        private Dictionary<string, string> _configItems;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new NzbgetSettings
                                          {
                                              Host = "127.0.0.1",
                                              Port = 2222,
                                              Username = "admin",
                                              Password = "pass",
                                              TvCategory = "tv",
                                              RecentTvPriority = (int)NzbgetPriority.High
                                          };

            _queued = new NzbgetQueueItem
                {
                    FileSizeLo = 1000,
                    RemainingSizeLo = 10,
                    Category = "tv",
                    NzbName = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                    Parameters = new List<NzbgetParameter> { new NzbgetParameter { Name = "drone", Value = "id" } }
                };

            _failed = new NzbgetHistoryItem
                {
                    FileSizeLo = 1000,
                    Category = "tv",
                    Name = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                    DestDir = "somedirectory",
                    Parameters = new List<NzbgetParameter> { new NzbgetParameter { Name = "drone", Value = "id" } },
                    ParStatus = "Some Error",
                    UnpackStatus = "NONE",
                    MoveStatus = "NONE",
                    ScriptStatus = "NONE",
                    DeleteStatus = "NONE",
                    MarkStatus = "NONE"
                };

            _completed = new NzbgetHistoryItem
                {
                    FileSizeLo = 1000,
                    Category = "tv",
                    Name = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                    DestDir = "/remote/mount/tv/Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                    Parameters = new List<NzbgetParameter> { new NzbgetParameter { Name = "drone", Value = "id" } },
                    ParStatus = "SUCCESS",
                    UnpackStatus = "NONE",
                    MoveStatus = "SUCCESS",
                    ScriptStatus = "NONE",
                    DeleteStatus = "NONE",
                    MarkStatus = "NONE"
                };

            _downloadClientItem = Builder<DownloadClientItem>
                                  .CreateNew()
                                  .With(d => d.DownloadId = "_Droned.S01E01.Pilot.1080p.WEB-DL-DRONE_0")
                                  .With(d => d.OutputPath = new OsPath("/remote/mount/tv/Droned.S01E01.Pilot.1080p.WEB-DL-DRONE".AsOsAgnostic()))
                                  .Build();

            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.GetGlobalStatus(It.IsAny<NzbgetSettings>()))
                .Returns(new NzbgetGlobalStatus
                {
                    DownloadRate = 7000000
                });

            Mocker.GetMock<INzbgetProxy>()
                .Setup(v => v.GetVersion(It.IsAny<NzbgetSettings>()))
                .Returns("14.0");

            _configItems = new Dictionary<string, string>();
            _configItems.Add("Category1.Name", "tv");
            _configItems.Add("Category1.DestDir", @"/remote/mount/tv");

            Mocker.GetMock<INzbgetProxy>()
                .Setup(v => v.GetConfig(It.IsAny<NzbgetSettings>()))
                .Returns(_configItems);
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<NzbgetSettings>()))
                .Returns((string)null);
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<NzbgetSettings>()))
                .Returns(Guid.NewGuid().ToString().Replace("-", ""));
        }

        protected virtual void GivenQueue(NzbgetQueueItem queue)
        {
            var list = new List<NzbgetQueueItem>();

            if (queue != null)
            {
                list.Add(queue);
            }

            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.GetQueue(It.IsAny<NzbgetSettings>()))
                .Returns(list);
        }

        protected virtual void GivenHistory(NzbgetHistoryItem history)
        {
            var list = new List<NzbgetHistoryItem>();

            if (history != null)
            {
                list.Add(history);
            }

            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.GetHistory(It.IsAny<NzbgetSettings>()))
                .Returns(list);
        }

        [Test]
        public void GetItems_should_return_no_items_when_queue_is_empty()
        {
            GivenQueue(null);
            GivenHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void RemoveItem_should_delete_folder()
        {
            GivenQueue(null);
            GivenHistory(_completed);

            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Subject.RemoveItem(_downloadClientItem, true);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            _queued.ActiveDownloads = 0;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyQueued(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void paused_item_should_have_required_properties()
        {
            _queued.PausedSizeLo = _queued.RemainingSizeLo;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyPaused(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            _queued.ActiveDownloads = 1;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyDownloading(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void post_processing_item_should_have_required_properties()
        {
            _queued.ActiveDownloads = 1;

            GivenQueue(_queued);
            GivenHistory(null);

            _queued.RemainingSizeLo = 0;

            var result = Subject.GetItems().Single();

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            GivenQueue(null);
            GivenHistory(_failed);

            var result = Subject.GetItems().Single();

            VerifyFailed(result);
        }

        [Test]
        public void should_report_deletestatus_health_as_failed()
        {
            _completed.DeleteStatus = "HEALTH";

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_report_deletestatus_dupe_as_failed()
        {
            _completed.DeleteStatus = "DUPE";

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_report_deletestatus_copy_as_failed()
        {
            _completed.DeleteStatus = "COPY";

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_report_unpackstatus_freespace_as_warning()
        {
            _completed.UnpackStatus = "SPACE";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Warning);
        }

        [Test]
        public void should_report_movestatus_failure_as_warning()
        {
            _completed.MoveStatus = "FAILURE";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Warning);
        }

        [Test]
        public void should_report_scriptstatus_failure_as_failed()
        {
            // TODO: We would love to have a way to distinguish between scripts reporting video corruption, or some internal script error.
            // That way we could return Warning instead of Failed to notify the user to take action.

            _completed.ScriptStatus = "FAILURE";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Download_should_throw_if_failed()
        {
            GivenFailedDownload();

            var remoteEpisode = CreateRemoteEpisode();

            Assert.Throws<DownloadClientRejectedReleaseException>(() => Subject.Download(remoteEpisode));
        }

        [Test]
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.Category = "mycat";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_return_status_with_outputdir()
        {
            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"/remote/mount/tv");
        }

        [Test]
        public void should_return_status_with_mounted_outputdir()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                .Returns(new OsPath(@"O:\mymount".AsOsAgnostic()));

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"O:\mymount".AsOsAgnostic());
        }

        [Test]
        public void should_remap_storage_if_mounted()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                .Returns(new OsPath(@"O:\mymount\Droned.S01E01.Pilot.1080p.WEB-DL-DRONE".AsOsAgnostic()));

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"O:\mymount\Droned.S01E01.Pilot.1080p.WEB-DL-DRONE".AsOsAgnostic());
        }

        [Test]
        public void should_use_dest_dir_if_final_dir_is_null()
        {
            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().First().OutputPath.Should().Be(_completed.DestDir);
        }

        [Test]
        public void should_use_dest_dir_if_final_dir_is_not_set()
        {
            _completed.FinalDir = string.Empty;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().First().OutputPath.Should().Be(_completed.DestDir);
        }

        [Test]
        public void should_report_deletestatus_manual_with_markstatus_bad_as_failed()
        {
            _completed.DeleteStatus = "MANUAL";
            _completed.MarkStatus = "BAD";

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.Status.Should().Be(DownloadItemStatus.Failed);
        }

        [Test]
        public void should_ignore_deletestatus_manual_without_markstatus()
        {
            _completed.DeleteStatus = "MANUAL";

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void should_use_final_dir_when_set_instead_of_dest_dir()
        {
            _completed.FinalDir = "/remote/mount/tv2/Droned.S01E01.Pilot.1080p.WEB-DL-DRONE";

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().First().OutputPath.Should().Be(_completed.FinalDir);
        }

        [TestCase("11.0", false)]
        [TestCase("12.0", true)]
        [TestCase("11.0-b30ef0134", false)]
        [TestCase("13.0-b30ef0134", true)]
        public void should_test_version(string version, bool expected)
        {
            Mocker.GetMock<INzbgetProxy>()
                .Setup(v => v.GetVersion(It.IsAny<NzbgetSettings>()))
                .Returns(version);

            var error = Subject.Test();

            error.IsValid.Should().Be(expected);
        }

        [TestCase("0", false)]
        [TestCase("1", true)]
        [TestCase(" 7", false)]
        [TestCase("5000000", false)]
        public void should_test_keephistory(string keephistory, bool expected)
        {
            _configItems["KeepHistory"] = keephistory;

            var error = Subject.Test();

            error.IsValid.Should().Be(expected);
        }
    }
}
