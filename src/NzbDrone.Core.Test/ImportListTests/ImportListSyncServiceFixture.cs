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

            Mocker.GetMock<IFetchAndParseImportList>()
                  .Setup(v => v.Fetch())
                  .Returns(_importListReports);

            Mocker.GetMock<ISearchForNewSeries>()
                  .Setup(v => v.SearchForNewSeries(It.IsAny<string>()))
                  .Returns(new List<Series>());

            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.Get(It.IsAny<int>()))
                  .Returns(new ImportListDefinition { ShouldMonitor = MonitorTypes.All });

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

        private void WithExistingSeries()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(v => v.FindByTvdbId(_importListReports.First().TvdbId))
                  .Returns(new Series { TvdbId = _importListReports.First().TvdbId });
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
                  .Setup(v => v.Get(It.IsAny<int>()))
                  .Returns(new ImportListDefinition { ShouldMonitor = monitor });
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
