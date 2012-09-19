// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class EpisodeAiredAfterCutoffSpecificationFixture : CoreTest
    {
        private EpisodeAiredAfterCutoffSpecification episodeAiredAfterCutoffSpecification;

        private EpisodeParseResult parseResultMulti;
        private EpisodeParseResult parseResultSingle;
        private Series fakeSeries;
        private Episode firstEpisode;
        private Episode secondEpisode;

        [SetUp]
        public void Setup()
        {
            episodeAiredAfterCutoffSpecification = Mocker.Resolve<EpisodeAiredAfterCutoffSpecification>();

            fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(c => c.DownloadEpisodesAiredAfter = null)
                .Build();

            parseResultMulti = new EpisodeParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3, 4 },
                SeasonNumber = 12,
            };

            parseResultSingle = new EpisodeParseResult
            {
                SeriesTitle = "Title",
                Series = fakeSeries,
                EpisodeNumbers = new List<int> { 3 },
                SeasonNumber = 12,
            };

            firstEpisode = new Episode { AirDate = DateTime.Today };
            secondEpisode = new Episode { AirDate = DateTime.Today };

            var singleEpisodeList = new List<Episode> { firstEpisode };
            var doubleEpisodeList = new List<Episode> { firstEpisode, secondEpisode };

            Mocker.GetMock<EpisodeProvider>().Setup(c => c.GetEpisodesByParseResult(parseResultSingle)).Returns(singleEpisodeList);
            Mocker.GetMock<EpisodeProvider>().Setup(c => c.GetEpisodesByParseResult(parseResultMulti)).Returns(doubleEpisodeList);
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
            fakeSeries.DownloadEpisodesAiredAfter = DateTime.Today.AddDays(-1);
        }

        private void WithAiredAfterLastWeek()
        {
            fakeSeries.DownloadEpisodesAiredAfter = DateTime.Today.AddDays(-7);
        }

        [Test]
        public void should_return_true_when_downloadEpisodesAiredAfter_is_null_for_single_episode()
        {
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_downloadEpisodesAiredAfter_is_null_for_multiple_episodes()
        {
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_both_episodes_air_after_cutoff()
        {
            WithAiredAfterLastWeek();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_airs_after_cutoff()
        {
            WithAiredAfterLastWeek();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultSingle).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_first_episode_aired_after_cutoff()
        {
            WithAiredAfterLastWeek();
            WithSecondEpisodeYear();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_second_episode_aired_after_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultMulti).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_both_episodes_aired_before_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            WithSecondEpisodeYear();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultMulti).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_episode_aired_before_cutoff()
        {
            WithAiredAfterLastWeek();
            WithFirstEpisodeLastYear();
            episodeAiredAfterCutoffSpecification.IsSatisfiedBy(parseResultSingle).Should().BeFalse();
        }
    }
}