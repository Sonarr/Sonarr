using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class CompletedDownloadServiceFixture : CoreTest<CompletedDownloadService>
    {
        private TrackedDownload _trackedDownload;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            var remoteEpisode = BuildRemoteEpisode();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadStage.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteEpisode = remoteEpisode)
                    .Build();


            Mocker.GetMock<IDownloadClient>()
              .SetupGet(c => c.Definition)
              .Returns(new DownloadClientDefinition { Id = 1, Name = "testClient" });

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.Get(It.IsAny<int>()))
                  .Returns(Mocker.GetMock<IDownloadClient>().Object);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                  .Returns(new History.History());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetSeries("Drone.S01E01.HDTV"))
                  .Returns(remoteEpisode.Series);

        }

        private RemoteEpisode BuildRemoteEpisode()
        {
            return new RemoteEpisode
            {
                Series = new Series(),
                Episodes = new List<Episode> { new Episode { Id = 1 } }
            };
        }


        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.MostRecentForDownloadId(_trackedDownload.DownloadItem.DownloadId))
                .Returns((History.History)null);
        }

        private void GivenSuccessfulImport()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision(new LocalEpisode() { Path = @"C:\TestPath\Droned.S01E01.mkv" }))
                    });
        }


        private void GivenABadlyNamedDownload()
        {
            _trackedDownload.DownloadItem.DownloadId = "1234";
            _trackedDownload.DownloadItem.Title = "Droned Pilot"; // Set a badly named download
            Mocker.GetMock<IHistoryService>()
               .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")))
               .Returns(new History.History() { SourceTitle = "Droned S01E01" });

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.GetSeries(It.IsAny<string>()))
               .Returns((Series)null);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetSeries("Droned S01E01"))
                .Returns(BuildRemoteEpisode().Series);
        }

        private void GivenSeriesMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetSeries(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteEpisode.Series);
        }

        [TestCase(DownloadItemStatus.Downloading)]
        [TestCase(DownloadItemStatus.Failed)]
        [TestCase(DownloadItemStatus.Queued)]
        [TestCase(DownloadItemStatus.Paused)]
        [TestCase(DownloadItemStatus.Warning)]
        public void should_not_process_if_download_status_isnt_completed(DownloadItemStatus status)
        {
            _trackedDownload.DownloadItem.Status = status;

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found_and_no_category_specified()
        {
            _trackedDownload.DownloadItem.Category = null;
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_process_if_matching_history_is_not_found_but_category_specified()
        {
            _trackedDownload.DownloadItem.Category = "tv";
            GivenNoGrabbedHistory();
            GivenSeriesMatch();
            GivenSuccessfulImport();

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_process_if_output_path_is_empty()
        {
            _trackedDownload.DownloadItem.OutputPath = new OsPath();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"})),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E02.mkv"}))
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E02.mkv"},new Rejection("Rejected!")), "Test Failure")
                           });

            Subject.Process(_trackedDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_no_episodes_were_parsed()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}, new Rejection("Rejected!")), "Test Failure"),

                                new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E02.mkv"},new Rejection("Rejected!")), "Test Failure")
                           });

            _trackedDownload.RemoteEpisode.Episodes.Clear();

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported_but_extra_files_were_not()
        {
            GivenSeriesMatch();

            _trackedDownload.RemoteEpisode.Episodes = new List<Episode>
            {
                new Episode()
            };

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"})),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_mark_as_failed_if_some_of_episodes_were_not_imported()
        {
            _trackedDownload.RemoteEpisode.Episodes = new List<Episode>
            {
                new Episode(),
                new Episode(),
                new Episode()
            };

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"})),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.GetSeries(It.IsAny<int>()))
                  .Returns(BuildRemoteEpisode().Series);

            Subject.Process(_trackedDownload);

            AssertCompletedDownload();
        }

        [Test]
        public void should_not_mark_as_imported_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Mocker.GetMock<IHistoryService>()
            .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_not_import_when_there_is_a_title_mismatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetSeries("Drone.S01E01.HDTV"))
                  .Returns((Series)null);

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload();
        }

        [Test]
        public void should_mark_as_import_title_mismatch_if_ignore_warnings_is_true()
        {
            _trackedDownload.RemoteEpisode.Episodes = new List<Episode>
            {
                new Episode()
            };

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Subject.Process(_trackedDownload, true);

            AssertCompletedDownload();
        }

        [Test]
        public void should_warn_if_path_is_not_valid_for_windows()
        {
            WindowsOnly();

            _trackedDownload.DownloadItem.OutputPath = new OsPath(@"/invalid/Windows/Path");

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        [Test]
        public void should_warn_if_path_is_not_valid_for_linux()
        {
            MonoOnly();

            _trackedDownload.DownloadItem.OutputPath = new OsPath(@"C:\Invalid\Mono\Path");

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport();
        }

        private void AssertNoAttemptedImport()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Verify(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<ImportMode>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()), Times.Never());

            AssertNoCompletedDownload();
        }

        private void AssertNoCompletedDownload()
        {
            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            _trackedDownload.State.Should().NotBe(TrackedDownloadStage.Imported);
        }

        private void AssertCompletedDownload()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Verify(v => v.ProcessPath(_trackedDownload.DownloadItem.OutputPath.FullPath, ImportMode.Auto, _trackedDownload.RemoteEpisode.Series, _trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            _trackedDownload.State.Should().Be(TrackedDownloadStage.Imported);
        }
    }
}
