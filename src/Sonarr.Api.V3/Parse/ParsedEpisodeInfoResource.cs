using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace Sonarr.Api.V3.Parse
{
    public class ParsedEpisodeInfoResource
    {
        public string ReleaseTitle { get; set; }
        public string SeriesTitle { get; set; }
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int[] SeasonNumbers { get; set; }
        public int SeasonNumber { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public decimal[] SpecialAbsoluteEpisodeNumbers { get; set; }
        public string AirDate { get; set; }
        public List<Language> Languages { get; set; }
        public bool FullSeason { get; set; }
        public bool IsPartialSeason { get; set; }
        public bool IsMultiSeason { get; set; }
        public bool IsSeasonExtra { get; set; }
        public bool IsSplitEpisode { get; set; }
        public bool IsMiniSeries { get; set; }
        public bool Special { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public int SeasonPart { get; set; }
        public string ReleaseTokens { get; set; }
        public int? DailyPart { get; set; }
        public bool IsDaily { get; set; }
        public bool IsAbsoluteNumbering { get; set; }
        public bool IsPossibleSpecialEpisode { get; set; }
        public bool IsPossibleSceneSeasonSpecial { get; set; }
        public ReleaseType ReleaseType { get; set; }
    }

    public static class ParsedEpisodeInfoResourceMapper
    {
        public static ParsedEpisodeInfoResource ToResource(this ParsedEpisodeInfo model)
        {
            if (model == null)
            {
                return null;
            }

            return new ParsedEpisodeInfoResource
            {
                ReleaseTitle = model.ReleaseTitle,
                SeriesTitle = model.SeriesTitle,
                SeriesTitleInfo = model.SeriesTitleInfo,
                Quality = model.Quality,
                SeasonNumbers = model.SeasonNumbers,
                SeasonNumber = model.SeasonNumber ?? -1,
                EpisodeNumbers = model.EpisodeNumbers,
                AbsoluteEpisodeNumbers = model.AbsoluteEpisodeNumbers,
                SpecialAbsoluteEpisodeNumbers = model.SpecialAbsoluteEpisodeNumbers,
                AirDate = model.AirDate,
                Languages = model.Languages,
                FullSeason = model.FullSeason,
                IsPartialSeason = model.IsPartialSeason,
                IsMultiSeason = model.IsMultiSeason,
                IsSeasonExtra = model.IsSeasonExtra,
                IsSplitEpisode = model.IsSplitEpisode,
                IsMiniSeries = model.IsMiniSeries,
                Special = model.Special,
                ReleaseGroup = model.ReleaseGroup,
                ReleaseHash = model.ReleaseHash,
                SeasonPart = model.SeasonPart,
                ReleaseTokens = model.ReleaseTokens,
                DailyPart = model.DailyPart,
                IsDaily = model.IsDaily,
                IsAbsoluteNumbering = model.IsAbsoluteNumbering,
                IsPossibleSpecialEpisode = model.IsPossibleSpecialEpisode,
                IsPossibleSceneSeasonSpecial = model.IsPossibleSceneSeasonSpecial,
                ReleaseType = model.ReleaseType
            };
        }
    }
}
