using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class NotInUseSpecificationFixture : CoreTest<NotInUseSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Series = Builder<Series>.CreateNew().Build()
            };
        }

        private void GivenChildOfSeries()
        {
            _localEpisode.ExistingFile = true;
        }

        [Test]
        public void should_return_true_if_file_is_under_series_folder()
        {
            GivenChildOfSeries();

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }

        [Test]
        public void should_not_check_for_file_in_use_if_child_of_series_folder()
        {
            GivenChildOfSeries();

            Subject.IsSatisfiedBy(_localEpisode);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.IsFileLocked(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_false_if_file_is_in_use()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsFileLocked(It.IsAny<string>()))
                .Returns(true);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_file_is_not_in_use()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsFileLocked(It.IsAny<string>()))
                .Returns(false);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }
    }
}
