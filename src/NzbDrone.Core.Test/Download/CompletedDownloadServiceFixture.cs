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
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
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
        private TrackedDownload _trackedMovieDownload;

        [SetUp]
        public void Setup()
        {
            var completed = Builder<DownloadClientItem>.CreateNew()
                                                    .With(h => h.Status = DownloadItemStatus.Completed)
                                                    .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                    .With(h => h.Title = "Drone.S01E01.HDTV")
                                                    .Build();

            var completedMovie = Builder<DownloadClientItem>.CreateNew()
                                                            .With(h => h.Status = DownloadItemStatus.Completed)
                                                            .With(h => h.OutputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic()))
                                                            .With(h => h.Title = "Movie.2015.HDTV")
                                                            .Build();

            var remoteEpisode = BuildRemoteEpisode();

            var remoteMovie = BuildRemoteMovie();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                    .With(c => c.State = TrackedDownloadStage.Downloading)
                    .With(c => c.DownloadItem = completed)
                    .With(c => c.RemoteItem = remoteEpisode)
                    .Build();

            _trackedMovieDownload = Builder<TrackedDownload>.CreateNew()
                .With(c => c.State = TrackedDownloadStage.Downloading)
                .With(c => c.DownloadItem = completedMovie)
                .With(c => c.RemoteItem = remoteMovie)
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

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie("Movie.2015.HDTV"))
                  .Returns(remoteMovie.Movie);
        }

        private RemoteEpisode BuildRemoteEpisode()
        {
            return new RemoteEpisode
            {
                Series = new Series(),
                Episodes = new List<Episode> { new Episode { Id = 1 } }
            };
        }

        private RemoteMovie BuildRemoteMovie()
        {
            return new RemoteMovie
            {
                Movie = new Movie()
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
            Mocker.GetMock<IDownloadedMediaImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision(new LocalEpisode() { Path = @"C:\TestPath\Droned.S01E01.mkv" }))
                    });
        }

        private void GivenSucessfulMovieImport()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(new ImportDecision(new LocalMovie() { Path = @"C:\TestPath\Movie.2015.mkv" }))
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

        private void GivenABadlyMovieNamedDownload()
        {
            _trackedDownload.DownloadItem.DownloadId = "1234";
            _trackedDownload.DownloadItem.Title = "The Movie Pilot"; // Set a badly named download
            Mocker.GetMock<IHistoryService>()
               .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")))
               .Returns(new History.History() { SourceTitle = "Movie 2015" });

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.GetMovie(It.IsAny<string>()))
               .Returns((Movie)null);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetMovie("Movie 2015"))
                .Returns(BuildRemoteMovie().Movie);
        }

        private void GivenSeriesMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetSeries(It.IsAny<string>()))
                  .Returns(_trackedDownload.RemoteItem.Media as Series);
        }

        private void GivenMovieMatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie(It.IsAny<string>()))
                  .Returns(_trackedMovieDownload.RemoteItem.Media as Movie);
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

            AssertNoAttemptedImport(_trackedDownload);
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found_and_no_category_specified()
        {
            _trackedDownload.DownloadItem.Category = null;
            GivenNoGrabbedHistory();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport(_trackedDownload);
        }

        [Test]
        public void should_process_if_matching_history_is_not_found_but_category_specified()
        {
            _trackedDownload.DownloadItem.Category = "tv";
            GivenNoGrabbedHistory();
            GivenSeriesMatch();
            GivenSuccessfulImport();

            Subject.Process(_trackedDownload);

            AssertCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_process_movie_if_matching_history_is_not_found_but_category_specified()
        {
            _trackedDownload.DownloadItem.Category = "movie";
            GivenNoGrabbedHistory();
            GivenMovieMatch();
            GivenSucessfulMovieImport();

            Subject.Process(_trackedMovieDownload);

            AssertCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_not_process_if_storage_directory_in_drone_factory()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.DownloadedEpisodesFolder)
                  .Returns(@"C:\DropFolder".AsOsAgnostic());

            _trackedDownload.DownloadItem.OutputPath = new OsPath(@"C:\DropFolder\SomeOtherFolder".AsOsAgnostic());

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport(_trackedDownload);
        }

        [Test]
        public void should_not_process_if_output_path_is_empty()
        {
            _trackedDownload.DownloadItem.OutputPath = new OsPath();

            Subject.Process(_trackedDownload);

            AssertNoAttemptedImport(_trackedDownload);
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
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

            AssertCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_mark_as_imported_if_movie_was_imported()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}))

                           });

            Subject.Process(_trackedMovieDownload);

            AssertCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_rejected()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
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

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_movie_was_rejected()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}, new Rejection("Rejected!")), "Test Failure"),
                           });

            Subject.Process(_trackedMovieDownload);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent<DownloadCompletedEvent>(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            AssertNoCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_no_episodes_were_parsed()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}, new Rejection("Rejected!")), "Test Failure"),
                               
                                new ImportResult(
                                   new ImportDecision(
                                       new LocalEpisode {Path = @"C:\TestPath\Droned.S01E02.mkv"},new Rejection("Rejected!")), "Test Failure")
                           });


            (_trackedDownload.RemoteItem as RemoteEpisode).Episodes.Clear();

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_all_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_movie_files_were_skipped()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedMovieDownload);

            AssertNoCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_mark_as_imported_if_all_episodes_were_imported_but_extra_files_were_not()
        {
            GivenSeriesMatch();

            (_trackedDownload.RemoteItem as RemoteEpisode).Episodes = new List<Episode>
            {
                new Episode()
            };

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"})),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });

            Subject.Process(_trackedDownload);

            AssertCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_mark_as_imported_if_movie_was_imported_but_extra_files_were_not()
        {
            GivenMovieMatch();

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"})),
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}),"Test Failure")
                           });

            Subject.Process(_trackedMovieDownload);

            AssertCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_mark_as_failed_if_some_of_episodes_were_not_imported()
        {
            (_trackedDownload.RemoteItem as RemoteEpisode).Episodes = new List<Episode>
            {
                new Episode(),
                new Episode(),
                new Episode()
            };

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"})),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure"),
                               new ImportResult(new ImportDecision(new LocalEpisode{Path = @"C:\TestPath\Droned.S01E01.mkv"}),"Test Failure")
                           });


            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_seriesid()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.GetSeries(It.IsAny<int>()))
                  .Returns(BuildRemoteEpisode().Series);

            Subject.Process(_trackedDownload);

            AssertCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_mark_as_imported_if_the_download_can_be_tracked_using_the_source_movieid()
        {
            GivenABadlyMovieNamedDownload();

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Movie.2015.mkv"}))
                           });

            Mocker.GetMock<IMovieService>()
                  .Setup(v => v.GetMovie(It.IsAny<int>()))
                  .Returns(BuildRemoteMovie().Movie);

            Subject.Process(_trackedMovieDownload);

            AssertCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally()
        {
            GivenABadlyNamedDownload();

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Mocker.GetMock<IHistoryService>()
            .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_not_mark_as_imported_if_the_download_cannot_be_tracked_using_the_source_title_as_it_was_initiated_externally_movie()
        {
            GivenABadlyMovieNamedDownload();

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}))
                           });

            Mocker.GetMock<IHistoryService>()
            .Setup(s => s.MostRecentForDownloadId(It.Is<string>(i => i == "1234")));

            Subject.Process(_trackedMovieDownload);

            AssertNoCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_not_import_when_there_is_a_title_mismatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetSeries("Drone.S01E01.HDTV"))
                  .Returns((Series)null);

            Subject.Process(_trackedDownload);

            AssertNoCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_not_import_when_there_is_a_movie_title_mismatch()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetMovie("Movie.2015.HDTV"))
                  .Returns((Movie)null);

            Subject.Process(_trackedMovieDownload);

            AssertNoCompletedDownload(_trackedMovieDownload);
        }

        [Test]
        public void should_mark_as_import_title_mismatch_if_ignore_warnings_is_true()
        {
            (_trackedDownload.RemoteItem as RemoteEpisode).Episodes = new List<Episode>
            {
                new Episode()
            };

            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalEpisode {Path = @"C:\TestPath\Droned.S01E01.mkv"}))
                           });

            Subject.Process(_trackedDownload, true);

            AssertCompletedDownload(_trackedDownload);
        }

        [Test]
        public void should_mark_as_import_movie_title_mismatch_if_ignore_warnings_is_true()
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                  .Setup(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(new ImportDecision(new LocalMovie {Path = @"C:\TestPath\Movie.2015.mkv"}))
                           });

            Subject.Process(_trackedMovieDownload, true);

            AssertCompletedDownload(_trackedMovieDownload);
        }

        private void AssertNoAttemptedImport(TrackedDownload trackedDownload)
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                .Verify(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Series>(), It.IsAny<DownloadClientItem>()), Times.Never());

            Mocker.GetMock<IDownloadedMediaImportService>()
                .Verify(v => v.ProcessPath(It.IsAny<string>(), It.IsAny<Movie>(), It.IsAny<DownloadClientItem>()), Times.Never());

            AssertNoCompletedDownload(trackedDownload);
        }

        private void AssertNoCompletedDownload(TrackedDownload trackedDownload)
        {
            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Never());

            trackedDownload.State.Should().NotBe(TrackedDownloadStage.Imported);
        }

        private void AssertCompletedDownload(TrackedDownload trackedDownload)
        {
            Mocker.GetMock<IDownloadedMediaImportService>()
                .Verify(v => v.ProcessPath(trackedDownload.DownloadItem.OutputPath.FullPath, trackedDownload.RemoteItem.Media, trackedDownload.DownloadItem), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<DownloadCompletedEvent>()), Times.Once());

            trackedDownload.State.Should().Be(TrackedDownloadStage.Imported);
        }
    }
}
