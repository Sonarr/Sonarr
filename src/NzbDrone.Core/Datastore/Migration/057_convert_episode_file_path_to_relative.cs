using System.Data;
using System.IO;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(57)]
    public class convert_episode_file_path_to_relative : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Column("RelativePath").OnTable("EpisodeFiles").AsString().Nullable();

            // TODO: Add unique contraint for series ID and Relative Path
            // TODO: Warn if multiple series share the same path

            Execute.WithConnection(UpdateRelativePaths);
        }

        private void UpdateRelativePaths(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Id, Path FROM Series";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var seriesId = seriesReader.GetInt32(0);
                        var seriesPath = seriesReader.GetString(1) + Path.DirectorySeparatorChar;

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE EpisodeFiles SET RelativePath = REPLACE(Path, ?, '') WHERE SeriesId = ?";
                            updateCmd.AddParameter(seriesPath);
                            updateCmd.AddParameter(seriesId);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
