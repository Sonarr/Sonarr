using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(71)]
    public class unknown_quality_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Weight").FromTable("QualityDefinitions");

            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetProfiles(conn, tran);

            foreach (var profile in profiles)
            {
                if (profile.Items.Any(p => p.Quality == 0)) continue;

                profile.Items.Insert(0, new ProfileItem71
                                  {
                                      Quality = 0,
                                      Allowed = false
                                  });

                var itemsJson = profile.Items.ToJson();

                using (IDbCommand updateProfileCmd = conn.CreateCommand())
                {
                    updateProfileCmd.Transaction = tran;
                    updateProfileCmd.CommandText = "UPDATE Profiles SET Items = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(itemsJson);
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }
        }

        private List<Profile71> GetProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = new List<Profile71>();

            using (IDbCommand getProfilesCmd = conn.CreateCommand())
            {
                getProfilesCmd.Transaction = tran;
                getProfilesCmd.CommandText = @"SELECT Id, Items FROM Profiles";

                using (IDataReader profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        var id = profileReader.GetInt32(0);
                        var itemsJson = profileReader.GetString(1);

                        var items = Json.Deserialize<List<ProfileItem71>>(itemsJson);
                        
                        profiles.Add(new Profile71
                        {
                            Id = id,
                            Items = items
                        });
                    }
                }
            }

            return profiles;
        }

        private class Profile71
        {
            public int Id { get; set; }
            public List<ProfileItem71> Items { get; set; }
        }

        private class ProfileItem71
        {
            public int Quality { get; set; }
            public bool Allowed { get; set; }
        }
    }
}
