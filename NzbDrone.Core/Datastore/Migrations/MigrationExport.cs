using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MigSharp;

namespace NzbDrone.Core.Datastore.Migrations
{
    [MigrationExport]
    internal class Migration1 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable("Series")
                .WithPrimaryKeyColumn("SeriesId", DbType.Int32).AsIdentity()
                .WithNullableColumn("Title", DbType.String)
                .WithNullableColumn("CleanTitle", DbType.String)
                .WithNullableColumn("Status", DbType.String)
                .WithNullableColumn("Overview", DbType.String)
                .WithNullableColumn("AirsDayOfWeek", DbType.Int16)
                .WithNullableColumn("AirTimes", DbType.String)
                .WithNullableColumn("Language", DbType.String)
                .WithNotNullableColumn("Path", DbType.String)
                .WithNotNullableColumn("Monitored", DbType.Boolean)
                .WithNotNullableColumn("QualityProfileId", DbType.Int16)
                .WithNotNullableColumn("SeasonFolder", DbType.Boolean)
                .WithNullableColumn("LastInfoSync", DbType.DateTime)
                .WithNullableColumn("LastDiskSync", DbType.DateTime);


            db.CreateTable("Episodes")
                .WithPrimaryKeyColumn("EpisodeId", DbType.Int32).AsIdentity()
                .WithNullableColumn("TvDbEpisodeId", DbType.Int32)

                .WithNotNullableColumn("SeriesId", DbType.Int32)
                .WithNotNullableColumn("SeasonNumber", DbType.Int16)
                .WithNotNullableColumn("EpisodeNumber", DbType.Int16)
                .WithNotNullableColumn("Title", DbType.String).HavingDefault(String.Empty)

                .WithNotNullableColumn("Overview", DbType.String).HavingDefault(String.Empty)
                .WithNotNullableColumn("Ignored", DbType.Boolean).HavingDefault(false)
                .WithNullableColumn("EpisodeFileId", DbType.Int32)
                .WithNullableColumn("AirDate", DbType.DateTime)
                .WithNullableColumn("GrabDate", DbType.DateTime);

            db.CreateTable("EpisodeFiles")
                .WithPrimaryKeyColumn("EpisodeFileId", DbType.Int32).AsIdentity()
                .WithNotNullableColumn("SeriesId", DbType.Int32)
                .WithNotNullableColumn("Path", DbType.String)
                .WithNotNullableColumn("Quality", DbType.Int16)
                .WithNotNullableColumn("Proper", DbType.Int16)
                .WithNotNullableColumn("Size", DbType.Int64)
                .WithNotNullableColumn("DateAdded", DbType.DateTime)
                .WithNotNullableColumn("SeasonNumber", DbType.Int16);
        }
    }
}
