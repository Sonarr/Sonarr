using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search.SingleEpisodeSearchMatchSpecificationTests
{
    [TestFixture]
    public class StandardEpisodeSearch : TestBase<SingleEpisodeSearchMatchSpecification>
    {
        private RemoteEpisode _remoteEpisode = new RemoteEpisode();
        private SingleEpisodeSearchCriteria _searchCriteria = new SingleEpisodeSearchCriteria();

        [SetUp]
        public void Setup()
        {
            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 5;
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = new[] { 1 };
            _remoteEpisode.MappedSeasonNumber = 5;

            _searchCriteria.SeasonNumber = 5;
            _searchCriteria.EpisodeNumber = 1;
        }

        [Test]
        public void should_return_false_if_season_does_not_match()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;
            _remoteEpisode.MappedSeasonNumber = 10;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_season_matches_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;
            _remoteEpisode.MappedSeasonNumber = 5; // 10 -> 5 mapping
            _searchCriteria.SeasonNumber = 10; // searching by tvdb 5 = 10 scene

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_season_does_not_match_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;
            _remoteEpisode.MappedSeasonNumber = 6; // 9 -> 5 mapping
            _searchCriteria.SeasonNumber = 9; // searching by tvdb 5 = 9 scene

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_full_season_result_for_single_episode_search()
        {
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = Array.Empty<int>();

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_episode_number_does_not_match_search_criteria()
        {
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = new[] { 2 };

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_full_season_result_for_full_season_search()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }
    }
}
