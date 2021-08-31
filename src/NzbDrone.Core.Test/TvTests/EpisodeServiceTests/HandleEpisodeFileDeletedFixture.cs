using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class HandleEpisodeFileDeletedFixture : CoreTest<EpisodeService>
    {
        private EpisodeFile _episodeFile;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodeFile = Builder<EpisodeFile>
                .CreateNew()
                .Build();
        }

        private void GivenSingleEpisodeFile()
        {
            _episodes = Builder<Episode>
                .CreateListOfSize(1)
                .All()
                .With(e => e.Monitored = true)
                .Build()
                .ToList();

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.GetEpisodeByFileId(_episodeFile.Id))
                  .Returns(_episodes);
        }

        private void GivenMultiEpisodeFile()
        {
            _episodes = Builder<Episode>
                .CreateListOfSize(2)
                .All()
                .With(e => e.Monitored = true)
                .Build()
                .ToList();

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.GetEpisodeByFileId(_episodeFile.Id))
                  .Returns(_episodes);
        }

        [Test]
        public void should_set_EpisodeFileId_to_zero()
        {
            GivenSingleEpisodeFile();

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_update_each_episode_for_file()
        {
            GivenMultiEpisodeFile();

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public void should_set_monitored_to_false_if_autoUnmonitor_is_true_and_is_not_for_an_upgrade()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk));

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), true), Times.Once());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_false()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(false);

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.Upgrade));

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_an_upgrade()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.Upgrade));

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }

        [Test]
        public void should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_manual_override()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            Subject.Handle(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.ManualOverride));

            Mocker.GetMock<IEpisodeRepository>()
                  .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }
    }
}
