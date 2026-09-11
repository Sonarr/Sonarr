using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search.SingleEpisodeSearchMatchSpecificationTests
{
    [TestFixture]
    public class StandardEpisodeSearch : TestBase<SingleEpisodeSearchMatchSpecification>
    {
        private readonly RemoteEpisode _remoteEpisode = new();
        private readonly SingleEpisodeSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeasonNumbers = [5],
                EpisodeNumbers = [1]
            };
            _remoteEpisode.MappedSeasonNumber = 5;

            _searchCriteria.SeasonNumber = 5;
            _searchCriteria.EpisodeNumber = 1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_season_does_not_match()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [10];
            _remoteEpisode.MappedSeasonNumber = 10;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_season_matches_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [10];
            _remoteEpisode.MappedSeasonNumber = 5; // 10 -> 5 mapping
            _searchCriteria.SeasonNumber = 10; // searching by tvdb 5 = 10 scene

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_season_does_not_match_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [10];
            _remoteEpisode.MappedSeasonNumber = 6; // 9 -> 5 mapping
            _searchCriteria.SeasonNumber = 9; // searching by tvdb 5 = 9 scene

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_full_season_result_for_single_episode_search()
        {
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = Array.Empty<int>();

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_episode_number_does_not_match_search_criteria()
        {
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = [2];

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_full_season_result_for_full_season_search()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_searched_season_is_within_multi_season_pack_range()
        {
            // e.g. "Show.S05E24.S06E01" - a season finale bundled with the next season's premiere.
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [5, 6];
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = [24, 1];
            _searchCriteria.SeasonNumber = 6;
            _searchCriteria.EpisodeNumber = 1;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_searched_season_is_outside_multi_season_pack_range()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [5, 6];
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = [24, 1];
            _searchCriteria.SeasonNumber = 7;
            _searchCriteria.EpisodeNumber = 1;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }
    }
}
