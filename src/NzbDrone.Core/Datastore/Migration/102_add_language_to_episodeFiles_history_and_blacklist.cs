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
    [Migration(102)]
    public class add_language_to_episodeFiles_history_and_blacklist : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Alter.Table("History")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Alter.Table("Blacklist")
                 .AddColumn("Language").AsInt32().NotNullable().WithDefaultValue(0);

            Execute.WithConnection(UpdateLanguage);
        }

        private void UpdateLanguage(IDbConnection conn, IDbTransaction tran)
        {
            var languageConverter = new EmbeddedDocumentConverter(new LanguageIntConverter());

            var profileLanguages = new Dictionary<int, int>();
            using (IDbCommand getProfileCmd = conn.CreateCommand())
            {
                getProfileCmd.Transaction = tran;
                getProfileCmd.CommandText = "SELECT Id, Language FROM Profiles";

                IDataReader profilesReader = getProfileCmd.ExecuteReader();
                while (profilesReader.Read())
                {
                    var profileId = profilesReader.GetInt32(0);
                    var episodeLanguage = Language.English.Id;
                    try
                    {
                        episodeLanguage = profilesReader.GetInt32(1);
                    }
                    catch (InvalidCastException e)
                    {
                        _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                    }

                    profileLanguages[profileId] = episodeLanguage;
                }
            }

            var seriesLanguages = new Dictionary<int, int>();
            using (IDbCommand getSeriesCmd = conn.CreateCommand())
            {
                getSeriesCmd.Transaction = tran;
                getSeriesCmd.CommandText = @"SELECT Id, ProfileId FROM Series";
                using (IDataReader seriesReader = getSeriesCmd.ExecuteReader())
                {
                    while (seriesReader.Read())
                    {
                        var seriesId = seriesReader.GetInt32(0);
                        var seriesProfileId = seriesReader.GetInt32(1);

                        seriesLanguages[seriesId] = profileLanguages.GetValueOrDefault(seriesProfileId, Language.English.Id);
                    }
                }
            }

            foreach (var group in seriesLanguages.GroupBy(v => v.Value, v => v.Key))
            {
                var languageJson = languageConverter.ToDB(Language.FindById(group.Key));

                var seriesIds = group.Select(v => v.ToString()).Join(",");

                using (IDbCommand updateEpisodeFilesCmd = conn.CreateCommand())
                {
                    updateEpisodeFilesCmd.Transaction = tran;
                    updateEpisodeFilesCmd.CommandText = $"UPDATE EpisodeFiles SET Language = ? WHERE SeriesId IN ({seriesIds})";
                    updateEpisodeFilesCmd.AddParameter(languageJson);

                    updateEpisodeFilesCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateHistoryCmd = conn.CreateCommand())
                {
                    updateHistoryCmd.Transaction = tran;
                    updateHistoryCmd.CommandText = $"UPDATE History SET Language = ? WHERE SeriesId IN ({seriesIds})";
                    updateHistoryCmd.AddParameter(languageJson);

                    updateHistoryCmd.ExecuteNonQuery();
                }

                using (IDbCommand updateBlacklistCmd = conn.CreateCommand())
                {
                    updateBlacklistCmd.Transaction = tran;
                    updateBlacklistCmd.CommandText = $"UPDATE Blacklist SET Language = ? WHERE SeriesId IN ({seriesIds})";
                    updateBlacklistCmd.AddParameter(languageJson);

                    updateBlacklistCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
