using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _episode = Builder<Episode>.CreateNew()
                                       .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllBySeriesId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(new Series());

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<Series> { new Series() });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>()))
                  .Returns(new RemoteEpisode { Episodes = new List<Episode> { _episode } });

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>(), It.IsAny<bool>(), null))
                  .Returns(new List<Episode> { _episode });
        }

        private void AddPending(int id, int seasonNumber, int[] episodes)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 ParsedEpisodeInfo = new ParsedEpisodeInfo { SeasonNumber = seasonNumber, EpisodeNumbers = episodes }
             });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 3, seasonNumber: 2, episodes: new[] { 3 });
            AddPending(id: 4, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 3, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_not_remove_different_season()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, seasonNumber: 3, episodes: new[] { 1 });
            AddPending(id: 4, seasonNumber: 3, episodes: new[] { 1 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_different_episodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 4, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multiepisodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singleepisodes()
        {
            AddPending(id: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(string.Format("pending-{0}-ep{1}", 2, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(2);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }
    }
}
