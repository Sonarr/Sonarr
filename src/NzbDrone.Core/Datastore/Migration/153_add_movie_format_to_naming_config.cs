using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(153)]
    public class add_movie_format_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("StandardMovieFormat").AsString().Nullable();
            Alter.Table("NamingConfig").AddColumn("MovieFolderFormat").AsString().Nullable();
            Alter.Table("NamingConfig").AddColumn("RenameMovies").AsBoolean().WithDefaultValue(false);

            Execute.WithConnection(ConvertConfig);
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand updateCmd = conn.CreateCommand())
            {
                var standardMovieFormat = "{Movie Title} - {Quality Title}";
                var movieFolderFormat = "{Movie Title} ({Year})";
                var text = String.Format("UPDATE NamingConfig " +
                                            "SET StandardMovieFormat = '{0}', MovieFolderFormat = '{1}'",
                                            standardMovieFormat,
                                            movieFolderFormat);
                updateCmd.Transaction = tran;
                updateCmd.CommandText = text;
                updateCmd.ExecuteNonQuery();
            }
        }
    }
}
