using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.TorrentRss;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                 new EpisodeHistory()
                {
                     DownloadId = "35238",
                     SourceTitle = "TV Series S01",
                     SeriesId = 5,
                     EpisodeId = 4
                }
                });
        }

        private static DownloadClientDefinition CreateDownloadClient()
        {
            return new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Usenet
            };
        }

        private static DownloadClientItem CreateDownloadItem(DownloadItemStatus status)
        {
            return new DownloadClientItem()
            {
                Title = "TV Series S01E01",
                DownloadId = "35238",
                Category = "sonarr",
                TotalSize = 1000,
                RemainingSize = 500,
                Status = status,
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "NZBGet",
                    Name = "NZBGet",
                    Protocol = DownloadProtocol.Usenet
                }
            };
        }

        private void GivenTrackedDownloadCanBeMapped()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(new RemoteEpisode
                  {
                      Release = new ReleaseInfo { Title = "TV Series S01E01" },
                      Series = new Series() { Id = 5 },
                      Episodes = new List<Episode> { new Episode { Id = 4 } },
                      ParsedEpisodeInfo = new ParsedEpisodeInfo()
                      {
                          SeriesTitle = "TV Series",
                          SeasonNumber = 1,
                          EpisodeNumbers = new[] { 1 }
                      },
                      MappedSeasonNumber = 1
                  });
        }

        [TestCase(DownloadItemStatus.Queued)]
        [TestCase(DownloadItemStatus.Paused)]
        public void should_reuse_stable_waiting_downloading_tracked_download(DownloadItemStatus status)
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(status);
            var updatedItem = CreateDownloadItem(status);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            trackedDownload.State.Should().Be(TrackedDownloadState.Downloading);
            refreshedTrackedDownload.Should().BeSameAs(trackedDownload);
            refreshedTrackedDownload.DownloadItem.Should().BeSameAs(updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Once());

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Once());
        }

        [Test]
        public void should_reprocess_when_waiting_download_starts_downloading()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Downloading);

            Subject.TrackDownload(client, item);
            Subject.TrackDownload(client, updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_identity_changes()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.TotalSize = 2000;

            Subject.TrackDownload(client, item);
            Subject.TrackDownload(client, updatedItem);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_has_warning_status()
        {
            GivenTrackedDownloadCanBeMapped();

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            trackedDownload.Warn("Temporary warning");

            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            refreshedTrackedDownload.Should().NotBeSameAs(trackedDownload);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_reprocess_when_waiting_download_is_not_mapped()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(new RemoteEpisode
                  {
                      ParsedEpisodeInfo = new ParsedEpisodeInfo
                      {
                          SeriesTitle = "TV Series",
                          SeasonNumber = 1,
                          EpisodeNumbers = new[] { 1 }
                      }
                  });

            var client = CreateDownloadClient();
            var item = CreateDownloadItem(DownloadItemStatus.Queued);
            var updatedItem = CreateDownloadItem(DownloadItemStatus.Queued);
            updatedItem.RemainingSize = 250;

            var trackedDownload = Subject.TrackDownload(client, item);
            var refreshedTrackedDownload = Subject.TrackDownload(client, updatedItem);

            refreshedTrackedDownload.Should().NotBeSameAs(trackedDownload);

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.FindByDownloadId(It.IsAny<string>()), Times.Exactly(2));

            Mocker.GetMock<IParsingService>()
                  .Verify(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null), Times.Exactly(2));
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1
                },
                MappedSeasonNumber = 1
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedEpisodeInfo>(i => i.SeasonNumber == 1 && i.SeriesTitle == "TV Series"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteEpisode);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Series.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Series.Id.Should().Be(5);
            trackedDownload.RemoteEpisode.Episodes.First().Id.Should().Be(4);
            trackedDownload.RemoteEpisode.ParsedEpisodeInfo.SeasonNumber.Should().Be(1);
            trackedDownload.RemoteEpisode.MappedSeasonNumber.Should().Be(1);
        }

        [Test]
        public void should_set_indexer()
        {
            var episodeHistory = new EpisodeHistory()
            {
                DownloadId = "35238",
                SourceTitle = "TV Series S01",
                SeriesId = 5,
                EpisodeId = 4,
                EventType = EpisodeHistoryEventType.Grabbed,
            };
            episodeHistory.Data.Add("indexer", "MyIndexer (Prowlarr)");
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                    episodeHistory
                });

            var indexerDefinition = new IndexerDefinition
            {
                Id = 1,
                Name = "MyIndexer (Prowlarr)",
                Settings = new TorrentRssIndexerSettings { MultiLanguages = new List<int> { Language.Original.Id, Language.French.Id } }
            };
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Get(indexerDefinition.Id))
                .Returns(indexerDefinition);
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.All())
                .Returns(new List<IndexerDefinition>() { indexerDefinition });

            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1
                },
                MappedSeasonNumber = 1
            };

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                .Returns(remoteEpisode);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV.Series.S01.MULTi.1080p.WEB.H265-RlsGroup",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Release.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Release.Indexer.Should().Be("MyIndexer (Prowlarr)");
        }

        [Test]
        public void should_parse_as_special_when_source_title_parsing_fails()
        {
            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 0,
                    EpisodeNumbers = new[] { 1 }
                },
                MappedSeasonNumber = 0
            };

            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<EpisodeHistory>()
                {
                 new EpisodeHistory()
                {
                     DownloadId = "35238",
                     SourceTitle = "TV Series Special",
                     SeriesId = 5,
                     EpisodeId = 4
                }
                });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.Is<ParsedEpisodeInfo>(i => i.SeasonNumber == 0 && i.SeriesTitle == "TV Series"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                  .Returns(remoteEpisode);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.ParseSpecialEpisodeTitle(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(remoteEpisode.ParsedEpisodeInfo);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Protocol = client.Protocol,
                    Id = client.Id,
                    Name = client.Name
                }
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Series.Should().NotBeNull();
            trackedDownload.RemoteEpisode.Series.Id.Should().Be(5);
            trackedDownload.RemoteEpisode.Episodes.First().Id.Should().Be(4);
            trackedDownload.RemoteEpisode.ParsedEpisodeInfo.SeasonNumber.Should().Be(0);
            trackedDownload.RemoteEpisode.MappedSeasonNumber.Should().Be(0);
        }

        [Test]
        public void should_unmap_tracked_download_if_episode_deleted()
        {
            GivenDownloadHistory();

            var remoteEpisode = new RemoteEpisode
                                {
                                    Series = new Series() { Id = 5 },
                                    Episodes = new List<Episode> { new Episode { Id = 4 } },
                                    ParsedEpisodeInfo = new ParsedEpisodeInfo()
                                                        {
                                                            SeriesTitle = "TV Series",
                                                            SeasonNumber = 1,
                                                            EpisodeNumbers = new[] { 1 }
                                                        },
                                    MappedSeasonNumber = 0
                                };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(remoteEpisode);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Series - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                                     {
                                         Id = 1,
                                         Type = "Blackhole",
                                         Name = "Blackhole Client",
                                         Protocol = DownloadProtocol.Torrent
                                     }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteEpisode));

            Subject.Handle(new EpisodeInfoRefreshedEvent(remoteEpisode.Series, new List<Episode>(), new List<Episode>(), remoteEpisode.Episodes));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteEpisode.Should().BeNull();
        }

        [Test]
        public void should_not_throw_when_processing_deleted_episodes()
        {
            GivenDownloadHistory();

            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1,
                    EpisodeNumbers = new[] { 1 }
                },
                MappedSeasonNumber = 0
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteEpisode));

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Series - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteEpisode));

            Subject.Handle(new EpisodeInfoRefreshedEvent(remoteEpisode.Series, new List<Episode>(), new List<Episode>(), remoteEpisode.Episodes));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteEpisode.Should().BeNull();
        }

        [Test]
        public void should_update_tracked_download_when_series_edited()
        {
            var originalSeries = new Series { Id = 5, TvdbId = 10, Title = "TV Series" };
            var updatedSeries = new Series { Id = 5, TvdbId = 10, Title = "TV Series Updated" };

            var remoteEpisode = new RemoteEpisode
            {
                Series = originalSeries,
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1,
                    EpisodeNumbers = new[] { 1 }
                }
            };

            var updatedRemoteEpisode = new RemoteEpisode
            {
                Series = updatedSeries,
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1,
                    EpisodeNumbers = new[] { 1 }
                }
            };

            Mocker.GetMock<IParsingService>()
                  .SetupSequence(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(remoteEpisode)
                  .Returns(updatedRemoteEpisode);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem
            {
                Title = "TV Series - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);

            Subject.Handle(new SeriesEditedEvent(updatedSeries, originalSeries));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteEpisode.Should().BeSameAs(updatedRemoteEpisode);
            trackedDownloads.First().RemoteEpisode.Series.Title.Should().Be("TV Series Updated");
        }

        [Test]
        public void should_not_throw_when_processing_deleted_series()
        {
            GivenDownloadHistory();

            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    SeriesTitle = "TV Series",
                    SeasonNumber = 1,
                    EpisodeNumbers = new[] { 1 }
                },
                MappedSeasonNumber = 0
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteEpisode));

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(new List<EpisodeHistory>());

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "TV Series - S01E01",
                DownloadId = "12345",
                DownloadClientInfo = new DownloadClientItemClientInfo
                {
                    Id = 1,
                    Type = "Blackhole",
                    Name = "Blackhole Client",
                    Protocol = DownloadProtocol.Torrent
                }
            };

            Subject.TrackDownload(client, item);
            Subject.GetTrackedDownloads().Should().HaveCount(1);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null))
                  .Returns(default(RemoteEpisode));

            Subject.Handle(new SeriesDeletedEvent(new List<Series> { remoteEpisode.Series }, true, true));

            var trackedDownloads = Subject.GetTrackedDownloads();
            trackedDownloads.Should().HaveCount(1);
            trackedDownloads.First().RemoteEpisode.Should().BeNull();
        }
    }
}
