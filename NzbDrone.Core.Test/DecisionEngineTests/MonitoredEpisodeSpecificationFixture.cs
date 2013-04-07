

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class MonitoredEpisodeSpecificationFixture : CoreTest
    {
        private MonitoredEpisodeSpecification monitoredEpisodeSpecification;

        private IndexerParseResult parseResultMulti;
        private IndexerParseResult parseResultSingle;
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

            parseResultMulti = new IndexerParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
            };

            parseResultSingle = new IndexerParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
            };

            firstEpisode = new Episode { Ignored = false };
            secondEpisode = new Episode { Ignored = false };

            var singleEpisodeList = new List<Episode> { firstEpisode };
            var doubleEpisodeList = new List<Episode> { firstEpisode, secondEpisode };

            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodesByParseResult(parseResultSingle)).Returns(singleEpisodeList);
            Mocker.GetMock<IEpisodeService>().Setup(c => c.GetEpisodesByParseResult(parseResultMulti)).Returns(doubleEpisodeList);

            Mocker.GetMock<ISeriesRepository>().Setup(c => c.GetByTitle(parseResultMulti.CleanTitle)).Returns(fakeSeries);
            Mocker.GetMock<ISeriesRepository>().Setup(c => c.GetByTitle(parseResultSingle.CleanTitle)).Returns(fakeSeries);
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
                 .Setup(p => p.GetByTitle(It.IsAny<String>()))
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