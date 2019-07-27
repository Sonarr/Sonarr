using System.Linq;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

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

            if (fileInfo.IsAbsoluteNumbering)
            {
                _logger.Debug("File uses absolute episode numbering, skipping check");
                return Decision.Accept();
            }

            if (folderInfo.SeasonNumber != fileInfo.SeasonNumber)
            {
                return Decision.Reject("Season number {0} was unexpected considering the folder name {1}", fileInfo.SeasonNumber, folderInfo.ReleaseTitle);
            }

            if (!folderInfo.EpisodeNumbers.Any())
            {
                _logger.Debug("No episode numbers in folder ParsedEpisodeInfo, skipping check");
                return Decision.Accept();
            }

            var unexpected = fileInfo.EpisodeNumbers.Where(f => !folderInfo.EpisodeNumbers.Contains(f)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode number(s) in file: {0}", string.Join(", ", unexpected));

                if (unexpected.Count == 1)
                {
                    return Decision.Reject("Episode number {0} was unexpected considering the {1} folder name", unexpected.First(), folderInfo.ReleaseTitle);
                }

                return Decision.Reject("Episode numbers {0} were unexpected considering the {1} folder name", string.Join(", ", unexpected), folderInfo.ReleaseTitle);
            }

            return Decision.Accept();
        }
    }
}
