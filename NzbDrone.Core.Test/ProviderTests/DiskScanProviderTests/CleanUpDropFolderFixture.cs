using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    // ReSharper disable InconsistentNaming
    public class CleanUpDropFolderFixture : CoreTest
    {
        [Test]
        public void should_do_nothing_if_no_files_are_found()
        {
            //Setup
            var folder = @"C:\Test\DropDir\The Office";
            
            Mocker.GetMock<DiskProvider>().Setup(s => s.GetFiles(folder, SearchOption.AllDirectories))
                    .Returns(new string[0]);

            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUpDropFolder(folder);

            //Assert
            Mocker.GetMock<MediaFileProvider>().Verify(v => v.GetFileByPath(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_do_nothing_if_no_conflicting_files_are_found()
        {
            //Setup
            var folder = @"C:\Test\DropDir\The Office";
            var filename = Path.Combine(folder, "NotAProblem.avi");

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.Path = filename.NormalizePath())
                    .With(f => f.SeriesId = 12345)
                    .Build();

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetFiles(folder, SearchOption.AllDirectories))
                    .Returns(new string[] { filename });

            Mocker.GetMock<MediaFileProvider>().Setup(s => s.GetFileByPath(filename))
                    .Returns(() => null);

            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUpDropFolder(folder);

            //Assert
            Mocker.GetMock<MediaFileProvider>().Verify(v => v.GetFileByPath(filename), Times.Once());
            Mocker.GetMock<ISeriesRepository>().Verify(v => v.Get(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_move_file_if_a_conflict_is_found()
        {
            //Setup
            var folder = @"C:\Test\DropDir\The Office";
            var filename = Path.Combine(folder, "Problem.avi");
            var seriesId = 12345;
            var newFilename = "S01E01 - Title";
            var newFilePath = @"C:\Test\TV\The Office\Season 01\S01E01 - Title.avi";

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                   .With(f => f.Path = filename.NormalizePath())
                   .With(f => f.SeriesId = seriesId)
                   .Build();

            var series = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = seriesId)
                    .With(s => s.Title = "The Office")
                    .Build();

            var episode = Builder<Episode>.CreateListOfSize(1)
                .All()
                    .With(e => e.SeriesId = seriesId)
                    .With(e => e.EpisodeFile = episodeFile)
                    .Build();

            Mocker.GetMock<MediaFileProvider>().Setup(v => v.GetFileByPath(filename))
                   .Returns(() => null);

            Mocker.GetMock<DiskProvider>().Setup(s => s.GetFiles(folder, SearchOption.AllDirectories))
                    .Returns(new string[] { filename });

            Mocker.GetMock<MediaFileProvider>().Setup(s => s.GetFileByPath(filename))
                    .Returns(episodeFile);

            Mocker.GetMock<ISeriesRepository>().Setup(s => s.Get(It.IsAny<int>()))
                .Returns(series);

            Mocker.GetMock<EpisodeService>().Setup(s => s.GetEpisodesByFileId(episodeFile.EpisodeFileId))
                    .Returns(episode);

            Mocker.GetMock<MediaFileProvider>().Setup(s => s.GetNewFilename(It.IsAny<IList<Episode>>(), series, QualityTypes.Unknown, false, It.IsAny<EpisodeFile>()))
                .Returns(newFilename);

            Mocker.GetMock<MediaFileProvider>().Setup(s => s.CalculateFilePath(It.IsAny<Series>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new FileInfo(newFilePath));

            Mocker.GetMock<DiskProvider>()
                  .Setup(s => s.FileExists(filename))
                  .Returns(true);

            Mocker.GetMock<DiskProvider>().Setup(s => s.MoveFile(episodeFile.Path, newFilePath));

            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUpDropFolder(folder);

            //Assert
            Mocker.GetMock<MediaFileProvider>().Verify(v => v.GetFileByPath(filename), Times.Once());
            Mocker.GetMock<DiskProvider>().Verify(v => v.MoveFile(filename.NormalizePath(), newFilePath), Times.Once());
        }
    }
}
