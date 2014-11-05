using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(53)]
    public class add_series_sorttitle : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Column("SortTitle").OnTable("Series").AsString().Nullable();

            Execute.WithConnection(SetSortTitles);
        }

        private void SetSortTitles(IDbConnection conn, IDbTransaction tran)
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

                        var sortTitle = Parser.Parser.NormalizeTitle(title).ToLower();

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE Series SET SortTitle = ? WHERE Id = ?";
                            updateCmd.AddParameter(sortTitle);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
