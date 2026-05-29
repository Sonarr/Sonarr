using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class MultiSeasonSpecificationFixture : CoreTest<MultiSeasonSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew()
                .With(s => s.Id = 1234)
                .With(s => s.Monitored = true)
                .With(s => s.Seasons = new List<Season>
                {
                    new Season { SeasonNumber = 1, Monitored = true },
                    new Season { SeasonNumber = 2, Monitored = true }
                })
                .Build();

            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true,
                    IsMultiSeason = true,
                    SeasonNumbers = new[] { 1, 2 }
                },
                Episodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.SeriesId = series.Id)
                                           .With(e => e.Monitored = true)
                                           .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-8))
                                           .TheFirst(2)
                                           .With(e => e.SeasonNumber = 1)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 2)
                                           .BuildList(),
                Series = series,
                Release = new ReleaseInfo
                {
                    Title = "Series.Title.S01-02.720p.BluRay.X264-RlsGrp"
                }
            };
        }

        [Test]
        public void should_return_true_if_is_not_a_multi_season_release()
        {
            _remoteEpisode.ParsedEpisodeInfo.IsMultiSeason = false;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_series_is_not_monitored()
        {
            _remoteEpisode.Series.Monitored = false;
            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.SeriesNotMonitored);
        }

        [Test]
        public void should_return_false_if_a_covered_season_is_not_monitored()
        {
            _remoteEpisode.Series.Seasons = new List<Season>
            {
                new Season { SeasonNumber = 1, Monitored = false },
                new Season { SeasonNumber = 2, Monitored = true }
            };

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.MultiSeasonNotAllMonitored);
        }

        [Test]
        public void should_return_false_if_not_all_episodes_are_monitored()
        {
            _remoteEpisode.Episodes.Last().Monitored = false;

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.EpisodeNotMonitored);
        }

        [Test]
        public void should_return_true_if_all_episodes_will_have_aired_within_24_hours()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddHours(23);
            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_all_episodes_have_aired()
        {
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);

            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.FullSeasonNotAired);
        }

        [Test]
        public void should_return_true_if_all_conditions_met()
        {
            var result = Subject.IsSatisfiedBy(_remoteEpisode, new());
            result.Accepted.Should().BeTrue();
        }
    }
}
