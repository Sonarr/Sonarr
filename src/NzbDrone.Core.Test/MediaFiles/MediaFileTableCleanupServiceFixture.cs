using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class MediaFileTableCleanupServiceFixture : CoreTest<MediaFileTableCleanupService>
    {
        private const string DELETED_PATH = "ANY FILE WITH THIS PATH IS CONSIDERED DELETED!";
        private List<Episode> _episodes;

        [SetUp]
        public void SetUp()
        {
            _episodes = Builder<Episode>.CreateListOfSize(10)
                  .Build()
                  .ToList();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<Int32>()))
                  .Returns(Builder<Series>.CreateNew().Build());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(e => e.FileExists(It.Is<String>(c => c != DELETED_PATH)))
                  .Returns(true);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.IsParent(It.IsAny<String>(), It.IsAny<String>()))
                  .Returns(true);
        }

        private void GivenEpisodeFiles(IEnumerable<EpisodeFile> episodeFiles)
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                  .Returns(episodeFiles.ToList());
        }

        private void GivenFilesAreNotAttachedToEpisode()
        {
            _episodes.ForEach(e => e.EpisodeFileId = 0);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenFileIsNotInSeriesFolder()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.IsParent(It.IsAny<String>(), It.IsAny<String>()))
                  .Returns(false);
        }

        [Test]
        public void should_skip_files_that_exist_in_disk()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .Build();

            GivenEpisodeFiles(episodeFiles);

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IEpisodeService>().Verify(c => c.UpdateEpisode(It.IsAny<Episode>()), Times.Never());
        }

        [Test]
        public void should_delete_none_existing_files()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .Random(2)
                .With(c => c.Path = DELETED_PATH)
                .Build();

            GivenEpisodeFiles(episodeFiles);

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.Is<EpisodeFile>(e => e.Path == DELETED_PATH), false), Times.Exactly(2));
        }

        [Test]
        public void should_delete_files_that_dont_belong_to_any_episodes()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.Path = "ExistingPath")
                                .Build();

            GivenEpisodeFiles(episodeFiles);
            GivenFilesAreNotAttachedToEpisode();

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.IsAny<EpisodeFile>(), false), Times.Exactly(10));
        }

        [Test]
        public void should_delete_files_that_do_not_belong_to_the_series_path()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.Path = "ExistingPath")
                                .Build();

            GivenEpisodeFiles(episodeFiles);
            GivenFileIsNotInSeriesFolder();

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.IsAny<EpisodeFile>(), false), Times.Exactly(10));
        }

        [Test]
        public void should_unlink_episode_when_episodeFile_does_not_exist()
        {
            GivenEpisodeFiles(new List<EpisodeFile>());

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IEpisodeService>().Verify(c => c.UpdateEpisode(It.Is<Episode>(e => e.EpisodeFileId == 0)), Times.Exactly(10));
        }

        [Test]
        public void should_not_update_episode_when_episodeFile_exists()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.Path = "ExistingPath")
                                .Build();

            GivenEpisodeFiles(episodeFiles);

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IEpisodeService>().Verify(c => c.UpdateEpisode(It.IsAny<Episode>()), Times.Never());
        }
    }
}
