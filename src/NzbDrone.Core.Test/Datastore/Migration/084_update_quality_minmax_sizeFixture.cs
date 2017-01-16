using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class update_quality_minmax_sizeFixture : MigrationTest<update_quality_minmax_size>
    {
        [Test]
        public void should_not_fail_if_empty()
        {
            var db = WithMigrationTestDb();

            var qualityDefinitions = db.Query<QualityDefinition84>("SELECT * FROM QualityDefinitions");

            qualityDefinitions.Should().BeEmpty();
        }

        [Test]
        public void should_set_rawhd_to_null()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 100
                })
                .Row(new
                {
                    Quality = 10,
                    Title = "RawHD",
                    MinSize = 0,
                    MaxSize = 100
                });
            });

            var qualityDefinitions = db.Query<QualityDefinition84>("SELECT * FROM QualityDefinitions");

            qualityDefinitions.Should().HaveCount(2);
            qualityDefinitions.First(v => v.Quality == 10).MaxSize.Should().NotHaveValue();
        }

        [Test]
        public void should_set_zero_maxsize_to_null()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 0
                });
            });

            var qualityDefinitions = db.Query<QualityDefinition84>("SELECT * FROM QualityDefinitions");

            qualityDefinitions.Should().HaveCount(1);
            qualityDefinitions.First(v => v.Quality == 1).MaxSize.Should().NotHaveValue();
        }

        [Test]
        public void should_preserve_values()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("QualityDefinitions").Row(new
                {
                    Quality = 1,
                    Title = "SDTV",
                    MinSize = 0,
                    MaxSize = 100
                })
                .Row(new
                {
                    Quality = 10,
                    Title = "RawHD",
                    MinSize = 0,
                    MaxSize = 100
                });
            });

            var qualityDefinitions = db.Query<QualityDefinition84>("SELECT * FROM QualityDefinitions");

            qualityDefinitions.Should().HaveCount(2);
            qualityDefinitions.First(v => v.Quality == 1).MaxSize.Should().Be(100);
        }
    }
}
