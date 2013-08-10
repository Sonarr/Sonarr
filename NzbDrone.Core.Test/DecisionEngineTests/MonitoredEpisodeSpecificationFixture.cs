using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

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
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void only_episode_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle, null).Should().BeFalse();
        }

        [Test]
        public void both_episodes_not_monitored_should_return_false()
        {
            WithFirstEpisodeUnmonitored();
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().BeFalse();
        }

        [Test]
        public void only_first_episode_not_monitored_should_return_monitored()
        {
            WithFirstEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void only_second_episode_not_monitored_should_return_monitored()
        {
            WithSecondEpisodeUnmonitored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, null).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti, new SeasonSearchCriteria()).Should().BeTrue();
        }
    }
}