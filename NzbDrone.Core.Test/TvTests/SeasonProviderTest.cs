

using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    public class SeasonProviderTest : DbTest<SeasonRepository, Season>
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Ismonitored_should_return_monitored_status_of_season(bool monitored)
        {
            var fakeSeason = Builder<Season>.CreateNew()
                                            .With(s => s.Monitored = monitored)
                                            .BuildNew<Season>();

            Db.Insert(fakeSeason);

            var result = Subject.IsMonitored(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            result.Should().Be(monitored);
        }

        [Test]
        public void Monitored_should_return_true_if_not_in_db()
        {
            Subject.IsMonitored(10, 0).Should().BeTrue();
        }

        [Test]
        public void GetSeason_should_return_seasons_for_specified_series_only()
        {
            var seriesA = new[] { 1, 2, 3 };
            var seriesB = new[] { 4, 5, 6 };

            var seasonsA = seriesA.Select(c => new Season {SeasonNumber = c, SeriesId = 1}).ToList();
            var seasonsB = seriesB.Select(c => new Season {SeasonNumber = c, SeriesId = 2}).ToList();

            Subject.InsertMany(seasonsA);
            Subject.InsertMany(seasonsB);

            Subject.GetSeasonNumbers(1).Should().Equal(seriesA);
            Subject.GetSeasonNumbers(2).Should().Equal(seriesB);
        }


        [Test]
        public void GetSeason_should_return_emptylist_if_series_doesnt_exist()
        {
            Subject.GetSeasonNumbers(1).Should().BeEmpty();
        }

    }
}
