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
            Create.TableForModel("Config")
                  .WithColumn("Key").AsString().Unique()
                  .WithColumn("Value").AsString();

            Create.TableForModel("RootFolders")
                  .WithColumn("Path").AsString().Unique();

            Create.TableForModel("Series")
                .WithColumn("TvdbId").AsInt32().Unique()
                .WithColumn("TvRageId").AsInt32().Unique()
                .WithColumn("ImdbId").AsString().Unique()
                .WithColumn("Title").AsString()
                .WithColumn("TitleSlug").AsString().Unique()
                .WithColumn("CleanTitle").AsString()
                .WithColumn("Status").AsInt32()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("AirTime").AsString().Nullable()
                .WithColumn("Images").AsString()
                .WithColumn("Path").AsString().Unique()
                .WithColumn("Monitored").AsBoolean()
                .WithColumn("QualityProfileId").AsInt32()
                .WithColumn("SeasonFolder").AsBoolean()
                .WithColumn("LastInfoSync").AsDateTime().Nullable()
                .WithColumn("LastDiskSync").AsDateTime().Nullable()
                .WithColumn("Runtime").AsInt32()
                .WithColumn("SeriesType").AsInt32()
                .WithColumn("BacklogSetting").AsInt32()
                .WithColumn("Network").AsString().Nullable()
                .WithColumn("CustomStartDate").AsDateTime().Nullable()
                .WithColumn("UseSceneNumbering").AsBoolean()
                .WithColumn("UtcOffSet").AsInt32()
                .WithColumn("FirstAired").AsDateTime().Nullable()
                .WithColumn("NextAiring").AsDateTime().Nullable();

            Create.TableForModel("Seasons")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("Ignored").AsBoolean();

            Create.TableForModel("Episodes")
                .WithColumn("TvDbEpisodeId").AsInt32().Unique()
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("EpisodeNumber").AsInt32()
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

            Create.TableForModel("EpisodeFiles")
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("Path").AsString().Unique()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime()
                  .WithColumn("SeasonNumber").AsInt32()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            Create.TableForModel("History")
                  .WithColumn("EpisodeId").AsInt32()
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("NzbTitle").AsString()
                  .WithColumn("Date").AsDateTime()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Indexer").AsString()
                  .WithColumn("NzbInfoUrl").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            Create.TableForModel("ExternalNotificationDefinitions")
                  .WithColumn("Enable").AsBoolean()
                  .WithColumn("Type").AsString().Unique()
                  .WithColumn("Name").AsString().Unique();

            Create.TableForModel("JobDefinitions")
                  .WithColumn("Enable").AsBoolean()
                  .WithColumn("Type").AsString().Unique()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Interval").AsInt32()
                  .WithColumn("LastExecution").AsDateTime()
                  .WithColumn("Success").AsBoolean();

            Create.TableForModel("IndexerDefinitions")
                  .WithColumn("Enable").AsBoolean()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Settings").AsString().Nullable();

            Create.TableForModel("NewznabDefinitions")
                  .WithColumn("Enable").AsBoolean()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Url").AsString()
                  .WithColumn("ApiKey").AsString().Nullable();

            Create.TableForModel("QualityProfiles")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Cutoff").AsInt32()
                  .WithColumn("Allowed").AsString();

            Create.TableForModel("QualitySizes")
                  .WithColumn("QualityId").AsInt32().Unique()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("MinSize").AsInt32()
                  .WithColumn("MaxSize").AsInt32();

            Create.TableForModel("SceneMappings")
                  .WithColumn("CleanTitle").AsString()
                  .WithColumn("SceneName").AsString()
                  .WithColumn("TvdbId").AsInt32()
                  .WithColumn("SeasonNumber").AsInt32();

        }

        protected override void LogDbUpgrade()
        {
            Create.TableForModel("Logs")
                  .WithColumn("Message").AsString()
                  .WithColumn("Time").AsDateTime()
                  .WithColumn("Logger").AsString()
                  .WithColumn("Method").AsString().Nullable()
                  .WithColumn("Exception").AsString().Nullable()
                  .WithColumn("ExceptionType").AsString().Nullable()
                  .WithColumn("Level").AsString();
        }
    }
}
