using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SameEpisodesSpecificationFixture : CoreTest<SameEpisodesSpecification>
    {
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodes = Builder<Episode>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.EpisodeFileId = 1)
                                        .BuildList();
        }

        private void GivenEpisodesInFile(List<Episode> episodes)
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesByFileId(It.IsAny<int>()))
                  .Returns(episodes);
        }

        [Test]
        public void should_not_upgrade_when_new_release_contains_less_episodes()
        {
            GivenEpisodesInFile(_episodes);

            Subject.IsSatisfiedBy(new List<Episode> { _episodes.First() }).Should().BeFalse();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_more_episodes()
        {
            GivenEpisodesInFile(new List<Episode> { _episodes.First() });

            Subject.IsSatisfiedBy(_episodes).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_the_same_episodes()
        {
            GivenEpisodesInFile(_episodes);

            Subject.IsSatisfiedBy(_episodes).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_release_contains_the_same_episodes_as_multiple_files()
        {
            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .BuildList();

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesByFileId(episodes.First().EpisodeFileId))
                  .Returns(new List<Episode> { episodes.First() });

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesByFileId(episodes.Last().EpisodeFileId))
                  .Returns(new List<Episode> { episodes.Last() });

            Subject.IsSatisfiedBy(episodes).Should().BeTrue();
        }
    }
}
