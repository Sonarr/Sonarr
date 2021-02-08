using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
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

        public MatchesFolderSpecification(IParsingService parsingService, Logger logger)
        {
            _logger = logger;
            _parsingService = parsingService;
        }

        public Decision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return Decision.Accept();
            }

            var fileInfo = localEpisode.FileEpisodeInfo;
            var folderInfo = localEpisode.FolderEpisodeInfo;

            if (fileInfo != null && fileInfo.IsPossibleSceneSeasonSpecial)
            {
                fileInfo = _parsingService.ParseSpecialEpisodeTitle(fileInfo, fileInfo.ReleaseTitle, localEpisode.Series.TvdbId, 0);
            }

            if (folderInfo != null && folderInfo.IsPossibleSceneSeasonSpecial)
            {
                folderInfo = _parsingService.ParseSpecialEpisodeTitle(folderInfo, folderInfo.ReleaseTitle, localEpisode.Series.TvdbId, 0);
            }

            if (folderInfo == null)
            {
                _logger.Debug("No folder ParsedEpisodeInfo, skipping check");
                return Decision.Accept();
            }

            if (fileInfo == null)
            {
                _logger.Debug("No file ParsedEpisodeInfo, skipping check");
                return Decision.Accept();
            }

            var folderEpisodes = _parsingService.GetEpisodes(folderInfo, localEpisode.Series, true);
            var fileEpisodes = _parsingService.GetEpisodes(fileInfo, localEpisode.Series, true);

            if (folderEpisodes.Empty())
            {
                _logger.Debug("No episode numbers in folder ParsedEpisodeInfo, skipping check");
                return Decision.Accept();
            }

            if (folderEpisodes.First().SeasonNumber != fileEpisodes.FirstOrDefault()?.SeasonNumber)
            {
                return Decision.Reject("Season number {0} was unexpected considering the folder name {1}", fileInfo.SeasonNumber, folderInfo.ReleaseTitle);
            }

            var unexpected = fileEpisodes.Where(e => folderEpisodes.All(o => o.Id != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode {0} was unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
                }

                return Decision.Reject("Episodes {0} were unexpected considering the {1} folder name", FormatEpisode(unexpected), folderInfo.ReleaseTitle);
            }

            return Decision.Accept();
        }

        private string FormatEpisode(List<Episode> episodes)
        {
            return string.Join(", ", episodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
