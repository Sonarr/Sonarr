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

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonProviderTest : CoreTest
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
                .With(c => c.SeasonNumber = fakeSeason.SeasonId)
                .With(c => c.Ignored = !ignoreFlag)
                .Build().ToList();

            fakeEpisodes.ForEach(c => Db.Insert(c));

            var id = Db.Insert(fakeSeason);

            //Act
            Mocker.Resolve<SeasonProvider>().SetIgnore(fakeSeason.SeriesId, fakeSeason.SeasonNumber, ignoreFlag);

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
            var result = Mocker.Resolve<SeasonProvider>().IsIgnored(fakeSeason.SeriesId, fakeSeason.SeasonNumber);

            //Assert
            result.Should().Be(ignoreFlag);
        }

        [Test]
        public void IsIgnored_should_throw_an_exception_if_not_in_db()
        {
            Assert.Throws<InvalidOperationException>(() => Mocker.Resolve<SeasonProvider>().IsIgnored(10, 0));
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

            foreach (var season in result)
            {
                season.Episodes.Count.Should().Be(2);
                season.Episodes.Should().OnlyContain(c => c.SeasonNumber == season.SeasonNumber);
            }
        }

        [Test]
        public void EnsureSeason_should_add_all_seasons_for_new_series()
        {
            var seasons = new[] { 0, 1, 2, 3, 4, 5 };
            Mocker.Resolve<SeasonProvider>().EnsureSeasons(12, seasons);

            Mocker.Resolve<SeasonProvider>().GetSeasons(12).SequenceEqual(seasons);
        }

        [Test]
        public void EnsureSeason_should_add_missing_seasons()
        {
            var seasonsA = new[] { 0, 1, 2, 3 };
            var seasonsB = new[] { 0, 1, 2, 3, 4, 5 };
            Mocker.Resolve<SeasonProvider>().EnsureSeasons(12, seasonsA);
            Mocker.Resolve<SeasonProvider>().GetSeasons(12).SequenceEqual(seasonsA);

            Mocker.Resolve<SeasonProvider>().EnsureSeasons(12, seasonsB);
            Mocker.Resolve<SeasonProvider>().GetSeasons(12).SequenceEqual(seasonsB);
        }


        [Test]
        public void EnsureSeason_marks_season_zero_as_ignored()
        {
            var seasons = new[] { 0, 1, 2, 3 };

            Mocker.Resolve<SeasonProvider>().EnsureSeasons(12, seasons);
            Db.Fetch<Season>().Should().Contain(c => c.SeasonNumber == 0 && c.Ignored);
        }


        [Test]
        public void EnsureSeason_none_zero_seasons_arent_ignored()
        {
            var seasons = new[] { 1, 2, 3 };

            Mocker.Resolve<SeasonProvider>().EnsureSeasons(12, seasons);
            Db.Fetch<Season>().Should().OnlyContain(c => c.Ignored == false);
        }

        [Test]
        public void GetSeason_should_return_seasons_for_specified_series_only()
        {
            var seriesA = new[] { 1, 2, 3 };
            var seriesB = new[] { 4, 5, 6 };

            Mocker.Resolve<SeasonProvider>().EnsureSeasons(1, seriesA);
            Mocker.Resolve<SeasonProvider>().EnsureSeasons(2, seriesB);

            Mocker.Resolve<SeasonProvider>().GetSeasons(1).Should().Equal(seriesA);
            Mocker.Resolve<SeasonProvider>().GetSeasons(2).Should().Equal(seriesB);
        }


        [Test]
        public void GetSeason_should_return_emptylist_if_series_doesnt_exist()
        {
            Mocker.Resolve<SeasonProvider>().GetSeasons(1).Should().BeEmpty();
        }

    }
}
