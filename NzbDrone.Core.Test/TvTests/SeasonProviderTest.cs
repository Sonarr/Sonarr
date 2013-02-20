// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonProviderTest : SqlCeTest
    {
        [SetUp]
        public void Setup()
        {
            WithRealDb();
        }


        [TestCase(true)]
        [TestCase(false)]
        public void SetIgnore_should_update_ignored_status(bool ignoreFlag)
        {
            var fakeSeason = Builder<Season>.CreateNew()
                .With(s => s.Ignored = !ignoreFlag)
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = fakeSeason.SeriesId)
                .With(c => c.SeasonNumber = fakeSeason.OID)
                .With(c => c.Ignored = !ignoreFlag)
                .Build().ToList();

            fakeEpisodes.ForEach(c => Db.Insert(c));

            var id = Db.Insert(fakeSeason);

            //Act
            Mocker.Resolve<ISeasonService>().SetIgnore(fakeSeason.SeriesId, fakeSeason.SeasonNumber, ignoreFlag);

            //Assert
            var season = Db.SingleOrDefault<Season>(id);
            season.Ignored.Should().Be(ignoreFlag);

            var episodes = Db.Fetch<Episode>();
            episodes.Should().HaveSameCount(fakeEpisodes);
            episodes.Should().OnlyContain(c => c.Ignored == ignoreFlag);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IsIgnored_should_return_ignored_status_of_season(bool ignoreFlag)
        {
            //Setup
            var fakeSeason = Builder<Season>.CreateNew()
                .With(s => s.Ignored = ignoreFlag)
                .Build();

            Db.Insert(fakeSeason);

            //Act
            var result = Mocker.Resolve<SeasonRepository>().IsIgnored(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            //Assert
            result.Should().Be(ignoreFlag);
        }

        [Test]
        public void IsIgnored_should_throw_an_exception_if_not_in_db()
        {
            Assert.Throws<InvalidOperationException>(() => Mocker.Resolve<SeasonRepository>().IsIgnored(10, 0));
        }

        [Test]
        public void All_should_return_seasons_with_episodes()
        {
            const int seriesId = 10;

            var season = Builder<Season>.CreateNew()
                .With(s => s.SeriesId = seriesId)
                .With(s => s.SeasonNumber = 4)
                .With(s => s.Ignored = true)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.SeasonNumber = season.SeasonNumber)
                .Build();

            Db.Insert(season);
            Db.InsertMany(episodes);

            //Act
            var result = Mocker.Resolve<SeasonRepository>().GetSeasonBySeries(seriesId);

            //Assert
            result.Should().HaveCount(1);
            result.First().Episodes.Should().HaveCount(episodes.Count);
        }

        [Test]
        public void All_should_return_all_seasons_with_episodes()
        {
            const int seriesId = 10;

            //Setup
            WithRealDb();

            var seasons = Builder<Season>.CreateListOfSize(5)
                .All()
                .With(s => s.SeriesId = seriesId)
                .Build();

            var episodes = new List<Episode>();

            for (int i = 0; i < seasons.Count; i++)
            {
                var newEps = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.SeasonNumber = i + 1)
                .Build();

                episodes.AddRange(newEps);
            }

            Db.InsertMany(seasons);
            Db.InsertMany(episodes);

            //Act
            var result = Mocker.Resolve<SeasonRepository>().GetSeasonBySeries(seriesId);

            //Assert
            result.Should().HaveCount(5);

            foreach (var season in result)
            {
                season.Episodes.Count.Should().Be(2);
                season.Episodes.Should().OnlyContain(c => c.SeasonNumber == season.SeasonNumber);
            }
        }

     

        [Test]
        public void GetSeason_should_return_seasons_for_specified_series_only()
        {
            var seriesA = new[] { 1, 2, 3 };
            var seriesB = new[] { 4, 5, 6 };

            Mocker.Resolve<SeasonRepository>().GetSeasonNumbers(1).Should().Equal(seriesA);
            Mocker.Resolve<SeasonRepository>().GetSeasonNumbers(2).Should().Equal(seriesB);
        }


        [Test]
        public void GetSeason_should_return_emptylist_if_series_doesnt_exist()
        {
            Mocker.Resolve<SeasonRepository>().GetSeasonNumbers(1).Should().BeEmpty();
        }

    }
}
