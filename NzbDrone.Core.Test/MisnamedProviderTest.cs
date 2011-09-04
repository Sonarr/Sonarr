using System;
using System.Collections.Generic;
using System.Text;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    public class MisnamedProviderTest
    {
        [Test]
        public void no_misnamed_files()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(f => f.EpisodeFileId = 1)
                .Has(f => f.Path = @"C:\Test\Title1.avi")
                .AndTheNext(1)
                .Has(f => f.EpisodeFileId = 2)
                .Has(f => f.Path = @"C:\Test\Title2.avi")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(e => e.EpisodeFileId = 1)
                .Has(e => e.EpisodeFile = episodeFiles[0])
                .Has(e => e.SeriesTitle = "SeriesTitle1")
                .AndTheNext(1)
                .Has(e => e.EpisodeFileId = 2)
                .Has(e => e.EpisodeFile = episodeFiles[1])
                .Has(e => e.SeriesTitle = "SeriesTitle2")
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);
            
            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.EpisodesWithFiles()).Returns(episodes);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> {episodes[0]}, "SeriesTitle1", It.IsAny<QualityTypes>()))
                .Returns("Title1");

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> {episodes[1]}, "SeriesTitle2", It.IsAny<QualityTypes>()))
                .Returns("Title2");

            //Act
            var totalItems = 0;
            var misnamedEpisodes = mocker.Resolve<MisnamedProvider>().MisnamedFiles(1, 10, out totalItems);

            //Assert
            misnamedEpisodes.Should().HaveCount(0);
        }

        [Test]
        public void all_misnamed_files()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(f => f.EpisodeFileId = 1)
                .Has(f => f.Path = @"C:\Test\Title1.avi")
                .AndTheNext(1)
                .Has(f => f.EpisodeFileId = 2)
                .Has(f => f.Path = @"C:\Test\Title2.avi")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(e => e.EpisodeFileId = 1)
                .Has(e => e.EpisodeFile = episodeFiles[0])
                .Has(e => e.SeriesTitle = "SeriesTitle1")
                .AndTheNext(1)
                .Has(e => e.EpisodeFileId = 2)
                .Has(e => e.EpisodeFile = episodeFiles[1])
                .Has(e => e.SeriesTitle = "SeriesTitle2")
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.EpisodesWithFiles()).Returns(episodes);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[0] }, "SeriesTitle1", It.IsAny<QualityTypes>()))
                .Returns("New Title 1");

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[1] }, "SeriesTitle2", It.IsAny<QualityTypes>()))
                .Returns("New Title 2");

            //Act
            var totalItems = 0;
            var misnamedEpisodes = mocker.Resolve<MisnamedProvider>().MisnamedFiles(1, 10, out totalItems);

            //Assert
            misnamedEpisodes.Should().HaveCount(2);
        }

        [Test]
        public void one_misnamed_file()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(f => f.EpisodeFileId = 1)
                .Has(f => f.Path = @"C:\Test\Title1.avi")
                .AndTheNext(1)
                .Has(f => f.EpisodeFileId = 2)
                .Has(f => f.Path = @"C:\Test\Title2.avi")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(e => e.EpisodeFileId = 1)
                .Has(e => e.EpisodeFile = episodeFiles[0])
                .Has(e => e.SeriesTitle = "SeriesTitle1")
                .AndTheNext(1)
                .Has(e => e.EpisodeFileId = 2)
                .Has(e => e.EpisodeFile = episodeFiles[1])
                .Has(e => e.SeriesTitle = "SeriesTitle2")
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.EpisodesWithFiles()).Returns(episodes);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[0] }, "SeriesTitle1", It.IsAny<QualityTypes>()))
                .Returns("New Title 1");

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[1] }, "SeriesTitle2", It.IsAny<QualityTypes>()))
                .Returns("Title2");

            //Act
            var totalItems = 0;
            var misnamedEpisodes = mocker.Resolve<MisnamedProvider>().MisnamedFiles(1, 10, out totalItems);

            //Assert
            misnamedEpisodes.Should().HaveCount(1);
            misnamedEpisodes[0].CurrentName.Should().Be("Title1");
            misnamedEpisodes[0].ProperName.Should().Be("New Title 1");
        }

        [Test]
        public void misnamed_multi_episode_file()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(f => f.EpisodeFileId = 1)
                .Has(f => f.Path = @"C:\Test\Title1.avi")
                .AndTheNext(1)
                .Has(f => f.EpisodeFileId = 2)
                .Has(f => f.Path = @"C:\Test\Title2.avi")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(3)
                .WhereTheFirst(2)
                .Has(e => e.EpisodeFileId = 1)
                .Has(e => e.EpisodeFile = episodeFiles[0])
                .Has(e => e.SeriesTitle = "SeriesTitle1")
                .AndTheNext(1)
                .Has(e => e.EpisodeFileId = 2)
                .Has(e => e.EpisodeFile = episodeFiles[1])
                .Has(e => e.SeriesTitle = "SeriesTitle2")
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.EpisodesWithFiles()).Returns(episodes);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[0], episodes[1] }, "SeriesTitle1", It.IsAny<QualityTypes>()))
                .Returns("New Title 1");

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[2] }, "SeriesTitle2", It.IsAny<QualityTypes>()))
                .Returns("Title2");

            //Act
            var totalItems = 0;
            var misnamedEpisodes = mocker.Resolve<MisnamedProvider>().MisnamedFiles(1, 10, out totalItems);

            //Assert
            misnamedEpisodes.Should().HaveCount(1);
            misnamedEpisodes[0].CurrentName.Should().Be("Title1");
            misnamedEpisodes[0].ProperName.Should().Be("New Title 1");
        }

        [Test]
        public void no_misnamed_multi_episode_file()
        {
            //Setup
            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(2)
                .WhereTheFirst(1)
                .Has(f => f.EpisodeFileId = 1)
                .Has(f => f.Path = @"C:\Test\Title1.avi")
                .AndTheNext(1)
                .Has(f => f.EpisodeFileId = 2)
                .Has(f => f.Path = @"C:\Test\Title2.avi")
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(3)
                .WhereTheFirst(2)
                .Has(e => e.EpisodeFileId = 1)
                .Has(e => e.EpisodeFile = episodeFiles[0])
                .Has(e => e.SeriesTitle = "SeriesTitle1")
                .AndTheNext(1)
                .Has(e => e.EpisodeFileId = 2)
                .Has(e => e.EpisodeFile = episodeFiles[1])
                .Has(e => e.SeriesTitle = "SeriesTitle2")
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.EpisodesWithFiles()).Returns(episodes);

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[0], episodes[1] }, "SeriesTitle1", It.IsAny<QualityTypes>()))
                .Returns("Title1");

            mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetNewFilename(new List<Episode> { episodes[2] }, "SeriesTitle2", It.IsAny<QualityTypes>()))
                .Returns("Title2");

            //Act
            var totalItems = 0;
            var misnamedEpisodes = mocker.Resolve<MisnamedProvider>().MisnamedFiles(1, 10, out totalItems);

            //Assert
            misnamedEpisodes.Should().HaveCount(0);
        }
    }
}
