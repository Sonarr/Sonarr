using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetTests
{
    public class QueueFixture : CoreTest<Nzbget>
    {
        private List<NzbgetQueueItem> _queue;

        [SetUp]
        public void Setup()
        {
            _queue = Builder<NzbgetQueueItem>.CreateListOfSize(5)
                                             .All()
                                             .With(q => q.NzbName = "30.Rock.S01E01.Pilot.720p.hdtv.nzb")
                                             .Build()
                                             .ToList();

            Subject.Definition = new DownloadClientDefinition();
            Subject.Definition.Settings = new NzbgetSettings
            {
                Host = "localhost",
                Port = 6789,
                Username = "nzbget",
                Password = "pass",
                TvCategory = "tv",
                RecentTvPriority = (int)NzbgetPriority.High
            };
        }

        private void WithFullQueue()
        {
            Mocker.GetMock<INzbgetProxy>()
                  .Setup(s => s.GetQueue(It.IsAny<NzbgetSettings>()))
                  .Returns(_queue);
        }

        private void WithEmptyQueue()
        {
            Mocker.GetMock<INzbgetProxy>()
                  .Setup(s => s.GetQueue(It.IsAny<NzbgetSettings>()))
                  .Returns(new List<NzbgetQueueItem>());
        }

        [Test]
        public void should_return_no_items_when_queue_is_empty()
        {
            WithEmptyQueue();

            Subject.GetQueue()
                   .Should()
                   .BeEmpty();
        }

        [Test]
        public void should_return_item_when_queue_has_item()
        {
            WithFullQueue();

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.Map(It.IsAny<ParsedEpisodeInfo>(), 0, null))
                  .Returns(new RemoteEpisode {Series = new Series()});

            Subject.GetQueue()
                   .Should()
                   .HaveCount(_queue.Count);
        }
    }
}
