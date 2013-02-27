// ReSharper disable RedundantUsingDirective

using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class UpgradePossibleSpecificationFixture : CoreTest
    {
        private void WithWebdlCutoff()
        {
            var profile = new QualityProfile { Cutoff = Quality.WEBDL720p };
            Mocker.GetMock<IQualityProfileService>().Setup(s => s.Get(It.IsAny<int>())).Returns(profile);
        }

        private Series _series;
        private EpisodeFile _episodeFile;
        private Episode _episode;

        [SetUp]
        public void SetUp()
        {
            _series = Builder<Series>.CreateNew()
                    .Build();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.Quality = Quality.SDTV)
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                    .With(e => e.SeriesId = _series.Id)
                    .With(e => e.Series = _series)
                    .With(e => e.EpisodeFile = _episodeFile)
                    .Build();
        }

        [Test]
        public void IsUpgradePossible_should_return_true_if_no_episode_file_exists()
        {
            var episode = Builder<Episode>.CreateNew()
                    .With(e => e.EpisodeFile = null)
                    .Build();

            //Act
            bool result = Mocker.Resolve<UpgradePossibleSpecification>().IsSatisfiedBy(episode);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgradePossible_should_return_true_if_current_episode_is_less_than_cutoff()
        {
            WithWebdlCutoff();

            //Act
            bool result = Mocker.Resolve<UpgradePossibleSpecification>().IsSatisfiedBy(_episode);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            WithWebdlCutoff();

            _episodeFile.Quality = Quality.WEBDL720p;

            //Act
            bool result = Mocker.Resolve<UpgradePossibleSpecification>().IsSatisfiedBy(_episode);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            WithWebdlCutoff();

            _episodeFile.Quality = Quality.Bluray720p;

            //Act
            bool result = Mocker.Resolve<UpgradePossibleSpecification>().IsSatisfiedBy(_episode);

            //Assert
            result.Should().BeFalse();
        }
    }
}