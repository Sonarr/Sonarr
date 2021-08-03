using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Sabnzbd;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.SabnzbdTests
{
    [TestFixture]
    public class SabnzbdFixture : DownloadClientFixtureBase<Sabnzbd>
    {
        private SabnzbdQueue _queued;
        private SabnzbdHistory _failed;
        private SabnzbdHistory _completed;
        private SabnzbdConfig _config;
        private SabnzbdFullStatus _fullStatus;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new SabnzbdSettings
                                          {
                                              Host = "127.0.0.1",
                                              Port = 2222,
                                              ApiKey = "5c770e3197e4fe763423ee7c392c25d1",
                                              Username = "admin",
                                              Password = "pass",
                                              TvCategory = "tv",
                                              RecentTvPriority = (int)SabnzbdPriority.High
                                          };
            _queued = new SabnzbdQueue
                {
                    DefaultRootFolder = @"Y:\nzbget\root".AsOsAgnostic(),
                    Paused = false,
                    Items = new List<SabnzbdQueueItem>()
                    {
                        new SabnzbdQueueItem
                        {
                            Status = SabnzbdDownloadStatus.Downloading,
                            Size = 1000,
                            Sizeleft = 10,
                            Timeleft = TimeSpan.FromSeconds(10),
                            Category = "tv",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE"
                        }
                    }
                };

            _failed = new SabnzbdHistory
                {
                    Items = new List<SabnzbdHistoryItem>()
                    {
                        new SabnzbdHistoryItem
                        {
                            Status = SabnzbdDownloadStatus.Failed,
                            Size = 1000,
                            Category = "tv",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE"
                        }
                    }
                };

            _completed = new SabnzbdHistory
                {
                    Items = new List<SabnzbdHistoryItem>()
                    {
                        new SabnzbdHistoryItem
                        {
                            Status = SabnzbdDownloadStatus.Completed,
                            Size = 1000,
                            Category = "tv",
                            Id = "sabnzbd_nzb12345",
                            Title = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                            Storage = "/remote/mount/vv/Droned.S01E01.Pilot.1080p.WEB-DL-DRONE"
                        }
                    }
                };

            _config = new SabnzbdConfig
                {
                    Misc = new SabnzbdConfigMisc
                        {
                            complete_dir = @"/remote/mount"
                        },
                    Categories = new List<SabnzbdCategory>
                        {
                            new SabnzbdCategory  { Name = "tv", Dir = "vv" }
                        }
                };

            _downloadClientItem = Builder<DownloadClientItem>
                                  .CreateNew()
                                  .With(d => d.Status = DownloadItemStatus.Completed)
                                  .With(d => d.DownloadId = _completed.Items.First().Id)
                                  .Build();

            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns("1.2.3");

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetConfig(It.IsAny<SabnzbdSettings>()))
                .Returns(_config);

            _fullStatus = new SabnzbdFullStatus
                {
                    CompleteDir = @"Y:\nzbget\root\complete".AsOsAgnostic()
                };

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetFullStatus(It.IsAny<SabnzbdSettings>()))
                .Returns(_fullStatus);
        }

        protected void GivenVersion(string version)
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetVersion(It.IsAny<SabnzbdSettings>()))
                .Returns(version);
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns((SabnzbdAddResponse)null);
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns(new SabnzbdAddResponse()
                {
                    Status = true,
                    Ids = new List<string> { "sabznbd_nzo12345" }
                });
        }

        protected virtual void GivenQueue(SabnzbdQueue queue)
        {
            if (queue == null)
            {
                queue = new SabnzbdQueue()
                {
                    DefaultRootFolder = _queued.DefaultRootFolder,
                    Items = new List<SabnzbdQueueItem>()
                };
            }

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetQueue(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()))
                .Returns(queue);
        }

        protected virtual void GivenHistory(SabnzbdHistory history)
        {
            if (history == null)
            {
                history = new SabnzbdHistory() { Items = new List<SabnzbdHistoryItem>() };
            }

            Mocker.GetMock<ISabnzbdProxy>()
                .Setup(s => s.GetHistory(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SabnzbdSettings>()))
                .Returns(history);
        }

        [Test]
        public void GetItems_should_return_no_items_when_queue_is_empty()
        {
            GivenQueue(null);
            GivenHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [TestCase(SabnzbdDownloadStatus.Grabbing)]
        [TestCase(SabnzbdDownloadStatus.Queued)]
        public void queued_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyQueued(result);

            result.RemainingTime.Should().NotBe(TimeSpan.Zero);
            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [TestCase(SabnzbdDownloadStatus.Paused)]
        public void paused_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyPaused(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [TestCase(SabnzbdDownloadStatus.Checking)]
        [TestCase(SabnzbdDownloadStatus.Downloading)]
        [TestCase(SabnzbdDownloadStatus.QuickCheck)]
        [TestCase(SabnzbdDownloadStatus.Verifying)]
        [TestCase(SabnzbdDownloadStatus.Repairing)]
        [TestCase(SabnzbdDownloadStatus.Fetching)]
        [TestCase(SabnzbdDownloadStatus.Extracting)]
        [TestCase(SabnzbdDownloadStatus.Moving)]
        [TestCase(SabnzbdDownloadStatus.Running)]
        public void downloading_item_should_have_required_properties(SabnzbdDownloadStatus status)
        {
            _queued.Items.First().Status = status;

            GivenQueue(_queued);
            GivenHistory(null);

            var result = Subject.GetItems().Single();

            VerifyDownloading(result);

            result.RemainingTime.Should().NotBe(TimeSpan.Zero);
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
            _completed.Items.First().Status = SabnzbdDownloadStatus.Failed;

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            VerifyFailed(result);

            result.CanBeRemoved.Should().BeTrue();
            result.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public void deleted_queue_item_should_be_ignored()
        {
            _queued.Items.First().Status = SabnzbdDownloadStatus.Deleted;

            GivenQueue(_queued);
            GivenHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void deleted_history_item_should_be_ignored()
        {
            _completed.Items.First().Status = SabnzbdDownloadStatus.Deleted;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.GetItems().Should().BeEmpty();
        }

        [TestCase("[ TOWN ]-[ http://www.town.ag ]-[ ANIME ]-[Usenet Provider >> http://www.ssl- <<] - [Commie] Aldnoah Zero 18 [234C8FC7]", "[ TOWN ]-[ http-++www.town.ag ]-[ ANIME ]-[Usenet Provider  http-++www.ssl- ] - [Commie] Aldnoah Zero 18 [234C8FC7].nzb")]
        public void Download_should_use_clean_title(string title, string filename)
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Release.Title = title;

            var id = Subject.Download(remoteEpisode);

            Mocker.GetMock<ISabnzbdProxy>()
                .Verify(v => v.DownloadNzb(It.IsAny<byte[]>(), filename, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SabnzbdSettings>()), Times.Once());
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
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.Items.First().Category = "myowncat";

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }

        [Test]
        public void should_report_diskspace_unpack_error_as_warning()
        {
            _completed.Items.First().FailMessage = "Unpacking failed, write error or disk is full?";
            _completed.Items.First().Status = SabnzbdDownloadStatus.Failed;

            GivenQueue(null);
            GivenHistory(_completed);

            var items = Subject.GetItems();

            items.First().Status.Should().Be(DownloadItemStatus.Warning);
        }

        [Test]
        public void Download_should_use_sabRecentTvPriority_when_recentEpisode_is_true()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                    .Setup(s => s.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), (int)SabnzbdPriority.High, It.IsAny<SabnzbdSettings>()))
                    .Returns(new SabnzbdAddResponse { Ids = new List<string> { "sonarrtest" } });

            var remoteEpisode = CreateRemoteEpisode();
            remoteEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                      .All()
                                                      .With(e => e.AirDate = DateTime.Today.ToString(Episode.AIR_DATE_FORMAT))
                                                      .Build()
                                                      .ToList();

            Subject.Download(remoteEpisode);

            Mocker.GetMock<ISabnzbdProxy>()
                  .Verify(v => v.DownloadNzb(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), (int)SabnzbdPriority.High, It.IsAny<SabnzbdSettings>()), Times.Once());
        }

        [TestCase(@"Droned.S01E01.Pilot.1080p.WEB-DL-DRONE", @"Droned.S01E01_Pilot_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.S01E01.Pilot.1080p.WEB-DL-DRONE", @"SubDir\Droned.S01E01_Pilot_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.mkv", @"SubDir\Droned.S01E01_Pilot_1080p_WEB-DL-DRONE.mkv")]
        [TestCase(@"Droned.S01E01.Pilot.1080p.WEB-DL-DRONE.mkv", @"SubDir\SubDir\Droned.S01E01_Pilot_1080p_WEB-DL-DRONE.mkv")]
        public void should_return_path_to_jobfolder(string title, string storage)
        {
            _completed.Items.First().Title = title;
            _completed.Items.First().Storage = (@"C:\sorted\" + title + @"\" + storage).AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(new OsPath((@"C:\sorted\" + title).AsOsAgnostic()).AsDirectory());
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
        public void should_not_blow_up_if_storage_is_drive_root()
        {
            _completed.Items.First().Storage = @"C:\".AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"C:\".AsOsAgnostic());
        }

        [Test]
        public void should_not_blow_up_if_storage_doesnt_have_jobfolder()
        {
            _completed.Items.First().Storage = @"C:\sorted\somewhere\asdfasdf\asdfasdf.mkv".AsOsAgnostic();

            GivenQueue(null);
            GivenHistory(_completed);

            var result = Subject.GetItems().Single();

            result.OutputPath.Should().Be(@"C:\sorted\somewhere\asdfasdf\asdfasdf.mkv".AsOsAgnostic());
        }

        [TestCase(@"Y:\nzbget\root", @"completed\downloads", @"vv", @"Y:\nzbget\root\completed\downloads", @"Y:\nzbget\root\completed\downloads\vv")]
        [TestCase(@"Y:\nzbget\root", @"completed", @"vv", @"Y:\nzbget\root\completed", @"Y:\nzbget\root\completed\vv")]
        [TestCase(@"/nzbget/root", @"completed/downloads", @"vv", @"/nzbget/root/completed/downloads", @"/nzbget/root/completed/downloads/vv")]
        [TestCase(@"/nzbget/root", @"completed", @"vv", @"/nzbget/root/completed", @"/nzbget/root/completed/vv")]
        public void should_return_status_with_outputdir_for_version_lt_2(string rootFolder, string completeDir, string categoryDir, string fullCompleteDir, string fullCategoryDir)
        {
            _fullStatus.CompleteDir = null;
            _queued.DefaultRootFolder = rootFolder;
            _config.Misc.complete_dir = completeDir;
            _config.Categories.First().Dir = categoryDir;

            GivenVersion("1.2.1");
            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(fullCategoryDir);
        }

        [TestCase(@"Y:\nzbget\root", @"completed\downloads", @"vv", @"Y:\nzbget\root\completed\downloads", @"Y:\nzbget\root\completed\downloads\vv")]
        [TestCase(@"Y:\nzbget\root", @"completed", @"vv", @"Y:\nzbget\root\completed", @"Y:\nzbget\root\completed\vv")]
        [TestCase(@"/nzbget/root", @"completed/downloads", @"vv", @"/nzbget/root/completed/downloads", @"/nzbget/root/completed/downloads/vv")]
        [TestCase(@"/nzbget/root", @"completed", @"vv", @"/nzbget/root/completed", @"/nzbget/root/completed/vv")]
        public void should_return_status_with_outputdir_for_version_gte_2(string rootFolder, string completeDir, string categoryDir, string fullCompleteDir, string fullCategoryDir)
        {
            _fullStatus.CompleteDir = fullCompleteDir;
            _queued.DefaultRootFolder = null;
            _config.Misc.complete_dir = completeDir;
            _config.Categories.First().Dir = categoryDir;

            GivenVersion("2.0.0beta1");
            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(fullCategoryDir);
        }

        [Test]
        public void should_return_status_with_mounted_outputdir()
        {
            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal("127.0.0.1", It.IsAny<OsPath>()))
                .Returns(new OsPath(@"O:\mymount".AsOsAgnostic()));

            GivenQueue(null);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"O:\mymount".AsOsAgnostic());
        }

        [TestCase("0.6.9", false)]
        [TestCase("0.7.0", true)]
        [TestCase("0.8.0", true)]
        [TestCase("1.0.0", true)]
        [TestCase("1.0.0RC1", true)]
        [TestCase("1.1.x", true)]
        public void should_test_version(string version, bool expected)
        {
            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns(version);

            var error = Subject.Test();

            error.IsValid.Should().Be(expected);
        }

        [Test]
        public void should_test_develop_version_successfully()
        {
            Mocker.GetMock<ISabnzbdProxy>()
                  .Setup(v => v.GetVersion(It.IsAny<SabnzbdSettings>()))
                  .Returns("develop");

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
            result.HasWarnings.Should().BeTrue();
        }

        [Test]
        public void should_test_success_if_tv_sorting_disabled()
        {
            _config.Misc.enable_tv_sorting = false;
            _config.Misc.tv_categories = null;

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_null()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = null;

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_empty()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new string[0];

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_success_if_tv_sorting_contains_different_category()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "tv-custom" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_contains_category()
        {
            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "tv" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_test_failed_if_tv_sorting_default_category()
        {
            Subject.Definition.Settings.As<SabnzbdSettings>().TvCategory = null;

            _config.Misc.enable_tv_sorting = true;
            _config.Misc.tv_categories = new[] { "Default" };

            var result = new NzbDroneValidationResult(Subject.Test());

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void should_remove_output_path_folder_when_deleting_a_completed_item_and_delete_data_is_true()
        {
            var path = @"C:\Test\Series.Title.S01E01".AsOsAgnostic();
            _downloadClientItem.OutputPath = new OsPath(path);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(path))
                  .Returns(true);

            _completed.Items.First().Storage = path;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.RemoveItem(_downloadClientItem, true);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(path, true), Times.Once);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(path), Times.Never);
        }

        [Test]
        public void should_remove_output_path_file_when_deleting_a_completed_item_and_delete_data_is_true()
        {
            var path = @"C:\Test\Series.Title.S01E01.mkv".AsOsAgnostic();
            _downloadClientItem.OutputPath = new OsPath(path);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(path))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(path))
                  .Returns(true);

            _completed.Items.First().Storage = path;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.RemoveItem(_downloadClientItem, true);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(path, true), Times.Never);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(path), Times.Once);
        }

        [Test]
        public void should_not_remove_output_path_file_when_deleting_a_completed_item_and_delete_data_is_true_if_it_does_not_exist()
        {
            var path = @"C:\Test\Series.Title.S01E01.mkv".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(path))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(path))
                  .Returns(false);

            _completed.Items.First().Storage = path;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.RemoveItem(_downloadClientItem, true);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(path, true), Times.Never);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(path), Times.Never);
        }

        [Test]
        public void should_not_remove_output_path_file_when_deleting_a_completed_item_and_delete_data_is_false()
        {
            var path = @"C:\Test\Series.Title.S01E01.mkv".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(path))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(path))
                  .Returns(false);

            _completed.Items.First().Storage = path;

            GivenQueue(null);
            GivenHistory(_completed);

            Subject.RemoveItem(_downloadClientItem, false);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FolderExists(path), Times.Never);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.FileExists(path), Times.Never);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFolder(path, true), Times.Never);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.DeleteFile(path), Times.Never);
        }
    }
}
