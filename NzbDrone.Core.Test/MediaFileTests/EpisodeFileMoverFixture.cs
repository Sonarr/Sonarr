using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFileTests
{
    public class EpisodeFileMoverFixture : CoreTest<MoveEpisodeFiles>
    {
        [Test]
        public void should_not_move_file_if_source_and_destination_are_the_same_path()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.Id = 5)
                    .With(s => s.Title = "30 Rock")
                    .Build();

            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.SeriesId = fakeSeries.Id)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.EpisodeNumber = 1)
                    .Build().ToList();

            const string filename = @"30 Rock - S01E01 - TBD";
            var fi = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", filename + ".avi");

            var file = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = fakeSeries.Id)
                    .With(f => f.Path = fi)
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                .Setup(e => e.Get(fakeSeries.Id))
                .Returns(fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                .Setup(e => e.GetEpisodesByFileId(file.Id))
                .Returns(fakeEpisode);

            Mocker.GetMock<IBuildFileNames>()
                .Setup(e => e.BuildFilename(fakeEpisode, fakeSeries, It.IsAny<EpisodeFile>()))
                .Returns(filename);

            Mocker.GetMock<IBuildFileNames>()
                .Setup(e => e.BuildFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".avi"))
                .Returns(fi);

            //Act
            var result = Subject.MoveEpisodeFile(file, false);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public void should_use_EpisodeFiles_quality()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.Id = 5)
                    .With(s => s.Title = "30 Rock")
                    .Build();

            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.SeriesId = fakeSeries.Id)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.EpisodeNumber = 1)
                    .Build().ToList();

            const string filename = @"30 Rock - S01E01 - TBD";
            var fi = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", filename + ".mkv");
            var currentFilename = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", "30.Rock.S01E01.Test.WED-DL.mkv");
            const string message = "30 Rock - 1x01 - [WEBDL]";

            var file = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = fakeSeries.Id)
                    .With(f => f.Path = currentFilename)
                    .With(f => f.Quality = Quality.WEBDL720p)
                    .With(f => f.Proper = false)
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                  .Setup(e => e.Get(fakeSeries.Id))
                  .Returns(fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(e => e.GetEpisodesByFileId(file.Id))
                  .Returns(fakeEpisode);

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(e => e.BuildFilename(fakeEpisode, fakeSeries, It.IsAny<EpisodeFile>()))
                  .Returns(filename);

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(e => e.BuildFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".mkv"))
                  .Returns(fi);

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FileExists(currentFilename))
                  .Returns(true);

            var result = Subject.MoveEpisodeFile(file, true);


        }

        [Test]
        public void should_log_error_and_return_null_when_source_file_does_not_exists()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.Id = 5)
                    .With(s => s.Title = "30 Rock")
                    .Build();

            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.SeriesId = fakeSeries.Id)
                    .With(e => e.SeasonNumber = 1)
                    .With(e => e.EpisodeNumber = 1)
                    .Build().ToList();

            const string filename = @"30 Rock - S01E01 - TBD";
            var fi = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", filename + ".mkv");
            var currentFilename = Path.Combine(@"C:\Test\TV\30 Rock\Season 01\", "30.Rock.S01E01.Test.WED-DL.mkv");
            const string message = "30 Rock - 1x01 - [WEBDL]";

            var file = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.SeriesId = fakeSeries.Id)
                    .With(f => f.Path = currentFilename)
                    .With(f => f.Quality = Quality.WEBDL720p)
                    .With(f => f.Proper = false)
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                .Setup(e => e.Get(fakeSeries.Id))
                .Returns(fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                .Setup(e => e.GetEpisodesByFileId(file.Id))
                .Returns(fakeEpisode);

            Mocker.GetMock<IBuildFileNames>()
                .Setup(e => e.BuildFilename(fakeEpisode, fakeSeries, It.IsAny<EpisodeFile>()))
                .Returns(filename);

            Mocker.GetMock<IBuildFileNames>()
                .Setup(e => e.BuildFilePath(It.IsAny<Series>(), fakeEpisode.First().SeasonNumber, filename, ".mkv"))
                .Returns(fi);

            var result = Subject.MoveEpisodeFile(file, true);

            result.Should().BeNull();
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
