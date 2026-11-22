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
    public class CleanupConflictingMetadataFilesFixture : DbTest<CleanupConflictingMetadataFiles, MetadataFile>
    {
        private const string RelativePath = "Season 1/Series Title - S01E01_chapters.xml";

        [Test]
        public void should_delete_episode_metadata_claimed_by_multiple_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.SeriesId = 1)
                                             .With(m => m.RelativePath = RelativePath)
                                             .TheFirst(1)
                                             .With(m => m.Consumer = "WdtvMetadata")
                                             .TheNext(1)
                                             .With(m => m.Consumer = "RoksboxMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_episode_metadata_with_different_paths()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.SeriesId = 1)
                                             .TheFirst(1)
                                             .With(m => m.Consumer = "WdtvMetadata")
                                             .TheNext(1)
                                             .With(m => m.Consumer = "RoksboxMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_episode_metadata_with_the_same_path_for_different_series()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeMetadata)
                                             .With(m => m.RelativePath = RelativePath)
                                             .TheFirst(1)
                                             .With(m => m.Consumer = "WdtvMetadata")
                                             .TheNext(1)
                                             .With(m => m.Consumer = "RoksboxMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_other_metadata_types_claimed_by_multiple_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.EpisodeImage)
                                             .With(m => m.SeriesId = 1)
                                             .With(m => m.RelativePath = "Season 1/Series Title - S01E01.jpg")
                                             .TheFirst(1)
                                             .With(m => m.Consumer = "WdtvMetadata")
                                             .TheNext(1)
                                             .With(m => m.Consumer = "RoksboxMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }
    }
}
