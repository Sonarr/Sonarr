using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(161)]
    public class remove_plex_hometheatre : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexHomeTheater" });
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexClient" });

            // Switch Quality and Language to int in pending releases
            Execute.WithConnection(FixPendingReleases);
        }

        private void FixPendingReleases(IDbConnection conn, IDbTransaction tran)
        {
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedEpisodeInfo161>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedEpisodeInfo162>());
            var rows = conn.Query<ParsedEpisodeInfoData161>("SELECT Id, ParsedEpisodeInfo from PendingReleases");

            var newRows = new List<ParsedEpisodeInfoData162>();

            foreach (var row in rows)
            {
                var old = row.ParsedEpisodeInfo;

                var newQuality = new QualityModel162
                {
                    Quality = old.Quality.Quality.Id,
                    Revision = old.Quality.Revision
                };

                var correct = new ParsedEpisodeInfo162
                {
                    SeriesTitle = old.SeriesTitle,
                    SeriesTitleInfo = old.SeriesTitleInfo,
                    Quality = newQuality,
                    SeasonNumber = old.SeasonNumber,
                    SeasonPart = old.SeasonPart,
                    EpisodeNumbers = old.EpisodeNumbers,
                    AbsoluteEpisodeNumbers = old.AbsoluteEpisodeNumbers,
                    SpecialAbsoluteEpisodeNumbers = old.SpecialAbsoluteEpisodeNumbers,
                    Language = old.Language?.Id ?? 0,
                    FullSeason = old.FullSeason,
                    IsPartialSeason = old.IsPartialSeason,
                    IsMultiSeason = old.IsMultiSeason,
                    IsSeasonExtra = old.IsSeasonExtra,
                    Speacial = old.Speacial,
                    ReleaseGroup = old.ReleaseGroup,
                    ReleaseHash = old.ReleaseHash,
                    ReleaseTokens = old.ReleaseTokens,
                    IsDaily = old.IsDaily,
                    IsAbsoluteNumbering = old.IsAbsoluteNumbering,
                    IsPossibleSpecialEpisode = old.IsPossibleSpecialEpisode,
                    IsPossibleSceneSeasonSpecial = old.IsPossibleSceneSeasonSpecial
                };

                newRows.Add(new ParsedEpisodeInfoData162
                {
                    Id = row.Id,
                    ParsedEpisodeInfo = correct
                });
            }

            var sql = $"UPDATE PendingReleases SET ParsedEpisodeInfo = @ParsedEpisodeInfo WHERE Id = @Id";

            conn.Execute(sql, newRows, transaction: tran);
        }

        private class ParsedEpisodeInfoData161 : ModelBase
        {
            public ParsedEpisodeInfo161 ParsedEpisodeInfo { get; set; }
        }

        private class ParsedEpisodeInfo161
        {
            public string SeriesTitle { get; set; }
            public SeriesTitleInfo161 SeriesTitleInfo { get; set; }
            public QualityModel161 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> EpisodeNumbers { get; set; }
            public List<int> AbsoluteEpisodeNumbers { get; set; }
            public List<int> SpecialAbsoluteEpisodeNumbers { get; set; }
            public Language161 Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class SeriesTitleInfo161
        {
            public string Title { get; set; }
            public string TitleWithoutYear { get; set; }
            public int Year { get; set; }
        }

        private class QualityModel161
        {
            public Quality161 Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Language161
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Quality161
        {
            public int Id { get; set; }
        }

        private class ParsedEpisodeInfoData162 : ModelBase
        {
            public ParsedEpisodeInfo162 ParsedEpisodeInfo { get; set; }
        }

        private class ParsedEpisodeInfo162
        {
            public string SeriesTitle { get; set; }
            public SeriesTitleInfo161 SeriesTitleInfo { get; set; }
            public QualityModel162 Quality { get; set; }
            public int SeasonNumber { get; set; }
            public List<int> EpisodeNumbers { get; set; }
            public List<int> AbsoluteEpisodeNumbers { get; set; }
            public List<int> SpecialAbsoluteEpisodeNumbers { get; set; }
            public int Language { get; set; }
            public bool FullSeason { get; set; }
            public bool IsPartialSeason { get; set; }
            public bool IsMultiSeason { get; set; }
            public bool IsSeasonExtra { get; set; }
            public bool Speacial { get; set; }
            public string ReleaseGroup { get; set; }
            public string ReleaseHash { get; set; }
            public int SeasonPart { get; set; }
            public string ReleaseTokens { get; set; }
            public bool IsDaily { get; set; }
            public bool IsAbsoluteNumbering { get; set; }
            public bool IsPossibleSpecialEpisode { get; set; }
            public bool IsPossibleSceneSeasonSpecial { get; set; }
        }

        private class QualityModel162
        {
            public int Quality { get; set; }
            public Revision162 Revision { get; set; }
        }

        private class Revision162
        {
            public int Version { get; set; }
            public int Real { get; set; }
            public bool IsRepack { get; set; }
        }
    }
}
