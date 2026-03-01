using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class release_profile_indexer_idsFixture : MigrationTest<release_profile_indexer_ids>
{
    [Test]
    public void should_convert_default_value_for_indexer_id_to_list()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("ReleaseProfiles").Row(new
            {
                Name = "Profile",
                Enabled = true,
                Required = "[]",
                Ignored = "[]",
                IndexerId = 0,
                Tags = "[]",
                ExcludedTags = "[]",
            });
        });

        var releaseProfiles = db.Query<ReleaseProfile224>("SELECT \"Id\", \"Name\", \"IndexerIds\" FROM \"ReleaseProfiles\"");

        releaseProfiles.Should().HaveCount(1);
        releaseProfiles.First().Name.Should().Be("Profile");
        releaseProfiles.First().IndexerIds.Should().BeEmpty();
    }

    [Test]
    public void should_convert_single_value_for_indexer_id_to_list()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("ReleaseProfiles").Row(new
            {
                Name = "Profile",
                Enabled = true,
                Required = "[]",
                Ignored = "[]",
                IndexerId = 42,
                Tags = "[]",
                ExcludedTags = "[]",
            });
        });

        var releaseProfiles = db.Query<ReleaseProfile224>("SELECT \"Id\", \"Name\", \"IndexerIds\" FROM \"ReleaseProfiles\"");

        releaseProfiles.Should().HaveCount(1);
        releaseProfiles.First().Name.Should().Be("Profile");
        releaseProfiles.First().IndexerIds.Should().BeEquivalentTo(new List<int> { 42 });
    }
}

internal class ReleaseProfile224
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<int> IndexerIds { get; set; }
}
