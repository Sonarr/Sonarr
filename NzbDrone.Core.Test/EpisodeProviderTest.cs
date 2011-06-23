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
    public class EpisodeProviderTest : TestBase
    {
        [Test]
        public void GetEpisodes_exists()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).Have(e => e.EpisodeFileId = 0).Build();


            db.InsertMany(fakeEpisodes);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
        }


        [Test]
        public void GetEpisodes_by_season_episode_exists()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.EpisodeNumber = 1)
                .And(e => e.SeasonNumber = 2)
                .With(e => e.EpisodeFileId = 0).Build();

            db.Insert(fakeEpisodes);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(fakeSeries.SeriesId, 2, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes);
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
        }

        [Test]
        public void GetEpisodes_by_season_episode_doesnt_exists()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);



            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.Should().BeNull();
        }

        [Test]
        public void GetEpisode_with_EpisodeFile()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).WhereTheFirst(1).Have(e => e.EpisodeFileId = 1).Have(e => e.EpisodeFile = fakeFile).Build();


            db.InsertMany(fakeEpisodes);
            db.Insert(fakeFile);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
        public void GetEpisodes_invalid_series()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            mocker.Resolve<SeriesProvider>();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                    .WhereAll().Have(e => e.SeriesId = 1).Build();


            db.InsertMany(fakeEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().GetEpisode(1);
        }

        [Test]
        public void AttachSeries_empty_list()
        {
            var mocker = new AutoMoqer();


            //Act
            var result = mocker.Resolve<EpisodeProvider>().AttachSeries(new List<Episode>());

            //Assert
            result.Should().HaveCount(0);
        }

        [Test]
        public void AttachSeries_list_success()
        {
            var mocker = new AutoMoqer();

            var fakeSeries = Builder<Series>.CreateNew().With(s => s.SeriesId = 12).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 12).Build();

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(12))
                .Returns(fakeSeries);

            //Act

            fakeEpisodes.Should().OnlyContain(e => e.Series == null);
            var returnedSeries = mocker.Resolve<EpisodeProvider>().AttachSeries(fakeEpisodes);

            //Assert
            fakeEpisodes.Should().OnlyContain(e => e.Series == fakeSeries);
            returnedSeries.Should().BeEquivalentTo(fakeEpisodes);
        }

        [Test]
        public void AttachSeries_null_episode_should_return_null()
        {
            var mocker = new AutoMoqer();

            Episode episode = null;

            //Act
            var result = mocker.Resolve<EpisodeProvider>().AttachSeries(episode);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public void AttachSeries_single_success()
        {
            var mocker = new AutoMoqer();

            var fakeSeries = Builder<Series>.CreateNew().With(s => s.SeriesId = 12).Build();
            var fakeEpisodes = Builder<Episode>.CreateNew().With(e => e.SeriesId = 12).Build();

            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.GetSeries(12))
                .Returns(fakeSeries);

            //Act
            var returnedEpisode = mocker.Resolve<EpisodeProvider>().AttachSeries(fakeEpisodes);

            //Assert
            fakeEpisodes.Series.Should().Be(fakeSeries);
            returnedEpisode.Should().Be(fakeEpisodes);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
        public void AttachSeries_single_invalid_series()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyDatabase());
            mocker.Resolve<SeriesProvider>();
            var fakeEpisodes = Builder<Episode>.CreateNew().With(e => e.SeriesId = 12).Build();

            //Act
            var returnedEpisode = mocker.Resolve<EpisodeProvider>().AttachSeries(fakeEpisodes);
        }

        [Test]
        public void GetEpisodesBySeason_success()
        {
            var episodes = Builder<Episode>.CreateListOfSize(10)
                .WhereAll().Have(c => c.SeriesId = 12)
                .WhereTheFirst(5).Have(c => c.SeasonNumber = 1)
                .AndTheRemaining().Have(c => c.SeasonNumber = 2).Build();

            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer();
            mocker.SetConstant(db);

            episodes.ToList().ForEach(c => db.Insert(c));

            //Act
            var seasonEposodes = mocker.Resolve<EpisodeProvider>().GetEpisodesBySeason(12, 2);

            //Assert
            db.Fetch<Episode>().Should().HaveCount(10);
            seasonEposodes.Should().HaveCount(5);
        }

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

            mocker.SetConstant(MockLib.GetEmptyDatabase());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var actualCount = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList().Count;
            mocker.GetMock<TvDbProvider>().VerifyAll();
            actualCount.Should().Be(episodeCount);
            mocker.VerifyAllMocks();
        }

        [Test]
        public void RefreshEpisodeInfo_should_set_older_than_1900_to_null()
        {
            //Arrange
            const int seriesId = 71663;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(10).
                                               WhereAll()
                                               .Have(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .WhereTheFirst(7).Have(e => e.FirstAired = new DateTime(1800, 1, 1))
                                               .AndTheRemaining().Have(e => e.FirstAired = DateTime.Now)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();


            var mocker = new AutoMoqer();

            mocker.SetConstant(MockLib.GetEmptyDatabase());

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var storedEpisodes = mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            storedEpisodes.Should().HaveCount(10);
            storedEpisodes.Where(e => e.AirDate == null).Should().HaveCount(7);
            storedEpisodes.Where(e => e.AirDate != null).Should().HaveCount(3);

            mocker.VerifyAllMocks();
        }

        [Test]
        public void new_episodes_only_calls_Insert()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var currentEpisodes = new List<Episode>();


            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(tvdbSeries);

            mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(currentEpisodes);


            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.GetMock<IDatabase>().Verify(c => c.Insert(It.IsAny<Object>()), Times.Exactly(tvdbSeries.Episodes.Count));
            mocker.GetMock<IDatabase>().Verify(c => c.Update(It.IsAny<Object>()), Times.Never());

            mocker.VerifyAllMocks();
        }

        [Test]
        public void existing_episodes_only_calls_Update()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var currentEpisodes = new List<Episode>();
            foreach (var tvDbEpisode in tvdbSeries.Episodes)
            {
                currentEpisodes.Add(new Episode { TvDbEpisodeId = tvDbEpisode.Id });
            }

            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(tvdbSeries);

            mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(currentEpisodes);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.GetMock<IDatabase>().Verify(c => c.Insert(It.IsAny<Object>()), Times.Never());
            mocker.GetMock<IDatabase>().Verify(c => c.Update(It.IsAny<Object>()), Times.Exactly(tvdbSeries.Episodes.Count));
            mocker.VerifyAllMocks();
        }

        [Test]
        public void should_try_to_get_existing_episode_using_tvdbid_first()
        {
            const int seriesId = 71663;
            var fakeTvDbResult = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Id = seriesId)
                .With(c => c.Episodes = new List<TvdbEpisode>(
                                                                Builder<TvdbEpisode>.CreateListOfSize(1)
                                                                .WhereAll().Have(g => g.Id = 99)
                                                                .Build())
                                                             )
                .Build();

            var fakeEpisodeList = new List<Episode> { new Episode { TvDbEpisodeId = 99, SeasonNumber = 10, EpisodeNumber = 10 } };
            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var mocker = new AutoMoqer();
            mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(fakeEpisodeList);

            mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(fakeTvDbResult);

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<IDatabase>().Verify(c => c.Update(fakeEpisodeList[0]), Times.Once());
        }

        [Test]
        public void should_try_to_get_existing_episode_using_tvdbid_first_then_season_episode()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
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

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(tvdbSeries);

            mocker.GetMock<IDatabase>()
               .Setup(d => d.Fetch<Episode, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(new List<Episode> { localEpisode });

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<IDatabase>().Verify(c => c.Update(localEpisode), Times.Once());
        }

        [Test]
        public void existing_episodes_keep_their_episodeId_file_id()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var currentEpisodes = new List<Episode>();
            foreach (var tvDbEpisode in tvdbSeries.Episodes)
            {
                currentEpisodes.Add(new Episode { TvDbEpisodeId = tvDbEpisode.Id, EpisodeId = 99, EpisodeFileId = 69, Ignored = true });
            }

            var mocker = new AutoMoqer();

            mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true))
                .Returns(tvdbSeries);

            var updatedEpisodes = new List<Episode>();

            mocker.GetMock<IDatabase>()
                 .Setup(d => d.Fetch<Episode, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                 .Returns(currentEpisodes);

            mocker.GetMock<IDatabase>()
                .Setup(c => c.Update(It.IsAny<Episode>()))
                .Returns(1)
                .Callback<Episode>(ep => updatedEpisodes.Add(ep));

            //Act
            mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            updatedEpisodes.Should().HaveSameCount(tvdbSeries.Episodes);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeId == 99);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeFileId == 69);
            updatedEpisodes.Should().OnlyContain(c => c.Ignored == true);
        }

        [Test]
        public void IsSeasonIgnored_should_return_true_if_all_episodes_ignored()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.SetConstant(db);

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .WhereAll()
                .Have(c => c.Ignored = true)
                .Have(c => c.SeriesId = 10)
                .Have(c => c.SeasonNumber = 2)
                .Build();

            episodes.ToList().ForEach(c => db.Insert(c));

            //Act
            var result = mocker.Resolve<EpisodeProvider>().IsIgnored(10, 2);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsSeasonIgnored_should_return_false_if_none_of_episodes_are_ignored()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.SetConstant(db);

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .WhereAll()
                .Have(c => c.Ignored = false)
                .Have(c => c.SeriesId = 10)
                .Have(c => c.SeasonNumber = 2)
                .Build();

            episodes.ToList().ForEach(c => db.Insert(c));

            //Act
            var result = mocker.Resolve<EpisodeProvider>().IsIgnored(10, 2);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsSeasonIgnored_should_return_false_if_some_of_episodes_are_ignored()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.SetConstant(db);

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .WhereAll()
                .Have(c => c.SeriesId = 10)
                .Have(c => c.SeasonNumber = 2)
                 .Have(c => c.Ignored = true)
                .Build();

            episodes[2].Ignored = false;


            episodes.ToList().ForEach(c => db.Insert(c));

            //Act
            var result = mocker.Resolve<EpisodeProvider>().IsIgnored(10, 2);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsSeasonIgnored_should_return_true_if_invalid_series()
        {
            var db = MockLib.GetEmptyDatabase();
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.SetConstant(db);

            //Act
            var result = mocker.Resolve<EpisodeProvider>().IsIgnored(10, 2);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        [Explicit]
        public void Add_daily_show_episodes()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);
            mocker.Resolve<TvDbProvider>();

            mocker.GetMock<ConfigProvider>()
                .Setup(e => e.DefaultQualityProfile).Returns(1);

            db.Insert(Builder<QualityProfile>.CreateNew().Build());


            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("c:\\test\\", tvDbSeriesId, 1);

            var episodeProvider = mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(seriesProvider.GetSeries(tvDbSeriesId));

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            episodes.Should().NotBeEmpty();
        }

        [Test]
        public void GetEpisode_by_Season_Episode_none_existing()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);


            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.Should().BeNull();
        }


        [Test]
        public void GetEpisode_by_Season_Episode_with_EpisodeFile()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).WhereTheFirst(1).Have(e => e.EpisodeFileId = 1).Have(e => e.EpisodeFile = fakeFile).Build();

            db.InsertMany(fakeEpisodes);
            db.Insert(fakeFile);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }



        [Test]
        public void GetEpisode_by_Season_Episode_without_EpisodeFile()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).WhereTheFirst(1).Have(e => e.EpisodeFileId = 0).Build();

            db.InsertMany(fakeEpisodes);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
            episode.EpisodeFile.Should().BeNull();
        }




        [Test]
        public void GetEpisode_by_AirDate_with_EpisodeFile()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).WhereTheFirst(1).Have(e => e.EpisodeFileId = 1).Have(e => e.EpisodeFile = fakeFile).Build();

            db.InsertMany(fakeEpisodes);
            db.Insert(fakeFile);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, fakeEpisodes[0].AirDate.Value);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }

        [Test]
        public void GetEpisode_by_AirDate_without_EpisodeFile()
        {
            var mocker = new AutoMoqer();
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll().Have(e => e.SeriesId = 1).WhereTheFirst(1).Have(e => e.EpisodeFileId = 0).Build();

            db.InsertMany(fakeEpisodes);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetSeries(1))
                .Returns(fakeSeries);

            //Act
            var episode = mocker.Resolve<EpisodeProvider>().GetEpisode(1, fakeEpisodes[0].AirDate.Value);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllProperties().EqualTo(fakeSeries);
            episode.EpisodeFile.Should().BeNull();
        }



    }
}