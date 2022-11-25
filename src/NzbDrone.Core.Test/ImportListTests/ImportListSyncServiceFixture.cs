using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ImportListTests
{
    public class ImportListSyncServiceFixture : CoreTest<ImportListSyncService>
    {
        private List<ImportListItemInfo> _importListReports;

        [SetUp]
        public void SetUp()
        {
            var importListItem1 = new ImportListItemInfo
            {
                Title = "Breaking Bad"
            };

            _importListReports = new List<ImportListItemInfo> { importListItem1 };

            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.AllSeriesTvdbIds())
                  .Returns(new List<int>());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeries(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListDefinition> { new ImportListDefinition { ShouldMonitor = MonitorTypes.All } });

            Mocker.GetMock<IFetchAndParseImportList>()
                  .Setup(v => v.Fetch())
                  .Returns(_importListReports);

            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>());
        }

        private void WithTvdbId()
        {
            _importListReports.First().TvdbId = 81189;
        }

        private void WithImdbId()
        {
            _importListReports.First().ImdbId = "tt0496424";
        }

        private void WithExistingSeries()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.AllSeriesTvdbIds())
                  .Returns(new List<int> { _importListReports.First().TvdbId });
        }

        private void WithExcludedSeries()
        {
            Mocker.GetMock<IImportListExclusionService>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListExclusion>
                    {
                      new ImportListExclusion
                        {
                          TvdbId = 81189
                        }
                    });
        }

        private void WithMonitorType(MonitorTypes monitor)
        {
            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.All())
                  .Returns(new List<ImportListDefinition> { new ImportListDefinition { ShouldMonitor = monitor } });
        }

        [Test]
        public void should_search_if_series_title_and_no_series_id()
        {
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_search_if_series_title_and_series_id()
        {
            WithTvdbId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeries(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_search_by_imdb_if_series_title_and_series_imdb()
        {
            WithImdbId();
            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<ISearchForNewSeries>()
                  .Verify(v => v.SearchForNewSeriesByImdbId(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_not_add_if_existing_series()
        {
            WithTvdbId();
            WithExistingSeries();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 0), It.IsAny<bool>()));
        }

        [TestCase(MonitorTypes.None, false)]
        [TestCase(MonitorTypes.All, true)]
        public void should_add_if_not_existing_series(MonitorTypes monitor, bool expectedSeriesMonitored)
        {
            WithTvdbId();
            WithMonitorType(monitor);

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 1 && t.First().Monitored == expectedSeriesMonitored), It.IsAny<bool>()));
        }

        [Test]
        public void should_not_add_if_excluded_series()
        {
            WithTvdbId();
            WithExcludedSeries();

            Subject.Execute(new ImportListSyncCommand());

            Mocker.GetMock<IAddSeriesService>()
                  .Verify(v => v.AddSeries(It.Is<List<Series>>(t => t.Count == 0), It.IsAny<bool>()));
        }
    }
}
