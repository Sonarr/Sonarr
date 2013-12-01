using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.Nzbget;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.NzbgetProviderTests
{
    public class QueueFixture : CoreTest<NzbgetClient>
    {
        private List<NzbGetQueueItem> _queue;

        [SetUp]
        public void Setup()
        {
            _queue = Builder<NzbGetQueueItem>.CreateListOfSize(5)
                                             .All()
                                             .With(q => q.NzbName = "30.Rock.S01E01.Pilot.720p.hdtv.nzb")
                                             .Build()
                                             .ToList();
        }

        private void WithFullQueue()
        {
            Mocker.GetMock<INzbGetCommunicationProxy>()
                  .Setup(s => s.GetQueue())
                  .Returns(_queue);
        }

        private void WithEmptyQueue()
        {
            Mocker.GetMock<INzbGetCommunicationProxy>()
                  .Setup(s => s.GetQueue())
                  .Returns(new List<NzbGetQueueItem>());
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
