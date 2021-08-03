using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
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

        [OneTimeSetUp]
        public void TestFixture()
        {
            UseRealHttp();

            _gameOfThrones = Mocker.Resolve<SkyHookProxy>().GetSeriesInfo(121361); //Game of thrones

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

        [Test]
        public void should_create_all_when_no_existing_episodes()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().BeEmpty();
            _deletedEpisodes.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_update_all_when_all_existing_episodes()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(GetEpisodes());

            Subject.RefreshEpisodeInfo(GetSeries(), GetEpisodes());

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_delete_all_when_all_existing_episodes_are_gone_from_datasource()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(episodes);

            Subject.RefreshEpisodeInfo(series, GetEpisodes());

            _updatedEpisodes.Should().HaveSameCount(GetEpisodes());
            _updatedEpisodes.Should().OnlyContain(e => e.Monitored == true);
        }

        [Test]
        public void should_not_set_monitored_status_for_old_episodes_to_false_if_episodes_existed()
        {
            var series = GetSeries();
            series.Seasons = new List<Season>();
            series.Seasons.Add(new Season { SeasonNumber = 1, Monitored = true });

            var episodes = GetEpisodes().OrderBy(v => v.SeasonNumber).ThenBy(v => v.EpisodeNumber).Take(5).ToList();

            episodes[1].AirDateUtc = DateTime.UtcNow.AddDays(-15);
            episodes[2].AirDateUtc = DateTime.UtcNow.AddDays(-10);
            episodes[3].AirDateUtc = DateTime.UtcNow.AddDays(1);

            var existingEpisodes = episodes.Skip(4).ToList();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existingEpisodes);

            Subject.RefreshEpisodeInfo(series, episodes);

            _insertedEpisodes = _insertedEpisodes.OrderBy(v => v.EpisodeNumber).ToList();

            _insertedEpisodes.Should().HaveCount(4);
            _insertedEpisodes[0].Monitored.Should().Be(true);
            _insertedEpisodes[1].Monitored.Should().Be(true);
            _insertedEpisodes[2].Monitored.Should().Be(true);
            _insertedEpisodes[3].Monitored.Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_set_monitored_status_for_old_episodes_to_false_if_no_episodes_existed()
        {
            var series = GetSeries();
            series.Seasons = new List<Season>();

            var episodes = GetEpisodes().OrderBy(v => v.SeasonNumber).ThenBy(v => v.EpisodeNumber).Take(4).ToList();

            episodes[1].AirDateUtc = DateTime.UtcNow.AddDays(-15);
            episodes[2].AirDateUtc = DateTime.UtcNow.AddDays(-10);
            episodes[3].AirDateUtc = DateTime.UtcNow.AddDays(1);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            Subject.RefreshEpisodeInfo(series, episodes);

            _insertedEpisodes = _insertedEpisodes.OrderBy(v => v.EpisodeNumber).ToList();

            _insertedEpisodes.Should().HaveSameCount(episodes);
            _insertedEpisodes[0].Monitored.Should().Be(false);
            _insertedEpisodes[1].Monitored.Should().Be(false);
            _insertedEpisodes[2].Monitored.Should().Be(false);
            _insertedEpisodes[3].Monitored.Should().Be(true);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_remove_duplicate_remote_episodes_before_processing()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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
        public void should_set_absolute_episode_number_for_anime()
        {
            var episodes = Builder<Episode>.CreateListOfSize(3).Build().ToList();

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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

            var existingEpisodes = episodes.JsonClone();
            existingEpisodes.ForEach(e => e.AbsoluteEpisodeNumber = null);

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(existingEpisodes);

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _insertedEpisodes.Should().BeEmpty();
            _updatedEpisodes.All(e => e.AbsoluteEpisodeNumber.HasValue).Should().BeTrue();
            _deletedEpisodes.Should().BeEmpty();
        }

        [Test]
        public void should_get_new_season_and_episode_numbers_when_absolute_episode_number_match_found()
        {
            const int expectedSeasonNumber = 10;
            const int expectedEpisodeNumber = 5;
            const int expectedAbsoluteNumber = 3;

            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeasonNumber = expectedSeasonNumber)
                                          .With(e => e.EpisodeNumber = expectedEpisodeNumber)
                                          .With(e => e.AbsoluteEpisodeNumber = expectedAbsoluteNumber)
                                          .Build();

            var existingEpisode = episode.JsonClone();
            existingEpisode.SeasonNumber = 1;
            existingEpisode.EpisodeNumber = 1;
            existingEpisode.AbsoluteEpisodeNumber = expectedAbsoluteNumber;

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode> { existingEpisode });

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

            var existingEpisode = new Episode
                                  {
                                      SeasonNumber = episodes[0].SeasonNumber,
                                      EpisodeNumber = episodes[0].EpisodeNumber,
                                      AbsoluteEpisodeNumber = episodes[1].AbsoluteEpisodeNumber
                                  };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
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

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            List<Episode> updateEpisodes = null;
            Mocker.GetMock<IEpisodeService>().Setup(c => c.InsertMany(It.IsAny<List<Episode>>()))
                .Callback<List<Episode>>(c => updateEpisodes = c);

            Subject.RefreshEpisodeInfo(series, episodes);

            updateEpisodes.Should().NotBeNull();
            updateEpisodes.Should().NotBeEmpty();
            updateEpisodes.All(v => v.AirDateUtc.HasValue).Should().BeTrue();
        }

        [Test]
        public void should_use_tba_for_episode_title_when_null()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.Title = null)
                                           .Build()
                                           .ToList();

            Subject.RefreshEpisodeInfo(GetSeries(), episodes);

            _insertedEpisodes.First().Title.Should().Be("TBA");
        }

        [Test]
        public void should_update_air_date_when_multiple_episodes_air_on_the_same_day()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            var now = DateTime.UtcNow;
            var series = GetSeries();

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshEpisodeInfo(series, episodes);

            _insertedEpisodes.First().AirDateUtc.Value.ToString("s").Should().Be(episodes.First().AirDateUtc.Value.ToString("s"));
            _insertedEpisodes.Last().AirDateUtc.Value.ToString("s").Should().Be(episodes.First().AirDateUtc.Value.AddMinutes(series.Runtime).ToString("s"));
        }

        [Test]
        public void should_not_update_air_date_when_more_than_three_episodes_air_on_the_same_day()
        {
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode>());

            var now = DateTime.UtcNow;
            var series = GetSeries();

            var episodes = Builder<Episode>.CreateListOfSize(4)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .With(e => e.AirDate = now.ToShortDateString())
                                           .With(e => e.AirDateUtc = now)
                                           .Build()
                                           .ToList();

            Subject.RefreshEpisodeInfo(series, episodes);

            _insertedEpisodes.Should().OnlyContain(e => e.AirDateUtc.Value.ToString("s") == episodes.First().AirDateUtc.Value.ToString("s"));
        }

        [Test]
        public void should_prefer_regular_season_when_absolute_numbers_conflict()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .Build()
                                           .ToList();

            episodes[0].AbsoluteEpisodeNumber = episodes[1].AbsoluteEpisodeNumber;
            episodes[0].SeasonNumber = 0;
            episodes[0].EpisodeNumber.Should().NotBe(episodes[1].EpisodeNumber);

            var existingEpisode = new Episode
            {
                SeasonNumber = episodes[0].SeasonNumber,
                EpisodeNumber = episodes[0].EpisodeNumber,
                AbsoluteEpisodeNumber = episodes[1].AbsoluteEpisodeNumber
            };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodeBySeries(It.IsAny<int>()))
                .Returns(new List<Episode> { existingEpisode });

            Subject.RefreshEpisodeInfo(GetAnimeSeries(), episodes);

            _updatedEpisodes.First().SeasonNumber.Should().Be(episodes[1].SeasonNumber);
            _updatedEpisodes.First().EpisodeNumber.Should().Be(episodes[1].EpisodeNumber);
            _updatedEpisodes.First().AbsoluteEpisodeNumber.Should().Be(episodes[1].AbsoluteEpisodeNumber);
        }
    }
}
