using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(80)]
    public class add_aditional_language_titles_to_series : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("AditionalLanguageTitle").AsString().Nullable();

            Execute.WithConnection(SetAditionalLanguageTitle);

            Alter.Table("Series").AddColumn("AditionalLanguageCleanTitle").AsString().Nullable();

            Execute.WithConnection(SetAditionalLanguageCleanTitle);
        }

        private void SetAditionalLanguageCleanTitle(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Id, CleanTitle FROM Series";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var cleanTitle = seriesReader.GetString(1);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE Series SET AditionalLanguageCleanTitle = ? WHERE Id = ?";
                            updateCmd.AddParameter(cleanTitle);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void SetAditionalLanguageTitle(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Id, Title FROM Series";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var id = seriesReader.GetInt32(0);
                        var title = seriesReader.GetString(1);

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE Series SET AditionalLanguageTitle = ? WHERE Id = ?";
                            updateCmd.AddParameter(title);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
