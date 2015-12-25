using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class metadata_files_extensionFixture : MigrationTest<extra_and_subtitle_files>
    {
        [Test]
        public void should_set_extension_using_relative_path()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("MetadataFiles").Row(new
                {
                    SeriesId = 1,
                    RelativePath = "banner.jpg",
                    LastUpdated = "2016-05-30 20:23:02.3725923",
                    Type = 3,
                    Consumer = "XbmcMetadata"
                });
            });

            var items = db.Query<MetadataFile99>("SELECT * FROM MetadataFiles");

            items.Should().HaveCount(1);
            items.First().Extension.Should().Be(".jpg");
        }
    }
}
