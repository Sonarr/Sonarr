// ReSharper disable RedundantUsingDirective
using System.Collections.Generic;
using AutoMoq;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProvider_GetNewFilenameTest : TestBase
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

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

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

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

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

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

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


            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

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

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

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

            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 5)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("South.Park.-.s05e06.-.City.Sushi.[HDTV]", result);
        }

        [Test]
        public void GetNewFilename_S01E05_Dash()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(false);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(false);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(false);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(2);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);


            var episode = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.SeasonNumber = 15)
                            .With(e => e.EpisodeNumber = 6)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episode }, "South Park", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("S15E06", result);
        }

        [Test]
        public void GetNewFilename_multi_Series_Episode_Quality_S01E05_Scene_Dash()
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
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(3);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("The Mentalist - S03E23-E24 - Strawberries and Cream (1) + Strawberries and Cream (2) [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_multi_Episode_Quality_1x05_Repeat_Dash()
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
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(2);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("3x23x24 - Strawberries and Cream (1) + Strawberries and Cream (2) [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_multi_Episode_Quality_01x05_Repeat_Space()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(false);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(true);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(true);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(1);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(0);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(false);
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(2);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("3x23x24 Strawberries and Cream (1) + Strawberries and Cream (2) [HDTV]", result);
        }

        [Test]
        public void GetNewFilename_multi_Series_Episode_s01e05_Duplicate_Period()
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
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(1);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("The.Mentalist.s03e23.s03e24.Strawberries.and.Cream.(1).+.Strawberries.and.Cream.(2)", result);
        }

        [Test]
        public void GetNewFilename_multi_Series_S01E05_Extend_Dash_Period()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(true);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(false);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(false);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(2);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(true);
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(0);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("The.Mentalist.-.S03E23-24", result);
        }

        [Test]
        public void GetNewFilename_multi_1x05_Repeat_Dash_Period()
        {
            //Setup
            var mocker = new AutoMoqer();

            var fakeConfig = mocker.GetMock<ConfigProvider>();
            fakeConfig.SetupGet(c => c.SeriesName).Returns(false);
            fakeConfig.SetupGet(c => c.EpisodeName).Returns(false);
            fakeConfig.SetupGet(c => c.AppendQuality).Returns(false);
            fakeConfig.SetupGet(c => c.SeparatorStyle).Returns(0);
            fakeConfig.SetupGet(c => c.NumberStyle).Returns(0);
            fakeConfig.SetupGet(c => c.ReplaceSpaces).Returns(true);
            fakeConfig.SetupGet(c => c.MultiEpisodeStyle).Returns(2);

            var episodeOne = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (1)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 23)
                            .Build();

            var episodeTwo = Builder<Episode>.CreateNew()
                            .With(e => e.Title = "Strawberries and Cream (2)")
                            .With(e => e.SeasonNumber = 3)
                            .With(e => e.EpisodeNumber = 24)
                            .Build();

            //Act
            string result = mocker.Resolve<MediaFileProvider>().GetNewFilename(new List<Episode> { episodeOne, episodeTwo }, "The Mentalist", QualityTypes.HDTV);

            //Assert
            Assert.AreEqual("3x23x24", result);
        }
    }
}