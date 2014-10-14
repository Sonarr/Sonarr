using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.Tvdb;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class RefreshEpisodeServiceFixture : CoreTest<RefreshEpisodeService>
    {
        private List<Episode> _insertedEpisodes;
        private List<Episode> _updatedEpisodes;
        private List<Episode> _deletedEpisodes;
        private Tuple<Series, List<Episode>> _gameOfThrones;

        [TestFixtureSetUp]
        public void TestFixture()
        {
            UseRealHttp();

            _gameOfThrones = Mocker.Resolve<TraktProxy>().GetSeriesInfo(121361);//Game of thrones

            // Remove specials.
            _gameOfThrones.Item2.RemoveAll(v => v.SeasonNumber == 0);
        }

        private List<Episode> GetEpisodes()
        {
            return _gameOfThrones.Item2.JsonClone();
        }

        private Series GetSeries()
        {
            var series = _gameOfThrones.Item1.JsonClone();
            series.Seasons = new List<Season>();

            return series;
        }

        private Series GetAnimeSeries()
        {
            var series = Builder<Series>.CreateNew().Build();
            series.SeriesType = SeriesTypes.Anime;
            series.Seasons = new List<Season>();

            return series;
        }

        [SetUp]
        public void Setup()
        {
            _insertedEpisodes = new List<Episode>();
            _updatedEpisodes = new List<Episode>();
            _deletedEpisodes = new List<Episode>();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.InsertMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _insertedEpisodes = e);


            Mocker.GetMock<IEpisodeService>().Setup(c => c.UpdateMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _updatedEpisodes = e);


            Mocker.GetMock<IEpisodeService>().Setup(c => c.DeleteMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(e => _deletedEpisodes = e);
        }

        private void GivenAnimeEpisodes(List<Episode> episodes)
        {
            Mocker.GetMock<ITvdbProxy>()
                  .Setup(s => s.GetEpisodeInfo(It.IsAny<Int32>()))
                  .Returns(episodes);
        }

        [Test]
        public void should_create_all_when_no_existing_episodes()
        {

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_update_all_when_all_existing_episodes()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(GetEpisodes());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_delete_all_when_all_existing_episodes_are_gone_from_trakt()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(GetEpisodes());

            Subject.RefreshEpisodeInfo(GetSeries(), new List<Episode>());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().HaveSameCount(GetEpisodes());
        }

        [Test]
        public void should_delete_duplicated_episodes_based_on_season_episode_number()
        {
            var duplicateEpisodes = GetEpisodes().Skip(5).Take(2).ToList();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(GetEpisodes().Union(duplicateEpisodes).ToList());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _deletedEpisodes.Should().HaveSameCount(duplicateEpisodes);
        }

        [Test]
        public void should_not_change_monitored_status_for_existing_episodes()
        {
            var series = GetSeries();
            series.Seasons = new List<Season>();
            series.Seasons.Add(new Season { SeasonNumber = 1, Monitored = false });

            var episodes = GetEpisodes();

            episodes.ForEach(e => e.Monitored = true);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(episodes);

            Subject.RefreshEpisodeInfo(series, GetEpisodes());

            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_remove_duplicate_remote_episodes_before_processing()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>());

            var episodes = Builder<Episode>.CreateListOfSize(5)
                                           .TheFirst(2)
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.EpisodeNumber = 1)
                                           .Build()
                                           .ToList();

            Subject.RefreshEpisodeInfo(GetSeries(), episodes);

            _insertedEpisodes.Should().HaveCount(episodes.Count - 1);
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_not_set_absolute_episode_number_for_non_anime()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                  .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.All(e => !e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
        }

        [Test]
        public void should_set_absolute_episode_number_for_anime()
        {
            var episodes = Builder<Episode>.CreateListOfSize(3).Build().ToList();
            GivenAnimeEpisodes(episodes);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _insertedEpisodes.All(e => e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_set_absolute_episode_number_even_if_not_previously_set_for_anime()
        {
            var episodes = Builder<Episode>.CreateListOfSize(3).Build().ToList();
            GivenAnimeEpisodes(episodes);

            var existingEpisodes = episodes.JsonClone();
            existingEpisodes.ForEach(e => e.AbsoluteEpisodeNumber = null);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(existingEpisodes);

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.All(e => e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_get_new_season_and_episode_numbers_when_absolute_episode_number_match_found()
        {
            const Int32 expectedSeasonNumber = 10;
            const Int32 expectedEpisodeNumber = 5;
            const Int32 expectedAbsoluteNumber = 3;

            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeasonNumber = expectedSeasonNumber)
                                          .With(e => e.EpisodeNumber = expectedEpisodeNumber)
                                          .With(e => e.AbsoluteEpisodeNumber = expectedAbsoluteNumber)
                                          .Build();

            GivenAnimeEpisodes(new List<Episode> { episode });

            var existingEpisode = episode.JsonClone();
            existingEpisode.SeasonNumber = 1;
            existingEpisode.EpisodeNumber = 1;
            existingEpisode.AbsoluteEpisodeNumber = expectedAbsoluteNumber;

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>{ existingEpisode });

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), new List<Episode> { episode });

            _insertedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();

            _updatedEpisodes.First().SeasonNumber.Should().Be(expectedSeasonNumber);
            _updatedEpisodes.First().EpisodeNumber.Should().Be(expectedEpisodeNumber);
            _updatedEpisodes.First().AbsoluteEpisodeNumber.Should().Be(expectedAbsoluteNumber);
        }

        [Test]
        public void should_prefer_absolute_match_over_season_and_epsiode_match()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .Build()
                                           .ToList();

            episodes[0].AbsoluteEpisodeNumber = null;
            episodes[0].SeasonNumber.Should().NotBe(episodes[1].SeasonNumber);
            episodes[0].EpisodeNumber.Should().NotBe(episodes[1].EpisodeNumber);

            GivenAnimeEpisodes(episodes);

            var existingEpisode = new Episode
                                  {
                                      SeasonNumber = episodes[0].SeasonNumber,
                                      EpisodeNumber = episodes[0].EpisodeNumber,
                                      AbsoluteEpisodeNumber = episodes[1].AbsoluteEpisodeNumber
                                  };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode> { existingEpisode });

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _updatedEpisodes.First().SeasonNumber.Should().Be(episodes[1].SeasonNumber);
            _updatedEpisodes.First().EpisodeNumber.Should().Be(episodes[1].EpisodeNumber);
            _updatedEpisodes.First().AbsoluteEpisodeNumber.Should().Be(episodes[1].AbsoluteEpisodeNumber);
        }

        [Test]
        public void should_ignore_episodes_with_no_absolute_episode_in_distinct_by_absolute()
        {
            var episodes = Builder<Episode>.CreateListOfSize(10)
                                           .Build()
                                           .ToList();

            episodes[0].AbsoluteEpisodeNumber = null;
            episodes[1].AbsoluteEpisodeNumber = null;
            episodes[2].AbsoluteEpisodeNumber = null;
            episodes[3].AbsoluteEpisodeNumber = null;
            episodes[4].AbsoluteEpisodeNumber = null;

            GivenAnimeEpisodes(episodes);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _insertedEpisodes.Should().HaveCount(episodes.Count);

        }

        [Test]
        public void should_override_empty_airdate_for_direct_to_dvd()
        {
            var series = GetSeries();
            series.Status = SeriesStatusType.Ended;

            var episodes = Builder<Episode>.CreateListOfSize(10)
                                           .All()
                                           .With(v => v.AirDateUtc = null)
                                           .BuildListOfNew();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<Int32>()))
                .Returns(new List<Episode>());

            List<Episode> updateEpisodes = null;
            Mocker.GetMock<IEpisodeService>().Setup(c => c.InsertMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(c => updateEpisodes = c);

            Subject.RefreshEpisodeInfo(series, episodes);

            updateEpisodes.Should().NotBeNull();
            updateEpisodes.Should().NotBeEmpty();
            updateEpisodes.All(v => v.AirDateUtc.HasValue).Should().BeTrue();
        }
    }
}