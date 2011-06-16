using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations.Legacy
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

            MigrationsHelper.RemoveDeletedColumns(Database);
            MigrationsHelper.AddNewColumns(Database);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration(20110604)]
    public class Migration20110604 : Migration
    {
        public override void Up()
        {
            MigrationsHelper.ForceSubSonicMigration(Connection.CreateSimpleRepository(Connection.MainConnectionString));

            var episodesTable = RepositoryProvider.EpisodesSchema;
            //Database.AddIndex("idx_episodes_series_season_episode", episodesTable.Name, true,
            //    episodesTable.GetColumnByPropertyName("SeriesId").Name,
            //    episodesTable.GetColumnByPropertyName("SeasonNumber").Name,
            //    episodesTable.GetColumnByPropertyName("EpisodeNumber").Name);

            Database.AddIndex("idx_episodes_series_season", episodesTable.Name, false,
                episodesTable.GetColumnByPropertyName("SeriesId").Name,
                episodesTable.GetColumnByPropertyName("SeasonNumber").Name);

            Database.AddIndex("idx_episodes_series", episodesTable.Name, false,
                             episodesTable.GetColumnByPropertyName("SeriesId").Name);

            MigrationsHelper.RemoveDeletedColumns(Database);
            MigrationsHelper.AddNewColumns(Database);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
