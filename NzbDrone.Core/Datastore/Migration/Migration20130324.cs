using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(20130324)]
    public class Migration20130324 : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("Config")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Key").AsString().PrimaryKey()
                  .WithColumn("Value").AsString().NotNullable();

            Create.Table("EpisodeFiles")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("Path").AsString().NotNullable()
                  .WithColumn("Quality").AsInt32().NotNullable()
                  .WithColumn("Proper").AsBoolean().NotNullable()
                  .WithColumn("Size").AsInt64().NotNullable()
                  .WithColumn("DateAdded").AsDateTime().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            Create.Table("Episodes")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("TvDbEpisodeId").AsInt32().Nullable()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("EpisodeNumber").AsInt32().NotNullable()
                  .WithColumn("Title").AsString().Nullable()
                  .WithColumn("Overview").AsString().Nullable()
                  .WithColumn("Ignored").AsBoolean().Nullable()
                  .WithColumn("EpisodeFileId").AsInt32().Nullable()
                  .WithColumn("AirDate").AsDateTime().Nullable()
                  .WithColumn("GrabDate").AsDateTime().Nullable()
                  .WithColumn("PostDownloadStatus").AsInt32().Nullable()
                  .WithColumn("AbsoluteEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("SceneAbsoluteEpisodeNumber").AsInt32().Nullable()
                  .WithColumn("SceneSeasonNumber").AsInt32().Nullable()
                  .WithColumn("SceneEpisodeNumber").AsInt32().Nullable();

            Create.Table("ExternalNotificationDefinitions")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Name").AsString().NotNullable();

            Create.Table("History")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("EpisodeId").AsInt32().NotNullable()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("NzbTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTime().NotNullable()
                  .WithColumn("Quality").AsString().NotNullable()
                  .WithColumn("Indexer").AsString().NotNullable()
                  .WithColumn("NzbInfoUrl").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            Create.Table("IndexerDefinitions")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Name").AsString().NotNullable();

            Create.Table("JobDefinitions")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Interval").AsInt32().NotNullable()
                  .WithColumn("LastExecution").AsDateTime().NotNullable()
                  .WithColumn("Success").AsBoolean().NotNullable();

            Create.Table("NewznabDefinitions")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Url").AsString().NotNullable()
                  .WithColumn("ApiKey").AsString().Nullable()
                  .WithColumn("BuiltIn").AsBoolean().NotNullable();

            Create.Table("QualityProfiles")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Cutoff").AsInt32().NotNullable()
                  .WithColumn("Allowed").AsString().NotNullable();

            Create.Table("QualitySizes")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("QualityId").AsInt32().NotNullable().Unique()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("MinSize").AsInt32().NotNullable()
                  .WithColumn("MaxSize").AsInt32().NotNullable();

            Create.Table("RootFolders")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Path").AsString().NotNullable();

            Create.Table("SceneMappings")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("CleanTitle").AsString().NotNullable()
                  .WithColumn("SceneName").AsString().NotNullable()
                  .WithColumn("TvdbId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable();

            Create.Table("Seasons")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("SeasonNumber").AsInt32().NotNullable()
                  .WithColumn("Ignored").AsBoolean().NotNullable();

            Create.Table("Series")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("TvdbId").AsInt32().NotNullable()
                  .WithColumn("Title").AsString().NotNullable()
                  .WithColumn("CleanTitle").AsString().NotNullable()
                  .WithColumn("Status").AsInt32().NotNullable()
                  .WithColumn("Overview").AsString().Nullable()
                  .WithColumn("AirTime").AsString().Nullable()
                  .WithColumn("Language").AsString().NotNullable()
                  .WithColumn("Path").AsString().NotNullable()
                  .WithColumn("Monitored").AsBoolean().NotNullable()
                  .WithColumn("QualityProfileId").AsString().NotNullable()
                  .WithColumn("SeasonFolder").AsBoolean().NotNullable()
                  .WithColumn("LastInfoSync").AsDateTime().Nullable()
                  .WithColumn("LastDiskSync").AsDateTime().Nullable()
                  .WithColumn("Runtime").AsInt32().NotNullable()
                  .WithColumn("SeriesType").AsInt32().NotNullable()
                  .WithColumn("BacklogSetting").AsInt32().NotNullable()
                  .WithColumn("Network").AsString().Nullable()
                  .WithColumn("CustomStartDate").AsDateTime().Nullable()
                  .WithColumn("UseSceneNumbering").AsBoolean().NotNullable()
                  .WithColumn("TvRageId").AsInt32().Nullable()
                  .WithColumn("TvRageTitle").AsString().Nullable()
                  .WithColumn("UtcOffSet").AsInt32().NotNullable()
                  .WithColumn("FirstAired").AsDateTime().Nullable()
                  .WithColumn("NextAiring").AsDateTime().Nullable();

            Create.Table("MediaCovers")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("Url").AsString().NotNullable()
                  .WithColumn("CoverType").AsInt32().NotNullable();
        }

        protected override void LogDbUpgrade()
        {
            Create.Table("Logs")
                  .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                  .WithColumn("Message").AsString().NotNullable()
                  .WithColumn("Time").AsDateTime().NotNullable()
                  .WithColumn("Logger").AsString().NotNullable()
                  .WithColumn("Method").AsString().Nullable()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString().NotNullable();
        }
    }
}
