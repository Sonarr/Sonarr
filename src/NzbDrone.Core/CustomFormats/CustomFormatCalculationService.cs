using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatCalculationService
    {
        List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteEpisode, long size);
        List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile, Series series);
        List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile);
        List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Series series);
        List<CustomFormat> ParseCustomFormat(EpisodeHistory history, Series series);
        List<CustomFormat> ParseCustomFormat(LocalEpisode localEpisode);
    }

    public class CustomFormatCalculationService : ICustomFormatCalculationService
    {
        private readonly ICustomFormatService _formatService;

        public CustomFormatCalculationService(ICustomFormatService formatService)
        {
            _formatService = formatService;
        }

        public List<CustomFormat> ParseCustomFormat(RemoteEpisode remoteEpisode, long size)
        {
            var input = new CustomFormatInput
            {
                EpisodeInfo = remoteEpisode.ParsedEpisodeInfo,
                Series = remoteEpisode.Series,
                Size = size,
                Languages = remoteEpisode.Languages
            };

            return ParseCustomFormat(input);
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile, Series series)
        {
            return ParseCustomFormat(episodeFile, series, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile)
        {
            return ParseCustomFormat(episodeFile, episodeFile.Series.Value, _formatService.All());
        }

        public List<CustomFormat> ParseCustomFormat(Blocklist blocklist, Series series)
        {
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

        public List<CustomFormat> ParseCustomFormat(EpisodeHistory history, Series series)
        {
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

        public List<CustomFormat> ParseCustomFormat(LocalEpisode localEpisode)
        {
            var episodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = localEpisode.Series.Title,
                ReleaseTitle = localEpisode.SceneName.IsNotNullOrWhiteSpace() ? localEpisode.SceneName : Path.GetFileName(localEpisode.Path),
                Quality = localEpisode.Quality,
                Languages = localEpisode.Languages,
                ReleaseGroup = localEpisode.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                EpisodeInfo = episodeInfo,
                Series = localEpisode.Series,
                Size = localEpisode.Size,
                Languages = localEpisode.Languages,
                Filename = Path.GetFileName(localEpisode.Path)
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

            return matches.OrderBy(x => x.Name).ToList();
        }

        private static List<CustomFormat> ParseCustomFormat(EpisodeFile episodeFile, Series series, List<CustomFormat> allCustomFormats)
        {
            var releaseTitle = string.Empty;

            if (episodeFile.SceneName.IsNotNullOrWhiteSpace())
            {
                releaseTitle = episodeFile.SceneName;
            }
            else if (episodeFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                releaseTitle = Path.GetFileName(episodeFile.OriginalFilePath);
            }
            else if (episodeFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                releaseTitle = Path.GetFileName(episodeFile.RelativePath);
            }

            var episodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = series.Title,
                ReleaseTitle = releaseTitle,
                Quality = episodeFile.Quality,
                Languages = episodeFile.Languages,
                ReleaseGroup = episodeFile.ReleaseGroup
            };

            var input = new CustomFormatInput
            {
                EpisodeInfo = episodeInfo,
                Series = series,
                Size = episodeFile.Size,
                Languages = episodeFile.Languages,
                Filename = Path.GetFileName(episodeFile.RelativePath)
            };

            return ParseCustomFormat(input, allCustomFormats);
        }
    }
}
