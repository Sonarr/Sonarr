using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesFolderSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IParsingService _parsingService;
        private readonly ConditionalWeakTable<ParsedEpisodeInfo, FolderEpisodes> _folderEpisodesCache = new();

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var fileInfo = localEpisode.FileEpisodeInfo;
            var folderInfo = localEpisode.FolderEpisodeInfo;

            if (fileInfo != null && fileInfo.IsPossibleSceneSeasonSpecial)
            {
                fileInfo = _parsingService.ParseSpecialEpisodeTitle(fileInfo, fileInfo.ReleaseTitle, localEpisode.Series.TvdbId, 0, null);
            }

            var folderEpisodes = folderInfo == null ? null : GetFolderEpisodes(folderInfo, localEpisode.Series);

            if (folderEpisodes == null)
            {
                _logger.Debug("No folder ParsedEpisodeInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (fileInfo == null)
            {
                _logger.Debug("No file ParsedEpisodeInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var fileEpisodes = _parsingService.GetEpisodes(fileInfo, localEpisode.Series, true);

            if (folderEpisodes.Empty())
            {
                _logger.Debug("No episode numbers in folder ParsedEpisodeInfo, skipping check");
                return ImportSpecDecision.Accept();
            }

            var unexpected = fileEpisodes.Where(e => folderEpisodes.All(o => o.Id != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeUnexpected, "Episode {0} was unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeUnexpected, "Episodes {0} were unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
            }

            return ImportSpecDecision.Accept();
        }

        private List<Episode> GetFolderEpisodes(ParsedEpisodeInfo folderInfo, Series series)
        {
            if (_folderEpisodesCache.TryGetValue(folderInfo, out var cached) && cached.SeriesId == series.Id)
            {
                return cached.Episodes;
            }

            var parsedFolderInfo = folderInfo.IsPossibleSceneSeasonSpecial
                ? _parsingService.ParseSpecialEpisodeTitle(folderInfo, folderInfo.ReleaseTitle, series.TvdbId, 0, null)
                : folderInfo;

            var episodes = parsedFolderInfo == null ? null : _parsingService.GetEpisodes(parsedFolderInfo, series, true);

            _folderEpisodesCache.AddOrUpdate(folderInfo, new FolderEpisodes { SeriesId = series.Id, Episodes = episodes });

            return episodes;
        }

        private class FolderEpisodes
        {
            public int SeriesId { get; set; }
            public List<Episode> Episodes { get; set; }
        }

        private string FormatEpisode(List<Episode> episodes)
        {
            return string.Join(", ", episodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
