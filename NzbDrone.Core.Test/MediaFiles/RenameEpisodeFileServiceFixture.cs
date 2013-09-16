using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class RenameEpisodeFileServiceFixture : CoreTest<RenameEpisodeFileService>
    {
        private Series _series;
        private List<EpisodeFile> _episodeFiles;
            
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .Build();

            _episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.SeriesId = _series.Id)
                                                .With(e => e.SeasonNumber = 1)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(_series.Id))
                  .Returns(_series);
        }

        private void GivenNoEpisodeFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeries(_series.Id))
                  .Returns(new List<EpisodeFile>());

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeason(_series.Id, _episodeFiles.First().SeasonNumber))
                  .Returns(new List<EpisodeFile>());
        }

        private void GivenEpisodeFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeries(_series.Id))
                  .Returns(_episodeFiles);
            
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeason(_series.Id, _episodeFiles.First().SeasonNumber))
                  .Returns(_episodeFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), _series));
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoEpisodeFiles();

            Subject.Execute(new RenameSeriesCommand(_series.Id));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenEpisodeFiles();

            Mocker.GetMock<IMoveEpisodeFiles>()
                  .Setup(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<Series>()))
                  .Throws(new SameFilenameException("Same file name", "Filename"));

            Subject.Execute(new RenameSeriesCommand(_series.Id));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameSeriesCommand(_series.Id));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameSeriesCommand(_series.Id));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));
        }

        [Test]
        public void rename_season_should_get_episodefiles_for_season()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameSeasonCommand(_series.Id, _episodeFiles.First().SeasonNumber));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetFilesBySeries(_series.Id), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetFilesBySeason(_series.Id, _episodeFiles.First().SeasonNumber), Times.Once());
        }

        [Test]
        public void rename_series_should_get_episodefiles_for_series()
        {
            GivenEpisodeFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameSeriesCommand(_series.Id));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetFilesBySeries(_series.Id), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.GetFilesBySeason(_series.Id, _episodeFiles.First().SeasonNumber), Times.Never());
        }
    }
}
