using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    public class PerformSearchTestBase : TestBase
    {
        protected Series _series;
        protected Episode _episode;
        protected List<Episode> _episodes;
        protected ProgressNotification notification = new ProgressNotification("Testing");

        protected Mock<IndexerBase> _indexer1;
        protected Mock<IndexerBase> _indexer2;
        protected List<IndexerBase> _indexers;
        protected IList<EpisodeParseResult> _parseResults;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.OID)
                    .With(e => e.Series = _series)
                    .Build();

            _episodes = Builder<Episode>
                    .CreateListOfSize(10)
                    .All()
                    .With(e => e.SeriesId = _series.OID)
                    .With(e => e.Series = _series)
                    .Build()
                    .ToList();

            _parseResults = Builder<EpisodeParseResult>
                    .CreateListOfSize(10)
                    .Build();

            _indexer1 = new Mock<IndexerBase>();
            _indexer2 = new Mock<IndexerBase>();
            _indexers = new List<IndexerBase> { _indexer1.Object, _indexer2.Object };

            Mocker.GetMock<IIndexerService>()
                  .Setup(c => c.GetEnabledIndexers())
                  .Returns(_indexers);
        }

        protected void WithValidIndexers()
        {
            _indexer1.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults);
            _indexer1.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(_parseResults);
            _indexer1.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults);

            _indexer2.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults);
            _indexer2.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(_parseResults);
            _indexer2.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_parseResults);
        }

        protected void WithInvalidIndexers()
        {
            _indexer1.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());
            _indexer1.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Throws(new Exception());
            _indexer1.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());

            _indexer2.Setup(c => c.FetchEpisode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());
            _indexer2.Setup(c => c.FetchDailyEpisode(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Throws(new Exception());
            _indexer2.Setup(c => c.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());

            _indexer1.SetupGet(c => c.Name).Returns("Indexer1");
            _indexer1.SetupGet(c => c.Name).Returns("Indexer2");
        }
    }
}
