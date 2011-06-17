using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20110523)]
    public class Migration20110523 : Migration
    {
        public override void Up()
        {
            Database.RemoveTable(RepositoryProvider.JobsSchema.Name);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration(20110603)]
    public class Migration20110603 : Migration
    {
        public override void Up()
        {
            Database.RemoveTable("Seasons");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration(20110604)]
    public class Migration20110616 : Migration
    {
        public override void Up()
        {
            Database.AddTable("Series", "SQLite", new[]
                                                      {
                                                          new Column("SeriesId", DbType.Int32, ColumnProperty.PrimaryKey),
                                                          new Column("Title", DbType.String, ColumnProperty.NotNull, String.Empty),
                                                          new Column("CleanTitle", DbType.String,  ColumnProperty.NotNull, String.Empty),
                                                          new Column("Status", DbType.String, ColumnProperty.Null),
                                                          new Column("Overview", DbType.String,  ColumnProperty.NotNull, String.Empty),
                                                          new Column("AirsDayOfWeek", DbType.Int16, ColumnProperty.Null),
                                                          new Column("AirTimes", DbType.String,  ColumnProperty.NotNull, String.Empty),
                                                          new Column("Language", DbType.String,  ColumnProperty.NotNull, String.Empty),
                                                          new Column("Path", DbType.String, ColumnProperty.NotNull),
                                                          new Column("Monitored", DbType.Boolean, ColumnProperty.NotNull),
                                                          new Column("QualityProfileId", DbType.Int16, ColumnProperty.NotNull),
                                                          new Column("SeasonFolder", DbType.Boolean, ColumnProperty.NotNull),
                                                          new Column("LastInfoSync", DbType.DateTime, ColumnProperty.Null),
                                                          new Column("LastDiskSync", DbType.DateTime, ColumnProperty.Null)
                                                      });

            Database.AddTable("Episodes", "SQLite", new[]
                                                        {
                                                            new Column("EpisodeId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                            new Column("TvDbEpisodeId", DbType.Int32, ColumnProperty.Null),
                                                            new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                            new Column("SeasonNumber", DbType.Int16, ColumnProperty.NotNull),
                                                            new Column("EpisodeNumber", DbType.Int16, ColumnProperty.NotNull),
                                                            new Column("Title", DbType.String, ColumnProperty.NotNull, String.Empty),
                                                            new Column("Overview", DbType.String, ColumnProperty.NotNull, String.Empty),
                                                            new Column("Ignored", DbType.Boolean, ColumnProperty.NotNull, false),
                                                            new Column("EpisodeFileId", DbType.Int32, ColumnProperty.Null),
                                                            new Column("AirDate", DbType.DateTime, ColumnProperty.Null),
                                                            new Column("GrabDate", DbType.DateTime, ColumnProperty.Null)
                                                        });


            Database.AddTable("EpisodeFiles", "SQLite", new[]
                                                            {
                                                                new Column("EpisodeFileId", DbType.Int32,
                                                                           ColumnProperty.PrimaryKeyWithIdentity),
                                                                new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                                new Column("Path", DbType.String, ColumnProperty.NotNull),
                                                                new Column("Quality", DbType.Int16, ColumnProperty.NotNull),
                                                                new Column("Proper", DbType.Int16, ColumnProperty.NotNull),
                                                                new Column("Size", DbType.Int64, ColumnProperty.NotNull),
                                                                new Column("DateAdded", DbType.DateTime, ColumnProperty.NotNull),
                                                                new Column("SeasonNumber", DbType.Int16, ColumnProperty.NotNull)
                                                            });


            Database.AddTable("Config", "SQLite", new[]
                                                      {
                                                          new Column("Key", DbType.String, ColumnProperty.PrimaryKey),
                                                          new Column("Value", DbType.String, ColumnProperty.NotNull)
                                                      });

            Database.AddTable("History", "SQLite", new[]
                                                            {
                                                                new Column("HistoryId", DbType.Int64, ColumnProperty.PrimaryKey),
                                                                new Column("EpisodeId", DbType.Int32, ColumnProperty.NotNull),
                                                                new Column("SeriesId", DbType.Int32, ColumnProperty.NotNull),
                                                                new Column("NzbTitle", DbType.String, ColumnProperty.NotNull),
                                                                new Column("Date", DbType.DateTime, ColumnProperty.NotNull),
                                                                new Column("Quality", DbType.Int16, ColumnProperty.NotNull),
                                                                new Column("IsProper", DbType.Boolean, ColumnProperty.NotNull),
                                                                new Column("Indexer", DbType.String, ColumnProperty.NotNull)
                                                            });

            Database.AddTable("RootDirs", "SQLite", new[]
                                                      {
                                                          new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                                                          new Column("Path", DbType.String, ColumnProperty.NotNull)
                                                      });

            Database.AddTable("ExternalNotificationSettings", "SQLite", new[]
                                                      {
                                                          new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                                                          new Column("Enabled", DbType.Boolean, ColumnProperty.NotNull),
                                                          new Column("NotifierName", DbType.String, ColumnProperty.NotNull),
                                                          new Column("Name", DbType.String, ColumnProperty.NotNull)
                                                      });

            Database.AddTable("JobSettings", "SQLite", new[]
                                                            {
                                                                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                                                                new Column("Enable", DbType.Boolean, ColumnProperty.NotNull),
                                                                new Column("TypeName", DbType.String, ColumnProperty.NotNull),
                                                                new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                                new Column("Interval", DbType.Int32, ColumnProperty.NotNull),
                                                                new Column("LastExecution", DbType.DateTime, ColumnProperty.NotNull),
                                                                new Column("Success", DbType.Boolean, ColumnProperty.NotNull)
                                                            });

            Database.AddTable("QualityProfiles", "SQLite", new[]
                                                      {
                                                          new Column("QualityProfileId", DbType.Int32, ColumnProperty.PrimaryKey),
                                                          new Column("Name", DbType.String, ColumnProperty.NotNull),
                                                          new Column("Cutoff", DbType.Int32, ColumnProperty.NotNull),
                                                          new Column("SonicAllowed", DbType.String, ColumnProperty.NotNull),
                                                      });
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}