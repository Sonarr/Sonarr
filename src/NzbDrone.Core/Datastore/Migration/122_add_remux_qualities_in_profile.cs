using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(122)]
    public class add_remux_qualities_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater122(conn, tran);

            // Insert 2060p, in case the user grouped Bluray 1080p and 2160p together.

            updater.SplitQualityAppend(19, 21); // Bluray2160pRemux after Bluray2160p
            updater.SplitQualityAppend(7, 20);  // Bluray1080pRemux after Bluray1080p

            updater.Commit();
        }
    }

    public class Profile122
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem122> Items { get; set; }
    }

    public class ProfileItem122
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Quality { get; set; }

        public bool Allowed { get; set; }
        public List<ProfileItem122> Items { get; set; }
    }

    public class ProfileUpdater122
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile122> _profiles;
        private HashSet<Profile122> _changedProfiles = new HashSet<Profile122>();

        public ProfileUpdater122(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
        }

        public void Commit()
        {
            foreach (var profile in _changedProfiles)
            {
                using (var updateProfileCmd = _connection.CreateCommand())
                {
                    updateProfileCmd.Transaction = _transaction;
                    updateProfileCmd.CommandText = "UPDATE Profiles SET Name = ?, Cutoff = ?, Items = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void SplitQualityAppend(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                if (profile.Items.Any(v => v.Quality == quality))
                {
                    continue;
                }

                var findIndex = profile.Items.FindIndex(v =>
                {
                    return v.Quality == find || (v.Items != null && v.Items.Any(b => b.Quality == find));
                });

                profile.Items.Insert(findIndex + 1, new ProfileItem122
                {
                    Quality = quality,
                    Allowed = false
                });

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile122> GetProfiles()
        {
            var profiles = new List<Profile122>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items FROM Profiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile122
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem122>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
