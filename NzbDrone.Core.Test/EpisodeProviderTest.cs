// ReSharper disable RedundantUsingDirective
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest : TestBase
    {
        [Test]
        public void RefreshEpisodeInfo_emptyRepo()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyRepository());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var actualCount = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).Count;
            mocker.GetMock<TvDbProvider>().VerifyAll();
            actualCount.Should().Be(episodeCount);
            mocker.VerifyAllMocks();
        }


        [Test]
        public void EnsureSeason_is_called_once_per_season()
        {
            const int seriesId = 71663;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(6).
                                                                  WhereAll().Have(
                                                                      l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                                                  .WhereTheFirst(3).Have(d => d.SeasonNumber = 1).And(
                                                                      d => d.SeasonId = 11)
                                                                  .AndTheRemaining().Have(d => d.SeasonNumber = 2).And(
                                                                      d => d.SeasonId = 22)
                                                                  .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);

            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);


            mocker.GetMock<SeasonProvider>().Verify(c => c.EnsureSeason(seriesId, 11, 1), Times.Once());
            mocker.GetMock<SeasonProvider>().Verify(c => c.EnsureSeason(seriesId, 22, 2), Times.Once());

            mocker.VerifyAllMocks();
        }


        [Test]
        public void new_episodes_only_calls_AddMany()
        {
            const int seriesId = 71663;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.GetMock<IRepository>().Verify(
                c => c.AddMany(It.Is<IEnumerable<Episode>>(e => e.Count() == fakeEpisodes.Episodes.Count)), Times.Once());
            mocker.GetMock<IRepository>().Verify(c => c.UpdateMany(It.Is<IEnumerable<Episode>>(e => e.Count() == 0)),
                                                 Times.AtMostOnce());
            mocker.VerifyAllMocks();
        }


        [Test]
        public void existing_episodes_only_calls_UpdateMany()
        {
            const int seriesId = 71663;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);

            mocker.GetMock<IRepository>()
                .Setup(c => c.Single(It.IsAny<Expression<Func<Episode, bool>>>()))
                .Returns(Builder<Episode>.CreateNew().Build);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.GetMock<IRepository>().Verify(c => c.AddMany(It.Is<IEnumerable<Episode>>(e => e.Count() == 0)),
                                                 Times.AtMostOnce());
            mocker.GetMock<IRepository>().Verify(
                c => c.UpdateMany(It.Is<IEnumerable<Episode>>(e => e.Count() == fakeEpisodes.Episodes.Count)),
                Times.Once());
            mocker.VerifyAllMocks();
        }


        [Test]
        public void should_try_to_get_existing_episode_using_tvdbid_first()
        {
            const int seriesId = 71663;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Id = seriesId)
                .With(c => c.Episodes = new List<TvdbEpisode>(
                                                                Builder<TvdbEpisode>.CreateListOfSize(1)
                                                                .WhereAll().Have(g => g.Id = 99)
                                                                .Build())
                                                             )
                .Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            var repo = MockLib.GetEmptyRepository();
            repo.Add<Episode>(
                Builder<Episode>.CreateNew().With(c => c.TvDbEpisodeId = fakeEpisodes.Episodes[0].Id).Build());
            mocker.SetConstant(repo);

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.VerifyAllMocks();
            repo.All<Episode>().Should().HaveCount(1);
        }

        [Test]
        public void should_try_to_get_existing_episode_using_tvdbid_first_then_season_episode()
        {
            const int seriesId = 71663;
            var fakeEpisodes = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Id = seriesId)
                .With(c => c.Episodes = new List<TvdbEpisode>{
                                                                Builder<TvdbEpisode>.CreateNew()
                                                                .With(g => g.Id = 99)
                                                                .With(g => g.SeasonNumber = 4)
                                                                .With(g => g.EpisodeNumber = 15)
                                                                .With(g=>g.SeriesId = seriesId)
                                                                .Build()
                                                               })
                .Build();

            var localEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = seriesId)
                .With(c => c.SeasonNumber = 4)
                .With(c => c.EpisodeNumber = 15)
                .Build();


            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            var repo = MockLib.GetEmptyRepository();
            repo.Add<Episode>(localEpisode);
            mocker.SetConstant(repo);

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.VerifyAllMocks();
            repo.All<Episode>().Should().HaveCount(1);
        }


        [Test]
        public void existing_episodes_keep_their_episodeId_file_id()
        {
            const int seriesId = 71663;
            var faketvDbResponse = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();
            var fakeEpisode =
                Builder<Episode>.CreateNew().With(c => c.EpisodeFileId = 69).And(c => c.EpisodeId = 99).Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(faketvDbResponse);

            IEnumerable<Episode> updatedEpisodes = null;

            mocker.GetMock<IRepository>()
                .Setup(c => c.Single(It.IsAny<Expression<Func<Episode, bool>>>()))
                .Returns(fakeEpisode);

            mocker.GetMock<IRepository>()
                .Setup(c => c.UpdateMany(It.IsAny<IEnumerable<Episode>>()))
                .Returns(faketvDbResponse.Episodes.Count)
                .Callback<IEnumerable<Episode>>(r => updatedEpisodes = r);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.GetMock<IRepository>().Verify(c => c.AddMany(It.Is<IEnumerable<Episode>>(e => e.Count() == 0)),
                                                 Times.AtMostOnce());
            mocker.GetMock<IRepository>().Verify(
                c => c.UpdateMany(It.Is<IEnumerable<Episode>>(e => e.Count() == faketvDbResponse.Episodes.Count)),
                Times.Once());
            mocker.GetMock<IRepository>().Verify(
                c =>
                c.UpdateMany(
                    It.Is<IEnumerable<Episode>>(
                        e => e.Where(g => g.EpisodeFileId == 69).Count() == faketvDbResponse.Episodes.Count)),
                Times.Once());


            updatedEpisodes.Should().HaveSameCount(faketvDbResponse.Episodes);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeId == 99);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeFileId == 69);
        }


        [Test]
        [Explicit]
        public void Add_daily_show_episodes()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyRepository());
            mocker.Resolve<TvDbProvider>();
            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("c:\\test\\", tvDbSeriesId, 0);

            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(seriesProvider.GetSeries(tvDbSeriesId));

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            episodes.Should().NotBeEmpty();
        }
    }
}