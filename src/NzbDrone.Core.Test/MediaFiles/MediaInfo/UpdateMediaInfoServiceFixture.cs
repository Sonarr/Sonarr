using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class UpdateMediaInfoServiceFixture : CoreTest<UpdateMediaInfoService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = new Series
                      {
                          Id = 1,
                          Path = @"C:\series".AsOsAgnostic()
                      };

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableMediaInfo)
                  .Returns(true);
        }

        private void GivenFileExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenSuccessfulScan()
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(It.IsAny<string>()))
                  .Returns(new MediaInfoModel());
        }

        private void GivenFailedScan(string path)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                  .Setup(v => v.GetMediaInfo(path))
                  .Returns((MediaInfoModel)null);
        }

        [Test]
        public void should_skip_up_to_date_media_info()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(3)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.CURRENT_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_skip_not_yet_date_media_info()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(3)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel { SchemaRevision = VideoFileInfoReader.MINIMUM_MEDIA_INFO_SCHEMA_REVISION })
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(2));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_update_outdated_media_info()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(3)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.MediaInfo = new MediaInfoModel())
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(3));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(3));
        }

        [Test]
        public void should_ignore_missing_files()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.RelativePath = "media.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo("media.mkv"), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_continue_after_failure()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                   .All()
                   .With(v => v.RelativePath = "media.mkv")
                   .TheFirst(1)
                   .With(v => v.RelativePath = "media2.mkv")
                   .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(v => v.GetFilesBySeries(1))
                  .Returns(episodeFiles);

            GivenFileExists();
            GivenSuccessfulScan();
            GivenFailedScan(Path.Combine(_series.Path, "media2.mkv"));

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                  .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Exactly(1));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Exactly(1));
        }

        [Test]
        public void should_not_update_files_if_media_info_disabled()
        {
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .All()
                .With(v => v.RelativePath = "media.mkv")
                .TheFirst(1)
                .With(v => v.RelativePath = "media2.mkv")
                .BuildList();

            Mocker.GetMock<IMediaFileService>()
                .Setup(v => v.GetFilesBySeries(1))
                .Returns(episodeFiles);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Handle(new SeriesScannedEvent(_series));

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_not_update_if_media_info_disabled()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .Build();

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.EnableMediaInfo)
                .Returns(false);

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(episodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_update_media_info()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenSuccessfulScan();

            Subject.Update(episodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(Path.Combine(_series.Path, "media.mkv")), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(episodeFile), Times.Once());
        }

        [Test]
        public void should_not_update_media_info_if_new_info_is_null()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_series.Path, "media.mkv"));

            Subject.Update(episodeFile, _series);

            episodeFile.MediaInfo.Should().NotBeNull();
        }

        [Test]
        public void should_not_save_episode_file_if_new_info_is_null()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(v => v.RelativePath = "media.mkv")
                .With(e => e.MediaInfo = new MediaInfoModel { SchemaRevision = 3 })
                .Build();

            GivenFileExists();
            GivenFailedScan(Path.Combine(_series.Path, "media.mkv"));

            Subject.Update(episodeFile, _series);

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(episodeFile), Times.Never());
        }
    }
}
