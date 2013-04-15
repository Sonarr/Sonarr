using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
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
        private MonitoredEpisodeSpecification monitoredEpisodeSpecification;

        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private Series fakeSeries;
        private Episode firstEpisode;
        private Episode secondEpisode;

        [SetUp]
        public void Setup()
        {
            monitoredEpisodeSpecification = Mocker.Resolve<MonitoredEpisodeSpecification>();

            fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();


            var singleEpisodeList = new List<Episode> { firstEpisode };
            var doubleEpisodeList = new List<Episode> { firstEpisode, secondEpisode };

            parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                Episodes = doubleEpisodeList
            };

            parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                Episodes = singleEpisodeList
            };

            firstEpisode = new Episode { Ignored = false };
            secondEpisode = new Episode { Ignored = false };


        }

        private void WithFirstEpisodeIgnored()
        {
            firstEpisode.Ignored = true;
        }

        private void WithSecondEpisodeIgnored()
        {
            secondEpisode.Ignored = true;
        }

        [Test]
        public void setup_should_return_monitored_episode_should_return_true()
        {
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }


        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            fakeSeries.Monitored = false;
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }


        [Test]
        public void not_in_db_should_be_skipped()
        {
            Mocker.GetMock<ISeriesRepository>()
                 .Setup(p => p.FindByTitle(It.IsAny<String>()))
                 .Returns<Series>(null);

            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }


        [Test]
        public void only_episode_ignored_should_return_false()
        {
            WithFirstEpisodeIgnored();
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }


        [Test]
        public void both_episodes_ignored_should_return_false()
        {
            WithFirstEpisodeIgnored();
            WithSecondEpisodeIgnored();
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }


        [Test]
        public void only_first_episode_ignored_should_return_monitored()
        {
            WithFirstEpisodeIgnored();
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }


        [Test]
        public void only_second_episode_ignored_should_return_monitored()
        {
            WithSecondEpisodeIgnored();
            monitoredEpisodeSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

    }
}