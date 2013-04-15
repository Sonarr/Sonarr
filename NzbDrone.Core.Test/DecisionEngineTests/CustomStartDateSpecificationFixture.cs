using System;
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

    public class CustomStartDateSpecificationFixture : CoreTest<CustomStartDateSpecification>
    {
        private CustomStartDateSpecification _customStartDateSpecification;

        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private Series fakeSeries;
        private Episode firstEpisode;
        private Episode secondEpisode;

        [SetUp]
        public void Setup()
        {
            _customStartDateSpecification = Mocker.Resolve<CustomStartDateSpecification>();

            firstEpisode = new Episode { AirDate = DateTime.Today };
            secondEpisode = new Episode { AirDate = DateTime.Today };

            fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(c => c.CustomStartDate = null)
                .Build();

            parseResultMulti = new RemoteEpisode
            {
                Series = fakeSeries,
                Episodes = new List<Episode> { firstEpisode, secondEpisode }
            };

            parseResultSingle = new RemoteEpisode
            {
                Series = fakeSeries,
                Episodes = new List<Episode> { firstEpisode }
            };
        }

        private void WithFirstEpisodeLastYear()
        {
            firstEpisode.AirDate = DateTime.Today.AddYears(-1);
        }

        private void WithSecondEpisodeYear()
        {
            secondEpisode.AirDate = DateTime.Today.AddYears(-1);
        }

        private void WithAiredAfterYesterday()
        {
            fakeSeries.CustomStartDate = DateTime.Today.AddDays(-1);
        }

        private void WithAiredAfterLastWeek()
        {
            fakeSeries.CustomStartDate = DateTime.Today.AddDays(-7);
        }

        [Test]
        public void should_return_true_when_downloadEpisodesAiredAfter_is_null_for_single_episode()
        {
            _customStartDateSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_downloadEpisodesAiredAfter_is_null_for_multiple_episodes()
        {
            _customStartDateSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_both_episodes_air_after_cutoff()
        {
            WithAiredAfterLastWeek();
            _customStartDateSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_airs_after_cutoff()
        {
            WithAiredAfterLastWeek();
            _customStartDateSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_first_episode_aired_after_cutoff()
        {
            WithAiredAfterLastWeek();
            WithSecondEpisodeYear();
            _customStartDateSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_second_episode_aired_after_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            _customStartDateSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_both_episodes_aired_before_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            WithSecondEpisodeYear();
            _customStartDateSpecification.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_episode_aired_before_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            _customStartDateSpecification.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_episode_airs_the_same_day_as_the_cutoff()
        {
            fakeSeries.CustomStartDate = DateTime.Today;
            _customStartDateSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }
    }
}