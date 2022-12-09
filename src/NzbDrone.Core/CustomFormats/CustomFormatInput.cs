using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormatInput
    {
        public ParsedEpisodeInfo EpisodeInfo { get; set; }
        public Series Series { get; set; }
        public long Size { get; set; }
        public List<Language> Languages { get; set; }
        public string Filename { get; set; }

        public CustomFormatInput()
        {
            Languages = new List<Language>();
        }

        // public CustomFormatInput(ParsedEpisodeInfo episodeInfo, Series series)
        // {
        //     EpisodeInfo = episodeInfo;
        //     Series = series;
        // }
        //
        // public CustomFormatInput(ParsedEpisodeInfo episodeInfo, Series series, long size, List<Language> languages)
        // {
        //     EpisodeInfo = episodeInfo;
        //     Series = series;
        //     Size = size;
        //     Languages = languages;
        // }
        //
        // public CustomFormatInput(ParsedEpisodeInfo episodeInfo, Series series, long size, List<Language> languages, string filename)
        // {
        //     EpisodeInfo = episodeInfo;
        //     Series = series;
        //     Size = size;
        //     Languages = languages;
        //     Filename = filename;
        // }
    }
}
