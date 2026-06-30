using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class add_size_to_quality_profilesFixture : MigrationTest<add_size_to_quality_profiles>
{
    private static readonly int HdtvQualityId = (int)Quality.HDTV1080p;
    private static readonly int RawhdQualityId = (int)Quality.RAWHD;

    private string GenerateQualityJson(int quality, bool allowed)
    {
        return $"{{ \"quality\": {quality}, \"items\": [], \"allowed\": {allowed.ToString().ToLowerInvariant()} }}";
    }

    [Test]
    public void should_copy_sizes_from_definition()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("QualityDefinitions").Row(new
            {
                Quality = HdtvQualityId,
                Title = "HDTV-1080p",
                MinSize = 4.0,
                MaxSize = 125.0,
                PreferredSize = 120.0
            });

            c.Insert.IntoTable("QualityProfiles").Row(new
            {
                Name = "HD-1080p",
                Cutoff = HdtvQualityId,
                Items = $"[{GenerateQualityJson(HdtvQualityId, true)}]"
            });
        });

        var profiles = db.Query<Profile207>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

        var item = profiles.First().Items.Single();
        item.MinSize.Should().Be(4.0);
        item.MaxSize.Should().Be(125.0);
        item.PreferredSize.Should().Be(120.0);
    }

    [Test]
    public void should_leave_null_max_and_preferred_size_as_null()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("QualityDefinitions").Row(new
            {
                Quality = RawhdQualityId,
                Title = "Raw-HD",
                MinSize = 4.0,
                MaxSize = (double?)null,
                PreferredSize = (double?)null
            });

            c.Insert.IntoTable("QualityProfiles").Row(new
            {
                Name = "RAW",
                Cutoff = RawhdQualityId,
                Items = $"[{GenerateQualityJson(RawhdQualityId, true)}]"
            });
        });

        var profiles = db.Query<Profile207>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

        var item = profiles.First().Items.Single();
        item.MinSize.Should().Be(4.0);
        item.MaxSize.Should().BeNull();
        item.PreferredSize.Should().BeNull();
    }

    [Test]
    public void should_leave_null_min_size_as_null()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("QualityDefinitions").Row(new
            {
                Quality = HdtvQualityId,
                Title = "HDTV-1080p",
                MinSize = (double?)null,
                MaxSize = 125.0,
                PreferredSize = 120.0
            });

            c.Insert.IntoTable("QualityProfiles").Row(new
            {
                Name = "HD-1080p",
                Cutoff = HdtvQualityId,
                Items = $"[{GenerateQualityJson(HdtvQualityId, true)}]"
            });
        });

        var profiles = db.Query<Profile207>("SELECT \"Items\" FROM \"QualityProfiles\" LIMIT 1");

        var item = profiles.First().Items.Single();
        item.MinSize.Should().BeNull();
        item.MaxSize.Should().Be(125.0);
        item.PreferredSize.Should().Be(120.0);
    }
}
