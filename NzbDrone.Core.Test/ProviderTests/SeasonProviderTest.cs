// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonProviderTest : CoreTest
    {
        [Test]
        public void AddSeason_should_insert_season_to_database_with_ignored_false()
        {
            WithRealDb();

            var seriesId = 10;
            var seasonNumber = 50;

            //Act
            Mocker.Resolve<SeasonProvider>().Add(seriesId, seasonNumber);

            //Assert
            var result = Db.Fetch<Season>();
            result.Should().HaveCount(1);
            result.First().SeriesId.Should().Be(seriesId);
            result.First().SeasonNumber.Should().Be(seasonNumber);
            result.First().Ignored.Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddSeason_should_insert_season_to_database_with_preset_ignored_status(bool isIgnored)
        {
            WithRealDb();

            var seriesId = 10;
            var seasonNumber = 50;

            //Act
            Mocker.Resolve<SeasonProvider>().Add(seriesId, seasonNumber, isIgnored);

            //Assert
            var result = Db.Fetch<Season>();
            result.Should().HaveCount(1);
            result.First().SeriesId.Should().Be(seriesId);
            result.First().SeasonNumber.Should().Be(seasonNumber);
            result.First().Ignored.Should().Be(isIgnored);
        }

        [Test]
        public void DeleteSeason_should_remove_season_from_database()
        {
            WithRealDb();

            var fakeSeason = Builder<Season>.CreateNew().Build();

            Db.Insert(fakeSeason);

            //Act
            Mocker.Resolve<SeasonProvider>().Delete(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            //Assert
            var result = Db.Fetch<Season>();
            result.Should().BeEmpty();
        }

        [Test]
        public void SetIgnore_should_update_ignored_status()
        {
            WithRealDb();

            var fakeSeason = Builder<Season>.CreateNew()
                .With(s => s.Ignored = false)
                .Build();

            var id = Db.Insert(fakeSeason);

            //Act
            Mocker.Resolve<SeasonProvider>().SetIgnore(fakeSeason.SeriesId, fakeSeason.SeasonNumber, true);

            //Assert
            var result = Db.SingleOrDefault<Season>(id);
            result.Ignored.Should().BeTrue();
        }

        [Test]
        public void IsIgnored_should_return_ignored_status_of_season()
        {
            WithRealDb();

            //Setup
            var fakeSeason = Builder<Season>.CreateNew()
                .With(s => s.Ignored = false)
                .Build();

            Db.Insert(fakeSeason);

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            //Assert
            result.Should().Be(fakeSeason.Ignored);
            Db.Fetch<Season>().Count.Should().Be(1);
        }

        [Test]
        public void IsIgnored_should_return_true_if_not_in_db_and_is_season_zero()
        {
            //Setup
            WithRealDb();

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(10, 0);

            //Assert
            result.Should().BeTrue();
            Db.Fetch<Season>().Should().HaveCount(1);
        }

        [Test]
        public void IsIgnored_should_return_false_if_not_in_db_and_is_season_one()
        {
            //Setup
            WithRealDb();

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(10, 1);

            //Assert
            result.Should().BeFalse();
            Db.Fetch<Season>().Should().HaveCount(1);
        }

        [Test]
        public void IsIgnored_should_return_false_if_not_in_db_and_previous_season_is_not_ignored()
        {
            //Setup
            WithRealDb();

            var lastSeason = Builder<Season>.CreateNew()
                .With(s => s.SeriesId = 10)
                .With(s => s.SeasonNumber = 4)
                .With(s => s.Ignored = false)
                .Build();

            Db.Insert(lastSeason);

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(10, 5);

            //Assert
            result.Should().BeFalse();
            Db.Fetch<Season>().Should().HaveCount(2);
        }

        [Test]
        public void IsIgnored_should_return_true_if_not_in_db_and_previous_season_is_ignored()
        {
            //Setup
            WithRealDb();

            var lastSeason = Builder<Season>.CreateNew()
                .With(s => s.SeriesId = 10)
                .With(s => s.SeasonNumber = 4)
                .With(s => s.Ignored = true)
                .Build();

            Db.Insert(lastSeason);

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(10, 5);

            //Assert
            result.Should().BeTrue();
            Db.Fetch<Season>().Should().HaveCount(2);
        }

        [Test]
        public void IsIgnored_should_return_false_if_not_in_db_and_previous_season_does_not_exist()
        {
            //Setup
            WithRealDb();

            //Act
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(10, 5);

            //Assert
            result.Should().BeFalse();
            Db.Fetch<Season>().Should().HaveCount(1);
        }

        [Test]
        public void All_should_return_seasons_with_episodes()
        {
            const int seriesId = 10;

            //Setup
            WithRealDb();

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
            var result = Mocker.Resolve<SeasonProvider>().All(seriesId);

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
            var result = Mocker.Resolve<SeasonProvider>().All(seriesId);

            //Assert
            result.Should().HaveCount(5);

            foreach(var season in result)
            {
                season.Episodes.Count.Should().Be(2);
            }
        }
    }
}
