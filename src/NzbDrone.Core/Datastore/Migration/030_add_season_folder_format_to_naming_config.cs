using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(30)]
    public class add_season_folder_format_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SeasonFolderFormat").AsString().Nullable();
            Execute.WithConnection(ConvertConfig);
            Execute.Sql("DELETE FROM Config WHERE [Key] = 'seasonfolderformat'");
            Execute.Sql("DELETE FROM Config WHERE [Key] = 'useseasonfolder'");
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = @"SELECT [Value] FROM Config WHERE [Key] = 'seasonfolderformat'";
                var seasonFormat = "Season {season}";

                using (IDataReader namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    while (namingConfigReader.Read())
                    {
                        //only getting one column, so its index is 0
                        seasonFormat = namingConfigReader.GetString(0);

                        seasonFormat = seasonFormat.Replace("%sn", "{Series Title}")
                                                   .Replace("%s.n", "{Series.Title}")
                                                   .Replace("%s", "{season}")
                                                   .Replace("%0s", "{season:00}")
                                                   .Replace("%e", "{episode}")
                                                   .Replace("%0e", "{episode:00}");
                    }
                }

                using (IDbCommand updateCmd = conn.CreateCommand())
                {
                    var text = string.Format("UPDATE NamingConfig " +
                                             "SET SeasonFolderFormat = '{0}'",
                                             seasonFormat);

                    updateCmd.Transaction = tran;
                    updateCmd.CommandText = text;
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
