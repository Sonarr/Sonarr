using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation;
using NzbDrone.Core.MediaFiles.EpisodeImport.Manual;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Manual
{
    [TestFixture]
    public class ManualImportCompletionFixture : CoreTest<ManualImportService>
    {
        private List<Series> _series;
        private List<DownloadCompletedEvent> _completedEvents;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateListOfSize(2)
                                     .All()
                                     .With(s => s.Path = null)
                                     .TheFirst(1)
                                     .With(s => s.Path = @"C:\Test\TV\Series One".AsOsAgnostic())
                                     .TheNext(1)
                                     .With(s => s.Path = @"C:\Test\TV\Series Two".AsOsAgnostic())
                                     .BuildList();

            _completedEvents = new List<DownloadCompletedEvent>();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns<int>(id => _series.Single(s => s.Id == id));

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<IEnumerable<int>>()))
                  .Returns<IEnumerable<int>>(ids => ids.Select(id => new Episode { Id = id, SeriesId = id }).ToList());

            Mocker.GetMock<IAggregationService>()
                  .Setup(s => s.Augment(It.IsAny<LocalEpisode>(), It.IsAny<DownloadClientItem>()))
                  .Returns<LocalEpisode, DownloadClientItem>((localEpisode, _) => localEpisode);

            Mocker.GetMock<IImportApprovedEpisodes>()
                  .Setup(s => s.Import(It.IsAny<List<ImportDecision>>(), It.IsAny<bool>(), It.IsAny<DownloadClientItem>(), It.IsAny<ImportMode>()))
                  .Returns<List<ImportDecision>, bool, DownloadClientItem, ImportMode>((decisions, _, _, _) =>
                      decisions.Select(d => new ImportResult(d, new EpisodeFile { SeriesId = d.LocalEpisode.Series.Id })).ToList());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IEventAggregator>()
                  .Setup(s => s.PublishEvent(It.IsAny<DownloadCompletedEvent>()))
                  .Callback<DownloadCompletedEvent>(e => _completedEvents.Add(e));
        }

        private void GivenTrackedDownload(string downloadId)
        {
            var downloadItem = new DownloadClientItem
            {
                DownloadId = downloadId,
                Title = downloadId,
                OutputPath = new OsPath(@"C:\Test\Downloads\".AsOsAgnostic() + downloadId)
            };

            var trackedDownload = new TrackedDownload
            {
                DownloadItem = downloadItem,
                ImportItem = downloadItem,
                RemoteEpisode = new RemoteEpisode { Episodes = new List<Episode> { new Episode() } }
            };

            Mocker.GetMock<ITrackedDownloadService>()
                  .Setup(s => s.Find(downloadId))
                  .Returns(trackedDownload);
        }

        private ManualImportFile GivenFile(int seriesId, string downloadId)
        {
            return new ManualImportFile
            {
                Path = @"C:\Test\Downloads\".AsOsAgnostic() + downloadId + "/Series.S01E01.720p.HDTV-Sonarr.mkv",
                SeriesId = seriesId,
                EpisodeIds = new List<int> { seriesId },
                Quality = new QualityModel(Quality.HDTV720p),
                DownloadId = downloadId
            };
        }

        [Test]
        public void should_publish_download_completed_with_the_series_each_download_was_imported_into()
        {
            GivenTrackedDownload("download1");
            GivenTrackedDownload("download2");

            Subject.Execute(new ManualImportCommand
            {
                Files = new List<ManualImportFile>
                {
                    GivenFile(1, "download1"),
                    GivenFile(2, "download2")
                },
                ImportMode = ImportMode.Auto
            });

            _completedEvents.Should().HaveCount(2);
            _completedEvents.Single(e => e.TrackedDownload.DownloadItem.DownloadId == "download1").Series.Id.Should().Be(1);
            _completedEvents.Single(e => e.TrackedDownload.DownloadItem.DownloadId == "download2").Series.Id.Should().Be(2);
        }
    }
}
