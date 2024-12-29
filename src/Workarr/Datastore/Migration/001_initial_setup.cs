using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(1)]
    public class InitialSetup : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "Config")
                  .WithColumn("Key").AsString().Unique()
                  .WithColumn("Value").AsString();

            MigrationExtension.TableForModel(Create, "RootFolders")
                  .WithColumn("Path").AsString().Unique();

            MigrationExtension.TableForModel(Create, "Series")
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
                .WithColumn("Path").AsString()
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
                .WithColumn("FirstAired").AsDateTime().Nullable()
                .WithColumn("NextAiring").AsDateTime().Nullable();

            MigrationExtension.TableForModel(Create, "Seasons")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("Ignored").AsBoolean();

            MigrationExtension.TableForModel(Create, "Episodes")
                .WithColumn("TvDbEpisodeId").AsInt32().Unique()
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("SeasonNumber").AsInt32()
                .WithColumn("EpisodeNumber").AsInt32()
                .WithColumn("Title").AsString().Nullable()
                .WithColumn("Overview").AsString().Nullable()
                .WithColumn("Ignored").AsBoolean().Nullable()
                .WithColumn("EpisodeFileId").AsInt32().Nullable()
                .WithColumn("AirDate").AsDateTime().Nullable()
                .WithColumn("AbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneAbsoluteEpisodeNumber").AsInt32().Nullable()
                .WithColumn("SceneSeasonNumber").AsInt32().Nullable()
                .WithColumn("SceneEpisodeNumber").AsInt32().Nullable();

            MigrationExtension.TableForModel(Create, "EpisodeFiles")
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("Path").AsString().Unique()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Size").AsInt64()
                  .WithColumn("DateAdded").AsDateTime()
                  .WithColumn("SeasonNumber").AsInt32()
                  .WithColumn("SceneName").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            MigrationExtension.TableForModel(Create, "History")
                  .WithColumn("EpisodeId").AsInt32()
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("NzbTitle").AsString()
                  .WithColumn("Date").AsDateTime()
                  .WithColumn("Quality").AsString()
                  .WithColumn("Indexer").AsString()
                  .WithColumn("NzbInfoUrl").AsString().Nullable()
                  .WithColumn("ReleaseGroup").AsString().Nullable();

            MigrationExtension.TableForModel(Create, "Notifications")
                  .WithColumn("Name").AsString()
                  .WithColumn("OnGrab").AsBoolean()
                  .WithColumn("OnDownload").AsBoolean()
                  .WithColumn("Settings").AsString()
                  .WithColumn("Implementation").AsString();

            MigrationExtension.TableForModel(Create, "ScheduledTasks")
                  .WithColumn("TypeName").AsString().Unique()
                  .WithColumn("Interval").AsInt32()
                  .WithColumn("LastExecution").AsDateTime();

            MigrationExtension.TableForModel(Create, "Indexers")
                  .WithColumn("Enable").AsBoolean()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("Settings").AsString().Nullable();

            MigrationExtension.TableForModel(Create, "QualityProfiles")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Cutoff").AsInt32()
                  .WithColumn("Allowed").AsString();

            MigrationExtension.TableForModel(Create, "QualitySizes")
                  .WithColumn("QualityId").AsInt32().Unique()
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("MinSize").AsInt32()
                  .WithColumn("MaxSize").AsInt32();

            MigrationExtension.TableForModel(Create, "SceneMappings")
                  .WithColumn("CleanTitle").AsString()
                  .WithColumn("SceneName").AsString()
                  .WithColumn("TvdbId").AsInt32()
                  .WithColumn("SeasonNumber").AsInt32();

            MigrationExtension.TableForModel(Create, "NamingConfig")
                  .WithColumn("UseSceneName").AsBoolean()
                  .WithColumn("Separator").AsString()
                  .WithColumn("NumberStyle").AsInt32()
                  .WithColumn("IncludeSeriesTitle").AsBoolean()
                  .WithColumn("MultiEpisodeStyle").AsInt32()
                  .WithColumn("IncludeEpisodeTitle").AsBoolean()
                  .WithColumn("IncludeQuality").AsBoolean()
                  .WithColumn("ReplaceSpaces").AsBoolean()
                  .WithColumn("SeasonFolderFormat").AsString();
        }

        protected override void LogDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "Logs")
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
