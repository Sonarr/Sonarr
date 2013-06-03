using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class CoverAlreadyExistsSpecificationFixture : CoreTest<CoverAlreadyExistsSpecification>
    {
        private Dictionary<string, string> _headers;

        [SetUp]
        public void Setup()
        {
            _headers = new Dictionary<string, string>();
            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetFileSize(It.IsAny<string>())).Returns(100);
            Mocker.GetMock<IHttpProvider>().Setup(c => c.GetHeader(It.IsAny<string>())).Returns(_headers);

        }


        private void GivenFileExistsOnDisk()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(It.IsAny<string>())).Returns(true);
        }


        private void GivenExistingFileSize(long bytes)
        {
            GivenFileExistsOnDisk();
            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetFileSize(It.IsAny<string>())).Returns(bytes);

        }


        [Test]
        public void should_return_false_if_file_not_exists()
        {
            Subject.AlreadyExists("http://url", "c:\\file.exe").Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_file_exists_but_diffrent_size()
        {
            GivenExistingFileSize(100);
            _headers.Add(HttpProvider.ContentLenghtHeader, "200");

            Subject.AlreadyExists("http://url", "c:\\file.exe").Should().BeFalse();
        }


        [Test]
        public void should_return_ture_if_file_exists_and_same_size()
        {
            GivenExistingFileSize(100);
            _headers.Add(HttpProvider.ContentLenghtHeader, "100");

            Subject.AlreadyExists("http://url", "c:\\file.exe").Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_there_is_no_size_header_and_file_exist()
        {
            GivenExistingFileSize(100);
            Subject.AlreadyExists("http://url", "c:\\file.exe").Should().BeFalse();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}