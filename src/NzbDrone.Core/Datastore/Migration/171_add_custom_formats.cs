using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(171)]
    public class add_custom_formats : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Add Custom Format Columns
            Create.TableForModel("CustomFormats")
                .WithColumn("Name").AsString().Unique()
                .WithColumn("Specifications").AsString().WithDefaultValue("[]")
                .WithColumn("IncludeCustomFormatWhenRenaming").AsBoolean().WithDefaultValue(false);

            //Add Custom Format Columns to Quality Profiles
            Alter.Table("QualityProfiles").AddColumn("FormatItems").AsString().WithDefaultValue("[]");
            Alter.Table("QualityProfiles").AddColumn("MinFormatScore").AsInt32().WithDefaultValue(0);
            Alter.Table("QualityProfiles").AddColumn("CutoffFormatScore").AsInt32().WithDefaultValue(0);

            //Migrate Preferred Words to Custom Formats
            Execute.WithConnection(MigratePreferredTerms);
            Execute.WithConnection(MigrateNamingConfigs);

            //Remove Preferred Word Columns from ReleaseProfiles
            Delete.Column("Preferred").FromTable("ReleaseProfiles");
            Delete.Column("IncludePreferredWhenRenaming").FromTable("ReleaseProfiles");

            //Remove Profiles that will no longer validate
            Execute.Sql("DELETE FROM ReleaseProfiles WHERE Required == '[]' AND Ignored == '[]'");

            //TODO: Kill any references to Preferred in History and Files
            //Data.PreferredWordScore
        }

        private void MigratePreferredTerms(IDbConnection conn, IDbTransaction tran)
        {
            var updatedCollections = new List<CustomFormat171>();

            // Pull list of quality Profiles
            var qualityProfiles = new List<QualityProfile171>();
            using (var getProfiles = conn.CreateCommand())
            {
                getProfiles.Transaction = tran;
                getProfiles.CommandText = @"SELECT Id FROM QualityProfiles";

                using (var definitionsReader = getProfiles.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        var id = definitionsReader.GetInt32(0);
                        qualityProfiles.Add(new QualityProfile171
                        {
                            Id = id,
                        });
                    }
                }
            }

            //Generate List of Custom Formats from Preferred Words
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Preferred, Name, IncludePreferredWhenRenaming, Enabled, Id FROM ReleaseProfiles WHERE Preferred IS NOT NULL";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var preferred = reader.GetString(0);
                        var nameObj = reader.GetValue(1);
                        var includeName = reader.GetBoolean(2);
                        var enabled = reader.GetBoolean(3);
                        var releaseProfileId = reader.GetInt32(4);

                        var name = nameObj == DBNull.Value ? null : (string)nameObj;

                        if (name.IsNullOrWhiteSpace())
                        {
                            name = $"Unnamed_{releaseProfileId}";
                        }
                        else
                        {
                            name = $"{name}_{releaseProfileId}";
                        }

                        var data = STJson.Deserialize<List<PreferredWord170>>(preferred);

                        var specs = new List<CustomFormatSpec171>();

                        var nameIdentifier = 0;

                        foreach (var term in data)
                        {
                            var regexTerm = term.Key.TrimStart('/').TrimEnd("/i");

                            // Validate Regex before creating a CF
                            try
                            {
                                Regex.Match("", regexTerm);
                            }
                            catch (ArgumentException)
                            {
                                continue;
                            }

                            updatedCollections.Add(new CustomFormat171
                            {
                                Name = data.Count > 1 ? $"{name}_{nameIdentifier++}" : name,
                                PreferredName = name,
                                IncludeCustomFormatWhenRenaming = includeName,
                                Score = term.Value,
                                Enabled = enabled,
                                Specifications = new List<CustomFormatSpec171>
                                {
                                    new CustomFormatSpec171
                                    {
                                        Type = "ReleaseTitleSpecification",
                                        Body = new CustomFormatReleaseTitleSpec171
                                        {
                                            Order = 1,
                                            ImplementationName = "Release Title",
                                            Name = regexTerm,
                                            Value = regexTerm
                                        }
                                    }
                                }.ToJson()
                            });
                        }
                    }
                }
            }

            // Insert Custom Formats
            var updateSql = "INSERT INTO CustomFormats (Name, IncludeCustomFormatWhenRenaming, Specifications) VALUES (@Name, @IncludeCustomFormatWhenRenaming, @Specifications)";
            conn.Execute(updateSql, updatedCollections, transaction: tran);

            // Pull List of Custom Formats with new Ids
            var formats = new List<CustomFormat171>();
            using (var getProfiles = conn.CreateCommand())
            {
                getProfiles.Transaction = tran;
                getProfiles.CommandText = @"SELECT Id, Name FROM CustomFormats";

                using (var definitionsReader = getProfiles.ExecuteReader())
                {
                    while (definitionsReader.Read())
                    {
                        var id = definitionsReader.GetInt32(0);
                        var name = definitionsReader.GetString(1);
                        formats.Add(new CustomFormat171
                        {
                            Id = id,
                            Name = name
                        });
                    }
                }
            }

            // Update each profile with original scores
            foreach (var profile in qualityProfiles)
            {
                profile.FormatItems = formats.Select(x => new { Format = x.Id, Score = updatedCollections.First(f => f.Name == x.Name).Enabled ? updatedCollections.First(f => f.Name == x.Name).Score : 0 }).ToJson();
            }

            // Push profile updates to DB
            var updateProfilesSql = "UPDATE QualityProfiles SET FormatItems = @FormatItems WHERE Id = @Id";
            conn.Execute(updateProfilesSql, qualityProfiles, transaction: tran);
        }

        private void MigrateNamingConfigs(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand namingConfigCmd = conn.CreateCommand())
            {
                namingConfigCmd.Transaction = tran;
                namingConfigCmd.CommandText = @"SELECT * FROM NamingConfig LIMIT 1";
                using (IDataReader namingConfigReader = namingConfigCmd.ExecuteReader())
                {
                    var standardEpisodeFormatIndex = namingConfigReader.GetOrdinal("StandardEpisodeFormat");
                    var dailyEpisodeFormatIndex = namingConfigReader.GetOrdinal("DailyEpisodeFormat");
                    var animeEpisodeFormatIndex = namingConfigReader.GetOrdinal("AnimeEpisodeFormat");

                    while (namingConfigReader.Read())
                    {
                        var standardEpisodeFormat = NameReplace(namingConfigReader.GetString(standardEpisodeFormatIndex));
                        var dailyEpisodeFormat = NameReplace(namingConfigReader.GetString(dailyEpisodeFormatIndex));
                        var animeEpisodeFormat = NameReplace(namingConfigReader.GetString(animeEpisodeFormatIndex));

                        using (IDbCommand updateCmd = conn.CreateCommand())
                        {
                            var text = string.Format("UPDATE NamingConfig " +
                                                     "SET StandardEpisodeFormat = '{0}', " +
                                                     "DailyEpisodeFormat = '{1}', " +
                                                     "AnimeEpisodeFormat = '{2}'",
                                                     standardEpisodeFormat,
                                                     dailyEpisodeFormat,
                                                     animeEpisodeFormat);

                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = text;
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private string NameReplace(string oldTokenString)
        {
            var newTokenString = oldTokenString.Replace("Preferred Words", "Custom Formats")
                                               .Replace("Preferred.Words", "Custom.Formats")
                                               .Replace("Preferred-Words", "Custom-Formats")
                                               .Replace("Preferred_Words", "Custom_Formats");

            return newTokenString;
        }

        private class PreferredWord170
        {
            public string Key { get; set; }
            public int Value { get; set; }
        }

        private class QualityProfile171
        {
            public int Id { get; set; }
            public string FormatItems { get; set; }
        }

        private class CustomFormat171
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PreferredName { get; set; }
            public bool IncludeCustomFormatWhenRenaming { get; set; }
            public string Specifications { get; set; }
            public int Score { get; set; }
            public bool Enabled { get; set; }
        }

        private class CustomFormatSpec171
        {
            public string Type { get; set; }
            public CustomFormatReleaseTitleSpec171 Body { get; set; }
        }

        private class CustomFormatReleaseTitleSpec171
        {
            public int Order { get; set; }
            public string ImplementationName { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public bool Required { get; set; }
            public bool Negate { get; set; }
        }
    }
}
