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
    [Migration(215)]
    public class add_blurary576p_quality_in_profiles_with_grouped_blurary480p : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater215(conn, tran);

            updater.InsertQualityAfter(13, 22); // Group Bluray576p with Bluray480p
            updater.Commit();
        }
    }

    public class Profile215
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem215> Items { get; set; }
    }

    public class ProfileItem215
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem215> Items { get; set; }
        public bool Allowed { get; set; }

        public ProfileItem215()
        {
            Items = new List<ProfileItem215>();
        }
    }

    public class ProfileUpdater215
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile215> _profiles;
        private HashSet<Profile215> _changedProfiles = new HashSet<Profile215>();

        public ProfileUpdater215(IDbConnection conn, IDbTransaction tran)
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
                // Don't update if Bluray 576p was already added to the profile in 214
                if (profile.Items.FindIndex(v => v.Quality == quality || v.Items.Any(i => i.Quality == quality)) > -1)
                {
                    continue;
                }

                var findIndex = profile.Items.FindIndex(v => v.Quality == find || v.Items.Any(i => i.Quality == find));

                if (findIndex > -1)
                {
                    profile.Items.Insert(findIndex + 1, new ProfileItem215
                    {
                        Quality = quality,
                        Allowed = profile.Items[findIndex].Allowed
                    });
                }

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile215> GetProfiles()
        {
            var profiles = new List<Profile215>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = "SELECT \"Id\", \"Name\", \"Cutoff\", \"Items\" FROM \"QualityProfiles\"";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile215
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Cutoff = profileReader.GetInt32(2),
                            Items = Json.Deserialize<List<ProfileItem215>>(profileReader.GetString(3))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
