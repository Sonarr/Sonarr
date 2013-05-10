using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using System.Linq;

namespace NzbDrone.Core.Test.MediaFileTests
{
    public class GhostFileCleanupFixture : CoreTest<GhostFileCleanupService>
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
        }

        [Test]
        public void should_skip_files_that_exist_in_disk()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .Build();

            GiveEpisodeFiles(episodeFiles);

            Subject.RemoveNonExistingFiles(0);

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

            Subject.RemoveNonExistingFiles(0);

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Delete(It.Is<EpisodeFile>(e => e.Path == DeletedPath)), Times.Exactly(2));

        }
    }
}
