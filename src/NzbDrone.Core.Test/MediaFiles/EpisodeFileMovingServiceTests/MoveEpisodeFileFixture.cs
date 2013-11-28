using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

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
            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Path = @"C:\Test\File.avi")
                                               .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Series = Builder<Series>.CreateNew().Build())
                                                 .With(l => l.Episodes = Builder<Episode>.CreateListOfSize(1).Build().ToList())
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFilename(It.IsAny<List<Episode>>(), It.IsAny<Series>(), It.IsAny<EpisodeFile>()))
                  .Returns("File Name");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFilePath(It.IsAny<Series>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<String>()))
                  .Returns(@"C:\Test\File Name.avi");

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<String>()))
                  .Returns(true);
        }

        [Test]
        public void should_catch_UnauthorizedAccessException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<String>()))
                  .Throws<UnauthorizedAccessException>();

            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<String>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveEpisodeFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_not_catch_generic_Exception_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<String>()))
                  .Throws<Exception>();

            Assert.Throws<Exception>(() => Subject.MoveEpisodeFile(_episodeFile, _localEpisode));
        }
    }
}
