using FluentMigrator;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(96)]
    public class create_language_profile : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("LanguageProfiles").WithColumn("Name").AsString().Unique()
                                                    .WithColumn("Languages").AsString()
                                                    .WithColumn("Cutoff").AsInt32();

            Alter.Table("Series").AddColumn("LanguageProfileId").AsInt32().WithDefaultValue(1);

            Execute.WithConnection(InsertDefaultLanguages);

            Delete.Column("Language").FromTable("Profiles");                                
        }

        private void InsertDefaultLanguages(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetLanguageProfiles(conn, tran);
            var LanguageConverter = new EmbeddedDocumentConverter(new LanguageIntConverter());

            foreach (var profile in profiles.OrderBy(p => p.Id))
            {
                using (IDbCommand insertNewLanguageProfileCmd = conn.CreateCommand())
                {
                    var itemsJson = LanguageConverter.ToDB(profile.Languages);
                    insertNewLanguageProfileCmd.Transaction = tran;
                    insertNewLanguageProfileCmd.CommandText = "INSERT INTO LanguageProfiles (Id, Name, Cutoff, Languages) VALUES (?, ?, ?, ?)";
                    insertNewLanguageProfileCmd.AddParameter(profile.Id);
                    insertNewLanguageProfileCmd.AddParameter(profile.Name);
                    insertNewLanguageProfileCmd.AddParameter(profile.cutoff.Id);
                    insertNewLanguageProfileCmd.AddParameter(itemsJson);

                    insertNewLanguageProfileCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateSeriesCmd = conn.CreateCommand())
                {
                    foreach (var profileId in profile.ProfileId)
                    {
                        updateSeriesCmd.Transaction = tran;
                        updateSeriesCmd.CommandText = "UPDATE Series SET LanguageProfileId = ? WHERE ProfileId = ?";
                        updateSeriesCmd.AddParameter(profile.Id);
                        updateSeriesCmd.AddParameter(profileId);
                        updateSeriesCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private List<LangProfile> GetDefaultLanguageProfiles()
        {
            var profiles = new List<LangProfile>();

            // English
            var languages = Language.All
                                        .OrderByDescending(l => l.Name)
                                        .Select(v => new ProfileLanguageItem { Language = v, Allowed = v == Language.English })
                                        .ToList();
            profiles.Add(new LangProfile { Name = "English", cutoff = Language.English, Id = 1, Languages = languages });

            // Spanish
            languages = Language.All
                                    .OrderByDescending(l => l.Name)
                                    .Select(v => new ProfileLanguageItem { Language = v, Allowed = v == Language.Spanish })
                                    .ToList();
            profiles.Add(new LangProfile { Name = "Spanish", cutoff = Language.Spanish, Id = 2, Languages = languages });

            // French
            languages = Language.All
                                    .OrderByDescending(l => l.Name)
                                    .Select(v => new ProfileLanguageItem { Language = v, Allowed = v == Language.French })
                                    .ToList();
            profiles.Add(new LangProfile { Name = "French", cutoff = Language.French, Id = 3, Languages = languages });

            return profiles;
            
        }

        private List<LangProfile> GetLanguageProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetDefaultLanguageProfiles();
            var thereAreProfiles = false;


            using (IDbCommand getProfilesCmd = conn.CreateCommand())
            {
                getProfilesCmd.Transaction = tran;
                getProfilesCmd.CommandText = @"SELECT Id, Name, Language FROM Profiles";

                using (IDataReader profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        thereAreProfiles = true;
                        var profileId = profileReader.GetInt32(0);
                        var name = profileReader.GetString(1);
                        var lang = Language.English.Id;
                        try
                        {
                            lang = profileReader.GetInt32(2);
                        } catch (InvalidCastException e)
                        {
                            _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                        }
                        

                        if (!profiles.Any(p => p.cutoff.Id == lang))
                        {
                            var languages = Language.All
                                                    .OrderByDescending(l => l.Name)
                                                    .Select(v => new ProfileLanguageItem { Language = v, Allowed = v.Id == lang })
                                                    .ToList();

                            profiles.Add(new LangProfile { Id = profiles.Count+1, Name = "Language for profile " + name, cutoff = Language.FindById(lang), Languages = languages, ProfileId = new List<int> { profileId } });
                        }
                        else
                        {
                            profiles = profiles.Select(p =>
                            {
                                if (p.cutoff.Id == lang)
                                    p.ProfileId.Add(profileId);
                                return p;
                            }).ToList();
                        }
                    }
                }
            }

            if (!thereAreProfiles)
            {
                return new List<LangProfile>();
            }
            return profiles;
        }

        private class LangProfile
        {
            public int Id { get; set; }
            public List<int> ProfileId { get; set; }
            public string Name { get; set; }
            public Language cutoff { get; set; }
            public List<ProfileLanguageItem> Languages { get; set; }

            public LangProfile ()
            {
                ProfileId = new List<int>();
            }
        }
    }
}
