using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatCalculationService
    {
        List<CustomFormat> ParseCustomFormat(ParsedEpisodeInfo episodeInfo, Series series);
        List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist);
        List<CustomFormat> ParseCustomFormat(EpisodeHistory history);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;
        private readonly IParsingService _parsingService;
        private readonly ISeriesService _seriesService;

        public CustomFormatCalculationService(ICustomFormatService formatService,
                                              IParsingService parsingService,
                                              ISeriesService seriesService)
        {
            _formatService = formatService;
            _parsingService = parsingService;
            _seriesService = seriesService;
        }

        public static List<CustomFormat> ParseCustomFormat(ParsedEpisodeInfo episodeInfo, List<CustomFormat> allCustomFormats)
        {
            var matches = new List<CustomFormat>();

            foreach (var customFormat in allCustomFormats)
            {
                var specificationMatches = customFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(episodeInfo))
                    })
                    .ToList();

                if (specificationMatches.All(x => x.DidMatch))
                {
                    matches.Add(customFormat);
                }
            }

            return matches;
        }

        public static List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile, List<CustomFormat> allCustomFormats)
        {
            var sceneName = string.Empty;
            if (episodeFile.SceneName.IsNotNullOrWhiteSpace())
            {
                sceneName = episodeFile.SceneName;
            }
            else if (episodeFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                sceneName = episodeFile.OriginalFilePath;
            }
            else if (episodeFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                sceneName = Path.GetFileName(episodeFile.RelativePath);
            }

            var info = new ParsedEpisodeInfo
            {
                SeriesTitle = episodeFile.Series.Value.Title,
                ReleaseTitle  = sceneName,
                Quality = episodeFile.Quality,
                Languages = episodeFile.Languages,
                ReleaseGroup = episodeFile.ReleaseGroup,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "Size", episodeFile.Size },
                    { "Filename", Path.GetFileName(episodeFile.RelativePath) },
                    { "OriginalLanguage", episodeFile.Series.Value.OriginalLanguage }
                }
            };

            return ParseCustomFormat(info, allCustomFormats);
        }

        public List<CustomFormat> ParseCustomFormat(ParsedEpisodeInfo episodeInfo, Series series)
        {
            if (series?.OriginalLanguage != null)
            {
                episodeInfo.ExtraInfo["OriginalLanguage"] = series.OriginalLanguage;
            }

            return ParseCustomFormat(episodeInfo, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile)
        {
            return ParseCustomFormat(episodeFile, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist)
        {
            var series = _seriesService.GetSeries(blocklist.SeriesId);
            var parsed = Parser.Parser.ParseTitle(blocklist.SourceTitle);

            var info = new ParsedEpisodeInfo
            {
                SeriesTitle = series.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? blocklist.SourceTitle,
                Quality = blocklist.Quality,
                Languages = blocklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "Size", blocklist.Size }
                }
            };

            return ParseCustomFormat(info, series);
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeHistory history)
        {
            var series = _seriesService.GetSeries(history.SeriesId);
            var parsed = Parser.Parser.ParseTitle(history.SourceTitle);

            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);

            var info = new ParsedEpisodeInfo
            {
                SeriesTitle = series.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? history.SourceTitle,
                Quality = history.Quality,
                Languages = history.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
                ExtraInfo = new Dictionary<string, object>
                {
                    { "Size", size }
                }
            };

            return ParseCustomFormat(info, series);
        }
    }
}
