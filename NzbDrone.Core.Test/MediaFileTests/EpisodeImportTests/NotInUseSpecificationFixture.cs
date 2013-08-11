using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFileTests.EpisodeImportTests
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
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsParent(_localEpisode.Series.Path, _localEpisode.Path))
                .Returns(true);
        }

        private void GivenNewFile()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsParent(_localEpisode.Series.Path, _localEpisode.Path))
                .Returns(false);
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
                .Verify(v => v.IsFileLocked(It.IsAny<FileInfo>()), Times.Never());
        }

        [Test]
        public void should_return_false_if_file_is_in_use()
        {
            GivenNewFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsFileLocked(It.IsAny<FileInfo>()))
                .Returns(true);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_file_is_not_in_use()
        {
            GivenNewFile();

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.IsFileLocked(It.IsAny<FileInfo>()))
                .Returns(false);

            Subject.IsSatisfiedBy(_localEpisode).Should().BeTrue();
        }
    }
}
