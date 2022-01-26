using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class remove_invalid_roksbox_metadata_imagesFixture : MigrationTest<remove_invalid_roksbox_metadata_images>
    {
        [Test]
        public void should_remove_incorrect_roksbox_metadata_images()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    Consumer = "RoksboxMetadata",
                    Type = 5,
                    RelativePath = @"Season 01\metadata\S01E01.jpg",
                    LastUpdated = "2023-01-21 00:00:00.000",
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    Added = "2023-01-21 00:00:00.000",
                    Extension = ".jpg"
                });

                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    Consumer = "RoksboxMetadata",
                    Type = 5,
                    RelativePath = @"Season 01\S01E01.jpg",
                    LastUpdated = "2023-01-21 00:00:00.000",
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    Added = "2023-01-21 00:00:00.000",
                    Extension = ".jpg"
                });
            });

            var metadataFiles = db.Query<MetadataFile184>("SELECT * FROM \"MetadataFiles\"");

            metadataFiles.Should().HaveCount(1);
            metadataFiles.First().RelativePath.Should().NotContain("metadata");
        }
    }

    public class MetadataFile184
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int? EpisodeFileId { get; set; }
        public int? SeasonNumber { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public string Consumer { get; set; }
        public int Type { get; set; }
    }
}
