using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class UpdateMediaInfoServiceFixture : CoreTest<UpdateMediaInfoService>
    {
        private void GivenFileExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<String>()))
                  .Returns(true);
        }

        private void GivenSuccessfulScan()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(It.IsAny<String>()))
                  .Returns(new MediaInfoModel());
        }

        private void GivenFailedScan(String path)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(path))
                  .Returns((MediaInfoModel)null);
        }

        [Test]
        public void should_get_for_existing_episodefile_on_after_series_scan()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(3)
                .All()
                .With(v => v.Path = @"C:\series\media.mkv".AsOsAgnostic())
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel())
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(new Tv.Series { Id = 1 }));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(@"C:\series\media.mkv".AsOsAgnostic()), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_ignore_missing_files()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.Path = @"C:\series\media.mkv".AsOsAgnostic())
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(new Tv.Series { Id = 1 }));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(@"C:\series\media.mkv".AsOsAgnostic()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_continue_after_failure()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.Path = @"C:\series\media.mkv".AsOsAgnostic())
                   .TheFirst(1)
                   .With(v => v.Path = @"C:\series\media2.mkv".AsOsAgnostic())
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();
            GivenFailedScan(@"C:\series\media2.mkv".AsOsAgnostic());

            Subject.Handle(new SeriesScannedEvent(new Tv.Series { Id = 1 }));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(@"C:\series\media.mkv".AsOsAgnostic()), Times.Exactly(1));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(1));
        }
    }
}
