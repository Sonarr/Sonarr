using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(214)]
    public class add_blurary576p_quality_in_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater214(conn, tran);

            updater.InsertQualityAfter(13, 22); // Group Bluray576p with Bluray480p
            updater.Commit();
        }
    }

    public class Profile214
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem214> Items { get; set; }
    }

    public class ProfileItem214
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem214> Items { get; set; }
        public bool Allowed { get; set; }

        public ProfileItem214()
        {
            Items = new List<ProfileItem214>();
        }
    }

    public class ProfileUpdater214
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile214> _profiles;
        private HashSet<Profile214> _changedProfiles = new HashSet<Profile214>();

        public ProfileUpdater214(IDbConnection conn, IDbTransaction tran)
        {
            _connection = conn;
            _transaction = tran;

            _profiles = GetProfiles();
        }

        public void Commit()
        {
            var profilesToUpdate = _changedProfiles.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Cutoff = p.Cutoff,
                Items = p.Items.ToJson()
            });

            var updateSql = $"UPDATE \"QualityProfiles\" SET \"Name\" = @Name, \"Cutoff\" = @Cutoff, \"Items\" = @Items WHERE \"Id\" = @Id";
            _connection.Execute(updateSql, profilesToUpdate, transaction: _transaction);

            _changedProfiles.Clear();
        }

        public void InsertQualityAfter(int find, int quality)
        {
            foreach (var profile in _profiles)
            {
                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                if (findIndex > -1)
                {
                    profile.Items.Insert(findIndex + 1, new ProfileItem214
                    {
                        Quality = quality,
                        Allowed = profile.Items[findIndex].Allowed
                    });
                }

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile214> GetProfiles()
        {
            var profiles = new List<Profile214>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = "SELECT \"Id\", \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile214
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem214>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
