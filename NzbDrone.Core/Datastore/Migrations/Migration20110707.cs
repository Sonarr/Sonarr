using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110707)]
    public class Migration20110707 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddTable("Series", new[]
                                            {
                                                new Column("SeriesId", DbType.Int32, ColumnProperty.PrimaryKey),
                                                new Column("Title", DbType.String, ColumnProperty.Null),
                                                new Column("CleanTitle", DbType.String, ColumnProperty.Null),
                                                new Column("Status", DbType.String, ColumnProperty.Null),
                                                new Column("Overview", DbType.String,4000, ColumnProperty.Null),
                                                new Column("AirsDayOfWeek", DbType.Int32, ColumnProperty.Null),
                                                new Column("AirTimes", DbType.String, ColumnProperty.Null),
                                                new Column("Language", DbType.String, ColumnProperty.Null),
                                                new Column("Path", DbType.String,4000, ColumnProperty.NotNull),
                                                new Column("Monitored", DbType.Boolean, ColumnProperty.NotNull),
                                                new Column("QualityProfileId", DbType.Int32, ColumnProperty.NotNull),
                                                new Column("SeasonFolder", DbType.Boolean, ColumnProperty.NotNull),
                                                new Column("LastInfoSync", DbType.DateTime, ColumnProperty.Null),
                                                new Column("LastDiskSync", DbType.DateTime, ColumnProperty.Null)
                                            });

            Database.AddTable("Episodes", new[]
                                              {
                                                  new Column("EpisodeId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                  new Column("TvDbEpisodeId", DbType.Int32, ColumnProperty.Null),
                                                  new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                  new Column("SeasonNumber", DbType.Int32, ColumnProperty.NotNull),
                                                  new Column("EpisodeNumber", DbType.Int32, ColumnProperty.NotNull),
                                                  new Column("Title", DbType.String,100, ColumnProperty.Null),
                                                  new Column("Overview", DbType.String,4000, ColumnProperty.Null),
                                                  new Column("Ignored", DbType.Boolean, ColumnProperty.NotNull),
                                                  new Column("EpisodeFileId", DbType.Int32, ColumnProperty.Null),
                                                  new Column("AirDate", DbType.DateTime, ColumnProperty.Null),
                                                  new Column("GrabDate", DbType.DateTime, ColumnProperty.Null)
                                              });

            var indexName = MigrationsHelper.GetIndexName("Episodes", "SeriesId");
            Database.AddIndex(indexName, "Episodes", "SeriesId");

            indexName = MigrationsHelper.GetIndexName("Episodes", "EpisodeFileId");
            Database.AddIndex(indexName, "Episodes", "EpisodeFileId");

            indexName = MigrationsHelper.GetIndexName("Episodes", "AirDate");
            Database.AddIndex(indexName, "Episodes", "AirDate");

            indexName = MigrationsHelper.GetIndexName("Episodes", "TvDbEpisodeId");
            Database.AddIndex(indexName, "Episodes", "TvDbEpisodeId");


            Database.AddTable("EpisodeFiles", new[]
                                                  {
                                                      new Column("EpisodeFileId", DbType.Int32,
                                                                 ColumnProperty.PrimaryKeyWithIdentity),
                                                      new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Path", DbType.String,4000, ColumnProperty.NotNull),
                                                      new Column("Quality", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Proper", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Size", DbType.Int64, ColumnProperty.NotNull),
                                                      new Column("DateAdded", DbType.DateTime, ColumnProperty.NotNull),
                                                      new Column("SeasonNumber", DbType.Int32, ColumnProperty.NotNull)
                                                  });

            indexName = MigrationsHelper.GetIndexName("EpisodeFiles", "SeriesId");
            Database.AddIndex(indexName, "Episodes", "SeriesId");


            Database.AddTable("Config", new[]
                                            {
                                                new Column("Key", DbType.String, ColumnProperty.PrimaryKey),
                                                new Column("Value", DbType.String, ColumnProperty.NotNull)
                                            });

            Database.AddTable("SceneMappings", new[]
                                                   {
                                                       new Column("CleanTitle", DbType.String, ColumnProperty.PrimaryKey),
                                                       new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                       new Column("SceneName", DbType.String, ColumnProperty.NotNull)
                                                   });

            Database.AddTable("History", new[]
                                             {
                                                 new Column("HistoryId", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                                                 new Column("EpisodeId", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("NzbTitle", DbType.String, ColumnProperty.NotNull),
                                                 new Column("Date", DbType.DateTime, ColumnProperty.NotNull),
                                                 new Column("Quality", DbType.Int32, ColumnProperty.NotNull),
                                                 new Column("IsProper", DbType.Boolean, ColumnProperty.NotNull),
                                                 new Column("Indexer", DbType.String, ColumnProperty.NotNull)
                                             });

            indexName = MigrationsHelper.GetIndexName("History", "EpisodeId");
            Database.AddIndex(indexName, "History", "EpisodeId");

            indexName = MigrationsHelper.GetIndexName("History", "SeriesId");
            Database.AddIndex(indexName, "History", "SeriesId");

            Database.AddTable("RootDirs", new[]
                                              {
                                                  new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                  new Column("Path", DbType.String, 4000, ColumnProperty.NotNull)
                                              });

            Database.AddTable("ExternalNotificationSettings", new[]
                                                                  {
                                                                      new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                                      new Column("Enabled", DbType.Boolean, ColumnProperty.NotNull),
                                                                      new Column("NotifierName", DbType.String, ColumnProperty.NotNull),
                                                                      new Column("Name", DbType.String, ColumnProperty.NotNull)
                                                                  });

            Database.AddTable("JobDefinitions", new[]
                                                 {
                                                     new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                     new Column("Enable", DbType.Boolean, ColumnProperty.NotNull),
                                                     new Column("TypeName", DbType.String, ColumnProperty.NotNull),
                                                     new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                     new Column("Interval", DbType.Int32, ColumnProperty.NotNull),
                                                     new Column("LastExecution", DbType.DateTime, ColumnProperty.NotNull),
                                                     new Column("Success", DbType.Boolean, ColumnProperty.NotNull)
                                                 });

            Database.AddTable("QualityProfiles", new[]
                                                     {
                                                         new Column("QualityProfileId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                         new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                         new Column("Cutoff", DbType.Int32, ColumnProperty.NotNull),
                                                         new Column("SonicAllowed", DbType.String, ColumnProperty.NotNull),
                                                     });

            Database.AddTable("IndexerDefinitions", new[]
                                                     {
                                                         new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                         new Column("Enable", DbType.Boolean, ColumnProperty.NotNull),
                                                         new Column("IndexProviderType", DbType.String, ColumnProperty.NotNull),
                                                         new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                     });
        }


        protected override void LogDbUpgrade()
        {

            Database.AddTable("Logs", new[]
                                          {
                                              new Column("LogId", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                                              new Column("Message", DbType.String,4000, ColumnProperty.NotNull),
                                              new Column("Time", DbType.DateTime, ColumnProperty.NotNull),
                                              new Column("Logger", DbType.String, ColumnProperty.NotNull),
                                              new Column("Method", DbType.String, ColumnProperty.NotNull),
                                              new Column("Exception", DbType.String,4000, ColumnProperty.Null),
                                              new Column("ExceptionType", DbType.String, ColumnProperty.Null),
                                              new Column("Level", DbType.String, ColumnProperty.NotNull)
                                          });
        }
    }
}