// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
    public class EpisodeProviderTest_DeleteInvalidEpisodes : TestBase
    {
        [Test]
        public void Delete_None_Valid_TvDbEpisodeId()
        {
            //Setup
            const int seriesId = 71663;
            const int episodeCount = 10;

            var tvDbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.TvDbEpisodeId = tvDbSeries.Episodes.First().Id)
                .Build();

            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeSeries);
            db.Insert(fakeEpisode);

            //Act
            mocker.Resolve<EpisodeProvider>().DeleteInvalidEpisodes(fakeSeries, tvDbSeries);

            //Assert
            var result = db.Fetch<Episode>();
            result.Should().HaveCount(1);
        }

        [Test]
        public void Delete_None_TvDbEpisodeId_is_zero()
        {
            //Setup
            const int seriesId = 71663;
            const int episodeCount = 10;

            var tvDbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.TvDbEpisodeId = 0)
                .Build();

            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeSeries);
            db.Insert(fakeEpisode);

            //Act
            mocker.Resolve<EpisodeProvider>().DeleteInvalidEpisodes(fakeSeries, tvDbSeries);

            //Assert
            var result = db.Fetch<Episode>();
            result.Should().HaveCount(1);
        }

        [Test]
        public void Delete_None_TvDbEpisodeId_is_null()
        {
            //Setup
            const int seriesId = 71663;
            const int episodeCount = 10;

            var tvDbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.TvDbEpisodeId = null)
                .Build();

            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeSeries);
            db.Insert(fakeEpisode);

            //Act
            mocker.Resolve<EpisodeProvider>().DeleteInvalidEpisodes(fakeSeries, tvDbSeries);

            //Assert
            var result = db.Fetch<Episode>();
            result.Should().HaveCount(1);
        }

        [Test]
        public void Delete_TvDbId()
        {
            //Setup
            const int seriesId = 71663;
            const int episodeCount = 10;

            var tvDbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.SeasonNumber = 20)
                .With(e => e.EpisodeNumber = 20)
                .With(e => e.TvDbEpisodeId = 300)
                .Build();

            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeSeries);
            db.Insert(fakeEpisode);

            //Act
            mocker.Resolve<EpisodeProvider>().DeleteInvalidEpisodes(fakeSeries, tvDbSeries);

            //Assert
            var result = db.Fetch<Episode>();
            result.Should().HaveCount(0);
        }

        //Other series, by season/episode + by tvdbid
        [Test]
        public void Delete_TvDbId_multiple_series()
        {
            //Setup
            const int seriesId = 71663;
            const int episodeCount = 10;

            var tvDbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = seriesId)
                .With(e => e.SeasonNumber = 20)
                .With(e => e.EpisodeNumber = 20)
                .With(e => e.TvDbEpisodeId = 300)
                .Build();

            //Other Series
            var otherFakeSeries = Builder<Series>.CreateNew()
                .With(c => c.SeriesId = 12345)
                .Build();

            var otherFakeEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 12345)
                .With(e => e.SeasonNumber = 20)
                .With(e => e.EpisodeNumber = 20)
                .With(e => e.TvDbEpisodeId = 300)
                .Build();

            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeSeries);
            db.Insert(fakeEpisode);
            db.Insert(otherFakeSeries);
            db.Insert(otherFakeEpisode);

            //Act
            mocker.Resolve<EpisodeProvider>().DeleteInvalidEpisodes(fakeSeries, tvDbSeries);

            //Assert
            var result = db.Fetch<Episode>();
            result.Should().HaveCount(1);
        }
    }
}