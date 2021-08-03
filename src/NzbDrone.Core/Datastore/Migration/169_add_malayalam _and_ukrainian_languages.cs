using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(169)]
    public class add_malayalam_and_ukrainian_languages : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ConvertProfile);
        }

        private void ConvertProfile(IDbConnection conn, IDbTransaction tran)
        {
            var updater = new LanguageProfileUpdater169(conn, tran);

            updater.AppendMissing();

            updater.Commit();
        }
    }

    public class LanguageProfile169 : ModelBase
    {
        public string Name { get; set; }
        public List<LanguageProfileItem169> Languages { get; set; }
        public bool UpgradeAllowed { get; set; }
        public Language Cutoff { get; set; }
    }

    public class LanguageProfileItem169
    {
        public int Language { get; set; }
        public bool Allowed { get; set; }
    }

    public class LanguageProfileUpdater169
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        private List<LanguageProfile169> _profiles;
        private HashSet<LanguageProfile169> _changedProfiles = new HashSet<LanguageProfile169>();

        public LanguageProfileUpdater169(IDbConnection conn, IDbTransaction tran)
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
                    profile.Languages.InsertRange(0, missing.Select(l => new LanguageProfileItem169 { Language = l.Id, Allowed = false }));

                    _changedProfiles.Add(profile);
                }
            }
        }

        private List<LanguageProfile169> GetProfiles()
        {
            var profiles = new List<LanguageProfile169>();

            using (var getProfilesCmd = _connection.CreateCommand())
            {
                getProfilesCmd.Transaction = _transaction;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Languages, UpgradeAllowed, Cutoff FROM LanguageProfiles";

                using (var profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        profiles.Add(new LanguageProfile169
                        {
                            Id = profileReader.GetInt32(0),
                            Name = profileReader.GetString(1),
                            Languages = Json.Deserialize<List<LanguageProfileItem169>>(profileReader.GetString(2)),
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
