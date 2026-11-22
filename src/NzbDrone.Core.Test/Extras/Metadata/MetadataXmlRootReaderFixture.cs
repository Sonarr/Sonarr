using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Extras.Metadata
{
    [TestFixture]
    public class MetadataXmlRootReaderFixture : CoreTest<MetadataXmlRootReader>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.OpenReadStream(It.IsAny<string>()))
                  .Returns<string>(File.OpenRead);
        }

        private string GivenFile(string contents)
        {
            var path = Path.Combine(TempFolder, "episode.xml");
            File.WriteAllText(path, contents);

            return path;
        }

        [TestCase("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><details><title>Title</title></details>", "details")]
        [TestCase("<?xml version=\"1.0\"?>\n<!-- comment -->\n<video>\n  <title>Title</title>\n</video>", "video")]
        [TestCase("<?xml version=\"1.0\"?><!DOCTYPE Chapters SYSTEM \"matroskachapters.dtd\"><Chapters><EditionEntry /></Chapters>", "Chapters")]
        public void should_return_root_element_name(string contents, string expected)
        {
            var path = GivenFile(contents);

            Subject.GetRootElementName(path).Should().Be(expected);
        }

        [TestCase("")]
        [TestCase("not xml")]
        public void should_return_null_for_invalid_xml(string contents)
        {
            var path = GivenFile(contents);

            Subject.GetRootElementName(path).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_file_cannot_be_read()
        {
            Subject.GetRootElementName(Path.Combine(TempFolder, "missing.xml")).Should().BeNull();
        }
    }
}
