using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20110622)]
    public class Migration20110622 : Migration
    {
        public override void Up()
        {
            Database.AddTable("Series", new[]
                                            {
                                                new Column("SeriesId", DbType.Int32, ColumnProperty.PrimaryKey),
                                                new Column("Title", DbType.String, ColumnProperty.Null),
                                                new Column("CleanTitle", DbType.String, ColumnProperty.Null),
                                                new Column("Status", DbType.String, ColumnProperty.Null),
                                                new Column("Overview", DbType.String, ColumnProperty.Null),
                                                new Column("AirsDayOfWeek", DbType.Int32, ColumnProperty.Null),
                                                new Column("AirTimes", DbType.String, ColumnProperty.Null),
                                                new Column("Language", DbType.String, ColumnProperty.Null),
                                                new Column("Path", DbType.String, ColumnProperty.NotNull),
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
                                                  new Column("Title", DbType.String, ColumnProperty.Null),
                                                  new Column("Overview", DbType.String, ColumnProperty.Null),
                                                  new Column("Ignored", DbType.Boolean, ColumnProperty.NotNull),
                                                  new Column("EpisodeFileId", DbType.Int32, ColumnProperty.Null),
                                                  new Column("AirDate", DbType.DateTime, ColumnProperty.Null),
                                                  new Column("GrabDate", DbType.DateTime, ColumnProperty.Null)
                                              });


            Database.AddTable("EpisodeFiles", new[]
                                                  {
                                                      new Column("EpisodeFileId", DbType.Int32,
                                                                 ColumnProperty.PrimaryKeyWithIdentity),
                                                      new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Path", DbType.String, ColumnProperty.NotNull),
                                                      new Column("Quality", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Proper", DbType.Int32, ColumnProperty.NotNull),
                                                      new Column("Size", DbType.Int64, ColumnProperty.NotNull),
                                                      new Column("DateAdded", DbType.DateTime, ColumnProperty.NotNull),
                                                      new Column("SeasonNumber", DbType.Int32, ColumnProperty.NotNull)
                                                  });


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

            Database.AddTable("RootDirs", new[]
                                              {
                                                  new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                  new Column("Path", DbType.String, ColumnProperty.NotNull)
                                              });

            Database.AddTable("ExternalNotificationSettings", new[]
                                                                  {
                                                                      new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                                      new Column("Enabled", DbType.Boolean, ColumnProperty.NotNull),
                                                                      new Column("NotifierName", DbType.String, ColumnProperty.NotNull),
                                                                      new Column("Name", DbType.String, ColumnProperty.NotNull)
                                                                  });

            Database.AddTable("JobSettings", new[]
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

            Database.AddTable("Logs", new[]
                                          {
                                              new Column("LogId", DbType.Int64, ColumnProperty.PrimaryKeyWithIdentity),
                                              new Column("Message", DbType.String, ColumnProperty.NotNull),
                                              new Column("Time", DbType.DateTime, ColumnProperty.NotNull),
                                              new Column("Logger", DbType.String, ColumnProperty.NotNull),
                                              new Column("Method", DbType.String, ColumnProperty.NotNull),
                                              new Column("Exception", DbType.String, ColumnProperty.Null),
                                              new Column("ExceptionType", DbType.String, ColumnProperty.Null),
                                              new Column("Level", DbType.String, ColumnProperty.NotNull)
                                          });

            Database.AddTable("IndexerSettings", new[]
                                                     {
                                                         new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                         new Column("Enable", DbType.Boolean, ColumnProperty.NotNull),
                                                         new Column("IndexProviderType", DbType.String, ColumnProperty.NotNull),
                                                         new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                     });
        }


        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}