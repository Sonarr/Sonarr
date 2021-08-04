using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(111)]
    public class create_language_profiles : NzbDroneMigrationBase
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
            var languageConverter = new EmbeddedDocumentConverter<List<LanguageProfileItem111>>(new LanguageIntConverter());

            foreach (var profile in profiles.OrderBy(p => p.Id))
            {
                using (IDbCommand insertNewLanguageProfileCmd = conn.CreateCommand())
                {
                    insertNewLanguageProfileCmd.Transaction = tran;
                    insertNewLanguageProfileCmd.CommandText = "INSERT INTO LanguageProfiles (Id, Name, Cutoff, Languages) VALUES (?, ?, ?, ?)";
                    insertNewLanguageProfileCmd.AddParameter(profile.Id);
                    insertNewLanguageProfileCmd.AddParameter(profile.Name);
                    insertNewLanguageProfileCmd.AddParameter(profile.Cutoff.Id);
                    var param = insertNewLanguageProfileCmd.CreateParameter();
                    languageConverter.SetValue(param, profile.Languages);
                    insertNewLanguageProfileCmd.Parameters.Add(param);

                    insertNewLanguageProfileCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateSeriesCmd = conn.CreateCommand())
                {
                    foreach (var profileId in profile.ProfileIds)
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

        private List<LanguageProfile111> GetDefaultLanguageProfiles()
        {
            var profiles = new List<LanguageProfile111>();

            var languages = GetOrderedLanguages().Select(v => new LanguageProfileItem111 { Language = v, Allowed = v == Language.English })
                                                 .ToList();

            profiles.Add(new LanguageProfile111
                         {
                            Id = 1,
                            Name = "English",
                            Cutoff = Language.English,
                            Languages = languages
                         });

            return profiles;
        }

        private List<LanguageProfile111> GetLanguageProfiles(IDbConnection conn, IDbTransaction tran)
        {
            var profiles = GetDefaultLanguageProfiles();
            var thereAreProfiles = false;

            using (IDbCommand getProfilesCmd = conn.CreateCommand())
            {
                getProfilesCmd.Transaction = tran;
                getProfilesCmd.CommandText = @"SELECT Id, Language FROM Profiles";

                using (IDataReader profileReader = getProfilesCmd.ExecuteReader())
                {
                    while (profileReader.Read())
                    {
                        thereAreProfiles = true;
                        var profileId = profileReader.GetInt32(0);
                        var lang = Language.English.Id;

                        try
                        {
                            lang = profileReader.GetInt32(1);
                        }
                        catch (InvalidCastException e)
                        {
                            _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                        }

                        if (profiles.None(p => p.Cutoff.Id == lang))
                        {
                            var language = Language.FindById(lang);
                            var languages = GetOrderedLanguages().Select(l => new LanguageProfileItem111 { Language = l, Allowed = l.Id == lang })
                                                                 .ToList();

                            profiles.Add(new LanguageProfile111
                            {
                                Id = profiles.Count + 1,
                                Name = language.Name,
                                Cutoff = language,
                                Languages = languages,
                                ProfileIds = new List<int> { profileId }
                            });
                        }
                        else
                        {
                            profiles = profiles.Select(p =>
                            {
                                if (p.Cutoff.Id == lang)
                                {
                                    p.ProfileIds.Add(profileId);
                                }

                                return p;
                            }).ToList();
                        }
                    }
                }
            }

            if (!thereAreProfiles)
            {
                return new List<LanguageProfile111>();
            }

            return profiles;
        }

        private List<Language> GetOrderedLanguages()
        {
            var orderedLanguages = Language.All
                                           .Where(l => l != Language.Unknown)
                                           .OrderByDescending(l => l.Name)
                                           .ToList();

            orderedLanguages.Insert(0, Language.Unknown);

            return orderedLanguages;
        }

        private class LanguageProfile111
        {
            public int Id { get; set; }
            public List<int> ProfileIds { get; set; }
            public string Name { get; set; }
            public Language Cutoff { get; set; }
            public List<LanguageProfileItem111> Languages { get; set; }

            public LanguageProfile111()
            {
                ProfileIds = new List<int>();
            }
        }

        private class LanguageProfileItem111
        {
            public Language Language { get; set; }
            public bool Allowed { get; set; }
        }
    }
}
