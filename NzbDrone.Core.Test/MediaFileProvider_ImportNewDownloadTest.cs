// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;
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
    public class MediaFileProvider_ImportNewDownloadTest : TestBase
    {
        private Episode episode;
        private Episode episode2;
        private EpisodeFile episodeFile;
        private EpisodeFile episodeFile2;
        private Series series;

        [SetUp]
        public new void Setup()
        {
            series = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.Path = @"C:\Test\TV\30 Rock")
                .Build();

            episode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 5)
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.Title = "Episode One Title")
                .Build();

            episode2 = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 6)
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.Title = "Episode Two Title")
                .Build();

            episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.Quality = QualityTypes.SDTV)
                .With(e => e.Episodes = new List<Episode> { episode })
                .Build();

            episodeFile2 = Builder<EpisodeFile>.CreateNew()
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.Quality = QualityTypes.SDTV)
                .With(e => e.Episodes = new List<Episode> { episode })
                .Build();

            episode.EpisodeFile = episodeFile;

            base.Setup();
        }

        [Test]
        [Description("Verifies that a new download will import successfully")]
        public void import_new_download_imported()
        {
            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".avi");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> {episode});
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will import successfully, deletes previous episode")]
        public void import_new_download_imported_delete_equal_quality()
        {
            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".avi");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode });
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will not import successfully, because existing episode is better")]
        public void import_new_download_not_imported_greater_quality()
        {
            //Alternate Setups
            episodeFile.Quality = QualityTypes.DVD;

            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".avi");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode });

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will not import successfully, because of invalid episode in new file")]
        public void import_new_download_not_imported_non_existant_episode()
        {
            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode>());

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(0, result.Count);
            ExceptionVerification.ExcpectedErrors(1);
        }

        [Test]
        [Description("Verifies that a new download will import successfully, deletes previous episode")]
        public void import_new_download_imported_delete_lesser_quality_multi_episodes()
        {
            //Alternate Setup
            episodeFile.Episodes.Add(episode2);
            episode2.EpisodeFile = episodeFile;

            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05x06 - Episode Title\30.Rock.S01E05E06.Gibberish.x264.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".mkv");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode, episode2 });
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 6)).Returns(episode2);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05x06 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will import successfully, deletes previous episode")]
        public void import_new_download_imported_delete_lesser_quality_multi_episode_files()
        {
            //Alternate Setup
            episodeFile2.Episodes.Add(episode2);
            episode2.EpisodeFile = episodeFile2;

            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05x06 - Episode Title\30.Rock.S01E05E06.Gibberish.x264.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".mkv");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode, episode2 });
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 6)).Returns(episode2);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05x06 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will not import successfully, previous episode is better quality")]
        public void import_new_download_not_imported_multi_episode_files()
        {
            //Alternate Setup
            episodeFile2.Episodes.Add(episode2);
            episode2.EpisodeFile = episodeFile2;
            episodeFile2.Quality = QualityTypes.Bluray720p;

            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05x06 - Episode Title\30.Rock.S01E05E06.Gibberish.x264.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".mkv");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode, episode2 });

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05x06 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will not import successfully, episode is sample under 40MB")]
        public void import_new_download_not_imported_episode_sample_under_40MB()
        {
            //Alternate Setup
            episodeFile2.Episodes.Add(episode2);
            episode2.EpisodeFile = episodeFile2;
            episodeFile2.Quality = QualityTypes.Bluray720p;

            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05x06 - Episode Title\30.Rock.S01E05.Gibberish.x264-sample.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(30000000);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05x06 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will import successfully, even though the episode title contains Sample")]
        public void import_new_download_imported_contains_sample_over_40MB()
        {
            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Fourty.Samples.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(90000000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".avi");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode });
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Fourty Samples", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Description("Verifies that a new download will import successfully, even though the efile size is under 40MB")]
        public void import_new_download_imported_under_40MB()
        {
            //Mocks
            var mocker = new AutoMoqer();

            var diskProvider = mocker.GetMock<DiskProvider>();
            diskProvider.Setup(d => d.GetFiles(It.IsAny<string>(), "*.*", SearchOption.AllDirectories)).Returns(new string[] { @"C:\Test\30 Rock - 1x05 - Episode Title\30.Rock.S01E05.Gibberish.XviD.avi" });
            diskProvider.Setup(d => d.GetSize(It.IsAny<string>())).Returns(30000000);
            diskProvider.Setup(d => d.CreateDirectory(It.IsAny<string>())).Returns("ok");
            diskProvider.Setup(d => d.RenameFile(It.IsAny<string>(), It.IsAny<string>()));
            diskProvider.Setup(d => d.GetExtension(It.IsAny<string>())).Returns(".avi");

            var episodeProvider = mocker.GetMock<EpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisodes(It.IsAny<EpisodeParseResult>())).Returns(new List<Episode> { episode });
            episodeProvider.Setup(e => e.GetEpisode(series.SeriesId, 1, 5)).Returns(episode);

            var configProvider = mocker.GetMock<ConfigProvider>();
            configProvider.SetupGet(c => c.UseSeasonFolder).Returns(true);
            configProvider.SetupGet(c => c.SeasonFolderFormat).Returns(@"Season %0s");
            configProvider.SetupGet(c => c.SeriesName).Returns(true);
            configProvider.SetupGet(c => c.EpisodeName).Returns(true);
            configProvider.SetupGet(c => c.AppendQuality).Returns(true);
            configProvider.SetupGet(c => c.SeparatorStyle).Returns(0);
            configProvider.SetupGet(c => c.NumberStyle).Returns(2);
            configProvider.SetupGet(c => c.ReplaceSpaces).Returns(false);

            var database = mocker.GetMock<IDatabase>();
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1);

            //Act
            var result = mocker.Resolve<MediaFileProvider>().ImportNewFiles(@"C:\Test\30 Rock - 1x05 - Episode Title", series);

            //Assert
            mocker.VerifyAllMocks();
            Assert.AreEqual(1, result.Count);
        }
    }
}