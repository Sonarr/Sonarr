using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Primitives;
using Moq;
using NLog.LayoutRenderers;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.LibTorrent.Models;
using NzbDrone.Core.Download.Clients.Porla;
using NzbDrone.Core.Download.Clients.Porla.Models;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.PorlaTests
{
    [TestFixture]
    public class PorlaFixture : DownloadClientFixtureBase<Porla>
    {
        protected PorlaTorrentDetail _queued;
        protected PorlaTorrentDetail _downloading;
        protected PorlaTorrentDetail _failed;
        protected PorlaTorrentDetail _completed;

        private static readonly IList<string> SomeTags = new  List<string> { "sometag", "someothertag" };

        [SetUp]
        public void Setup()
        {
            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new PorlaSettings();

            _queued = new PorlaTorrentDetail
            {
                ActiveDuration = 0L,
                AllTimeDownload = 0L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0L,
                Error = null,
                ETA = 0L,
                FinishedDuration = 0L,
                Flags = new (null),
                InfoHash = new ("HASH", null),
                LastDownload = 0L,
                LastUpload = 0L,
                ListPeers = 0L,
                ListSeeds = 0L,
                Metadata = new (null),
                MovingStorage = false,
                Name = _title,
                NumPeers = 0L,
                NumSeeds = 0L,
                Progress = 0.0d,
                QueuePosition = -1,
                Ratio = 0.0d,
                SavePath = "somepath",
                SeedingDuration = 0L,
                Session = "",
                Size = 1000L,
                State = LibTorrentStatus.checking_files, // check this one
                Tags = new (SomeTags),  // do we have a remte episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 0L,
                UploadRate = 0L
            };

            _downloading = new PorlaTorrentDetail
            {
                ActiveDuration = 0L,
                AllTimeDownload = 0L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0L,
                Error = null,
                ETA = 0L,
                FinishedDuration = 0L,
                Flags = new (null),
                InfoHash = new ("HASH", null),
                LastDownload = 0L,
                LastUpload = 0L,
                ListPeers = 0L,
                ListSeeds = 0L,
                Metadata = new (null),
                MovingStorage = false,
                Name = _title,
                NumPeers = 0L,
                NumSeeds = 0L,
                Progress = 0.0d,
                QueuePosition = -1,
                Ratio = 0.0d,
                SavePath = "somepath",
                SeedingDuration = 0L,
                Session = "",
                Size = 1000L,
                State = LibTorrentStatus.checking_files, // check this one
                Tags = new (SomeTags),  // do we have a remte episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 0L,
                UploadRate = 0L
            };

            _failed = new PorlaTorrentDetail
            {
                ActiveDuration = 0L,
                AllTimeDownload = 0L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0L,
                Error = null,
                ETA = 0L,
                FinishedDuration = 0L,
                Flags = new (null),
                InfoHash = new ("HASH", null),
                LastDownload = 0L,
                LastUpload = 0L,
                ListPeers = 0L,
                ListSeeds = 0L,
                Metadata = new (null),
                MovingStorage = false,
                Name = _title,
                NumPeers = 0L,
                NumSeeds = 0L,
                Progress = 0.0d,
                QueuePosition = -1,
                Ratio = 0.0d,
                SavePath = "somepath",
                SeedingDuration = 0L,
                Session = "",
                Size = 1000L,
                State = LibTorrentStatus.checking_files, // check this one
                Tags = new (SomeTags),  // do we have a remte episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 0L,
                UploadRate = 0L
            };

            _completed = new PorlaTorrentDetail
            {
                ActiveDuration = 0L,
                AllTimeDownload = 0L,
                AllTimeUpload = 0L,
                Category = "sonarr-tv",
                DownloadRate = 0L,
                Error = null,
                ETA = 0L,
                FinishedDuration = 0L,
                Flags = new (null),
                InfoHash = new ("HASH", null),
                LastDownload = 0L,
                LastUpload = 0L,
                ListPeers = 0L,
                ListSeeds = 0L,
                Metadata = new (null),
                MovingStorage = false,
                Name = _title,
                NumPeers = 0L,
                NumSeeds = 0L,
                Progress = 0.0d,
                QueuePosition = -1,
                Ratio = 0.0d,
                SavePath = "somepath",
                SeedingDuration = 0L,
                Session = "",
                Size = 1000L,
                State = LibTorrentStatus.checking_files, // check this one
                Tags = new (SomeTags),  // do we have a remte episode aviable? can I use CreateRemoteEpisode
                Total = 0L,
                TotalDone = 0L,
                UploadRate = 0L
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Array.Empty<byte>()));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddMagnetTorrent(It.IsAny<PorlaSettings>(), It.IsAny<string>(), SomeTags))
                .Throws<InvalidOperationException>();

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<PorlaSettings>(), It.IsAny<byte[]>(), SomeTags))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.GetAsync(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => Task.FromResult(new HttpResponse(r, new HttpHeader(), new byte[1000])));

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddMagnetTorrent(It.IsAny<PorlaSettings>(), It.IsAny<string>(), SomeTags))
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.AddTorrentFile(It.IsAny<PorlaSettings>(), It.IsAny<byte[]>(), SomeTags))
                .Returns(new PorlaTorrent("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951", null))
                .Callback(PrepareClientToReturnQueuedItem);
        }

        protected virtual void GivenTorrents(ReadOnlyCollection<PorlaTorrentDetail> torrents)
        {
            if (torrents == null)
            {
                torrents = new ReadOnlyCollection<PorlaTorrentDetail>();
            }

            Mocker.GetMock<IPorlaProxy>()
                .Setup(s => s.ListTorrents(It.IsAny<PorlaSettings>()))
                .Returns(torrents.ToArray());
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrent>
                {
                    _queued
                });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrent>
                {
                    _downloading
                });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrent>
                {
                    _failed
                });
        }

        protected void PrepareClientToReturnCompletedItem()
        {
            GivenTorrents(new ReadOnlyCollection<PorlaTorrent>
                {
                    _completed
                });
        }

        [Test]
        public void queued_item_should_have_required_properties()
        {
            PrepareClientToReturnQueuedItem();
            var item = Subject.GetItems().Single();
            VerifyQueued(item);
        }

        [Test]
        public void downloading_item_should_have_required_properties()
        {
            PrepareClientToReturnDownloadingItem();
            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
        }

        [Test]
        public void failed_item_should_have_required_properties()
        {
            PrepareClientToReturnFailedItem();
            var item = Subject.GetItems().Single();
            VerifyWarning(item);
        }

        [Test]
        public void completed_download_should_have_required_properties()
        {
            PrepareClientToReturnCompletedItem();
            var item = Subject.GetItems().Single();
            VerifyCompleted(item);

            item.CanBeRemoved.Should().BeTrue();
            item.CanMoveFiles.Should().BeTrue();
        }

        [Test]
        public async Task Download_should_return_unique_id()
        {
            GivenSuccessfulDownload();

            var remoteEpisode = CreateRemoteEpisode();

            var id = await Subject.Download(remoteEpisode, CreateIndexer());

            id.Should().NotBeNullOrEmpty();
        }

        // TODO: figure out presets
        [Test]
        public void should_return_status_with_outputdirs()
        {
            var configItems = new Dictionary<string, object>();

            configItems.Add("bittorrent.defaultSavePath", @"C:\Downloads\Downloading\deluge".AsOsAgnostic());

            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.GetSessionSettings(It.IsAny<PorlaSettings>()))
                .Returns(configItems);

            var result = Subject.GetStatus();

            result.IsLocalhost.Should().BeTrue();
            result.OutputRootFolders.Should().NotBeNull();
            result.OutputRootFolders.First().Should().Be(@"C:\Downloads\Downloading\deluge".AsOsAgnostic());
        }

        [Test]
        public void GetItems_should_ignore_torrents_with_a_different_category()
        {
            var torrent = new PorlaTorrent
            {
                InfoHash = "hash",
                IsFinished = true,
                State = PorlaTorrentState.Paused,
                Name = _title,
                TotalSize = 1000,
                DownloadedBytes = 1000,
                Progress = 100.0,
                SavePath = "somepath",
                Label = "sonarr-tv-other"
            };

            var torrents = new PorlaTorrent[] { torrent };
            Mocker.GetMock<IPorlaProxy>()
                .Setup(v => v.ListTorrents(It.IsAny<PorlaSettings>(), 0, long.MaxValue))
                .Returns(torrents);

            Subject.GetItems().Should().BeEmpty();
        }

        // We are not incompatible yet, when we are, you can uncomment this
        // [Test]
        // public void Test_should_return_validation_failure_for_old_Porla()
        // {
        //     var systemInfo = new PorlaSysVersions()
        //     {
        //         Porla = new PorlaSysVersionsPorla()
        //         {
        //             Version = "0.37.0"
        //         }
        //     };
        //
        //     Mocker.GetMock<IPorlaProxy>()
        //        .Setup(v => v.GetSysVersion(It.IsAny<PorlaSettings>()))
        //        .Returns(systemInfo);
        //
        //     var result = Subject.Test();
        //
        //     result.Errors.Count.Should().Be(1);
        // }
    }
}
