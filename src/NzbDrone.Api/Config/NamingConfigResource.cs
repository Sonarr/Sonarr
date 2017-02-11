using Sonarr.Http.REST;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Api.Config
{
    public class NamingConfigResource : RestResource
    {
        public bool RenameEpisodes { get; set; }
        public bool ReplaceIllegalCharacters { get; set; }
        public int MultiEpisodeStyle { get; set; }
        public string StandardEpisodeFormat { get; set; }
        public string DailyEpisodeFormat { get; set; }
        public string AnimeEpisodeFormat { get; set; }
        public string SeriesFolderFormat { get; set; }
        public string SeasonFolderFormat { get; set; }
        public bool IncludeSeriesTitle { get; set; }
        public bool IncludeEpisodeTitle { get; set; }
        public bool IncludeQuality { get; set; }
        public bool ReplaceSpaces { get; set; }
        public string Separator { get; set; }
        public string NumberStyle { get; set; }
    }

    public static class NamingConfigResourceMapper
    {
        public static NamingConfigResource ToResource(this NamingConfig model)
        {
            return new NamingConfigResource
            {
                Id = model.Id,

                RenameEpisodes = model.RenameEpisodes,
                ReplaceIllegalCharacters = model.ReplaceIllegalCharacters,
                MultiEpisodeStyle = model.MultiEpisodeStyle,
                StandardEpisodeFormat = model.StandardEpisodeFormat,
                DailyEpisodeFormat = model.DailyEpisodeFormat,
                AnimeEpisodeFormat = model.AnimeEpisodeFormat,
                SeriesFolderFormat = model.SeriesFolderFormat,
                SeasonFolderFormat = model.SeasonFolderFormat
                //IncludeSeriesTitle
                //IncludeEpisodeTitle
                //IncludeQuality
                //ReplaceSpaces
                //Separator
                //NumberStyle
            };
        }

        public static void AddToResource(this BasicNamingConfig basicNamingConfig, NamingConfigResource resource)
        {
            resource.IncludeSeriesTitle = basicNamingConfig.IncludeSeriesTitle;
            resource.IncludeEpisodeTitle = basicNamingConfig.IncludeEpisodeTitle;
            resource.IncludeQuality = basicNamingConfig.IncludeQuality;
            resource.ReplaceSpaces = basicNamingConfig.ReplaceSpaces;
            resource.Separator = basicNamingConfig.Separator;
            resource.NumberStyle = basicNamingConfig.NumberStyle;
        }

        public static NamingConfig ToModel(this NamingConfigResource resource)
        {
            return new NamingConfig
            {
                Id = resource.Id,

                RenameEpisodes = resource.RenameEpisodes,
                ReplaceIllegalCharacters = resource.ReplaceIllegalCharacters,
                MultiEpisodeStyle = resource.MultiEpisodeStyle,
                StandardEpisodeFormat = resource.StandardEpisodeFormat,
                DailyEpisodeFormat = resource.DailyEpisodeFormat,
                AnimeEpisodeFormat = resource.AnimeEpisodeFormat,
                SeriesFolderFormat = resource.SeriesFolderFormat,
                SeasonFolderFormat = resource.SeasonFolderFormat
            };
        }
    }
}