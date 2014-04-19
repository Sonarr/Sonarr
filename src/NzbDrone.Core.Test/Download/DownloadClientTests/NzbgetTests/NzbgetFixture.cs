using System;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetTests
{
    [TestFixture]
    public class NzbgetFixture : DownloadClientFixtureBase<Nzbget>
    {
        private NzbgetQueueItem _queued;
        private NzbgetHistoryItem _failed;
        private NzbgetHistoryItem _completed;

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new NzbgetSettings
                                          {
                                              Host = "192.168.5.55",
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
                    ParStatus = "Some Error"
                };

            _completed = new NzbgetHistoryItem
                {
                    FileSizeLo = 1000,
                    Category = "tv",
                    Name = "Droned.S01E01.Pilot.1080p.WEB-DL-DRONE",
                    DestDir = "somedirectory",
                    Parameters = new List<NzbgetParameter> { new NzbgetParameter { Name = "drone", Value = "id" } },
                    ParStatus = "SUCCESS",
                    ScriptStatus = "NONE"
                };
        }

        protected void WithFailedDownload()
        {
            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<Stream>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<int>(), It.IsAny<NzbgetSettings>()))
                .Returns((String)null);
        }

        protected void WithSuccessfulDownload()
        {
            Mocker.GetMock<INzbgetProxy>()
                .Setup(s => s.DownloadNzb(It.IsAny<Stream>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<int>(), It.IsAny<NzbgetSettings>()))
                .Returns(Guid.NewGuid().ToString().Replace("-", ""));
        }

        protected virtual void WithQueue(NzbgetQueueItem queue)
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

        protected virtual void WithHistory(NzbgetHistoryItem history)
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
            WithQueue(null);
            WithHistory(null);

            Subject.GetItems().Should().BeEmpty();
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            _queued.ActiveDownloads = 0;

            WithQueue(_queued);
            WithHistory(null);
            
            var result = Subject.GetItems().Single();

            VerifyQueued(result);
        }

        [Test]
        public void paused_item_should_have_required_properties()
        {
            _queued.PausedSizeLo = _queued.FileSizeLo;

            WithQueue(_queued);
            WithHistory(null);

            var result = Subject.GetItems().Single();

            VerifyPaused(result);
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            _queued.ActiveDownloads = 1;

            WithQueue(_queued);
            WithHistory(null);

            var result = Subject.GetItems().Single();

            VerifyDownloading(result);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            WithQueue(null);
            WithHistory(_completed);

            var result = Subject.GetItems().Single();

            VerifyCompleted(result);
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            WithQueue(null);
            WithHistory(_failed);

            var result = Subject.GetItems().Single();

            VerifyFailed(result);
        }

        [Test]
        public void Download_should_return_unique_id()
        {
            WithSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = Subject.Download(remoteEpisode);

            id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetItems_should_ignore_downloads_from_other_categories()
        {
            _completed.Category = "mycat";

            WithQueue(null);
            WithHistory(_completed);

            var items = Subject.GetItems();

            items.Should().BeEmpty();
        }
    }
}
