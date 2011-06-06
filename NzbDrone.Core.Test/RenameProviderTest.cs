// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RenameProviderTest : TestBase
    {
        [Test]
        public void GetNewFilename_Series_Episode_Quality_S01E05_Dash()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(true);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(true);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(2);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> {episode});

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("South Park - S15E06 - City Sushi [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_Episode_Quality_1x05_Dash()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(false);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(true);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(true);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(0);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> { episode });

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("15x06 - City Sushi [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_Series_Quality_01x05_Space()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(false);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(true);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(1);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(1);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> { episode });

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("South Park 05x06 [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_Series_s01e05_Space()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(false);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(false);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(1);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(3);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> { episode });

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("South Park s05e06", result);
        }

        [Test]
        public void GetNewFilename_Series_Episode_s01e05_Periods()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(true);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(false);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(1);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(3);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(true);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> { episode });

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("South.Park.s05e06.City.Sushi", result);
        }

        [Test]
        public void GetNewFilename_Series_Episode_s01e05_Dash_Periods_Quality()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(true);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(true);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(3);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(true);

            var series = Builder<Series>.CreateNew().With(s => s.Title = "South Park").Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                .With(e => e.EpisodeFileId = 12345)
                                .With(e => e.SeriesId = series.SeriesId)
                                .With(e => e.Quality = QualityTypes.HDTV)
                                .Build();

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.EpisodeFileId = episodeFile.EpisodeFileId)
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .With(e => e.SeriesId = series.SeriesId)
                            .Build();

            var fakeMediaFileProvider = mocker.GetMock<MediaFileProvider>();
            fakeMediaFileProvider.Setup(m => m.GetEpisodeFile(12345)).Returns(episodeFile);

            var fakeEpisodeProvider = mocker.GetMock<EpisodeProvider>();
            fakeEpisodeProvider.Setup(m => m.EpisodesByFileId(12345)).Returns(new List<Episode> { episode });

            var fakeSeriesProvider = mocker.GetMock<SeriesProvider>();
            fakeSeriesProvider.Setup(m => m.GetSeries(series.SeriesId)).Returns(series);

            //Act
            string result = mocker.Resolve<RenameProvider>().GetNewFilename(12345);

            //Assert
            Assert.AreEqual("South.Park.-.s05e06.-.City.Sushi.[HDTV]", result);
        }
    }
}