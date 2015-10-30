
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using FizzWare.NBuilder;
using System.Linq;
using FluentAssertions;
using NzbDrone.Core.Tv;
using Moq;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class FullSeasonSpecificationFixture : CoreTest<FullSeasonSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            var show = Builder<Series>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true
                },
                Episodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.AirDateUtc = System.DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.SeriesId = show.Id)
                                           .BuildList(),
                Series = show
            };

            Mocker.GetMock<IEpisodeService>().Setup(s => s.EpisodesBetweenDates(It.IsAny<System.DateTime>(), It.IsAny<System.DateTime>(), false))
                                             .Returns(new List<Episode>());
        }

        [Test]
        public void should_return_true_if_all_episodes_were_emited_over_7_days_ago()
        {
            Mocker.Resolve<FullSeasonSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_episode_has_not_been_emited()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = System.DateTime.UtcNow.AddDays(+2);
            Mocker.Resolve<FullSeasonSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_last_episode_was_emited_last_week()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = System.DateTime.UtcNow.AddDays(-5);
            Mocker.Resolve<FullSeasonSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_is_not_a_full_season()
        {
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = false;
            _remoteEpisode.Episodes.Last().AirDateUtc = System.DateTime.UtcNow.AddDays(+2);
            Mocker.Resolve<FullSeasonSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_an_episode_was_emited_today()
        {
            var episode = Builder<Episode>.CreateNew().With(s => s.SeriesId = _remoteEpisode.Series.Id).Build();
            Mocker.GetMock<IEpisodeService>().Setup(s => s.EpisodesBetweenDates(It.IsAny<System.DateTime>(), It.IsAny<System.DateTime>(), false))
                                             .Returns(new List<Episode> { episode });
            Mocker.Resolve<FullSeasonSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
