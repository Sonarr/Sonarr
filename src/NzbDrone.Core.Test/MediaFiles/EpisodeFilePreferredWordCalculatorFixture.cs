using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class EpisodeFilePreferredWordCalculatorFixture : CoreTest<EpisodeFilePreferredWordCalculator>
    {
        private Series _series;
        private EpisodeFile _episodeFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();
            _episodeFile = Builder<EpisodeFile>.CreateNew().Build();
        }

        private void GivenPreferredWordScore(string title, int score)
        {
            Mocker.GetMock<IPreferredWordService>()
                  .Setup(s => s.Calculate(It.IsAny<Series>(), title, 0))
                  .Returns(score);
        }

        [Test]
        public void should_return_score_for_relative_file_name_when_it_is_higher_than_scene_name()
        {
            GivenPreferredWordScore(_episodeFile.SceneName, 10);
            GivenPreferredWordScore(_episodeFile.RelativePath, 20);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_full_file_name_when_relative_file_name_is_not_available()
        {
            _episodeFile.SceneName = null;
            _episodeFile.RelativePath = null;

            GivenPreferredWordScore(_episodeFile.Path, 20);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_relative_file_name_when_scene_name_is_null()
        {
            _episodeFile.SceneName = null;

            GivenPreferredWordScore(_episodeFile.RelativePath, 20);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_scene_name_when_higher_than_relative_file_name()
        {
            GivenPreferredWordScore(_episodeFile.SceneName, 50);
            GivenPreferredWordScore(_episodeFile.RelativePath, 20);

            Subject.Calculate(_series, _episodeFile).Should().Be(50);
        }

        [Test]
        public void should_return_score_for_relative_file_if_available()
        {
            GivenPreferredWordScore(_episodeFile.RelativePath, 20);
            GivenPreferredWordScore(_episodeFile.Path, 50);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }
    }
}