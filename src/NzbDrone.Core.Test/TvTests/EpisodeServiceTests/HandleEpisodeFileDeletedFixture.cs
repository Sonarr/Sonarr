using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class HandleEpisodeFileDeletedFixture : CoreTest<EpisodeService>
    {
        private Series _series;
        private EpisodeFile _episodeFile;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                .CreateNew()
                .Build();

            _episodeFile = Builder<EpisodeFile>
                .CreateNew()
                .With(e => e.SeriesId = _series.Id)
                .Build();
        }

        private void GivenSingleEpisodeFile()
        {
            _episodes = Builder<Episode>
                .CreateListOfSize(1)
                .All()
                .With(e => e.SeriesId = _series.Id)
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
                .With(e => e.SeriesId = _series.Id)
                .With(e => e.Monitored = true)
                .Build()
                .ToList();

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.GetEpisodeByFileId(_episodeFile.Id))
                  .Returns(_episodes);
        }

        [Test]
        public async Task should_set_EpisodeFileId_to_zero()
        {
            GivenSingleEpisodeFile();

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public async Task should_update_each_episode_for_file()
        {
            GivenMultiEpisodeFile();

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), It.IsAny<bool>()), Times.Exactly(2));
        }

        [Test]
        public async Task should_set_monitored_to_false_if_autoUnmonitor_is_true_and_is_not_for_an_upgrade()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk), CancellationToken.None);
            await Subject.HandleAsync(new SeriesScannedEvent(_series, new List<string>()), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.SetMonitored(It.IsAny<IEnumerable<int>>(), false), Times.Once());
        }

        [Test]
        public async Task should_leave_monitored_if_autoUnmonitor_is_true_and_missing_episode_is_replaced()
        {
            GivenSingleEpisodeFile();

            var newEpisodeFile = _episodeFile.JsonClone();
            newEpisodeFile.Id = 123;
            newEpisodeFile.Episodes = new LazyLoaded<List<Episode>>(_episodes);

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                .Returns(true);

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.MissingFromDisk), CancellationToken.None);
            await Subject.HandleAsync(new EpisodeFileAddedEvent(newEpisodeFile), CancellationToken.None);
            await Subject.HandleAsync(new SeriesScannedEvent(_series, new List<string>()), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.SetMonitored(It.IsAny<IEnumerable<int>>(), false), Times.Never());
        }

        [Test]
        public async Task should_leave_monitored_to_true_if_autoUnmonitor_is_false()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(false);

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.Upgrade), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }

        [Test]
        public async Task should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_an_upgrade()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.Upgrade), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }

        [Test]
        public async Task should_leave_monitored_to_true_if_autoUnmonitor_is_true_and_is_for_manual_override()
        {
            GivenSingleEpisodeFile();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoUnmonitorPreviouslyDownloadedEpisodes)
                  .Returns(true);

            await Subject.HandleAsync(new EpisodeFileDeletedEvent(_episodeFile, DeleteMediaFileReason.ManualOverride), CancellationToken.None);

            Mocker.GetMock<IEpisodeRepository>()
                  .Verify(v => v.ClearFileId(It.IsAny<Episode>(), false), Times.Once());
        }
    }
}
