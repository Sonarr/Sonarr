// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests : TestBase
    {
       


        [Test]
        public void get_series_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 12).Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 20).Build();

            var mocker = new AutoMoqer();

            var database = MockLib.GetEmptyDatabase(true);

            foreach (var file in firstSeriesFiles)
                database.Insert(file);

            foreach (var file in secondSeriesFiles)
                database.Insert(file);

            mocker.SetConstant(database);

            var result = mocker.Resolve<MediaFileProvider>().GetSeriesFiles(12);


            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void Scan_series_should_skip_series_with_no_episodes()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(12))
                .Returns(new List<Episode>());

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12).Build();

            //Act
            mocker.Resolve<DiskScanProvider>().Scan(series);

            //Assert
            mocker.VerifyAllMocks();

        }

        [Test]
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV]", "Law & Order- Criminal Intent - S10E07 - Icarus [HDTV]")]
        public void CleanFileName(string name, string expectedName)
        {
            //Act
            var result = MediaFileProvider.CleanFilename(name);

            //Assert
            Assert.AreEqual(expectedName, result);
        }

        [Test]
        public void CleanEpisodesWithNonExistantFiles()
        {
            //Setup
            var episodes = Builder<Episode>.CreateListOfSize(10).Build();

            var mocker = new AutoMoqer();
            var database = MockLib.GetEmptyDatabase(true);
            mocker.SetConstant(database);
            database.InsertMany(episodes);

            //Act
            mocker.Resolve<MediaFileProvider>().CleanEpisodesWithNonExistantFiles();
            var result = database.Fetch<Episode>();

            //Assert
            result.Should().HaveSameCount(episodes);
            result.Should().OnlyContain(e => e.EpisodeFileId == 0);
        }

        [Test]
        public void DeleteOrphanedEpisodeFiles()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(10).Build();
            var episodes = Builder<Episode>.CreateListOfSize(5).Build();

            var mocker = new AutoMoqer();
            var database = MockLib.GetEmptyDatabase(true);
            mocker.SetConstant(database);
            database.InsertMany(episodes);
            database.InsertMany(episodeFiles);

            //Act
            mocker.Resolve<MediaFileProvider>().DeleteOrphanedEpisodeFiles();
            var result = database.Fetch<EpisodeFile>();

            //Assert
            result.Should().HaveCount(5);
            result.Should().OnlyContain(e => e.EpisodeFileId > 0);
        }
    }
}