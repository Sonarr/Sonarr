using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Xbmc;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Xbmc
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<XbmcMetadata>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\The.Series".AsOsAgnostic())
                                     .Build();
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_series.Path, "file.jpg");

            Subject.FindMetadataFile(_series, path).Should().BeNull();
        }

        [Test]
        public void should_return_metadata_for_xbmc_nfo()
        {
            var path = Path.Combine(_series.Path, "the.series.s01e01.episode.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(true);

            Subject.FindMetadataFile(_series, path).Type.Should().Be(MetadataType.EpisodeMetadata);

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_null_for_scene_nfo()
        {
            var path = Path.Combine(_series.Path, "the.series.s01e01.episode.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(false);

            Subject.FindMetadataFile(_series, path).Should().BeNull();

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }
    }
}
