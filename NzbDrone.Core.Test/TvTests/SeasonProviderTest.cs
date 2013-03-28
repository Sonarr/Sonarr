// ReSharper disable RedundantUsingDirective

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
        public void IsIgnored_should_return_ignored_status_of_season(bool ignoreFlag)
        {
            var fakeSeason = Builder<Season>.CreateNew()
                                            .With(s => s.Ignored = ignoreFlag)
                                            .BuildNew<Season>();

            Db.Insert(fakeSeason);

            var result = Subject.IsIgnored(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            result.Should().Be(ignoreFlag);
        }

        [Test]
        public void IsIgnored_should_return_false_if_not_in_db()
        {
            Subject.IsIgnored(10, 0).Should().BeFalse();
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
