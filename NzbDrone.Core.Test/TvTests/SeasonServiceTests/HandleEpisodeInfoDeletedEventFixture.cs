using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.TvTests.SeasonServiceTests
{
    [TestFixture]
    public class HandleEpisodeInfoDeletedEventFixture : CoreTest<SeasonService>
    {
        private List<Season> _seasons;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _seasons = Builder<Season>
                .CreateListOfSize(1)
                .All()
                .With(s => s.SeriesId = 1)
                .Build()
                .ToList();

            _episodes = Builder<Episode>
                .CreateListOfSize(1)
                .All()
                .With(e => e.SeasonNumber = _seasons.First().SeasonNumber)
                .With(s => s.SeriesId = _seasons.First().SeasonNumber)
                .Build()
                .ToList();

            Mocker.GetMock<ISeasonRepository>()
                  .Setup(s => s.GetSeasonBySeries(It.IsAny<int>()))
                  .Returns(_seasons);

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), _seasons.First().SeasonNumber))
                .Returns(_episodes);
        }

        private void GivenAbandonedSeason()
        {
            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodesBySeason(It.IsAny<int>(), _seasons.First().SeasonNumber))
                .Returns(new List<Episode>());
        }

        [Test]
        public void should_not_delete_when_season_is_still_valid()
        {           
            Subject.Handle(new EpisodeInfoDeletedEvent(_episodes));

            Mocker.GetMock<ISeasonRepository>()
                .Verify(v => v.Delete(It.IsAny<Season>()), Times.Never());
        }

        [Test]
        public void should_delete_season_if_no_episodes_exist_in_that_season()
        {
            GivenAbandonedSeason();

            Subject.Handle(new EpisodeInfoDeletedEvent(_episodes));

            Mocker.GetMock<ISeasonRepository>()
                .Verify(v => v.Delete(It.IsAny<Season>()), Times.Once());
        }

        [Test]
        public void should_only_delete_a_season_once()
        {
            _episodes = Builder<Episode>
                .CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = _seasons.First().SeasonNumber)
                .With(s => s.SeriesId = _seasons.First().SeasonNumber)
                .Build()
                .ToList();

            GivenAbandonedSeason();

            Subject.Handle(new EpisodeInfoDeletedEvent(_episodes));

            Mocker.GetMock<ISeasonRepository>()
                .Verify(v => v.Delete(It.IsAny<Season>()), Times.Once());
        }
    }
}
