using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Wdtv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Wdtv
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<WdtvMetadata>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\The.Series".AsOsAgnostic())
                                     .Build();
        }

        private void GivenXmlRootElement(string path, string rootElement)
        {
            Mocker.GetMock<IReadMetadataXmlRoot>()
                  .Setup(s => s.GetRootElementName(path))
                  .Returns(rootElement);
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_series.Path, "file.jpg");

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [TestCase("Specials")]
        [TestCase("specials")]
        [TestCase("Season 1")]
        public void should_return_season_image(string folder)
        {
            var path = Path.Combine(_series.Path, folder, "folder.jpg");

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.SeasonImage);
        }

        [TestCase(".metathumb", MetadataType.EpisodeImage)]
        public void should_return_metadata_for_episode_if_valid_file_for_episode(string extension, MetadataType type)
        {
            var path = Path.Combine(_series.Path, "the.series.s01e01.episode" + extension);

            Subject.FindMetadataFile(_series, path).Type.Should().Be(type);
        }

        [Test]
        public void should_return_episode_metadata_for_wdtv_xml()
        {
            var path = Path.Combine(_series.Path, "the.series.s01e01.episode.xml");

            GivenXmlRootElement(path, "details");

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.EpisodeMetadata);
        }

        [TestCase("Chapters")]
        [TestCase("video")]
        [TestCase(null)]
        public void should_return_null_for_xml_that_is_not_wdtv_metadata(string rootElement)
        {
            var path = Path.Combine(_series.Path, "the.series.s01e01.episode_chapters.xml");

            GivenXmlRootElement(path, rootElement);

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [TestCase(".xml")]
        [TestCase(".metathumb")]
        public void should_return_null_if_not_valid_file_for_episode(string extension)
        {
            var path = Path.Combine(_series.Path, "the.series.episode" + extension);

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [Test]
        public void should_return_series_image_for_folder_jpg_in_series_folder()
        {
            var path = Path.Combine(_series.Path, "folder.jpg");

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.SeriesImage);
        }
    }
}
