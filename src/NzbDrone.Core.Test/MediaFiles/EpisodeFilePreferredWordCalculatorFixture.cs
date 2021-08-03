using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly KeyValuePair<string, int> _positiveScore = new KeyValuePair<string, int>("Positive", 10);
        private readonly KeyValuePair<string, int> _negativeScore = new KeyValuePair<string, int>("Negative", -10);
        private KeyValuePair<string, int> _neutralScore = new KeyValuePair<string, int>("Neutral", 0);
        private Series _series;
        private EpisodeFile _episodeFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().Build();
            _episodeFile = Builder<EpisodeFile>.CreateNew().Build();

            Mocker.GetMock<IPreferredWordService>()
                .Setup(s => s.GetMatchingPreferredWordsAndScores(It.IsAny<Series>(), It.IsAny<string>(), 0))
                .Returns(new List<KeyValuePair<string, int>>());
        }

        private void GivenPreferredWordScore(string title, params KeyValuePair<string, int>[] matches)
        {
            Mocker.GetMock<IPreferredWordService>()
                  .Setup(s => s.GetMatchingPreferredWordsAndScores(It.IsAny<Series>(), title, 0))
                  .Returns(matches.ToList());
        }

        [Test]
        public void should_return_score_for_relative_file_name_when_it_is_higher_than_scene_name()
        {
            GivenPreferredWordScore(_episodeFile.SceneName, _positiveScore);
            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_full_file_name_when_relative_file_name_is_not_available()
        {
            _episodeFile.SceneName = null;
            _episodeFile.RelativePath = null;

            GivenPreferredWordScore(_episodeFile.Path, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_relative_file_name_when_scene_name_is_null()
        {
            _episodeFile.SceneName = null;

            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_scene_name_when_higher_than_relative_file_name()
        {
            GivenPreferredWordScore(_episodeFile.SceneName, _positiveScore, _positiveScore, _positiveScore);
            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(30);
        }

        [Test]
        public void should_return_score_for_relative_file_if_available()
        {
            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore, _positiveScore);
            GivenPreferredWordScore(_episodeFile.Path, _positiveScore, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(20);
        }

        [Test]
        public void should_return_score_for_original_path_folder_name_if_highest()
        {
            var folderName = "folder-name";
            var fileName = "file-name";

            _episodeFile.OriginalFilePath = Path.Combine(folderName, fileName);

            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore);
            GivenPreferredWordScore(_episodeFile.Path, _positiveScore, _positiveScore);
            GivenPreferredWordScore(folderName, _positiveScore, _positiveScore, _positiveScore);
            GivenPreferredWordScore(fileName, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(30);
        }

        [Test]
        public void should_return_score_for_original_path_file_name_if_highest()
        {
            var folderName = "folder-name";
            var fileName = "file-name";

            _episodeFile.OriginalFilePath = Path.Combine(folderName, fileName);

            GivenPreferredWordScore(_episodeFile.RelativePath, _positiveScore);
            GivenPreferredWordScore(_episodeFile.Path, _positiveScore, _positiveScore);
            GivenPreferredWordScore(folderName, _positiveScore, _positiveScore);
            GivenPreferredWordScore(fileName, _positiveScore, _positiveScore, _positiveScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(30);
        }

        [Test]
        public void should_return_negative_score_if_0_result_has_no_matches()
        {
            var folderName = "folder-name";
            var fileName = "file-name";

            _episodeFile.OriginalFilePath = Path.Combine(folderName, fileName);

            GivenPreferredWordScore(_episodeFile.RelativePath, _negativeScore);
            GivenPreferredWordScore(fileName);

            Subject.Calculate(_series, _episodeFile).Should().Be(-10);
        }

        [Test]
        public void should_return_0_score_if_0_result_has_matches()
        {
            var folderName = "folder-name";
            var fileName = "file-name";

            _episodeFile.OriginalFilePath = Path.Combine(folderName, fileName);

            GivenPreferredWordScore(_episodeFile.RelativePath, _negativeScore);
            GivenPreferredWordScore(_episodeFile.Path, _negativeScore);
            GivenPreferredWordScore(folderName, _negativeScore);
            GivenPreferredWordScore(fileName, _neutralScore);

            Subject.Calculate(_series, _episodeFile).Should().Be(0);
        }
    }
}
