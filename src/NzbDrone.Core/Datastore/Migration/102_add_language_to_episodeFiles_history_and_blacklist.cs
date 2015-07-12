using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Languages;
using System;

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
            var LanguageConverter = new EmbeddedDocumentConverter(new LanguageIntConverter());

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

                        using (IDbCommand getProfileCmd = conn.CreateCommand())
                        {
                            getProfileCmd.Transaction = tran;
                            getProfileCmd.CommandText = "SELECT Language FROM Profiles WHERE Id = ?";
                            getProfileCmd.AddParameter(seriesProfileId);
                            IDataReader profilesReader = getProfileCmd.ExecuteReader();
                            while (profilesReader.Read())
                            {
                                var episodeLanguage = Language.English.Id;
                                try
                                {
                                    episodeLanguage = profilesReader.GetInt32(0);
                                } catch (InvalidCastException e)
                                {
                                    _logger.Debug("Language field not found in Profiles, using English as default." + e.Message);
                                }

                                var validJson = LanguageConverter.ToDB(Language.FindById(episodeLanguage));

                                using (IDbCommand updateEpisodeFilesCmd = conn.CreateCommand())
                                {
                                    updateEpisodeFilesCmd.Transaction = tran;
                                    updateEpisodeFilesCmd.CommandText = "UPDATE EpisodeFiles SET Language = ? WHERE SeriesId = ?";
                                    updateEpisodeFilesCmd.AddParameter(validJson);
                                    updateEpisodeFilesCmd.AddParameter(seriesId);

                                    updateEpisodeFilesCmd.ExecuteNonQuery();
                                }

                                using (IDbCommand updateHistoryCmd = conn.CreateCommand())
                                {
                                    updateHistoryCmd.Transaction = tran;
                                    updateHistoryCmd.CommandText = "UPDATE History SET Language = ? WHERE SeriesId = ?";
                                    updateHistoryCmd.AddParameter(validJson);
                                    updateHistoryCmd.AddParameter(seriesId);

                                    updateHistoryCmd.ExecuteNonQuery();
                                }

                                using (IDbCommand updateBlacklistCmd = conn.CreateCommand())
                                {
                                    updateBlacklistCmd.Transaction = tran;
                                    updateBlacklistCmd.CommandText = "UPDATE Blacklist SET Language = ? WHERE SeriesId = ?";
                                    updateBlacklistCmd.AddParameter(validJson);
                                    updateBlacklistCmd.AddParameter(seriesId);

                                    updateBlacklistCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
