using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class FullSeasonSpecificationFixture : CoreTest<FullSeasonSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(new List<ReleaseProfile>());

            var show = Builder<Series>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true
                },
                Episodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.SeriesId = show.Id)
                                           .BuildList(),
                Series = show,
                Release = new ReleaseInfo
                {
                    Title = "Series.Title.S01.720p.BluRay.X264-RlsGrp"
                }
            };
        }

        [Test]
        public void should_return_true_if_is_not_a_full_season()
        {
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = false;
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_episodes_have_aired()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_episodes_will_have_aired_in_the_next_24_hours()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddHours(23);

            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_episode_has_not_aired()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_an_episode_does_not_have_an_air_date()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = null;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_one_episode_has_not_aired_and_release_profile_allows_it()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(new List<ReleaseProfile>
                  {
                      new()
                      {
                          AllowSeasonPackWithoutAllEpisodesAired = true
                      }
                  });

            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_release_profile_does_not_allow_it()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(new List<ReleaseProfile>
                  {
                      new()
                      {
                          AllowSeasonPackWithoutAllEpisodesAired = true
                      },
                      new()
                      {
                          AllowSeasonPackWithoutAllEpisodesAired = false
                      }
                  });

            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }
    }
}
