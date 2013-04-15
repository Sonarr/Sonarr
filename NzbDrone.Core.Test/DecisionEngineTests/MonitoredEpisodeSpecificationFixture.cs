using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
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

            _firstEpisode = new Episode { Ignored = false };
            _secondEpisode = new Episode { Ignored = false };


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

        private void WithFirstEpisodeIgnored()
        {
            _firstEpisode.Ignored = true;
        }

        private void WithSecondEpisodeIgnored()
        {
            _secondEpisode.Ignored = true;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle).Should().BeTrue();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti).Should().BeTrue();
        }


        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti).Should().BeFalse();
        }


        [Test]
        public void only_episode_ignored_should_return_false()
        {
            WithFirstEpisodeIgnored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultSingle).Should().BeFalse();
        }


        [Test]
        public void both_episodes_ignored_should_return_false()
        {
            WithFirstEpisodeIgnored();
            WithSecondEpisodeIgnored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti).Should().BeFalse();
        }


        [Test]
        public void only_first_episode_ignored_should_return_monitored()
        {
            WithFirstEpisodeIgnored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti).Should().BeTrue();
        }


        [Test]
        public void only_second_episode_ignored_should_return_monitored()
        {
            WithSecondEpisodeIgnored();
            _monitoredEpisodeSpecification.IsSatisfiedBy(_parseResultMulti).Should().BeTrue();
        }

    }
}