using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeFileMovingServiceTests
{
    [TestFixture]
    public class MoveEpisodeFileFixture : CoreTest<EpisodeFileMovingService>
    {
        private Series _series;
        private EpisodeFile _episodeFile;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Series".AsOsAgnostic())
                                     .Build();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Path = null)
                                               .With(f => f.RelativePath = @"Season 1\File.avi")
                                               .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Series = _series)
                                                 .With(l => l.Episodes = Builder<Episode>.CreateListOfSize(1).Build().ToList())
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFileName(It.IsAny<List<Episode>>(), It.IsAny<Series>(), It.IsAny<EpisodeFile>(), null))
                  .Returns("File Name");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFilePath(It.IsAny<Series>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(@"C:\Test\TV\Series\Season 01\File Name.avi".AsOsAgnostic());

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildSeasonPath(It.IsAny<Series>(), It.IsAny<int>()))
                  .Returns(@"C:\Test\TV\Series\Season 01".AsOsAgnostic());

            var rootFolder = @"C:\Test\TV\".AsOsAgnostic();
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(rootFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        [Test]
        public void should_catch_UnauthorizedAccessException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<UnauthorizedAccessException>();

            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_notify_on_series_folder_creation()
        {
            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.SeriesFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_notify_on_season_folder_creation()
        {
            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.SeasonFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_not_notify_if_series_folder_already_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_series.Path))
                  .Returns(true);

            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<EpisodeFolderCreatedEvent>(It.Is<EpisodeFolderCreatedEvent>(p =>
                      p.SeriesFolder.IsNotNullOrWhiteSpace())), Times.Never());
        }
    }
}
