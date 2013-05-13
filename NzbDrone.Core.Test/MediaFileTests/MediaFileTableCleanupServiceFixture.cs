using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using System.Linq;

namespace NzbDrone.Core.Test.MediaFileTests
{
    public class MediaFileTableCleanupServiceFixture : CoreTest<MediaFileTableCleanupService>
    {

        private void GiveEpisodeFiles(IEnumerable<EpisodeFile> episodeFiles)
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                .Returns(episodeFiles.ToList());
        }


        private const string DeletedPath = "ANY FILE WITH THIS PATH IS CONSIDERED DELETED!";

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IDiskProvider>()
             .Setup(e => e.FileExists(It.Is<String>(c => c != DeletedPath)))
             .Returns(true);

            Mocker.GetMock<IEpisodeService>()
            .Setup(c => c.GetEpisodesByFileId(It.IsAny<int>()))
            .Returns(new List<Episode> { new Episode() });
        }

        [Test]
        public void should_skip_files_that_exist_in_disk()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .Build();

            GiveEpisodeFiles(episodeFiles);

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IEpisodeService>().Verify(c => c.UpdateEpisode(It.IsAny<Episode>()), Times.Never());
        }

        [Test]
        public void should_delete_none_existing_files()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .Random(2)
                .With(c => c.Path = DeletedPath)
                .Build();

            GiveEpisodeFiles(episodeFiles);

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.Is<EpisodeFile>(e => e.Path == DeletedPath)), Times.Exactly(2));

        }

        [Test]
        public void should_delete_files_that_dont_belong_to_any_episodes()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                                .Random(10)
                                .With(c => c.Path = "ExistingPath")
                                .Build();

            GiveEpisodeFiles(episodeFiles);
            GivenFilesAreNotAttachedToEpisode();

            Subject.Execute(new CleanMediaFileDb(0));

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.IsAny<EpisodeFile>()), Times.Exactly(10));
        }

        private void GivenFilesAreNotAttachedToEpisode()
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(c => c.GetEpisodesByFileId(It.IsAny<int>()))
                  .Returns(new List<Episode>());
        }
    }
}
