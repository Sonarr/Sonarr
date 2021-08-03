using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Transmission;
using NzbDrone.Core.MediaFiles.TorrentInfo;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.TransmissionTests
{
    public abstract class TransmissionFixtureBase<TClient> : DownloadClientFixtureBase<TClient>
        where TClient : class, IDownloadClient
    {
        protected TransmissionSettings _settings;
        protected TransmissionTorrent _queued;
        protected TransmissionTorrent _downloading;
        protected TransmissionTorrent _failed;
        protected TransmissionTorrent _completed;
        protected TransmissionTorrent _magnet;
        protected Dictionary<string, object> _transmissionConfigItems;

        [SetUp]
        public void Setup()
        {
            _settings = new TransmissionSettings
            {
                Host = "127.0.0.1",
                Port = 2222,
                Username = "admin",
                Password = "pass"
            };

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = _settings;

            _queued = new TransmissionTorrent
            {
                HashString = "HASH",
                IsFinished = false,
                Status = TransmissionTorrentStatus.Queued,
                Name = _title,
                TotalSize = 1000,
                LeftUntilDone = 1000,
                DownloadDir = "somepath"
            };

            _downloading = new TransmissionTorrent
            {
                HashString = "HASH",
                IsFinished = false,
                Status = TransmissionTorrentStatus.Downloading,
                Name = _title,
                TotalSize = 1000,
                LeftUntilDone = 100,
                DownloadDir = "somepath"
            };

            _failed = new TransmissionTorrent
            {
                HashString = "HASH",
                IsFinished = false,
                Status = TransmissionTorrentStatus.Stopped,
                Name = _title,
                TotalSize = 1000,
                LeftUntilDone = 100,
                ErrorString = "Error",
                DownloadDir = "somepath"
            };

            _completed = new TransmissionTorrent
            {
                HashString = "HASH",
                IsFinished = true,
                Status = TransmissionTorrentStatus.Seeding,
                Name = _title,
                TotalSize = 1000,
                LeftUntilDone = 0,
                DownloadDir = "somepath",
                DownloadedEver = 1000,
                UploadedEver = 900
            };

            _magnet = new TransmissionTorrent
            {
                HashString = "HASH",
                IsFinished = false,
                Status = TransmissionTorrentStatus.Downloading,
                Name = _title,
                TotalSize = 0,
                LeftUntilDone = 100,
                DownloadDir = "somepath"
            };

            Mocker.GetMock<ITorrentFileInfoReader>()
                  .Setup(s => s.GetHashFromTorrentFile(It.IsAny<byte[]>()))
                  .Returns("CBC2F069FE8BB2F544EAE707D75BCD3DE9DCF951");

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));

            _transmissionConfigItems = new Dictionary<string, object>();

            _transmissionConfigItems.Add("download-dir", @"C:/Downloads/Finished/transmission");
            _transmissionConfigItems.Add("incomplete-dir", null);
            _transmissionConfigItems.Add("incomplete-dir-enabled", false);

            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(v => v.GetConfig(It.IsAny<TransmissionSettings>()))
                  .Returns(() => Json.Deserialize<TransmissionConfig>(_transmissionConfigItems.ToJson()));
        }

        protected void GivenTvCategory()
        {
            _settings.TvCategory = "sonarr";
        }

        protected void GivenTvDirectory()
        {
            _settings.TvDirectory = @"C:/Downloads/Finished/sonarr";
        }

        protected void GivenFailedDownload()
        {
            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransmissionSettings>()))
                  .Throws<InvalidOperationException>();
        }

        protected void GivenSuccessfulDownload()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[1000]));

            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.AddTorrentFromUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransmissionSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);

            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.AddTorrentFromData(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<TransmissionSettings>()))
                  .Callback(PrepareClientToReturnQueuedItem);
        }

        protected virtual void GivenTorrents(List<TransmissionTorrent> torrents)
        {
            if (torrents == null)
            {
                torrents = new List<TransmissionTorrent>();
            }

            Mocker.GetMock<ITransmissionProxy>()
                  .Setup(s => s.GetTorrents(It.IsAny<TransmissionSettings>()))
                  .Returns(torrents);
        }

        protected void PrepareClientToReturnQueuedItem()
        {
            GivenTorrents(new List<TransmissionTorrent>
            {
                _queued
            });
        }

        protected void PrepareClientToReturnDownloadingItem()
        {
            GivenTorrents(new List<TransmissionTorrent>
            {
                _downloading
            });
        }

        protected void PrepareClientToReturnFailedItem()
        {
            GivenTorrents(new List<TransmissionTorrent>
            {
                _failed
            });
        }

        protected void PrepareClientToReturnCompletedItem(bool stopped = false, double ratio = 0.9, int seedingTime = 60, double? ratioLimit = null, int? idleLimit = null)
        {
            if (stopped)
            {
                _completed.Status = TransmissionTorrentStatus.Stopped;
            }

            _completed.UploadedEver = (int)(_completed.DownloadedEver * ratio);
            _completed.SecondsSeeding = seedingTime * 60;

            if (ratioLimit.HasValue)
            {
                if (double.IsPositiveInfinity(ratioLimit.Value))
                {
                    _completed.SeedRatioMode = 2;
                }
                else
                {
                    _completed.SeedRatioMode = 1;
                    _completed.SeedRatioLimit = ratioLimit.Value;
                }
            }

            if (idleLimit.HasValue)
            {
                if (double.IsPositiveInfinity(idleLimit.Value))
                {
                    _completed.SeedIdleMode = 2;
                }
                else
                {
                    _completed.SeedIdleMode = 1;
                    _completed.SeedIdleLimit = idleLimit.Value;
                }
            }

            GivenTorrents(new List<TransmissionTorrent>
            {
                _completed
            });
        }

        protected void PrepareClientToReturnMagnetItem()
        {
            GivenTorrents(new List<TransmissionTorrent>
            {
                _magnet
            });
        }

        protected void GivenGlobalSeedLimits(double? ratioLimit = null, int? idleLimit = null)
        {
            _transmissionConfigItems["seedRatioLimited"] = ratioLimit.HasValue;
            if (ratioLimit.HasValue)
            {
                _transmissionConfigItems["seedRatioLimit"] = ratioLimit.Value;
            }

            _transmissionConfigItems["idle-seeding-limit-enabled"] = idleLimit.HasValue;
            if (idleLimit.HasValue)
            {
                _transmissionConfigItems["idle-seeding-limit"] = idleLimit.Value;
            }
        }
    }
}
