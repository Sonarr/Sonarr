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
        public void should_reject_as_full_season_if_searched_season_is_within_multi_season_pack_range()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [1, 2, 3, 4, 5];
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = [];
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;
            _searchCriteria.SeasonNumber = 3;

            var decision = Subject.IsSatisfiedBy(_remoteEpisode, _information);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.FullSeason);
        }

        [Test]
        public void should_reject_as_wrong_season_if_searched_season_is_outside_multi_season_pack_range()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [1, 2, 3, 4, 5];
            _remoteEpisode.ParsedEpisodeInfo.EpisodeNumbers = [];
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;
            _searchCriteria.SeasonNumber = 7;

            var decision = Subject.IsSatisfiedBy(_remoteEpisode, _information);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.WrongSeason);
        }
    }
}
