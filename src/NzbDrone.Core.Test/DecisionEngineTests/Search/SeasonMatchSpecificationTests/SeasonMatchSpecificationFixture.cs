using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search.SeasonMatchSpecificationTests
{
    [TestFixture]
    public class SeasonMatchSpecificationFixture : TestBase<SeasonMatchSpecification>
    {
        private readonly RemoteEpisode _remoteEpisode = new();
        private readonly SeasonSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeasonNumbers = [5]
            };

            _searchCriteria.SeasonNumber = 5;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_true_if_season_matches()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_season_does_not_match()
        {
            _searchCriteria.SeasonNumber = 10;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_searched_season_is_within_multi_season_pack_range()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [1, 2, 3, 4, 5];
            _searchCriteria.SeasonNumber = 3;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_searched_season_is_outside_multi_season_pack_range()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = [1, 2, 3, 4, 5];
            _searchCriteria.SeasonNumber = 7;

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }
    }
}
