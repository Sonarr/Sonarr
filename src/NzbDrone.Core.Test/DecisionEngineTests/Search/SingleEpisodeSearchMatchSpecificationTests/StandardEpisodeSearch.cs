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

            _searchCriteria.SeasonNumber = 5;
            _searchCriteria.EpisodeNumber = 1;

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.GetTvdbSeasonNumber(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, i) => i);
        }

        private void GivenMapping(int sceneSeasonNumber, int seasonNumber)
        {
            Mocker.GetMock<ISceneMappingService>()
                  .Setup(v => v.GetTvdbSeasonNumber(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns<string, string, int>((s, r, i) => i >= sceneSeasonNumber ? (seasonNumber + i - sceneSeasonNumber) : i);
        }

        [Test]
        public void should_return_false_if_season_does_not_match()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_season_matches_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;

            GivenMapping(10, 5);

            Subject.IsSatisfiedBy(_remoteEpisode, _searchCriteria).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_season_does_not_match_after_scenemapping()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 10;

            GivenMapping(9, 5);

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