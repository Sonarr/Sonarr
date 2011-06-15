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
        }
    }
}
