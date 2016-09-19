using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class fix_metadata_file_extensionsFixture : MigrationTest<fix_metadata_file_extensions>
    {
        [Test]
        public void should_fix_extension_when_relative_path_contained_multiple_periods()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    SeasonNumber = 1,
                    EpisodeFileId = 1,
                    RelativePath = "Series.Title.S01E01.jpg",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Type = 5,
                    Consumer = "XbmcMetadata",
                    Extension = ".S01E01.jpg"
                });
            });

            var items = db.Query<MetadataFile99>("SELECT * FROM MetadataFiles");

            items.Should().HaveCount(1);
            items.First().Extension.Should().Be(".jpg");
        }
    }
}
