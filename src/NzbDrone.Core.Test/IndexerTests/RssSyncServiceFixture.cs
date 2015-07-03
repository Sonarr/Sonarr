using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests
{
    [TestFixture]
    public class RssSyncServiceFixture : CoreTest<RssSyncService>
    {
        private Mock<IIndexer> _mockIndexer;

        [SetUp]
        public void SetUp()
        {
            _mockIndexer = Mocker.GetMock<IIndexer>();
            _mockIndexer.SetupGet(s => s.Definition).Returns(new IndexerDefinition { Id = 1, Name = "Blaat" });
            _mockIndexer.SetupGet(s => s.SupportsRss).Returns(true);
            _mockIndexer.SetupGet(s => s.SupportsSearch).Returns(true);

            Mocker.GetMock<IIndexerFactory>()
                  .Setup(s => s.RssEnabled())
                  .Returns(new List<IIndexer> { _mockIndexer.Object });

            Mocker.GetMock<IFetchAndParseRss>()
                .Setup(v => v.Fetch())
                .Returns(new List<ReleaseInfo>());

            Mocker.GetMock<IPendingReleaseService>()
                .Setup(v => v.GetPending())
                .Returns(new List<ReleaseInfo>());

            Mocker.GetMock<IMakeDownloadDecision>()
                .Setup(v => v.GetRssDecision(It.IsAny<List<ReleaseInfo>>()))
                .Returns(new List<DownloadDecision>());

            Mocker.GetMock<IProcessDownloadDecisions>()
                .Setup(v => v.ProcessDecisions(It.IsAny<List<DownloadDecision>>()))
                .Returns(new ProcessedDecisions(new List<DownloadDecision>(), new List<DownloadDecision>(), new List<DownloadDecision>()));

        }

        private DateTime WithGap(double hours)
        {
            var date = DateTime.UtcNow.Subtract(TimeSpan.FromHours(hours));

            Mocker.GetMock<IIndexerStatusService>()
                .Setup(v => v.GetIndexerStatus(It.IsAny<int>()))
                .Returns(new IndexerStatus { LastRecentSearch = date });

            return date;
        }

        [Test]
        public void should_run_missing_search_if_feed_ran_with_last_3_hours()
        {
            Subject.Execute(new RssSyncCommand() { LastExecutionTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2.0)) });

            Mocker.GetMock<IIndexerFactory>()
                  .Verify(v => v.RssEnabled(), Times.Never());

            Mocker.GetMock<IEpisodeSearchService>()
                  .Verify(v => v.MissingEpisodesAiredAfter(It.IsAny<DateTime>(), It.IsAny<IEnumerable<int>>()), Times.Never());
        }

        [Test]
        public void should_run_missing_search_if_feed_had_gap()
        {
            var gap = WithGap(5.0);

            Subject.Execute(new RssSyncCommand() { LastExecutionTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(5.0)) });

            Mocker.GetMock<IIndexerFactory>()
                  .Verify(v => v.RssEnabled(), Times.Once());

            Mocker.GetMock<IEpisodeSearchService>()
                  .Verify(v => v.MissingEpisodesAiredAfter(gap, It.IsAny<IEnumerable<int>>()), Times.Once());
        }

        [Test]
        public void should_only_check_indexers_with_search_capability()
        {
            _mockIndexer.SetupGet(s => s.SupportsSearch).Returns(false);

            var gap = WithGap(5.0);

            Subject.Execute(new RssSyncCommand() { LastExecutionTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(5.0)) });

            Mocker.GetMock<IIndexerStatusService>()
                  .Verify(v => v.GetIndexerStatus(It.IsAny<int>()), Times.Never());
        }
    }
}
