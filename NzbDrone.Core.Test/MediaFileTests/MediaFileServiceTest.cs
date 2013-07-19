using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFileTests
{
    [TestFixture]
    public class MediaFileServiceTest : CoreTest<MediaFileService>
    {

        [Test]
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV-720p]",
            "Law & Order- Criminal Intent - S10E07 - Icarus [HDTV-720p]")]
        public void CleanFileName(string name, string expectedName)
        {
            FileNameBuilder.CleanFilename(name).Should().Be(expectedName);
        }

        [Test]
        public void filter_should_return_all_files_if_no_existing_files()
        {
            var files = new List<string>()
            {
                "c:\\file1.avi",
                "c:\\file2.avi",
                "c:\\file3.avi",
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                .Returns(new List<EpisodeFile>());


            Subject.FilterExistingFiles(files, 10).Should().BeEquivalentTo(files);
        }


        [Test]
        public void filter_should_return_none_if_all_files_exist()
        {
            var files = new List<string>()
            {
                "c:\\file1.avi",
                "c:\\file2.avi",
                "c:\\file3.avi",
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                .Returns(files.Select(f => new EpisodeFile { Path = f }).ToList());


            Subject.FilterExistingFiles(files, 10).Should().BeEmpty();
        }

        [Test]
        public void filter_should_return_none_existing_files()
        {
            var files = new List<string>()
            {
                "c:\\file1.avi",
                "c:\\file2.avi",
                "c:\\file3.avi",
            };

            Mocker.GetMock<IMediaFileRepository>()
                .Setup(c => c.GetFilesBySeries(It.IsAny<int>()))
                .Returns(new List<EpisodeFile>
                {
                    new EpisodeFile{Path = "c:\\file2.avi"}
                });


            Subject.FilterExistingFiles(files, 10).Should().HaveCount(2);
            Subject.FilterExistingFiles(files, 10).Should().NotContain("c:\\file2.avi");
        }
    }
}