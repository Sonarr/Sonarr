using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class NotInQueueSpecificationFixture : CoreTest<NotInQueueSpecification>
    {
        private Series _series;
        private Episode _episode;
        private RemoteEpisode _remoteEpisode;

        private Series _otherSeries;
        private Episode _otherEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();

            _episode = Builder<Episode>.CreateNew()
                                       .With(e => e.SeriesId = _series.Id)
                                       .Build();

            _otherSeries = Builder<Series>.CreateNew()
                                          .With(s => s.Id = 2)
                                          .Build();

            _otherEpisode = Builder<Episode>.CreateNew()
                                            .With(e => e.SeriesId = _otherSeries.Id)
                                            .With(e => e.Id = 2)
                                            .With(e => e.SeasonNumber = 2)
                                            .With(e => e.EpisodeNumber = 2)
                                            .Build();

            _remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                   .With(r => r.Series = _series)
                                                   .With(r => r.Episodes = new List<Episode> { _episode })
                                                   .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.DVD)})
                                                   .Build();
        }

        [Test]
        public void should_return_false_when_queue_is_empty()
        {
            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode>()).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_series_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                       .With(r => r.Series = _otherSeries)
                                                       .With(r => r.Episodes = new List<Episode> { _episode })
                                                       .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_quality_in_queue_is_lower()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.SDTV)
                                                                                       })
                                                      .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_episode_doesnt_match()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD)
                                                                                       })
                                                      .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_qualities_are_the_same()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.DVD)
                                                                                       })
                                                      .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_quality_in_queue_is_better()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                       {
                                                                                           Quality = new QualityModel(Quality.HDTV720p)
                                                                                       })
                                                      .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_matching_multi_episode_is_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p)
                                                      })
                                                      .Build();

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_multi_episode_has_one_episode_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p)
                                                      })
                                                      .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_multi_part_episode_is_already_in_queue()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Series = _series)
                                                      .With(r => r.Episodes = new List<Episode> { _episode, _otherEpisode })
                                                      .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                      {
                                                          Quality = new QualityModel(Quality.HDTV720p)
                                                      })
                                                      .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);

            Subject.IsInQueue(_remoteEpisode, new List<RemoteEpisode> { remoteEpisode }).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_multi_part_episode_has_two_episodes_in_queue()
        {
            var remoteEpisodes = Builder<RemoteEpisode>.CreateListOfSize(2)
                                                       .All()
                                                       .With(r => r.Series = _series)
                                                       .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                                                        {
                                                                                            Quality =
                                                                                                new QualityModel(
                                                                                                Quality.HDTV720p)
                                                                                        })
                                                       .TheFirst(1)
                                                       .With(r => r.Episodes = new List<Episode> {_episode})
                                                       .TheNext(1)
                                                       .With(r => r.Episodes = new List<Episode> {_otherEpisode})
                                                       .Build();

            _remoteEpisode.Episodes.Add(_otherEpisode);

            Subject.IsInQueue(_remoteEpisode, remoteEpisodes ).Should().BeTrue();
        }
    }
}