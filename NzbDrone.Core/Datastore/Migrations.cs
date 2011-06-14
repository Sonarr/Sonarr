using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Migrator.Framework;
using NLog;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Extensions;
using SubSonic.Repository;
using SubSonic.Schema;

namespace NzbDrone.Core.Datastore
{
    public class Migrations
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Run(string connetionString, bool trace)
        {
            Logger.Info("Preparing run database migration");

            try
            {
                Migrator.Migrator migrator;
                if (trace)
                {
                    migrator = new Migrator.Migrator("Sqlite", connetionString, Assembly.GetAssembly(typeof(Migrations)), true, new MigrationLogger());
                }
                else
                {
                    migrator = new Migrator.Migrator("Sqlite", connetionString, Assembly.GetAssembly(typeof(Migrations)));
                }



                migrator.MigrateToLastVersion();

                ForceSubSonicMigration(Connection.CreateSimpleRepository(connetionString));

                Logger.Info("Database migration completed");


            }
            catch (Exception e)
            {
                Logger.FatalException("An error has occured while migrating database", e);
            }
        }

        public static void ForceSubSonicMigration(IRepository repository)
        {
            repository.Single<Series>(1);
            repository.Single<Episode>(1);
            repository.Single<EpisodeFile>(1);
            repository.Single<QualityProfile>(1);
            repository.Single<History>(1);
            repository.Single<IndexerSetting>(1);
            repository.Single<SceneNameMapping>(1);
        }


        public static void RemoveDeletedColumns(ITransformationProvider transformationProvider)
        {
            var provider = new RepositoryProvider();
            var repoTypes = provider.GetRepositoryTypes();

            foreach (var repoType in repoTypes)
            {
                var typeSchema = provider.GetSchemaFromType(repoType);

                if (transformationProvider.TableExists(typeSchema.Name))
                {
                    var dbColumns = provider.GetColumnsFromDatabase(transformationProvider, typeSchema.Name);

                    var deletedColumns = provider.GetDeletedColumns(typeSchema, dbColumns);

                    foreach (var deletedColumn in deletedColumns)
                    {
                        Logger.Info("Removing column '{0}' from '{1}'", deletedColumn.Name, repoType.Name);
                        transformationProvider.RemoveColumn(typeSchema.Name, deletedColumn.Name);
                    }
                }
            }

        }

        public static void AddNewColumns(ITransformationProvider transformationProvider)
        {
            var provider = new RepositoryProvider();
            var repoTypes = provider.GetRepositoryTypes();

            foreach (var repoType in repoTypes)
            {
                var typeSchema = provider.GetSchemaFromType(repoType);
                if (transformationProvider.TableExists(typeSchema.Name))
                {
                    var dbColumns = provider.GetColumnsFromDatabase(transformationProvider, typeSchema.Name);

                    var newColumns = provider.GetNewColumns(typeSchema, dbColumns);

                    foreach (var newColumn in newColumns)
                    {
                        Logger.Info("Adding column '{0}' to '{1}'", newColumn.Name, repoType.Name);
                        transformationProvider.AddColumn(typeSchema.Name, newColumn);
                    }
                }

            }

        }

    }

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

            Migrations.RemoveDeletedColumns(Database);
            Migrations.AddNewColumns(Database);
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
            Migrations.ForceSubSonicMigration(Connection.CreateSimpleRepository(Connection.MainConnectionString));

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

            Migrations.RemoveDeletedColumns(Database);
            Migrations.AddNewColumns(Database);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}