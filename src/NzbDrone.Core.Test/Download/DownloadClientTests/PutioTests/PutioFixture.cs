using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Putio;
using NzbDrone.Core.MediaFiles.TorrentInfo;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.PutioTests
{
    public class PutioFixture : DownloadClientFixtureBase<Putio>
    {
        private PutioSettings _settings;
        private PutioTorrent _queued;
        private PutioTorrent _downloading;
        private PutioTorrent _failed;
        private PutioTorrent _completed;
        private PutioTorrent _completed_different_parent;
        private PutioTorrent _seeding;

        [SetUp]
        public void Setup()
        {
            _settings = new PutioSettings();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = _settings;

            _queued = new PutioTorrent
                {
                    Hash = "HASH",
                    Id = 1,
                    Status = PutioTorrentStatus.InQueue,
                    Name = _title,
                    Size = 1000,
                    Downloaded = 0,
                    SaveParentId = 1
                };

            _downloading = new PutioTorrent
                {
                    Hash = "HASH",
                    Id = 2,
                    Status = PutioTorrentStatus.Downloading,
                    Name = _title,
                    Size = 1000,
                    Downloaded = 980,
                    SaveParentId = 1,
                };

            _failed = new PutioTorrent
                {
                    Hash = "HASH",
                    Id = 3,
                    Status = PutioTorrentStatus.Error,
                    ErrorMessage = "Torrent has reached the maximum number of inactive days.",
                    Name = _title,
                    Size = 1000,
                    Downloaded = 980,
                    SaveParentId = 1,
                };

            _completed = new PutioTorrent
                {
                    Hash = "HASH",
                    Status = PutioTorrentStatus.Completed,
                    Id = 4,
                    Name = _title,
                    Size = 1000,
                    Downloaded = 1000,
                    SaveParentId = 1,
                    FileId = 1
                };

            _completed_different_parent = new PutioTorrent
                {
                    Hash = "HASH",
                    Id = 5,
                    Status = PutioTorrentStatus.Completed,
                    Name = _title,
                    Size = 1000,
                    Downloaded = 1000,
                    SaveParentId = 2,
                    FileId = 1
                };

            _seeding = new PutioTorrent
                {
                    Hash = "HASH",
                    Id = 6,
                    Status = PutioTorrentStatus.Seeding,
                    Name = _title,
                    Size = 1000,
                    Downloaded = 1000,
                    Uploaded = 1300,
                    SaveParentId = 1,
                    FileId = 2
                };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Array.Empty<byte>()));

            Mocker.GetMock<IPutioProxy>()
                .Setup(v => v.GetAccountSettings(It.IsAny<PutioSettings>()));
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<PutioSettings>()))
                .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));
            /*
            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<PutioSettings>()))
                .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.AddTorrentFromData(It.IsAny<byte[]>(), It.IsAny<PutioSettings>()))
                .Callback(PrepareClientToReturnQueuedItem);
            */
        }

        protected virtual void GivenTorrents(List<PutioTorrent> torrents)
        {
            torrents ??= new List<PutioTorrent>();

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.GetTorrents(It.IsAny<PutioSettings>()))
                .Returns(torrents);
        }

        protected virtual void GivenMetadata(List<PutioTorrentMetadata> metadata)
        {
            metadata ??= new List<PutioTorrentMetadata>();
            var result = new Dictionary<string, PutioTorrentMetadata>();
            foreach (var item in metadata)
            {
                result.Add(item.Id.ToString(), item);
            }

            Mocker.GetMock<IPutioProxy>()
                .Setup(s => s.GetAllTorrentMetadata(It.IsAny<PutioSettings>()))
                .Returns(result);
        }

        [Test]
        public void getItems_contains_all_items()
        {
            GivenTorrents(new List<PutioTorrent>
            {
                _queued,
                _downloading,
                _failed,
                _completed,
                _seeding,
                _completed_different_parent
            });
            GivenMetadata(new List<PutioTorrentMetadata>
            {
                PutioTorrentMetadata.fromTorrent(_completed, true),
                PutioTorrentMetadata.fromTorrent(_seeding, true),
                PutioTorrentMetadata.fromTorrent(_completed_different_parent, true),
            });

            var items = Subject.GetItems();

            VerifyQueued(items.ElementAt(0));
            VerifyDownloading(items.ElementAt(1));
            VerifyWarning(items.ElementAt(2));
            VerifyCompleted(items.ElementAt(3));
            VerifyCompleted(items.ElementAt(4));
            VerifyCompleted(items.ElementAt(5));

            items.Should().HaveCount(6);
        }

        [TestCase(1, 5)]
        [TestCase(2, 1)]
        [TestCase(3, 0)]
        public void getItems_contains_only_items_with_matching_parent_id(long configuredParentId, int expectedCount)
        {
            GivenTorrents(new List<PutioTorrent>
            {
                    _queued,
                    _downloading,
                    _failed,
                    _completed,
                    _seeding,
                    _completed_different_parent
            });

            _settings.SaveParentId = configuredParentId.ToString();

            Subject.GetItems().Should().HaveCount(expectedCount);
        }

        [TestCase("WAITING", DownloadItemStatus.Queued)]
        [TestCase("PREPARING_DOWNLOAD", DownloadItemStatus.Queued)]
        [TestCase("COMPLETED", DownloadItemStatus.Completed)]
        [TestCase("COMPLETING", DownloadItemStatus.Downloading)]
        [TestCase("DOWNLOADING", DownloadItemStatus.Downloading)]
        [TestCase("ERROR", DownloadItemStatus.Failed)]
        [TestCase("IN_QUEUE", DownloadItemStatus.Queued)]
        [TestCase("SEEDING", DownloadItemStatus.Completed)]
        public void test_getItems_maps_download_status(string given, DownloadItemStatus expectedItemStatus)
        {
            _queued.Status = given;

            GivenTorrents(new List<PutioTorrent>
            {
                    _queued
            });
            GivenMetadata(new List<PutioTorrentMetadata> { PutioTorrentMetadata.fromTorrent(_queued, true) });

            var item = Subject.GetItems().Single();

            item.Status.Should().Be(expectedItemStatus);
        }

        [Test]
        public void test_getItems_marks_non_existing_local_download_as_downloading()
        {
            GivenTorrents(new List<PutioTorrent> { _completed });
            GivenMetadata(new List<PutioTorrentMetadata> { PutioTorrentMetadata.fromTorrent(_completed, false) });

            var item = Subject.GetItems().Single();
            VerifyDownloading(item);
        }
    }
}
