using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(155)]
    public class add_arabic_and_hindi_languages : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new LanguageProfileUpdater128(conn, tran);

            updater.AppendMissing();

            updater.Commit();
        }
    }

    public class LanguageProfile128 : ModelBase
    {
        public string Name { get; set; }
        public List<LanguageProfileItem128> Languages { get; set; }
        public bool UpgradeAllowed { get; set; }
        public Language Cutoff { get; set; }
    }

    public class LanguageProfileItem128
    {
        public int Language { get; set; }
        public bool Allowed { get; set; }
    }

    public class LanguageProfileUpdater128
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<LanguageProfile128> _profiles;
        private HashSet<LanguageProfile128> _changedProfiles = new HashSet<LanguageProfile128>();

        public LanguageProfileUpdater128(IDbConnection conn, IDbTransaction tran)
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
                    updateProfileCmd.CommandText = "UPDATE LanguageProfiles SET Languages = ? WHERE Id = ?";
                    updateProfileCmd.AddParameter(profile.Languages.ToJson());
                    updateProfileCmd.AddParameter(profile.Id);

                    updateProfileCmd.ExecuteNonQuery();
                }
            }

            _changedProfiles.Clear();
        }

        public void AppendMissing()
        {
            foreach (var profile in _profiles)
            {
                var hash = new HashSet<int>(profile.Languages.Select(v => v.Language));

                var missing = Language.All.Where(l => !hash.Contains(l.Id))
                                          .OrderByDescending(l => l.Name)
                                          .ToList();

                if (missing.Any())
                {
                    profile.Languages.InsertRange(0, missing.Select(l => new LanguageProfileItem128 { Language = l.Id, Allowed = false }));

                    _changedProfiles.Add(profile);
                }
            }
        }

        private List<LanguageProfile128> GetProfiles()
        {
            var profiles = new List<LanguageProfile128>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Languages, UpgradeAllowed, Cutoff FROM LanguageProfiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new LanguageProfile128
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Languages = Json.Deserialize<List<LanguageProfileItem128>>(profileReader.GetString(2)),
                            UpgradeAllowed = profileReader.GetBoolean(3),
                            Cutoff = Language.FindById(profileReader.GetInt32(4))
                        });
                    }
                }
            }

            return profiles;
        }
    }
}
