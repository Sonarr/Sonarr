using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DeleteBadMediaCoversFixture : CoreTest<DeleteBadMediaCovers>
    {
        private List<MetadataFile> _metadata;
        private List<Series> _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateListOfSize(1)
                .All()
                .With(c => c.Path = "C:\\TV\\".AsOsAgnostic())
                .Build().ToList();

            _metadata = Builder<MetadataFile>.CreateListOfSize(1)
               .Build().ToList();

            Mocker.GetMock<ISeriesService>()
                .Setup(c => c.GetAllSeries())
                .Returns(_series);

            Mocker.GetMock<IMetadataFileService>()
                .Setup(c => c.GetFilesBySeries(_series.First().Id))
                .Returns(_metadata);

            Mocker.GetMock<IConfigService>().SetupGet(c => c.CleanupMetadataImages).Returns(true);
        }

        [Test]
        public void should_not_process_non_image_files()
        {
            _metadata.First().RelativePath = "season\\file.xml".AsOsAgnostic();
            _metadata.First().Type = MetadataType.EpisodeMetadata;

            Subject.Clean();

            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenReadStream(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_process_images_before_tvdb_switch()
        {
            _metadata.First().LastUpdated = new DateTime(2014, 12, 25);

            Subject.Clean();

            Mocker.GetMock<IDiskProvider>().Verify(c => c.OpenReadStream(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_not_run_if_flag_is_false()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.CleanupMetadataImages).Returns(false);

            Subject.Clean();

            Mocker.GetMock<IConfigService>().VerifySet(c => c.CleanupMetadataImages = true, Times.Never());
            Mocker.GetMock<ISeriesService>().Verify(c => c.GetAllSeries(), Times.Never());

            AssertImageWasNotRemoved();
        }

        [Test]
        public void should_set_clean_flag_to_false()
        {
            _metadata.First().LastUpdated = new DateTime(2014, 12, 25);

            Subject.Clean();

            Mocker.GetMock<IConfigService>().VerifySet(c => c.CleanupMetadataImages = false, Times.Once());
        }

        [Test]
        public void should_delete_html_images()
        {
            var imagePath = "C:\\TV\\Season\\image.jpg".AsOsAgnostic();
            _metadata.First().LastUpdated = new DateTime(2014, 12, 29);
            _metadata.First().RelativePath = "Season\\image.jpg".AsOsAgnostic();
            _metadata.First().Type = MetadataType.SeriesImage;

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.OpenReadStream(imagePath))
                .Returns(new FileStream(GetTestPath("Files/html_image.jpg"), FileMode.Open, FileAccess.Read));

            Subject.Clean();

            Mocker.GetMock<IDiskProvider>().Verify(c => c.DeleteFile(imagePath), Times.Once());
            Mocker.GetMock<IMetadataFileService>().Verify(c => c.Delete(_metadata.First().Id), Times.Once());
        }

        [Test]
        public void should_delete_empty_images()
        {
            var imagePath = "C:\\TV\\Season\\image.jpg".AsOsAgnostic();
            _metadata.First().LastUpdated = new DateTime(2014, 12, 29);
            _metadata.First().Type = MetadataType.SeasonImage;
            _metadata.First().RelativePath = "Season\\image.jpg".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.OpenReadStream(imagePath))
                              .Returns(new FileStream(GetTestPath("Files/emptyfile.txt"), FileMode.Open, FileAccess.Read));

            Subject.Clean();

            Mocker.GetMock<IDiskProvider>().Verify(c => c.DeleteFile(imagePath), Times.Once());
            Mocker.GetMock<IMetadataFileService>().Verify(c => c.Delete(_metadata.First().Id), Times.Once());
        }

        [Test]
        public void should_not_delete_non_html_files()
        {
            var imagePath = "C:\\TV\\Season\\image.jpg".AsOsAgnostic();
            _metadata.First().LastUpdated = new DateTime(2014, 12, 29);
            _metadata.First().RelativePath = "Season\\image.jpg".AsOsAgnostic();

            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.OpenReadStream(imagePath))
                              .Returns(new FileStream(GetTestPath("Files/Queue.txt"), FileMode.Open, FileAccess.Read));

            Subject.Clean();
            AssertImageWasNotRemoved();
        }

        private void AssertImageWasNotRemoved()
        {
            Mocker.GetMock<IDiskProvider>().Verify(c => c.DeleteFile(It.IsAny<string>()), Times.Never());
            Mocker.GetMock<IMetadataFileService>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }
    }
}
