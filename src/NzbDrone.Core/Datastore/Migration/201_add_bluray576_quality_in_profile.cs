using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(201)]
    public class add_bluray576_quality_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater201(conn, tran);

            updater.SplitQualityAppend(13, 22);  // Insert Bluray576p after Bluray480p

            updater.Commit();
        }
    }

    public class Profile201
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem201> Items { get; set; }
    }

    public class ProfileItem201
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Quality { get; set; }

        public bool Allowed { get; set; }
        public List<ProfileItem201> Items { get; set; }
    }

    public class ProfileUpdater201
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile201> _profiles;
        private HashSet<Profile201> _changedProfiles = new HashSet<Profile201>();

        public ProfileUpdater201(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText = "UPDATE QualityProfiles SET Name = ?, Cutoff = ?, Items = ? WHERE Id = ?";
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

                var newBlurayProfile = new ProfileItem201
                {
                    Quality = quality,
                    Allowed = false
                };

                // The Bluray576p quality should be allowed in profiles which have Bluray480p allowed already
                if (profile.Items.Any(v => v.Quality == 13 && v.Allowed))
                {
                    newBlurayProfile.Allowed = true;
                }

                profile.Items.Insert(findIndex + 1, newBlurayProfile);

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile201> GetProfiles()
        {
            var profiles = new List<Profile201>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items FROM QualityProfiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile201
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem201>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
