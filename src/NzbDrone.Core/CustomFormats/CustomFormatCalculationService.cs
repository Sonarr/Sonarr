using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatCalculationService
    {
        List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteEpisode);
        List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist);
        List<CustomFormat> ParseCustomFormat(EpisodeHistory history);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;
        private readonly ISeriesService _seriesService;

        public CustomFormatCalculationService(ICustomFormatService formatService,
                                              ISeriesService seriesService)
        {
            _formatService = formatService;
            _seriesService = seriesService;
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

            var episodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = episodeFile.Series.Value.Title,
                ReleaseTitle  = sceneName,
                Quality = episodeFile.Quality,
                Languages = episodeFile.Languages,
                ReleaseGroup = episodeFile.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                EpisodeInfo = episodeInfo,
                Series = episodeFile.Series.Value,
                Size = episodeFile.Size,
                Languages = episodeFile.Languages,
                Filename = Path.GetFileName(episodeFile.RelativePath)
            };

            return ParseCustomFormat(input, allCustomFormats);
        }

        public List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteEpisode)
        {
            var input = new CustomFormatInput
            {
                EpisodeInfo = remoteEpisode.ParsedEpisodeInfo,
                Series = remoteEpisode.Series,
                Size = remoteEpisode.Release.Size,
                Languages = remoteEpisode.Languages
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile)
        {
            return ParseCustomFormat(episodeFile, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist)
        {
            var series = _seriesService.GetSeries(blocklist.SeriesId);
            var parsed = Parser.Parser.ParseTitle(blocklist.SourceTitle);

            var episodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = series.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? blocklist.SourceTitle,
                Quality = blocklist.Quality,
                Languages = blocklist.Languages,
                ReleaseGroup = parsed?.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                EpisodeInfo = episodeInfo,
                Series = series,
                Size = blocklist.Size ?? 0,
                Languages = blocklist.Languages
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeHistory history)
        {
            var series = _seriesService.GetSeries(history.SeriesId);
            var parsed = Parser.Parser.ParseTitle(history.SourceTitle);

            long.TryParse(history.Data.GetValueOrDefault("size"), out var size);

            var episodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = series.Title,
                ReleaseTitle = parsed?.ReleaseTitle ?? history.SourceTitle,
                Quality = history.Quality,
                Languages = history.Languages,
                ReleaseGroup = parsed?.ReleaseGroup,
            };

            var input = new CustomFormatInput
            {
                EpisodeInfo = episodeInfo,
                Series = series,
                Size = size,
                Languages = history.Languages
            };

            return ParseCustomFormat(input);
        }

        private List<CustomFormat> ParseCustomFormat(CustomFormatInput input)
        {
            return ParseCustomFormat(input, _formatService.All());
        }

        private static List<CustomFormat> ParseCustomFormat(CustomFormatInput input, List<CustomFormat> allCustomFormats)
        {
            var matches = new List<CustomFormat>();

            foreach (var customFormat in allCustomFormats)
            {
                var specificationMatches = customFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .Select(g => new SpecificationMatchesGroup
                    {
                        Matches = g.ToDictionary(t => t, t => t.IsSatisfiedBy(input))
                    })
                    .ToList();

                if (specificationMatches.All(x => x.DidMatch))
                {
                    matches.Add(customFormat);
                }
            }

            return matches;
        }
    }
}
