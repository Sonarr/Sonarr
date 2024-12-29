using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(193)]
    public class add_import_list_items : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "ImportListItems")
                .WithColumn("ImportListId").AsInt32()
                .WithColumn("Title").AsString()
                .WithColumn("TvdbId").AsInt32()
                .WithColumn("Year").AsInt32().Nullable()
                .WithColumn("TmdbId").AsInt32().Nullable()
                .WithColumn("ImdbId").AsString().Nullable()
                .WithColumn("MalId").AsInt32().Nullable()
                .WithColumn("AniListId").AsInt32().Nullable()
                .WithColumn("ReleaseDate").AsDateTimeOffset().Nullable();

            Alter.Table("ImportListStatus")
                .AddColumn("HasRemovedItemSinceLastClean").AsBoolean().WithDefaultValue(false);
        }
    }
}
