using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupDuplicateMetadataFilesFixture : DbTest<CleanupDuplicateMetadataFiles, MetadataFile>
    {
        [Test]
        public async Task should_not_delete_metadata_files_when_they_are_for_the_same_series_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.SeriesMetadata)
                                             .With(m => m.SeriesId = 1)
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_not_delete_metadata_files_for_different_series()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.SeriesMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_delete_metadata_files_when_they_are_for_the_same_series_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.SeriesMetadata)
                                             .With(m => m.SeriesId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_delete_metadata_files_when_there_is_only_one_for_that_series_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                         .BuildNew();

            await Db.InsertAsync(file);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_delete_metadata_files_when_they_are_for_the_same_episode_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.EpisodeFileId = 1)
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_not_delete_metadata_files_for_different_episode()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_delete_metadata_files_when_they_are_for_the_same_episode_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.EpisodeFileId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_delete_metadata_files_when_there_is_only_one_for_that_episode_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                            .BuildNew();

            await Db.InsertAsync(file);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_delete_image_when_they_are_for_the_same_episode_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeImage)
                                             .With(m => m.EpisodeFileId = 1)
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_not_delete_image_for_different_episode()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeImage)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(files.Count);
        }

        [Test]
        public async Task should_delete_image_when_they_are_for_the_same_episode_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeImage)
                                             .With(m => m.EpisodeFileId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            await Db.InsertManyAsync(files);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_delete_image_when_there_is_only_one_for_that_episode_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                            .BuildNew();

            await Db.InsertAsync(file);
            Subject.Clean();
            var metadataFiles = await GetAllStoredModelsAsync();
            metadataFiles.Count.Should().Be(1);
        }
    }
}
