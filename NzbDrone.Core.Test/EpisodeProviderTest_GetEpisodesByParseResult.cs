// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest_GetEpisodesByParseResult : TestBase
    {

        [Test]
        public void Single_GetSeason_Episode_Exists()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(db);

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 2)
                .With(e => e.EpisodeNumber = 10)
                .Build();

            var fakeSeries = Builder<Series>.CreateNew().Build();

            db.Insert(fakeEpisode);
            db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10 }
                                  };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(1);
            parseResult.EpisodeTitle.Should().Be(fakeEpisode.Title);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
        }

        [Test]
        public void Single_GetSeason_Episode_Doesnt_exists_should_not_add()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 10 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().BeEmpty();
            db.Fetch<Episode>().Should().HaveCount(0);
        }

        [Test]
        public void Single_GetSeason_Episode_Doesnt_exists_should_add()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 10 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(1);
            db.Fetch<Episode>().Should().HaveCount(1);
        }

        [Test]
        public void Multi_GetSeason_Episode_Exists()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(db);

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 2)
                .With(e => e.EpisodeNumber = 10)
                .Build();

            var fakeEpisode2 = Builder<Episode>.CreateNew()
               .With(e => e.SeriesId = 1)
               .With(e => e.SeasonNumber = 2)
               .With(e => e.EpisodeNumber = 11)
               .Build();

            var fakeSeries = Builder<Series>.CreateNew().Build();

            db.Insert(fakeEpisode);
            db.Insert(fakeEpisode2);
            db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 10, 11 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(2);
            db.Fetch<Episode>().Should().HaveCount(2);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
            parseResult.EpisodeTitle.Should().Be(fakeEpisode.Title);
        }

        [Test]
        public void Multi_GetSeason_Episode_Doesnt_exists_should_not_add()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 10, 11 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().BeEmpty();
            db.Fetch<Episode>().Should().HaveCount(0);
        }

        [Test]
        public void Multi_GetSeason_Episode_Doesnt_exists_should_add()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 10, 11 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(2);
            db.Fetch<Episode>().Should().HaveCount(2);
        }

        [Test]
        public void Get_Episode_Zero_Doesnt_Exist_Should_add_ignored()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 0 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(1);
            db.Fetch<Episode>().Should().HaveCount(1);
            ep.First().Ignored.Should().BeTrue();
        }

        [Test]
        public void Get_Multi_Episode_Zero_Doesnt_Exist_Should_not_add_ignored()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { 0, 1 }
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(2);
            db.Fetch<Episode>().Should().HaveCount(2);
            ep.First().Ignored.Should().BeFalse();
        }

        [Test]
        public void Full_Season_return_all_episodes_for_season()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .WhereAll()
                .Have(e => e.SeriesId = fakeSeries.SeriesId)
                .Have(e => e.SeasonNumber = 2)
                .Build();

            db.Insert(fakeSeries);
            db.InsertMany(fakeEpisodes);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int>(),
                FullSeason = true
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(10);
            db.Fetch<Episode>().Should().HaveCount(10);
        }

        [Test]
        public void No_Episodes_Not_a_proper_full_season_release()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .WhereAll()
                .Have(e => e.SeriesId = fakeSeries.SeriesId)
                .Have(e => e.SeasonNumber = 2)
                .Build();

            db.Insert(fakeSeries);
            db.InsertMany(fakeEpisodes);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int>(),
                FullSeason = false
            };

            var ep = mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(0);
            db.Fetch<Episode>().Should().HaveCount(10);
        }
    }
}