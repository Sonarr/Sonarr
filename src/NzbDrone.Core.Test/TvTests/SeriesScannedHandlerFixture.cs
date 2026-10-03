using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesScannedHandlerFixture : CoreTest<SeriesScannedHandler>
    {
        [Test]
        public void should_queue_search_before_publishing_series_add_completed()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.AddOptions = new AddSeriesOptions { SearchForMissingEpisodes = true })
                                        .Build();

            var calls = new List<string>();

            Mocker.GetMock<IManageCommandQueue>()
                  .Setup(s => s.Push(It.IsAny<MissingEpisodeSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()))
                  .Callback(() => calls.Add("search"));

            Mocker.GetMock<IEventAggregator>()
                  .Setup(s => s.PublishEvent(It.IsAny<SeriesAddCompletedEvent>()))
                  .Callback(() => calls.Add("addCompleted"));

            Subject.Handle(new SeriesScannedEvent(series, new List<string>()));

            calls.Should().Equal("search", "addCompleted");
        }
    }
}
