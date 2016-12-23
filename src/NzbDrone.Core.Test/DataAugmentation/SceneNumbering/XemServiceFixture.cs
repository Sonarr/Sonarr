using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Xem;
using NzbDrone.Core.DataAugmentation.Xem.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DataAugmentation.SceneNumbering
{
    [TestFixture]
    public class XemServiceFixture : CoreTest<XemService>
    {
        private Series _series;
        private List<int> _theXemSeriesIds;
        private List<XemSceneTvdbMapping> _theXemTvdbMappings;
        private List<Episode> _episodes;

        [SetUp]
        public void SetUp()
        {
            _series = Builder<Series>.CreateNew()
                .With(v => v.TvdbId = 10)
                .With(v => v.UseSceneNumbering = false)
                .BuildNew();

            _theXemSeriesIds = new List<int> { 120 };
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetXemSeriesIds())
                  .Returns(_theXemSeriesIds);

            _theXemTvdbMappings = new List<XemSceneTvdbMapping>();
            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetSceneTvdbMappings(10))
                  .Returns(_theXemTvdbMappings);

            _episodes = new List<Episode>();
            _episodes.Add(new Episode { SeasonNumber = 1, EpisodeNumber = 1 });
            _episodes.Add(new Episode { SeasonNumber = 1, EpisodeNumber = 2 });
            _episodes.Add(new Episode { SeasonNumber = 2, EpisodeNumber = 1 });
            _episodes.Add(new Episode { SeasonNumber = 2, EpisodeNumber = 2 });
            _episodes.Add(new Episode { SeasonNumber = 2, EpisodeNumber = 3 });
            _episodes.Add(new Episode { SeasonNumber = 2, EpisodeNumber = 4 });
            _episodes.Add(new Episode { SeasonNumber = 2, EpisodeNumber = 5 });
            _episodes.Add(new Episode { SeasonNumber = 3, EpisodeNumber = 1 });
            _episodes.Add(new Episode { SeasonNumber = 3, EpisodeNumber = 2 });

            Mocker.GetMock<IEpisodeService>()
                  .Setup(v => v.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenTvdbMappings()
        {
            _theXemSeriesIds.Add(10);

            AddTvdbMapping(1, 1, 1, 1, 1, 1); // 1x01 -> 1x01
            AddTvdbMapping(2, 1, 2, 2, 1, 2); // 1x02 -> 1x02
            AddTvdbMapping(3, 2, 1, 3, 2, 1); // 2x01 -> 2x01
            AddTvdbMapping(4, 2, 2, 4, 2, 2); // 2x02 -> 2x02
            AddTvdbMapping(5, 2, 3, 5, 2, 3); // 2x03 -> 2x03
            AddTvdbMapping(6, 3, 1, 6, 2, 4); // 3x01 -> 2x04
            AddTvdbMapping(7, 3, 2, 7, 2, 5); // 3x02 -> 2x05
        }

        private void GivenExistingMapping()
        {
            _series.UseSceneNumbering = true;

            _episodes[0].SceneSeasonNumber = 1;
            _episodes[0].SceneEpisodeNumber = 1;
            _episodes[1].SceneSeasonNumber = 1;
            _episodes[1].SceneEpisodeNumber = 2;
            _episodes[2].SceneSeasonNumber = 2;
            _episodes[2].SceneEpisodeNumber = 1;
            _episodes[3].SceneSeasonNumber = 2;
            _episodes[3].SceneEpisodeNumber = 2;
            _episodes[4].SceneSeasonNumber = 2;
            _episodes[4].SceneEpisodeNumber = 3;
            _episodes[5].SceneSeasonNumber = 3;
            _episodes[5].SceneEpisodeNumber = 1;
            _episodes[6].SceneSeasonNumber = 3;
            _episodes[6].SceneEpisodeNumber = 1;
        }

        private void AddTvdbMapping(int sceneAbsolute, int sceneSeason, int sceneEpisode, int tvdbAbsolute, int tvdbSeason, int tvdbEpisode)
        {
            _theXemTvdbMappings.Add(new XemSceneTvdbMapping
            {
                Scene = new XemValues { Absolute = sceneAbsolute, Season = sceneSeason, Episode = sceneEpisode },
                Tvdb  = new XemValues { Absolute = tvdbAbsolute, Season = tvdbSeason, Episode = tvdbEpisode },
            });
        }


        [Test]
        public void should_not_fetch_scenenumbering_if_not_listed()
        {
            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<IXemProxy>()
                  .Verify(v => v.GetSceneTvdbMappings(10), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>()), Times.Never());
        }

        [Test]
        public void should_fetch_scenenumbering()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.UseSceneNumbering == true)), Times.Once());
        }

        [Test]
        public void should_clear_scenenumbering_if_removed_from_thexem()
        {
            GivenExistingMapping();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>()), Times.Once());
        }

        [Test]
        public void should_not_clear_scenenumbering_if_no_results_at_all_from_thexem()
        {
            GivenExistingMapping();

            _theXemSeriesIds.Clear();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_not_clear_scenenumbering_if_thexem_throws()
        {
            GivenExistingMapping();

            Mocker.GetMock<IXemProxy>()
                  .Setup(v => v.GetXemSeriesIds())
                  .Throws(new InvalidOperationException());

            Subject.Handle(new SeriesUpdatedEvent(_series));

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>()), Times.Never());

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_flag_unknown_future_episodes_if_existing_season_is_mapped()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Season == 2 && v.Tvdb.Episode == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_flag_unknown_future_season_if_future_season_is_shifted()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 1);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_flag_unknown_future_season_if_future_season_is_not_shifted()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Season == 3);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 1);

            episode.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_not_flag_past_episodes_if_not_causing_overlaps()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Season == 2);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 1);

            episode.UnverifiedSceneNumbering.Should().BeFalse();
        }

        [Test]
        public void should_flag_past_episodes_if_causing_overlap()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Season == 2 && v.Tvdb.Episode <= 1);
            _theXemTvdbMappings.First(v => v.Scene.Season == 2 && v.Scene.Episode == 2).Scene.Episode = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 1);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
        }

        [Test]
        public void should_not_extrapolate_season_with_specials()
        {
            GivenTvdbMappings();
            var specialMapping = _theXemTvdbMappings.First(v => v.Tvdb.Season == 2 && v.Tvdb.Episode == 5);
            specialMapping.Tvdb.Season = 0;
            specialMapping.Tvdb.Episode = 1;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
            episode.SceneSeasonNumber.Should().NotHaveValue();
            episode.SceneEpisodeNumber.Should().NotHaveValue();
        }

        [Test]
        public void should_extrapolate_season_with_future_episodes()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Season == 2 && v.Tvdb.Episode == 5);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
            episode.SceneSeasonNumber.Should().Be(3);
            episode.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_extrapolate_season_with_shifted_episodes()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Tvdb.Season == 2 && v.Tvdb.Episode == 5);
            var dualMapping = _theXemTvdbMappings.First(v => v.Tvdb.Season == 2 && v.Tvdb.Episode == 4);
            dualMapping.Scene.Season = 2;
            dualMapping.Scene.Episode = 3;

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 2 && v.EpisodeNumber == 5);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
            episode.SceneSeasonNumber.Should().Be(2);
            episode.SceneEpisodeNumber.Should().Be(4);
        }

        [Test]
        public void should_extrapolate_shifted_future_seasons()
        {
            GivenTvdbMappings();

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 2);

            episode.UnverifiedSceneNumbering.Should().BeTrue();
            episode.SceneSeasonNumber.Should().Be(4);
            episode.SceneEpisodeNumber.Should().Be(2);
        }

        [Test]
        public void should_not_extrapolate_matching_future_seasons()
        {
            GivenTvdbMappings();
            _theXemTvdbMappings.RemoveAll(v => v.Scene.Season != 1);

            Subject.Handle(new SeriesUpdatedEvent(_series));

            var episode = _episodes.First(v => v.SeasonNumber == 3 && v.EpisodeNumber == 2);

            episode.UnverifiedSceneNumbering.Should().BeFalse();
            episode.SceneSeasonNumber.Should().NotHaveValue();
            episode.SceneEpisodeNumber.Should().NotHaveValue();
        }
    }
}
