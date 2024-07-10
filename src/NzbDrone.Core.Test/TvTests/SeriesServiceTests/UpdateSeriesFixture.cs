using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateSeriesFixture : CoreTest<SeriesService>
    {
        private Series _fakeSeries;
        private Series _existingSeries;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Series>.CreateNew().Build();
            _existingSeries = Builder<Series>.CreateNew().Build();

            _fakeSeries.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true },
                new Season { SeasonNumber = 2, Monitored = true }
            };

            _existingSeries.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = true },
                new Season { SeasonNumber = 2, Monitored = true }
            };

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Series>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.Update(It.IsAny<Series>()))
                .Returns<Series>(r => r);
        }

        private void GivenExistingSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                  .Setup(s => s.Get(It.IsAny<int>()))
                  .Returns(_existingSeries);
        }

        [Test]
        public void should_not_update_episodes_if_season_hasnt_changed()
        {
            GivenExistingSeries();

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_update_series_when_it_changes()
        {
            GivenExistingSeries();
            var seasonNumber = 1;
            var monitored = false;

            _fakeSeries.Seasons.Single(s => s.SeasonNumber == seasonNumber).Monitored = monitored;

            Subject.UpdateSeries(_fakeSeries);

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, seasonNumber, monitored), Times.Once());

            Mocker.GetMock<IEpisodeService>()
                  .Verify(v => v.SetEpisodeMonitoredBySeason(_fakeSeries.Id, It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_add_and_remove_tags()
        {
            GivenExistingSeries();
            var seasonNumber = 1;
            var monitored = false;

            _fakeSeries.Tags = new HashSet<int> { 1, 2 };
            _fakeSeries.Seasons.Single(s => s.SeasonNumber == seasonNumber).Monitored = monitored;

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(_fakeSeries))
                .Returns(new AutoTaggingChanges
                {
                    TagsToAdd = new HashSet<int> { 3 },
                    TagsToRemove = new HashSet<int> { 1 }
                });

            var result = Subject.UpdateSeries(_fakeSeries);

            result.Tags.Should().BeEquivalentTo(new[] { 2, 3 });
        }
    }
}
