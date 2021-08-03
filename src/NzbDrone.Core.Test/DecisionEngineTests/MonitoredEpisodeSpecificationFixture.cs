using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredEpisodeSpecificationFixture : CoreTest<MonitoredEpisodeSpecification>
    {
        private MonitoredEpisodeSpecification _monitoredEpisodeSpecification;

        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Series _fakeSeries;
        private Episode _firstEpisode;
        private Episode _secondEpisode;

        [SetUp]
        public void Setup()
        {
            _monitoredEpisodeSpecification = Mocker.Resolve<MonitoredEpisodeSpecification>();

            _fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstEpisode = new Episode { Monitored = true };
            _secondEpisode = new Episode { Monitored = true };

            var singleEpisodeList = new List<Episode> { _firstEpisode };
            var doubleEpisodeList = new List<Episode> { _firstEpisode, _secondEpisode };

            _parseResultMulti = new RemoteEpisode
            {
                Series = _fakeSeries,
                Episodes = doubleEpisodeList
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = _fakeSeries,
                Episodes = singleEpisodeList
            };
        }

        private void WithFirstEpisodeUnmonitored()
        {
            _firstEpisode.Monitored = false;
        }

        private void WithSecondEpisodeUnmonitored()
        {
            _secondEpisode.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void both_episodes_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_first_episode_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_second_episode_not_monitored_should_return_false()
        {
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_episode_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new SingleEpisodeSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_is_monitored_for_season_search()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new SeasonSearchCriteria()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_is_not_monitored_for_season_search()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new SeasonSearchCriteria { MonitoredEpisodesOnly = true }).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_episode_is_not_monitored_and_monitoredEpisodesOnly_flag_is_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = false }).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_is_not_monitored_and_monitoredEpisodesOnly_flag_is_true()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, new SingleEpisodeSearchCriteria { MonitoredEpisodesOnly = true }).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_all_episodes_are_not_monitored_for_season_pack_release()
        {
            WithSecondEpisodeUnmonitored();
            _parseResultMulti.ParsedEpisodeInfo = new ParsedEpisodeInfo
                                                  {
                                                    FullSeason = true
                                                  };

            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().BeFalse();
        }
    }
}
