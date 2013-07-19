using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFileTests.EpisodeImportTests
{
    [TestFixture]
    public class NotAlreadyImportedSpecificationFixture : CoreTest<NotAlreadyImportedSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi"
            };
        }

        [Test]
        public void should_return_false_if_path_is_already_in_episodeFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Exists(_localEpisode.Path))
                  .Returns(true);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_new_file()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Exists(_localEpisode.Path))
                  .Returns(false);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }
    }
}
