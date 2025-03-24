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
    public class AnimeSearchFixture : TestBase<SingleEpisodeSearchMatchSpecification>
    {
        private RemoteEpisode _remoteEpisode = new();
        private AnimeEpisodeSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_full_season_result_for_single_episode_search()
        {
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_not_a_full_season_result()
        {
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = false;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_full_season_result_for_full_season_search()
        {
            _remoteEpisode.ParsedEpisodeInfo.FullSeason = true;
            _searchCriteria.IsSeasonSearch = true;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }
    }
}
