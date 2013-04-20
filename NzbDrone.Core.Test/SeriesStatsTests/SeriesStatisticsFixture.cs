using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.SeriesStatsTests
{
    [TestFixture]
    public class SeriesStatisticsFixture : DbTest<SeriesStatisticsRepository, Series>
    {
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .Build();

            series.Id = Db.Insert(series).Id;

            _episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.SeriesId = series.Id)
                                          .With(e => e.AirDate = DateTime.Today.AddDays(5))
                                          .Build();

            Db.Insert(_episode);
        }

        [Test]
        public void should_get_stats_for_series()
        {
            var stats = Subject.SeriesStatistics();

            stats.Should().HaveCount(1);
            stats.First().NextAiring.Should().Be(_episode.AirDate);
        }
    }
}
