// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class CustomStartDateSpecificationFixture : CoreTest
    {
        private CustomStartDateSpecification _customStartDateSpecification;

        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
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

            parseResultMulti = new EpisodeParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
                Episodes = new List<Episode> { firstEpisode, secondEpisode }
            };

            parseResultSingle = new EpisodeParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
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