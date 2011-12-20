// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest_GetEpisodesByParseResult : CoreTest
    {
        [Test]
        public void Single_GetSeason_Episode_Exists()
        {
            WithRealDb();

            var fakeEpisode = Builder<Episode>.CreateNew()
                    .With(e => e.SeriesId = 1)
                    .With(e => e.SeasonNumber = 2)
                    .With(e => e.EpisodeNumber = 10)
                    .Build();

            var fakeSeries = Builder<Series>.CreateNew().Build();

            Db.Insert(fakeEpisode);
            Db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(1);
            parseResult.EpisodeTitle.Should().Be(fakeEpisode.Title);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
        }

        [Test]
        public void Single_GetSeason_Episode_Doesnt_exists_should_not_add()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().BeEmpty();
            Db.Fetch<Episode>().Should().HaveCount(0);
        }

        [Test]
        public void Single_GetSeason_Episode_Doesnt_exists_should_add()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(1);
            Db.Fetch<Episode>().Should().HaveCount(1);
        }

        [Test]
        public void Multi_GetSeason_Episode_Exists()
        {
            WithRealDb();

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

            Db.Insert(fakeEpisode);
            Db.Insert(fakeEpisode2);
            Db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10, 11 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(2);
            Db.Fetch<Episode>().Should().HaveCount(2);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
        }

        [Test]
        public void Multi_GetSeason_Episode_Doesnt_exists_should_not_add()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10, 11 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().BeEmpty();
            Db.Fetch<Episode>().Should().HaveCount(0);
        }

        [Test]
        public void Multi_GetSeason_Episode_Doesnt_exists_should_add()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 10, 11 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(2);
            Db.Fetch<Episode>().Should().HaveCount(2);
        }

        [Test]
        public void Get_Episode_Zero_Doesnt_Exist_Should_add_ignored()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 0 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(1);
            Db.Fetch<Episode>().Should().HaveCount(1);
            ep.First().Ignored.Should().BeTrue();
        }

        [Test]
        public void Get_Multi_Episode_Zero_Doesnt_Exist_Should_not_add_ignored()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();


            var parseResult = new EpisodeParseResult
                                  {
                                      Series = fakeSeries,
                                      SeasonNumber = 2,
                                      EpisodeNumbers = new List<int> { 0, 1 }
                                  };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult, true);

            ep.Should().HaveCount(2);
            Db.Fetch<Episode>().Should().HaveCount(2);
            ep.First().Ignored.Should().BeFalse();
        }

        [Test]
        [Description("GetEpisodeParseResult should return empty list if episode list is null")]
        public void GetEpisodeParseResult_should_return_empty_list_if_episode_list_is_null()
        {
            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(new EpisodeParseResult());
            //Assert
            episodes.Should().NotBeNull();
            episodes.Should().BeEmpty();
        }

        [Test]
        [Description("GetEpisodeParseResult should return empty list if episode list is empty")]
        public void GetEpisodeParseResult_should_return_empty_list_if_episode_list_is_empty()
        {
            //Act
            var episodes =
                    Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(new EpisodeParseResult { EpisodeNumbers = new List<int>() });
            //Assert
            episodes.Should().NotBeNull();
            episodes.Should().BeEmpty();
        }

        [Test]
        public void GetEpisodeParseResult_should_return_single_episode_when_air_date_is_provided()
        {
            //Setup
            var fakeEpisode = Builder<Episode>.CreateListOfSize(1)
                    .All()
                    .With(e => e.AirDate = DateTime.Today)
                    .Build()
                    .ToList();

            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = 1)
                    .Build();

            Mocker.GetMock<IDatabase>().Setup(
                                              s =>
                                              s.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(),
                                                                                    It.IsAny<Object[]>()))
                    .Returns(fakeEpisode);

            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>()
                    .GetEpisodesByParseResult(new EpisodeParseResult { AirDate = DateTime.Today, Series = fakeSeries },
                                              true);

            //Assert
            episodes.Should().HaveCount(1);
            episodes.First().AirDate.Should().Be(DateTime.Today);

            Mocker.GetMock<IDatabase>().Verify(v => v.Insert(It.IsAny<Episode>()), Times.Never());
        }

        [Test]
        public void GetEpisodeParseResult_get_daily_should_add_new_episode()
        {
            //Setup
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = 1)
                    .Build();

            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>()
                    .GetEpisodesByParseResult(new EpisodeParseResult { AirDate = DateTime.Today, Series = fakeSeries },
                                              true);

            //Assert
            episodes.Should().HaveCount(1);
            episodes.First().AirDate.Should().Be(DateTime.Today);

            var episodesInDb = Db.Fetch<Episode>();

            episodesInDb.Should().HaveCount(1);
        }

        [Test]
        public void GetEpisodeParseResult_get_daily_should_not_add_new_episode_when_auto_add_is_false()
        {
            //Setup
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = 1)
                    .Build();

            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>()
                    .GetEpisodesByParseResult(new EpisodeParseResult { AirDate = DateTime.Today, Series = fakeSeries }, false);

            //Assert
            episodes.Should().BeEmpty();
            Db.Fetch<Episode>().Should().BeEmpty();
        }

        [Test]
        public void GetEpisodeParseResult_should_return_multiple_titles_for_multiple_episodes()
        {
            WithRealDb();

            var fakeEpisode = Builder<Episode>.CreateNew()
                    .With(e => e.SeriesId = 1)
                    .With(e => e.SeasonNumber = 2)
                    .With(e => e.EpisodeNumber = 10)
                    .With(e => e.Title = "Title1")
                    .Build();

            var fakeEpisode2 = Builder<Episode>.CreateNew()
                    .With(e => e.SeriesId = 1)
                    .With(e => e.SeasonNumber = 2)
                    .With(e => e.EpisodeNumber = 11)
                    .With(e => e.Title = "Title2")
                    .Build();

            var fakeSeries = Builder<Series>.CreateNew().Build();

            Db.Insert(fakeEpisode);
            Db.Insert(fakeEpisode2);
            Db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { fakeEpisode.EpisodeNumber, fakeEpisode2.EpisodeNumber }
            };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(2);
            Db.Fetch<Episode>().Should().HaveCount(2);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
            parseResult.EpisodeTitle.Should().Be(fakeEpisode.Title + " + " + fakeEpisode2.Title);
        }

        [Test]
        public void GetEpisodeParseResult_should_return_single_title_for_single_episode()
        {
            WithRealDb();

            var fakeEpisode = Builder<Episode>.CreateNew()
                    .With(e => e.SeriesId = 1)
                    .With(e => e.SeasonNumber = 2)
                    .With(e => e.EpisodeNumber = 10)
                    .With(e => e.Title = "Title1")
                    .Build();

            var fakeSeries = Builder<Series>.CreateNew().Build();

            Db.Insert(fakeEpisode);
            Db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                SeasonNumber = 2,
                EpisodeNumbers = new List<int> { fakeEpisode.EpisodeNumber }
            };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().HaveCount(1);
            Db.Fetch<Episode>().Should().HaveCount(1);
            ep.First().ShouldHave().AllPropertiesBut(e => e.Series);
            parseResult.EpisodeTitle.Should().Be(fakeEpisode.Title);
        }

        [Test]
        public void GetEpisodeParseResult_should_log_warning_when_series_is_not_dailt_but_parsed_daily()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.IsDaily = false)
                .Build();

            Db.Insert(fakeSeries);

            var parseResult = new EpisodeParseResult
            {
                Series = fakeSeries,
                AirDate = DateTime.Today
            };

            var ep = Mocker.Resolve<EpisodeProvider>().GetEpisodesByParseResult(parseResult);

            ep.Should().BeEmpty();
            ExceptionVerification.ExcpectedWarns(1);
        }
    }
}
