using System;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(162)]
    public class release_profile_to_array : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ChangeRequiredIgnoredTypes);
        }

        private void ChangeRequiredIgnoredTypes(IDbConnection conn, IDbTransaction tran)
        {
            using (var getEmailCmd = conn.CreateCommand())
            {
                getEmailCmd.Transaction = tran;
                getEmailCmd.CommandText = "SELECT Id, Required, Ignored FROM ReleaseProfiles";

                using (var reader = getEmailCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var requiredObj = reader.GetValue(1);
                        var ignoredObj = reader.GetValue(2);

                        var required = requiredObj == DBNull.Value
                            ? Enumerable.Empty<string>()
                            : requiredObj.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        var ignored = ignoredObj == DBNull.Value
                            ? Enumerable.Empty<string>()
                            : ignoredObj.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE ReleaseProfiles SET Required = ?, Ignored = ? WHERE Id = ?";
                            updateCmd.AddParameter(required.ToJson());
                            updateCmd.AddParameter(ignored.ToJson());
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
