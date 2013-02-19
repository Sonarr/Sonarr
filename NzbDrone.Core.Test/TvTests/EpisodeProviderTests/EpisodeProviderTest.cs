// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Test.TvTests.EpisodeProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeProviderTest : SqlCeTest
    {
        [Test]
        public void GetEpisodes_exists()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).With(e => e.EpisodeFileId = 0).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
        }

        [Test]
        public void GetEpisodes_by_season_episode_exists()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 1)
                .Build();
            var fakeEpisodes = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 1)
                .With(e => e.EpisodeNumber = 1)
                .And(e => e.SeasonNumber = 2)
                .With(e => e.EpisodeFileId = 0).Build();

            Db.Insert(fakeSeries);
            Db.Insert(fakeEpisodes);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(fakeSeries.SeriesId, 2, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes);
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
        }

        [Test]
        public void GetEpisodes_by_season_episode_doesnt_exists()
        {
            WithRealDb();

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.Should().BeNull();
        }

        [Test]
        public void GetEpisode_with_EpisodeFile()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).With(c => c.Quality = QualityTypes.SDTV).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 1).With(e => e.EpisodeFile = fakeFile).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);
            Db.Insert(fakeFile);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
        public void GetEpisodes_invalid_series()
        {
            WithRealDb();

            Mocker.Resolve<SeriesProvider>();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                    .All().With(e => e.SeriesId = 1).Build();


            Db.InsertMany(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().GetEpisode(1);
        }
     
        [Test]
        public void GetEpisodesBySeason_success()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .All().With(c => c.SeriesId = 12).And(c => c.SeasonNumber = 2)
                .TheFirst(5).With(c => c.SeasonNumber = 1)
                .Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(episodes);

            //Act
            var seasonEposodes = Mocker.Resolve<EpisodeProvider>().GetEpisodesBySeason(12, 2);

            //Assert
            Db.Fetch<Episode>().Should().HaveCount(10);
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
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var actualCount = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList().Count;
            Mocker.GetMock<TvDbProvider>().VerifyAll();
            actualCount.Should().Be(episodeCount);
        }

        [Test]
        public void RefreshEpisodeInfo_should_set_older_than_1900_to_null()
        {
            //Arrange
            const int seriesId = 71663;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(10).
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a")).And(e => e.FirstAired = DateTime.Now)
                                               .TheFirst(7).With(e => e.FirstAired = new DateTime(1800, 1, 1))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var storedEpisodes = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            storedEpisodes.Should().HaveCount(10);
            storedEpisodes.Where(e => e.AirDate == null).Should().HaveCount(7);
            storedEpisodes.Where(e => e.AirDate != null).Should().HaveCount(3);
        }

        [Test]
        public void RefreshEpisodeInfo_should_set_older_than_1900_to_null_for_existing_episodes()
        {
            //Arrange
            const int seriesId = 71663;

            var fakeEpisode = Builder<Episode>.CreateNew()
                    .With(e => e.TvDbEpisodeId = 12345)
                    .With(e => e.AirDate = DateTime.Today)
                    .Build();

            var fakeTvDbEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(1)
                         .All()
                         .With(l => l.Language = new TvdbLanguage(0, "eng", "a")).And(e => e.FirstAired = DateTime.Now)
                         .TheFirst(1).With(e => e.FirstAired = new DateTime(1800, 1, 1))
                         .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();
            Db.Insert(fakeSeries);
            Db.Insert(fakeEpisode);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeTvDbEpisodes);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var storedEpisodes = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            storedEpisodes.Should().HaveCount(1);
            storedEpisodes.Where(e => e.AirDate == null).Should().HaveCount(1);
        }

        [Test]
        public void RefreshEpisodeInfo_ignore_episode_zero()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .TheFirst(1)
                                               .With(e => e.EpisodeNumber = 0)
                                               .With(e => e.SeasonNumber = 15)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            Mocker.GetMock<TvDbProvider>().VerifyAll();
            result.Should().HaveCount(episodeCount);
            result.Where(e => e.EpisodeNumber == 0 && e.SeasonNumber == 15).Single().Ignored.Should().BeTrue();
        }

        [Test]
        public void RefreshEpisodeInfo_should_skip_future_episodes_with_no_title()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(a => c.FirstAired = DateTime.Now.AddDays(-2))
                                               .With(e => e.EpisodeName = "Something")
                                               .TheFirst(3)
                                               .With(e => e.EpisodeName = "")
                                               .With(e => e.FirstAired = DateTime.Now.AddDays(10))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            result.Should().HaveCount(episodeCount - 3);
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title) || c.AirDate < DateTime.Now);
        }

        [Test]
        public void RefreshEpisodeInfo_should_skip_older_than_1900_year_episodes_with_no_title()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(a => c.FirstAired = DateTime.Now.AddDays(-2))
                                               .With(e => e.EpisodeName = "Something")
                                               .TheFirst(3)
                                               .With(e => e.EpisodeName = "")
                                               .With(e => e.FirstAired = new DateTime(1889,1,1))
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            result.Should().HaveCount(episodeCount - 3);
            result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Title) || c.AirDate < DateTime.Now);
        }

        [Test]
        public void RefreshEpisodeInfo_should_add_future_episodes_with_title()
        {
            const int seriesId = 71663;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(10).
                                               All()
                                               .With(a => a.FirstAired = DateTime.Now.AddDays(10))
                                               .With(e => e.EpisodeName = "Something")
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            result.Should().HaveSameCount(fakeEpisodes.Episodes);
        }

        [Test]
        public void RefreshEpisodeInfo_should_add_old_episodes_with_no_title()
        {
            const int seriesId = 71663;


            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(10).
                                               All()
                                               .With(a => a.FirstAired = DateTime.Now.AddDays(-10))
                                               .With(e => e.EpisodeName = string.Empty)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            result.Should().HaveSameCount(fakeEpisodes.Episodes);
        }

        [Test]
        public void RefreshEpisodeInfo_ignore_season_zero()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 10;

            var fakeEpisodes = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .With(e => e.SeasonNumber = 0)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeEpisodes);

            Mocker.GetMock<SeasonProvider>()
                .Setup(s => s.IsIgnored(seriesId, 0))
                .Returns(true);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            Mocker.GetMock<TvDbProvider>().VerifyAll();
            result.Should().HaveCount(episodeCount);
            result.Where(e => e.Ignored).Should().HaveCount(episodeCount);
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

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            Mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(currentEpisodes);


            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            Mocker.GetMock<IDatabase>().Verify(c => c.InsertMany(It.Is<IEnumerable<Episode>>(l => l.Count() == 5)), Times.Once());
            Mocker.GetMock<IDatabase>().Verify(c => c.Update(It.IsAny<IEnumerable<Episode>>()), Times.Never());

            Mocker.VerifyAllMocks();
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
                currentEpisodes.Add(new Episode { TvDbEpisodeId = tvDbEpisode.Id, Series = fakeSeries });
            }

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            Mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(currentEpisodes);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            Mocker.GetMock<IDatabase>().Verify(c => c.InsertMany(It.Is<IEnumerable<Episode>>(l => l.Count() == 0)), Times.Once());
            Mocker.GetMock<IDatabase>().Verify(c => c.UpdateMany(It.Is<IEnumerable<Episode>>(l => l.Count() == 5)), Times.Once());
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void should_try_to_get_existing_episode_using_tvdbid_first()
        {
            const int seriesId = 71663;
            var fakeTvDbResult = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Id = seriesId)
                .With(c => c.Episodes = new List<TvdbEpisode>(
                                                                Builder<TvdbEpisode>.CreateListOfSize(1)
                                                                .All().With(g => g.Id = 99)
                                                                .Build())
                                                             )
                .Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();
            var fakeEpisodeList = new List<Episode> { new Episode { TvDbEpisodeId = 99, SeasonNumber = 10, EpisodeNumber = 10, Series = fakeSeries } };

            Mocker.GetMock<IDatabase>()
                .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(fakeEpisodeList);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(fakeTvDbResult);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<IDatabase>().Verify(c => c.UpdateMany(fakeEpisodeList), Times.Once());
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

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            Mocker.GetMock<IDatabase>()
               .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                .Returns(new List<Episode> { localEpisode });

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<IDatabase>().Verify(c => c.UpdateMany(new List<Episode> { localEpisode }), Times.Once());
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
                currentEpisodes.Add(new Episode
                                        {
                                                TvDbEpisodeId = tvDbEpisode.Id, 
                                                EpisodeId = 99,
                                                EpisodeFileId = 69,
                                                Ignored = true,
                                                Series = fakeSeries,
                                                EpisodeNumber = tvDbEpisode.EpisodeNumber,
                                                SeasonNumber = tvDbEpisode.SeasonNumber
                                        });
            }

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            var updatedEpisodes = new List<Episode>();

            Mocker.GetMock<IDatabase>()
                 .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                 .Returns(currentEpisodes);

            Mocker.GetMock<IDatabase>()
                .Setup(c => c.UpdateMany(It.IsAny<IEnumerable<Episode>>()))
                .Callback<IEnumerable<Episode>>(ep => updatedEpisodes = ep.ToList());

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            updatedEpisodes.Should().HaveSameCount(tvdbSeries.Episodes);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeId == 99);
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeFileId == 69);
            updatedEpisodes.Should().OnlyContain(c => c.Ignored == true);
        }

        [Test]
        public void existing_episodes_remote_their_episodeId_file_id_when_episode_number_doesnt_match_tvdbid()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var currentEpisodes = new List<Episode>();
            foreach (var tvDbEpisode in tvdbSeries.Episodes)
            {
                currentEpisodes.Add(new Episode
                {
                    TvDbEpisodeId = tvDbEpisode.Id,
                    EpisodeId = 99,
                    EpisodeFileId = 69,
                    Ignored = true,
                    Series = fakeSeries,
                    EpisodeNumber = tvDbEpisode.EpisodeNumber + 1,
                    SeasonNumber = tvDbEpisode.SeasonNumber
                });
            }

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            var updatedEpisodes = new List<Episode>();

            Mocker.GetMock<IDatabase>()
                 .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                 .Returns(currentEpisodes);

            Mocker.GetMock<IDatabase>()
                .Setup(c => c.UpdateMany(It.IsAny<IEnumerable<Episode>>()))
                .Callback<IEnumerable<Episode>>(ep => updatedEpisodes = ep.ToList());

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeFileId == 0);
        }

        [Test]
        public void existing_episodes_remote_their_episodeId_file_id_when_season_number_doesnt_match_tvdbid()
        {
            const int seriesId = 71663;
            var tvdbSeries = Builder<TvdbSeries>.CreateNew()
                .With(c => c.Episodes = new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(5).Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            var currentEpisodes = new List<Episode>();
            foreach (var tvDbEpisode in tvdbSeries.Episodes)
            {
                currentEpisodes.Add(new Episode
                {
                    TvDbEpisodeId = tvDbEpisode.Id,
                    EpisodeId = 99,
                    EpisodeFileId = 69,
                    Ignored = true,
                    Series = fakeSeries,
                    EpisodeNumber = tvDbEpisode.EpisodeNumber,
                    SeasonNumber = tvDbEpisode.SeasonNumber + 1
                });
            }

            Mocker.GetMock<TvDbProvider>(MockBehavior.Strict)
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            var updatedEpisodes = new List<Episode>();

            Mocker.GetMock<IDatabase>()
                 .Setup(d => d.Fetch<Episode, Series, EpisodeFile>(It.IsAny<String>(), It.IsAny<Object[]>()))
                 .Returns(currentEpisodes);

            Mocker.GetMock<IDatabase>()
                .Setup(c => c.UpdateMany(It.IsAny<IEnumerable<Episode>>()))
                .Callback<IEnumerable<Episode>>(ep => updatedEpisodes = ep.ToList());

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            updatedEpisodes.Should().OnlyContain(c => c.EpisodeFileId == 0);
        }

        [Test]
        public void RefreshEpisodeInfo_should_ignore_new_episode_for_ignored_season()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 2;

            var fakeEpisode = Builder<Episode>.CreateNew()
                    .With(e => e.SeasonNumber = 5)
                    .With(e => e.EpisodeNumber = 1)
                    .With(e => e.TvDbEpisodeId = 11)
                    .With(e => e.SeriesId = seriesId)
                    .With(e => e.Ignored = true)
                    .Build();

            var tvdbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .With(e => e.SeasonNumber = 5)
                                               .TheFirst(1)
                                               .With(e => e.EpisodeNumber = 1)
                                               .With(e => e.Id = 11)
                                               .TheNext(1)
                                               .With(e => e.EpisodeNumber = 2)
                                               .With(e => e.Id = 22)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);
            Db.Insert(fakeEpisode);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            Mocker.GetMock<SeasonProvider>()
                .Setup(s => s.IsIgnored(seriesId, It.IsAny<int>()))
                .Returns(true);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            Mocker.GetMock<TvDbProvider>().VerifyAll();
            result.Should().HaveCount(episodeCount);
            result.Where(e => e.Ignored).Should().HaveCount(episodeCount);
        }

        [Test]
        [Explicit]
        public void Add_daily_show_episodes()
        {
            WithRealDb();
            Mocker.Resolve<TvDbProvider>();

            Mocker.GetMock<ConfigProvider>()
                .Setup(e => e.DefaultQualityProfile).Returns(1);

            Db.Insert(Builder<QualityProfile>.CreateNew().Build());


            const int tvDbSeriesId = 71256;
            //act
            var seriesProvider = Mocker.Resolve<SeriesProvider>();

            seriesProvider.AddSeries("Test Series","c:\\test\\", tvDbSeriesId, 1, null);

            var episodeProvider = Mocker.Resolve<EpisodeProvider>();
            episodeProvider.RefreshEpisodeInfo(seriesProvider.GetSeries(tvDbSeriesId));

            //assert
            var episodes = episodeProvider.GetEpisodeBySeries(tvDbSeriesId);
            episodes.Should().NotBeEmpty();
        }

        [Test]
        public void GetEpisode_by_Season_Episode_none_existing()
        {
            WithRealDb();

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.Should().BeNull();
        }

        [Test]
        public void GetEpisode_by_Season_Episode_with_EpisodeFile()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).With(c => c.Quality = QualityTypes.SDTV).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 1).With(e => e.EpisodeFile = fakeFile).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);
            Db.Insert(fakeFile);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }

        [Test]
        public void GetEpisode_by_Season_Episode_without_EpisodeFile()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 0).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, 1, 1);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
            episode.EpisodeFile.Should().BeNull();
        }

        [Test]
        public void GetEpisode_by_AirDate_with_EpisodeFile()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).With(c => c.Quality = QualityTypes.SDTV).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 1).With(e => e.EpisodeFile = fakeFile).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);
            Db.Insert(fakeFile);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, fakeEpisodes[0].AirDate.Value);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series, e => e.EpisodeFile).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
            episode.EpisodeFile.Should().NotBeNull();
        }

        [Test]
        public void GetEpisode_by_AirDate_without_EpisodeFile()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 0).Build();

            Db.InsertMany(fakeEpisodes);
            Db.Insert(fakeSeries);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1, fakeEpisodes[0].AirDate.Value);

            //Assert
            episode.ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisodes.First());
            episode.Series.ShouldHave().AllPropertiesBut(s => s.EpisodeCount, s => s.EpisodeFileCount, s => s.SeasonCount, s => s.NextAiring).EqualTo(fakeSeries);
            episode.EpisodeFile.Should().BeNull();
        }

        [Test]
        public void MarkEpisodeAsFetched()
        {
            WithRealDb();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.GrabDate = null)
                .Build();

            Db.InsertMany(fakeEpisodes);

            //Act
            Mocker.Resolve<EpisodeProvider>().MarkEpisodeAsFetched(2);
            var episodes = Db.Fetch<Episode>();

            //Assert
            episodes.Where(e => e.EpisodeId == 2).Single().GrabDate.Should().BeWithin(TimeSpan.FromSeconds(5)).Before(
                DateTime.Now);

            episodes.Where(e => e.GrabDate == null).Should().HaveCount(4);
        }

        [Test]
        public void AddEpisode_episode_is_ignored_when_full_season_is_ignored()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.Ignored = true)
                .Build().ToList();

            episodes.ForEach(c => Db.Insert(c));

            var newEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 10)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 8)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            Mocker.GetMock<SeasonProvider>()
                .Setup(s => s.IsIgnored(newEpisode.SeriesId, newEpisode.SeasonNumber))
                .Returns(true);

            //Act
            Mocker.Resolve<EpisodeProvider>().AddEpisode(newEpisode);

            //Assert
            var episodesInDb = Db.Fetch<Episode>(@"SELECT * FROM Episodes");

            episodesInDb.Should().HaveCount(5);
            episodesInDb.Should().OnlyContain(e => e.Ignored);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void AddEpisode_episode_is_not_ignored_when_full_season_is_not_ignored()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.Ignored = false)
                .Build().ToList();

            episodes.ForEach(c => Db.Insert(c));

            var newEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 10)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 8)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            //Act
            Mocker.Resolve<EpisodeProvider>().AddEpisode(newEpisode);

            //Assert
            var episodesInDb = Db.Fetch<Episode>(@"SELECT * FROM Episodes");

            episodesInDb.Should().HaveCount(5);
            episodesInDb.Should().OnlyContain(e => e.Ignored == false);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void AddEpisode_episode_is_not_ignored_when_not_full_season_is_not_ignored()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .And(c => c.SeasonNumber = 1)
                .And(c => c.Ignored = true)
                .TheFirst(2)
                .With(c => c.Ignored = false)
               .Build().ToList();

            episodes.ForEach(c => Db.Insert(c));

            var newEpisode = Builder<Episode>.CreateNew()
                .With(e => e.SeriesId = 10)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumber = 8)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            //Act
            Mocker.Resolve<EpisodeProvider>().AddEpisode(newEpisode);

            //Assert
            var episodesInDb = Db.Fetch<Episode>(@"SELECT * FROM Episodes");

            episodesInDb.Should().HaveCount(5);
            episodesInDb.Where(e => e.EpisodeNumber == 8 && !e.Ignored).Should().HaveCount(1);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IgnoreEpisode_Ignore()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.Ignored = false)
                .Build().ToList();

            episodes.ForEach(c => Db.Insert(c));

            //Act
            Mocker.Resolve<EpisodeProvider>().SetEpisodeIgnore(1, true);

            //Assert
            var episodesInDb = Db.Fetch<Episode>(@"SELECT * FROM Episodes");

            episodesInDb.Should().HaveCount(4);
            episodesInDb.Where(e => e.Ignored).Should().HaveCount(1);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IgnoreEpisode_RemoveIgnore()
        {
            WithRealDb();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.Ignored = true)
                .Build().ToList();

            episodes.ForEach(c => Db.Insert(c));

            //Act
            Mocker.Resolve<EpisodeProvider>().SetEpisodeIgnore(1, false);

            //Assert
            var episodesInDb = Db.Fetch<Episode>(@"SELECT * FROM Episodes");

            episodesInDb.Should().HaveCount(4);
            episodesInDb.Where(e => !e.Ignored).Should().HaveCount(1);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void EpisodesWithoutFiles_no_specials()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.Ignored = true)
                .TheFirst(2)
                .With(c => c.EpisodeFileId = 0)
                .Section(1, 2)
                .With(c => c.Ignored = false)
                .Build().ToList();

            var specials = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 0)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.EpisodeFileId = 0)
                .With(c => c.Ignored = false)
                .TheFirst(1).With(c => c.Ignored = true)
                .Build().ToList();

            Db.Insert(series);
            Db.InsertMany(episodes);
            Db.InsertMany(specials);

            //Act
            var missingFiles = Mocker.Resolve<EpisodeProvider>().EpisodesWithoutFiles(false);

            //Assert
            missingFiles.Should().HaveCount(1);
            missingFiles.Where(e => e.EpisodeFileId == 0).Should().HaveCount(1);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void EpisodesWithoutFiles_with_specials()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.Ignored = true)
                .TheFirst(2)
                .With(c => c.EpisodeFileId = 0)
                .Section(1, 2)
                .With(c => c.Ignored = false)
                .Build().ToList();

            var specials = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 0)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.EpisodeFileId = 0)
                .With(c => c.Ignored = false)
                .TheFirst(1)
                .With(c => c.Ignored = true)
                .Build().ToList();

            Db.Insert(series);
            Db.InsertMany(episodes);
            Db.InsertMany(specials);

            //Act
            var missingFiles = Mocker.Resolve<EpisodeProvider>().EpisodesWithoutFiles(true);

            //Assert
            missingFiles.Should().HaveCount(2);
            missingFiles.Where(e => e.EpisodeFileId == 0).Should().HaveCount(2);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void EpisodesWithFiles_success()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(c => c.EpisodeFileId = 1)
                .With(c => c.Quality = QualityTypes.SDTV)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.Ignored = true)
                .With(c => c.EpisodeFile = episodeFile)
                .With(c => c.EpisodeFileId = episodeFile.EpisodeFileId)
                .Build().ToList();

            Db.Insert(series);
            Db.Insert(episodeFile);
            Db.InsertMany(episodes);

            //Act
            var withFiles = Mocker.Resolve<EpisodeProvider>().EpisodesWithFiles();

            //Assert
            withFiles.Should().HaveCount(2);
            withFiles.Where(e => e.EpisodeFileId == 0).Should().HaveCount(0);
            withFiles.Where(e => e.EpisodeFile == null).Should().HaveCount(0);

            foreach (var withFile in withFiles)
            {
                withFile.EpisodeFile.Should().NotBeNull();
                withFile.Series.Title.Should().NotBeNullOrEmpty();
            }

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void EpisodesWithFiles_no_files()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.AirDate = DateTime.Today.AddDays(-4))
                .With(c => c.Ignored = true)
                .With(c => c.EpisodeFileId = 0)
                .Build().ToList();

            Db.Insert(series);
            Db.InsertMany(episodes);

            //Act
            var withFiles = Mocker.Resolve<EpisodeProvider>().EpisodesWithFiles();

            //Assert
            withFiles.Should().HaveCount(0);

            Mocker.VerifyAllMocks();
        }

        [Test]
        public void GetEpisodesByFileId_multi_episodes()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.EpisodeFileId = 12345)
                .Build();

            Db.Insert(series);
            Db.InsertMany(fakeEpisodes);

            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>().GetEpisodesByFileId(12345);

            //Assert
            episodes.Should().HaveCount(2);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void GetEpisodesByFileId_single_episode()
        {
            WithRealDb();

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 10)
                .Build();

            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.EpisodeFileId = 12345)
                .Build();

            Db.Insert(series);
            Db.Insert(fakeEpisode);

            //Act
            var episodes = Mocker.Resolve<EpisodeProvider>().GetEpisodesByFileId(12345);

            //Assert
            episodes.Should().HaveCount(1);
            episodes.First().ShouldHave().AllPropertiesBut(e => e.Series).EqualTo(fakeEpisode);
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void IsFirstOrLastEpisodeInSeason_false()
        {
            WithRealDb();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .Build();

            Db.InsertMany(fakeEpisodes);

            //Act
            var result = Mocker.Resolve<EpisodeProvider>().IsFirstOrLastEpisodeOfSeason(10, 1, 5);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsFirstOrLastEpisodeInSeason_true_first()
        {
            WithRealDb();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .Build();

            Db.InsertMany(fakeEpisodes);

            //Act
            var result = Mocker.Resolve<EpisodeProvider>().IsFirstOrLastEpisodeOfSeason(10, 1, 1);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsFirstOrLastEpisodeInSeason_true_last()
        {
            WithRealDb();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(c => c.SeriesId = 10)
                .With(c => c.SeasonNumber = 1)
                .Build();

            Db.InsertMany(fakeEpisodes);

            //Act
            var result = Mocker.Resolve<EpisodeProvider>().IsFirstOrLastEpisodeOfSeason(10, 1, 10);

            //Assert
            result.Should().BeFalse();
        }

        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Unpacking, 1)]
        [TestCase("The Office (US) - S01E01 - Episode Title", PostDownloadStatusType.Failed, 1)]
        [TestCase("The Office (US) - S01E01E02 - Episode Title", PostDownloadStatusType.Unpacking, 2)]
        [TestCase("The Office (US) - S01E01E02 - Episode Title", PostDownloadStatusType.Failed, 2)]
        [TestCase("The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Unpacking, 10)]
        [TestCase("The Office (US) - Season 01 - Episode Title", PostDownloadStatusType.Failed, 10)]
        public void SetPostDownloadStatus(string folderName, PostDownloadStatusType postDownloadStatus, int episodeCount)
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.CleanTitle = "officeus")
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(episodeCount)
                .All()
                .With(c => c.SeriesId = 12345)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.PostDownloadStatus = PostDownloadStatusType.Unknown)
                .Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);

            //Act
            Mocker.Resolve<EpisodeProvider>().SetPostDownloadStatus(fakeEpisodes.Select(e => e.EpisodeId).ToList(), postDownloadStatus);

            //Assert
            var result = Db.Fetch<Episode>();
            result.Where(e => e.PostDownloadStatus == postDownloadStatus).Count().Should().Be(episodeCount);
        }

        [Test]
        public void SetPostDownloadStatus_Invalid_EpisodeId()
        {
            WithRealDb();

            var postDownloadStatus = PostDownloadStatusType.Failed;

            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.CleanTitle = "officeus")
                .Build();

            var fakeEpisodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(c => c.SeriesId = 12345)
                .With(c => c.SeasonNumber = 1)
                .With(c => c.PostDownloadStatus = PostDownloadStatusType.Unknown)
                .Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);

            Mocker.GetMock<SeriesProvider>().Setup(s => s.FindSeries("officeus")).Returns(fakeSeries);

            //Act
            Mocker.Resolve<EpisodeProvider>().SetPostDownloadStatus(new List<int> { 300 }, postDownloadStatus);

            //Assert
            var result = Db.Fetch<Episode>();
            result.Where(e => e.PostDownloadStatus == postDownloadStatus).Count().Should().Be(0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetPostDownloadStatus_should_throw_if_episode_list_is_empty()
        {
            Mocker.Resolve<EpisodeProvider>().SetPostDownloadStatus(new List<int>(), PostDownloadStatusType.Failed);
        }

        [Test]
        public void RefreshEpisodeInfo_should_ignore_episode_zero_except_if_season_one()
        {
            //Arrange
            const int seriesId = 71663;
            const int episodeCount = 5;

            var tvdbSeries = Builder<TvdbSeries>.CreateNew().With(
                c => c.Episodes =
                     new List<TvdbEpisode>(Builder<TvdbEpisode>.CreateListOfSize(episodeCount).
                                               All()
                                               .With(l => l.Language = new TvdbLanguage(0, "eng", "a"))
                                               .With(e => e.EpisodeNumber = 0)
                                               .TheFirst(1)
                                               .With(e => e.SeasonNumber = 1)
                                               .TheNext(1)
                                               .With(e => e.SeasonNumber = 2)
                                               .TheNext(1)
                                               .With(e => e.SeasonNumber = 3)
                                               .TheNext(1)
                                               .With(e => e.SeasonNumber = 4)
                                               .TheNext(1)
                                               .With(e => e.SeasonNumber = 5)
                                               .Build())
                ).With(c => c.Id = seriesId).Build();

            var fakeSeries = Builder<Series>.CreateNew().With(c => c.SeriesId = seriesId).Build();

            WithRealDb();

            Db.Insert(fakeSeries);

            Mocker.GetMock<TvDbProvider>()
                .Setup(c => c.GetSeries(seriesId, true, false))
                .Returns(tvdbSeries);

            //Act
            Mocker.Resolve<EpisodeProvider>().RefreshEpisodeInfo(fakeSeries);

            //Assert
            var result = Mocker.Resolve<EpisodeProvider>().GetEpisodeBySeries(seriesId).ToList();
            result.Should().HaveCount(episodeCount);
            result.Where(e => e.Ignored).Should().HaveCount(episodeCount - 1);
            result.Single(e => e.SeasonNumber == 1).Ignored.Should().BeFalse();
        }

        [Test]
        public void GetEpisode_with_EpisodeFile_should_have_quality_set_properly()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeFile = Builder<EpisodeFile>.CreateNew().With(f => f.EpisodeFileId).With(c => c.Quality = QualityTypes.WEBDL1080p).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(5)
                .All().With(e => e.SeriesId = 1).TheFirst(1).With(e => e.EpisodeFileId = 1).With(e => e.EpisodeFile = fakeFile).Build();

            Db.Insert(fakeSeries);
            Db.InsertMany(fakeEpisodes);
            Db.Insert(fakeFile);

            //Act
            var episode = Mocker.Resolve<EpisodeProvider>().GetEpisode(1);

            //Assert
            episode.EpisodeFile.Quality.Should().Be(QualityTypes.WEBDL1080p);
        }
    }
}
