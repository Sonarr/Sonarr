using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(103)]
    public class fix_metadata_file_extensions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(SetMetadataFileExtension);
        }

        private void SetMetadataFileExtension(IDbConnection conn, IDbTransaction tran)
        {
            var updatedMetadataFiles = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"Extension\" FROM \"MetadataFiles\"";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var extension = reader.GetString(1);
                        extension = extension.Substring(extension.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase));

                        updatedMetadataFiles.Add(new
                        {
                            Id = id,
                            Extension = extension
                        });
                    }
                }
            }

            var updateSql = $"UPDATE \"MetadataFiles\" SET \"Extension\" = @Extension WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updatedMetadataFiles, transaction: tran);
        }
    }
}
