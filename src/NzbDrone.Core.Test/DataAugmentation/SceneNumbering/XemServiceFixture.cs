using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Xem;
using NzbDrone.Core.DataAugmentation.Xem.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

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

            Mocker.GetMock<IEpisodeService>()
                  .Setup(v => v.GetEpisodeBySeries(It.IsAny<int>()))
                  .Returns(_episodes);
        }

        private void GivenTvdbMappings()
        {
            _theXemSeriesIds.Add(10);

            AddTvdbMapping(1, 1, 1, 1, 1, 1);
            AddTvdbMapping(2, 1, 2, 2, 1, 2);
            AddTvdbMapping(3, 2, 1, 3, 2, 1);
            AddTvdbMapping(4, 2, 2, 4, 2, 2);
            AddTvdbMapping(5, 2, 3, 5, 2, 3);
            AddTvdbMapping(6, 3, 1, 6, 2, 4);
            AddTvdbMapping(7, 3, 2, 7, 2, 5);
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
        }
    }
}
