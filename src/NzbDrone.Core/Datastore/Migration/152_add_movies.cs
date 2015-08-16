using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(152)]
    public class add_movies : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Movies").
                   WithColumn("ImdbId").AsString().Unique()
                  .WithColumn("Title").AsString()
                  .WithColumn("CleanTitle").AsString().Unique()
                  .WithColumn("OriginalTitle").AsString()
                  .WithColumn("Year").AsInt32()
                  .WithColumn("Runtime").AsInt32()
                  .WithColumn("TmdbId").AsInt32().Unique()
                  .WithColumn("Overview").AsString()
                  .WithColumn("TagLine").AsString().Nullable()
                  .WithColumn("ReleaseDate").AsDateTime().Nullable()
                  .WithColumn("Images").AsString()
                  .WithColumn("LastInfoSync").AsDateTime().Nullable()
                  .WithColumn("Path").AsString()
                  .WithColumn("Monitored").AsInt32()
                  .WithColumn("Tags").AsString()
                  .WithColumn("AddOptions").AsBoolean().Nullable()
                  .WithColumn("ProfileId").AsInt32()
                  .WithColumn("MovieFileId").AsInt32();

            Create.TableForModel("MovieFiles").
                   WithColumn("MovieId").AsString().Unique()
                  .WithColumn("RelativePath").AsString()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable()
                  .WithColumn("MediaInfo").AsString().Nullable()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime();
        }
    }
}
