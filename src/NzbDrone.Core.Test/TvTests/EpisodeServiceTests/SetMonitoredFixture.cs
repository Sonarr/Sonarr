using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.TvTests.EpisodeServiceTests
{
    [TestFixture]
    public class SetMonitoredFixture : CoreTest<EpisodeService>
    {
        [Test]
        public void should_publish_event_with_distinct_series_ids_when_setting_multiple_episodes()
        {
            var episodes = Builder<Episode>.CreateListOfSize(3)
                .TheFirst(2).With(e => e.SeriesId = 1)
                .TheLast(1).With(e => e.SeriesId = 2)
                .Build()
                .ToList();

            var ids = episodes.Select(e => e.Id).ToList();

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(episodes);

            Subject.SetMonitored(ids, false);

            Mocker.GetMock<IEpisodeRepository>()
                  .Verify(v => v.SetMonitored(It.Is<IEnumerable<int>>(i => i.SequenceEqual(ids)), false), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<SeriesEpisodesMonitoredChangedEvent>(e => e.SeriesIds.OrderBy(i => i).SequenceEqual(new[] { 1, 2 }))), Times.Once());
        }

        [Test]
        public void should_publish_event_with_series_id_when_setting_single_episode()
        {
            var episode = Builder<Episode>.CreateNew()
                .With(e => e.Id = 10)
                .With(e => e.SeriesId = 5)
                .Build();

            Mocker.GetMock<IEpisodeRepository>()
                  .Setup(s => s.Get(10))
                  .Returns(episode);

            Subject.SetEpisodeMonitored(10, true);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.Is<SeriesEpisodesMonitoredChangedEvent>(e => e.SeriesIds.SequenceEqual(new[] { 5 }))), Times.Once());
        }
    }
}
