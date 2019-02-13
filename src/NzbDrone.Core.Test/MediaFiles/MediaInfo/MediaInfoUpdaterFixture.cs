using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class MediaInfoUpdaterFixture : CoreTest<MediaInfoUpdater>
    {
        private static readonly string SeriesPath = @"C:\series".AsOsAgnostic();
        private static readonly string MediaFileSeriesPath = @"C:\series\media.mkv".AsOsAgnostic();
        private static readonly string MediaFilename = @"media.mkv";
        private static readonly string VideoCodec = "videoCodec";
        private static readonly string UpdatedVideoCodec = "updatedVideoCodec";

        private Series _series;
        private EpisodeFile _seriesEpisodeFile;

        [SetUp]
        public void Setup()
        {
            _series = new Series
            {
                Id = 1,
                Path = SeriesPath
            };

            _seriesEpisodeFile = Builder<EpisodeFile>.CreateNew().With(e => e.RelativePath = MediaFilename).Build();
        }


        private void GivenFileExists()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(v => v.FileExists(It.IsAny<string>()))
                .Returns(true);
        }

        private void GivenSuccessfulScan(string videoCodec = "")
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                .Setup(v => v.GetMediaInfo(It.IsAny<string>()))
                .Returns(new MediaInfoModel{VideoCodec = videoCodec, SchemaRevision = VideoFileInfoReader.CURRENT_MEDIA_INFO_SCHEMA_REVISION});
        }

        private void GivenFailedScan(string path)
        {
            Mocker.GetMock<IVideoFileInfoReader>()
                .Setup(v => v.GetMediaInfo(path))
                .Returns((MediaInfoModel)null);
        }

        [Test]
        public void should_ignore_missing_series_file()
        {
            GivenSuccessfulScan();

            Subject.Update(_seriesEpisodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(MediaFileSeriesPath), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_not_update_if_relative_path_is_null()
        {
            GivenSuccessfulScan();

            _seriesEpisodeFile.RelativePath = null;

            Subject.Update(_seriesEpisodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(MediaFileSeriesPath), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_not_update_if_series_path_is_null()
        {
            GivenSuccessfulScan();

            _series.Path = null;

            Subject.Update(_seriesEpisodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(MediaFileSeriesPath), Times.Never());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }

        [Test]
        public void should_update_mediainfo_for_series_file()
        {
            GivenSuccessfulScan();
            GivenFileExists();

            Subject.Update(_seriesEpisodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(MediaFileSeriesPath), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Once());
        }

        [Test]
        public void should_not_update_mediainfo_if_series_file_scan_fails()
        {
            GivenFailedScan(MediaFileSeriesPath);
            GivenFileExists();

            Subject.Update(_seriesEpisodeFile, _series);

            Mocker.GetMock<IVideoFileInfoReader>()
                .Verify(v => v.GetMediaInfo(MediaFileSeriesPath), Times.Once());

            Mocker.GetMock<IMediaFileService>()
                .Verify(v => v.Update(It.IsAny<EpisodeFile>()), Times.Never());
        }
    }
}
