using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class FreeSpaceSpecificationFixture : CoreTest<FreeSpaceSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;
        private string _rootFolder;

        [SetUp]
        public void Setup()
        {
             _rootFolder = @"C:\Test\TV".AsOsAgnostic();

             _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(s => s.Path = Path.Combine(_rootFolder, "30 Rock"))
                                     .Build();

             var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

             _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Episodes = episodes,
                                    Series = _series
                                };
        }

        private void GivenFileSize(long size)
        {
            _localEpisode.Size = size;
        }

        private void GivenFreeSpace(long? size)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(It.IsAny<string>()))
                  .Returns(size);
        }

        [Test]
        public void should_reject_when_there_isnt_enough_disk_space()
        {
            GivenFileSize(100.Megabytes());
            GivenFreeSpace(80.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_reject_when_there_isnt_enough_space_for_file_plus_min_free_space()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.MinimumFreeSpaceWhenImporting)
                .Returns(100);
            GivenFileSize(100.Megabytes());
            GivenFreeSpace(150.Megabytes());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_accept_when_there_is_enough_disk_space()
        {
            GivenFileSize(100.Megabytes());
            GivenFreeSpace(1.Gigabytes());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_use_series_paths_parent_for_free_space_check()
        {
            GivenFileSize(100.Megabytes());
            GivenFreeSpace(1.Gigabytes());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.GetAvailableSpace(_rootFolder), Times.Once());
        }

        [Test]
        public void should_pass_if_free_space_is_null()
        {
            GivenFileSize(100.Megabytes());
            GivenFreeSpace(null);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_pass_if_exception_is_thrown()
        {
            GivenFileSize(100.Megabytes());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(It.IsAny<string>()))
                  .Throws(new TestException());

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_skip_check_for_files_under_series_folder()
        {
            _localEpisode.ExistingFile = true;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();

            Mocker.GetMock<IDiskProvider>()
                  .Verify(s => s.GetAvailableSpace(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_return_true_if_free_space_is_null()
        {
            long? freeSpace = null;

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetAvailableSpace(It.IsAny<string>()))
                  .Returns(freeSpace);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_skip_check_is_enabled()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.SkipFreeSpaceCheckWhenImporting)
                  .Returns(true);

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
