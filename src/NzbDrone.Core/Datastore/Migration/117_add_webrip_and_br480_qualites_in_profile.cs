using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(117)]
    public class add_webrip_and_br480_qualites_in_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new ProfileUpdater117(conn, tran);

            updater.CreateGroupAt(8, 1000, "WEB 480p", new[] { 12, 8 }); // Group WEBRip480p with WEBDL480p
            updater.CreateGroupAt(2, 1001, "DVD", new[] { 2, 13 }); // Group Bluray480p with DVD
            updater.CreateGroupAt(5, 1002, "WEB 720p", new[] { 14, 5 }); // Group WEBRip720p with WEBDL720p
            updater.CreateGroupAt(3, 1003, "WEB 1080p", new[] { 15, 3 }); // Group WEBRip1080p with WEBDL1080p
            updater.CreateGroupAt(18, 1004, "WEB 2160p", new[] { 17, 18 }); // Group WEBRip2160p with WEBDL2160p

            updater.Commit();
        }
    }

    public class Profile117
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileItem117> Items { get; set; }
    }

    public class ProfileItem117
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Quality { get; set; }
        public List<ProfileItem117> Items { get; set; }
        public bool Allowed { get; set; }

        public ProfileItem117()
        {
            Items = new List<ProfileItem117>();
        }
    }

    public class ProfileUpdater117
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<Profile117> _profiles;
        private HashSet<Profile117> _changedProfiles = new HashSet<Profile117>();

        public ProfileUpdater117(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText =
                        "UPDATE Profiles SET Name = ?, Cutoff = ?, Items = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.Name);
                    updateProfileCmd.AddParameter(profile.Cutoff);
                    updateProfileCmd.AddParameter(profile.Items.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void CreateGroupAt(int find, int groupId, string name, int[] qualities)
        {
            foreach (var profile in _profiles)
            {
                var findIndex = profile.Items.FindIndex(v => v.Quality == find);

                if (findIndex > -1)
                {
                    var findQuality = profile.Items[findIndex];

                    profile.Items.Insert(findIndex, new ProfileItem117
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem117
                                                        {
                                                            Quality = q,
                                                            Allowed = findQuality.Allowed
                                                        }).ToList(),
                        Allowed = findQuality.Allowed
                    });
                }
                else
                {
                    // If the ID isn't found for some reason (mangled migration 71?)

                    profile.Items.Add(new ProfileItem117
                    {
                        Id = groupId,
                        Name = name,
                        Quality = null,
                        Items = qualities.Select(q => new ProfileItem117
                                                    {
                                                        Quality = q,
                                                        Allowed = false
                                                    }).ToList(),
                        Allowed = false
                    });
                }

                foreach (var quality in qualities)
                {
                    var index = profile.Items.FindIndex(v => v.Quality == quality);

                    if (index > -1)
                    {
                        profile.Items.RemoveAt(index);
                    }

                    if (profile.Cutoff == quality)
                    {
                        profile.Cutoff = groupId;
                    }
                }

                _changedProfiles.Add(profile);
            }
        }

        private List<Profile117> GetProfiles()
        {
            var profiles = new List<Profile117>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Cutoff, Items FROM Profiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new Profile117
                                     {
                                         Id = profileReader.GetInt32(0),
                                         Name = profileReader.GetString(1),
                                         Cutoff = profileReader.GetInt32(2),
                                         Items = Json.Deserialize<List<ProfileItem117>>(profileReader.GetString(3))
                                     });
                    }
                }
            }

            return profiles;
        }
    }
}
