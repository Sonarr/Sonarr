using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class import_list_tag_existing_typeFixture : MigrationTest<import_list_tag_existing_type>
{
    private void AddImportList(import_list_tag_existing_type c, string name, bool tagExisting)
    {
        c.Insert.IntoTable("ImportLists").Row(new
        {
            Name = name,
            Implementation = "PlexImport",
            Settings = "{}",
            ConfigContract = "PlexListSettings",
            EnableAutomaticAdd = true,
            RootFolderPath = "/tv",
            ShouldMonitor = 0,
            QualityProfileId = 1,
            Tags = "[1]",
            SeriesType = 0,
            SeasonFolder = true,
            MonitorNewItems = 0,
            SearchForMissingEpisodes = true,
            TagExisting = tagExisting
        });
    }

    [Test]
    public void should_convert_tag_existing_to_type()
    {
        var db = WithMigrationTestDb(c =>
        {
            AddImportList(c, "Enabled", true);
            AddImportList(c, "Disabled", false);
        });

        var importLists = db.Query<ImportList236>("SELECT \"Name\", \"TagExisting\" FROM \"ImportLists\"");

        importLists.Should().HaveCount(2);
        importLists.Single(i => i.Name == "Enabled").TagExisting.Should().Be(1);
        importLists.Single(i => i.Name == "Disabled").TagExisting.Should().Be(0);
    }
}

internal class ImportList236
{
    public string Name { get; set; }
    public int TagExisting { get; set; }
}
